using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NexoGest.Api.Data;
using NexoGest.Api.Entities;

#nullable disable
namespace NexoGest.Api.Migrations;

[DbContext(typeof(AppDbContext))]
public class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
        modelBuilder.Entity("NexoGest.Api.Entities.Employee", b => {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("Department").IsRequired().HasMaxLength(80).HasColumnType("TEXT");
            b.Property<string>("Email").IsRequired().HasMaxLength(180).UseCollation("NOCASE").HasColumnType("TEXT");
            b.Property<string>("FirstName").IsRequired().HasMaxLength(80).HasColumnType("TEXT");
            b.Property<DateOnly>("HireDate").HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("LastName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<string>("Position").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.HasKey("Id"); b.HasIndex("Email").IsUnique();
            b.ToTable("Employees", t => t.HasCheckConstraint("CK_Employees_Email", "length(Email) > 3"));
        });
        modelBuilder.Entity("NexoGest.Api.Entities.Expense", b => {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<long>("AmountCents").HasColumnType("INTEGER");
            b.Property<ExpenseCategory>("Category").HasConversion<string>().IsRequired().HasMaxLength(24).HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("Description").IsRequired().HasMaxLength(240).HasColumnType("TEXT");
            b.Property<int>("EmployeeId").HasColumnType("INTEGER");
            b.Property<DateOnly>("ExpenseDate").HasColumnType("TEXT");
            b.Property<ExpenseStatus>("Status").HasConversion<string>().IsRequired().HasMaxLength(24).HasColumnType("TEXT");
            b.HasKey("Id"); b.HasIndex("EmployeeId"); b.HasIndex("Status", "ExpenseDate");
            b.ToTable("Expenses", t => t.HasCheckConstraint("CK_Expenses_Amount", "AmountCents > 0"));
        });
        modelBuilder.Entity("NexoGest.Api.Entities.Product", b => {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<int>("CurrentStock").HasColumnType("INTEGER");
            b.Property<string>("Description").IsRequired().HasMaxLength(300).HasColumnType("TEXT");
            b.Property<int>("MinimumStock").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("Sku").IsRequired().HasMaxLength(40).UseCollation("NOCASE").HasColumnType("TEXT");
            b.HasKey("Id"); b.HasIndex("Sku").IsUnique();
            b.ToTable("Products", t => { t.HasCheckConstraint("CK_Products_Stock", "CurrentStock >= 0");
                t.HasCheckConstraint("CK_Products_Min", "MinimumStock >= 0"); });
        });
        modelBuilder.Entity("NexoGest.Api.Entities.StockMovement", b => {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<MovementType>("MovementType").HasConversion<string>().IsRequired().HasMaxLength(16).HasColumnType("TEXT");
            b.Property<int>("ProductId").HasColumnType("INTEGER");
            b.Property<int>("Quantity").HasColumnType("INTEGER");
            b.Property<string>("Reason").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
            b.HasKey("Id"); b.HasIndex("ProductId", "CreatedAt");
            b.ToTable("StockMovements", t => t.HasCheckConstraint("CK_Movements_Quantity", "Quantity > 0"));
        });
        modelBuilder.Entity("NexoGest.Api.Entities.Expense", b => {
            b.HasOne("NexoGest.Api.Entities.Employee", "Employee").WithMany("Expenses")
              .HasForeignKey("EmployeeId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.Navigation("Employee");
        });
        modelBuilder.Entity("NexoGest.Api.Entities.StockMovement", b => {
            b.HasOne("NexoGest.Api.Entities.Product", "Product").WithMany("Movements")
              .HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.Navigation("Product");
        });
        modelBuilder.Entity("NexoGest.Api.Entities.Employee", b => b.Navigation("Expenses"));
        modelBuilder.Entity("NexoGest.Api.Entities.Product", b => b.Navigation("Movements"));
    }
}
