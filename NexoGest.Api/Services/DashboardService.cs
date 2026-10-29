using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Data;
using NexoGest.Api.DTOs;

namespace NexoGest.Api.Services;

public sealed class DashboardService(AppDbContext db)
{
    public async Task<DashboardDto> Get(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var first = new DateOnly(today.Year, today.Month, 1);
        var next = first.AddMonths(1);
        var employees = await db.Employees.CountAsync(x => x.IsActive, ct);
        var month = await db.Expenses.Where(x => x.ExpenseDate >= first && x.ExpenseDate < next)
            .SumAsync(x => (long?)x.AmountCents, ct) ?? 0;
        var pending = await db.Expenses.CountAsync(x => x.Status == Entities.ExpenseStatus.Pendiente, ct);
        var low = await db.Products.CountAsync(x => x.CurrentStock <= x.MinimumStock, ct);
        var categories = await db.Expenses.Where(x => x.ExpenseDate >= first && x.ExpenseDate < next)
            .GroupBy(x => x.Category).Select(g => new { Category = g.Key, Total = g.Sum(x => x.AmountCents) })
            .ToListAsync(ct);
        var recent = await db.Expenses.AsNoTracking().Include(x => x.Employee)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(6).ToListAsync(ct);
        return new(employees, month, pending, low,
            categories.Select(x => new CategoryTotalDto(x.Category.ToString(), x.Total)).ToList(),
            recent.Select(ExpenseService.Map).ToList());
    }
}
