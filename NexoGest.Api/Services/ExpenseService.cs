using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Configuration;
using NexoGest.Api.Data;
using NexoGest.Api.DTOs;
using NexoGest.Api.Entities;

namespace NexoGest.Api.Services;

public sealed class ExpenseService(AppDbContext db)
{
    public static ExpenseDto Map(Expense e) => new(e.Id, e.EmployeeId,
        e.Employee.FirstName + " " + e.Employee.LastName, e.Category.ToString(), e.Description,
        e.AmountCents, e.ExpenseDate, e.Status.ToString(), e.CreatedAt);

    public async Task<List<ExpenseDto>> List(int? employeeId, ExpenseStatus? status,
        ExpenseCategory? category, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        if (from.HasValue && to.HasValue && from > to) throw ApiError.Invalid("Rango de fechas inválido.");
        var q = db.Expenses.AsNoTracking().Include(x => x.Employee).AsQueryable();
        if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId.Value);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (category.HasValue) q = q.Where(x => x.Category == category.Value);
        if (from.HasValue) q = q.Where(x => x.ExpenseDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.ExpenseDate <= to.Value);
        return (await q.OrderByDescending(x => x.ExpenseDate).ThenByDescending(x => x.Id)
            .ToListAsync(ct)).Select(Map).ToList();
    }
    public async Task<ExpenseDto> Get(int id, CancellationToken ct) =>
        Map(await db.Expenses.AsNoTracking().Include(x => x.Employee).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw ApiError.NotFound("Gasto"));
    public async Task<ExpenseDto> Create(ExpenseInput i, CancellationToken ct)
    {
        Validate(i);
        var employee = await db.Employees.FirstOrDefaultAsync(x => x.Id == i.EmployeeId, ct)
            ?? throw ApiError.Invalid("El empleado seleccionado no existe.");
        if (!employee.IsActive) throw ApiError.Invalid("No se pueden asignar gastos nuevos a empleados inactivos.");
        var e = new Expense { EmployeeId = i.EmployeeId, Employee = employee,
            Category = i.Category, Description = i.Description.Trim(), AmountCents = i.AmountCents,
            ExpenseDate = i.ExpenseDate };
        db.Expenses.Add(e);
        await db.SaveChangesAsync(ct);
        return Map(e);
    }
    public async Task<ExpenseDto> SetStatus(int id, ExpenseStatus status, CancellationToken ct)
    {
        if (status == ExpenseStatus.Pendiente || !Enum.IsDefined(status))
            throw ApiError.Invalid("Solo se puede aprobar o rechazar un gasto.");
        var e = await db.Expenses.Include(x => x.Employee).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw ApiError.NotFound("Gasto");
        // Update condicionado para evitar dos aprobaciones/rechazos simultáneos.
        var affected = await db.Expenses.Where(x => x.Id == id && x.Status == ExpenseStatus.Pendiente)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, status), ct);
        if (affected != 1) throw ApiError.Conflict("Este gasto ya ha sido resuelto.");
        e.Status = status;
        return Map(e);
    }
    public static void Validate(ExpenseInput i)
    {
        if (i.AmountCents <= 0 || i.AmountCents > 100000000000)
            throw ApiError.Invalid("El importe debe ser mayor que cero y no superar 1.000 millones de euros.");
        if (i.EmployeeId <= 0) throw ApiError.Invalid("Selecciona un empleado.");
        if (!Enum.IsDefined(i.Category)) throw ApiError.Invalid("La categoría no es válida.");
        if (string.IsNullOrWhiteSpace(i.Description) || i.Description.Trim().Length < 3)
            throw ApiError.Invalid("La descripción debe tener al menos tres caracteres.");
        if (i.ExpenseDate == default || i.ExpenseDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            throw ApiError.Invalid("La fecha del gasto es inválida o futura.");
    }
}
