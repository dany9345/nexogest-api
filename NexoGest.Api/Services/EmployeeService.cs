using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Configuration;
using NexoGest.Api.Data;
using NexoGest.Api.DTOs;
using NexoGest.Api.Entities;

namespace NexoGest.Api.Services;

public sealed class EmployeeService(AppDbContext db)
{
    private static EmployeeDto Map(Employee e) => new(e.Id, e.FirstName, e.LastName,
        e.Email, e.Department, e.Position, e.HireDate, e.IsActive, e.CreatedAt);

    public async Task<List<EmployeeDto>> List(string? search, bool? active, CancellationToken ct)
    {
        var q = db.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim();
            q = q.Where(x => EF.Functions.Like(x.FirstName, "%" + term + "%")
                || EF.Functions.Like(x.LastName, "%" + term + "%")
                || EF.Functions.Like(x.FirstName + " " + x.LastName, "%" + term + "%"));
        }
        if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
        return (await q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(ct)).Select(Map).ToList();
    }
    public async Task<EmployeeDto> Get(int id, CancellationToken ct) =>
        Map(await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw ApiError.NotFound("Empleado"));

    public async Task<EmployeeDto> Create(EmployeeInput input, CancellationToken ct)
    {
        Validate(input);
        await CheckEmail(input.Email, null, ct);
        var e = new Employee(); Assign(e, input);
        db.Employees.Add(e);
        await db.SaveChangesAsync(ct);
        return Map(e);
    }
    public async Task<EmployeeDto> Update(int id, EmployeeInput input, CancellationToken ct)
    {
        Validate(input);
        var e = await db.Employees.FindAsync([id], ct) ?? throw ApiError.NotFound("Empleado");
        await CheckEmail(input.Email, id, ct);
        Assign(e, input);
        await db.SaveChangesAsync(ct);
        return Map(e);
    }
    public async Task Deactivate(int id, CancellationToken ct)
    {
        var e = await db.Employees.FindAsync([id], ct) ?? throw ApiError.NotFound("Empleado");
        e.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
    private static void Validate(EmployeeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName)
            || string.IsNullOrWhiteSpace(input.Department) || string.IsNullOrWhiteSpace(input.Position))
            throw ApiError.Invalid("Nombre, apellidos, departamento y cargo son obligatorios.");
        if (input.HireDate == default || input.HireDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            throw ApiError.Invalid("La fecha de incorporación es inválida o futura.");
    }
    private async Task CheckEmail(string email, int? excludeId, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (await db.Employees.AnyAsync(x => x.Email == normalized && (!excludeId.HasValue || x.Id != excludeId.Value), ct))
            throw ApiError.Conflict("Ya existe un empleado con ese correo electrónico.");
    }
    private static void Assign(Employee e, EmployeeInput i)
    {
        e.FirstName = i.FirstName.Trim(); e.LastName = i.LastName.Trim();
        e.Email = i.Email.Trim().ToLowerInvariant(); e.Department = i.Department.Trim();
        e.Position = i.Position.Trim(); e.HireDate = i.HireDate; e.IsActive = i.IsActive;
    }
}
