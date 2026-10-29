namespace NexoGest.Api.Entities;

public sealed class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public string Position { get; set; } = "";
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Expense> Expenses { get; set; } = [];
}

public enum ExpenseCategory { Parking, Restaurante, Transporte, Dietas, Material, Otros }
public enum ExpenseStatus { Pendiente, Aprobado, Rechazado }

public sealed class Expense
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public ExpenseCategory Category { get; set; }
    public string Description { get; set; } = "";
    public long AmountCents { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pendiente;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public string Description { get; set; } = "";
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<StockMovement> Movements { get; set; } = [];
}

public enum MovementType { Entrada, Salida }
public sealed class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
