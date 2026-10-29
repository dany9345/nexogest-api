using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Configuration;
using NexoGest.Api.Data;
using NexoGest.Api.DTOs;
using NexoGest.Api.Entities;

namespace NexoGest.Api.Services;

public sealed class InventoryService(AppDbContext db)
{
    private static ProductDto Map(Product p) => new(p.Id, p.Name, p.Sku, p.Description,
        p.CurrentStock, p.MinimumStock, p.CreatedAt, p.CurrentStock <= p.MinimumStock);
    public async Task<List<ProductDto>> List(string? search, bool? lowStock, CancellationToken ct)
    {
        var q = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim();
            q = q.Where(x => EF.Functions.Like(x.Name, "%" + term + "%")
                || EF.Functions.Like(x.Sku, "%" + term + "%"));
        }
        if (lowStock == true) q = q.Where(x => x.CurrentStock <= x.MinimumStock);
        return (await q.OrderBy(x => x.Name).ToListAsync(ct)).Select(Map).ToList();
    }
    public async Task<ProductDto> Get(int id, CancellationToken ct) =>
        Map(await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw ApiError.NotFound("Producto"));
    public async Task<ProductDto> Create(ProductInput input, CancellationToken ct)
    {
        Validate(input);
        await CheckSku(input.Sku, null, ct);
        var p = new Product(); Assign(p, input);
        db.Products.Add(p);
        // El stock inicial se registra como movimiento para mantener trazabilidad.
        if (p.CurrentStock > 0) db.StockMovements.Add(new StockMovement {
            Product = p, MovementType = MovementType.Entrada,
            Quantity = p.CurrentStock, Reason = "Inventario inicial" });
        await db.SaveChangesAsync(ct);
        return Map(p);
    }
    public async Task<ProductDto> Update(int id, ProductInput input, CancellationToken ct)
    {
        Validate(input);
        var p = await db.Products.FindAsync([id], ct) ?? throw ApiError.NotFound("Producto");
        if (input.CurrentStock != p.CurrentStock)
            throw ApiError.Invalid("El stock solo puede modificarse mediante movimientos.");
        await CheckSku(input.Sku, id, ct);
        Assign(p, input);
        await db.SaveChangesAsync(ct);
        return Map(p);
    }
    public async Task<List<StockMovementDto>> Movements(int productId, CancellationToken ct)
    {
        if (!await db.Products.AnyAsync(x => x.Id == productId, ct)) throw ApiError.NotFound("Producto");
        return await db.StockMovements.AsNoTracking().Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Select(x => new StockMovementDto(x.Id, x.ProductId, x.MovementType.ToString(),
                x.Quantity, x.Reason, x.CreatedAt)).ToListAsync(ct);
    }
    public async Task<ProductDto> Move(int id, StockMovementInput input, CancellationToken ct)
    {
        if (!Enum.IsDefined(input.MovementType) || input.Quantity <= 0 ||
            string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Trim().Length < 3)
            throw ApiError.Invalid("Tipo, cantidad o motivo del movimiento no válidos.");
        if (!await db.Products.AnyAsync(x => x.Id == id, ct)) throw ApiError.NotFound("Producto");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var count = input.MovementType == MovementType.Entrada
            ? await db.Products.Where(x => x.Id == id && x.CurrentStock <= int.MaxValue - input.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.CurrentStock, x => x.CurrentStock + input.Quantity), ct)
            : await db.Products.Where(x => x.Id == id && x.CurrentStock >= input.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.CurrentStock, x => x.CurrentStock - input.Quantity), ct);
        if (count != 1) throw ApiError.Conflict(input.MovementType == MovementType.Salida
            ? "Stock insuficiente para registrar la salida." : "La entrada desbordaría el stock máximo.");
        db.StockMovements.Add(new StockMovement { ProductId = id, MovementType = input.MovementType,
            Quantity = input.Quantity, Reason = input.Reason.Trim() });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Get(id, ct);
    }
    private static void Validate(ProductInput i)
    {
        if (string.IsNullOrWhiteSpace(i.Name) || string.IsNullOrWhiteSpace(i.Sku))
            throw ApiError.Invalid("Nombre y SKU son obligatorios.");
        if (i.MinimumStock < 0 || i.CurrentStock < 0) throw ApiError.Invalid("El stock no puede ser negativo.");
    }
    private async Task CheckSku(string sku, int? id, CancellationToken ct)
    {
        var value = sku.Trim().ToUpperInvariant();
        if (await db.Products.AnyAsync(x => x.Sku == value && (!id.HasValue || x.Id != id.Value), ct))
            throw ApiError.Conflict("Ya existe un producto con ese SKU.");
    }
    private static void Assign(Product p, ProductInput i)
    {
        p.Name = i.Name.Trim(); p.Sku = i.Sku.Trim().ToUpperInvariant();
        p.Description = (i.Description ?? "").Trim(); p.MinimumStock = i.MinimumStock;
        p.CurrentStock = i.CurrentStock;
    }
}
