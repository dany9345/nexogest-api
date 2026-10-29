using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable
namespace NexoGest.Api.Migrations;

[DbContext(typeof(Data.AppDbContext))]
[Migration("20261028000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                FirstName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false, collation: "NOCASE"),
                Department = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                Position = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                HireDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Employees", x => x.Id);
                table.CheckConstraint("CK_Employees_Email", "length(Email) > 3"); });
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Sku = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false, collation: "NOCASE"),
                Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                CurrentStock = table.Column<int>(type: "INTEGER", nullable: false),
                MinimumStock = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Products", x => x.Id);
                table.CheckConstraint("CK_Products_Stock", "CurrentStock >= 0");
                table.CheckConstraint("CK_Products_Min", "MinimumStock >= 0"); });
        migrationBuilder.CreateTable(
            name: "Expenses",
            columns: table => new {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                Category = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 240, nullable: false),
                AmountCents = table.Column<long>(type: "INTEGER", nullable: false),
                ExpenseDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Expenses", x => x.Id);
                table.ForeignKey("FK_Expenses_Employees_EmployeeId", x => x.EmployeeId, "Employees", "Id", onDelete: ReferentialAction.Restrict);
                table.CheckConstraint("CK_Expenses_Amount", "AmountCents > 0"); });
        migrationBuilder.CreateTable(
            name: "StockMovements",
            columns: table => new {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                MovementType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                Reason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_StockMovements", x => x.Id);
                table.ForeignKey("FK_StockMovements_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Restrict);
                table.CheckConstraint("CK_Movements_Quantity", "Quantity > 0"); });
        migrationBuilder.CreateIndex(name: "IX_Employees_Email", table: "Employees", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Expenses_EmployeeId", table: "Expenses", column: "EmployeeId");
        migrationBuilder.CreateIndex(name: "IX_Expenses_Status_ExpenseDate", table: "Expenses", columns: new[] { "Status", "ExpenseDate" });
        migrationBuilder.CreateIndex(name: "IX_Products_Sku", table: "Products", column: "Sku", unique: true);
        migrationBuilder.CreateIndex(name: "IX_StockMovements_ProductId_CreatedAt", table: "StockMovements", columns: new[] { "ProductId", "CreatedAt" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Expenses");
        migrationBuilder.DropTable("StockMovements");
        migrationBuilder.DropTable("Employees");
        migrationBuilder.DropTable("Products");
    }
}
