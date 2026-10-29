using System.ComponentModel.DataAnnotations;
using NexoGest.Api.Entities;

namespace NexoGest.Api.DTOs;

public sealed class EmployeeInput
{
    [Required, StringLength(80, MinimumLength = 1)] public string FirstName { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 1)] public string LastName { get; set; } = "";
    [Required, EmailAddress, StringLength(180)] public string Email { get; set; } = "";
    [Required, StringLength(80)] public string Department { get; set; } = "";
    [Required, StringLength(100)] public string Position { get; set; } = "";
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;
}
public record EmployeeDto(int Id, string FirstName, string LastName, string Email,
    string Department, string Position, DateOnly HireDate, bool IsActive, DateTime CreatedAt);

public sealed class ExpenseInput
{
    [Range(1, int.MaxValue)] public int EmployeeId { get; set; }
    public ExpenseCategory Category { get; set; }
    [Required, StringLength(240, MinimumLength = 3)] public string Description { get; set; } = "";
    [Range(typeof(long), "1", "100000000000")] public long AmountCents { get; set; }
    public DateOnly ExpenseDate { get; set; }
}
public record ExpenseDto(int Id, int EmployeeId, string EmployeeName, string Category,
    string Description, long AmountCents, DateOnly ExpenseDate, string Status, DateTime CreatedAt);
public record ExpenseStatusInput(ExpenseStatus Status);

public sealed class ProductInput
{
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, StringLength(40)] public string Sku { get; set; } = "";
    [StringLength(300)] public string Description { get; set; } = "";
    [Range(0, int.MaxValue)] public int MinimumStock { get; set; }
    [Range(0, int.MaxValue)] public int CurrentStock { get; set; }
}
public record ProductDto(int Id, string Name, string Sku, string Description,
    int CurrentStock, int MinimumStock, DateTime CreatedAt, bool IsLowStock);
public sealed class StockMovementInput
{
    public MovementType MovementType { get; set; }
    [Range(1, int.MaxValue)] public int Quantity { get; set; }
    [Required, StringLength(200, MinimumLength = 3)] public string Reason { get; set; } = "";
}
public record StockMovementDto(int Id, int ProductId, string MovementType, int Quantity,
    string Reason, DateTime CreatedAt);
public record CategoryTotalDto(string Category, long AmountCents);
public record DashboardDto(int ActiveEmployees, long MonthExpensesCents, int PendingExpenses,
    int LowStockProducts, List<CategoryTotalDto> ExpensesByCategory, List<ExpenseDto> RecentExpenses);
