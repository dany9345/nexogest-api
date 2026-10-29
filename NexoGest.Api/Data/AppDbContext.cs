using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Entities;

namespace NexoGest.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var e = model.Entity<Employee>();
        e.ToTable("Employees", t => t.HasCheckConstraint("CK_Employees_Email", "length(Email) > 3"));
        e.HasKey(x => x.Id);
        e.Property(x => x.FirstName).IsRequired().HasMaxLength(80);
        e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        e.Property(x => x.Email).IsRequired().HasMaxLength(180).UseCollation("NOCASE");
        e.Property(x => x.Department).IsRequired().HasMaxLength(80);
        e.Property(x => x.Position).IsRequired().HasMaxLength(100);
        e.HasIndex(x => x.Email).IsUnique();

        var ex = model.Entity<Expense>();
        ex.ToTable("Expenses", t => t.HasCheckConstraint("CK_Expenses_Amount", "AmountCents > 0"));
        ex.HasKey(x => x.Id);
        ex.Property(x => x.Category).HasConversion<string>().HasMaxLength(24).IsRequired();
        ex.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        ex.Property(x => x.Description).IsRequired().HasMaxLength(240);
        ex.HasOne(x => x.Employee).WithMany(x => x.Expenses)
          .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        ex.HasIndex(x => new { x.Status, x.ExpenseDate });
        ex.HasIndex(x => x.EmployeeId);

        var p = model.Entity<Product>();
        p.ToTable("Products", t => {
            t.HasCheckConstraint("CK_Products_Stock", "CurrentStock >= 0");
            t.HasCheckConstraint("CK_Products_Min", "MinimumStock >= 0");
        });
        p.HasKey(x => x.Id);
        p.Property(x => x.Name).IsRequired().HasMaxLength(120);
        p.Property(x => x.Sku).IsRequired().HasMaxLength(40).UseCollation("NOCASE");
        p.Property(x => x.Description).HasMaxLength(300);
        p.HasIndex(x => x.Sku).IsUnique();

        var m = model.Entity<StockMovement>();
        m.ToTable("StockMovements", t => t.HasCheckConstraint("CK_Movements_Quantity", "Quantity > 0"));
        m.HasKey(x => x.Id);
        m.Property(x => x.MovementType).HasConversion<string>().HasMaxLength(16).IsRequired();
        m.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        m.HasOne(x => x.Product).WithMany(x => x.Movements)
          .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        m.HasIndex(x => new { x.ProductId, x.CreatedAt });
    }
}
