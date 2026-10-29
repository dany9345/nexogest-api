using Microsoft.AspNetCore.Mvc;
using NexoGest.Api.DTOs;
using NexoGest.Api.Services;

namespace NexoGest.Api.Controllers;

[ApiController, Route("api/products")]
public sealed class ProductsController(InventoryService svc) : ControllerBase
{
    [HttpGet] public Task<List<ProductDto>> List(string? search, bool? lowStock, CancellationToken ct)
        => svc.List(search, lowStock, ct);
    [HttpGet("{id:int}")] public Task<ProductDto> Get(int id, CancellationToken ct) => svc.Get(id, ct);
    [HttpPost] public async Task<ActionResult<ProductDto>> Create(ProductInput input, CancellationToken ct)
    {
        var saved = await svc.Create(input, ct);
        return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
    }
    [HttpPut("{id:int}")] public Task<ProductDto> Update(int id, ProductInput input, CancellationToken ct)
        => svc.Update(id, input, ct);
    [HttpGet("{id:int}/movements")] public Task<List<StockMovementDto>> Movements(int id, CancellationToken ct)
        => svc.Movements(id, ct);
    [HttpPost("{id:int}/movements")] public Task<ProductDto> Move(int id, StockMovementInput input, CancellationToken ct)
        => svc.Move(id, input, ct);
}
