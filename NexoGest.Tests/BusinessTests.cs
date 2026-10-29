global using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Configuration;
using NexoGest.Api.Data;
using NexoGest.Api.DTOs;
using NexoGest.Api.Entities;
using NexoGest.Api.Services;

namespace NexoGest.Tests;

public sealed class BusinessTests
{
    private static async Task<(SqliteConnection, AppDbContext)> Database()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        context.Employees.Add(new Employee { FirstName = "Eva", LastName = "Prueba",
            Email = "eva@demo.local", Department = "Ventas", Position = "Técnica",
            HireDate = new DateOnly(2023, 1, 1) });
        context.Products.Add(new Product { Name = "Monitor", Sku = "MON-1",
            CurrentStock = 2, MinimumStock = 3 });
        await context.SaveChangesAsync();
        return (connection, context);
    }
    [Fact]
    public void GastoNegativo_SeRechaza()
    {
        var ex = Assert.Throws<ApiError>(() => ExpenseService.Validate(new ExpenseInput {
            EmployeeId = 1, AmountCents = -1, Description = "Parking cliente",
            Category = ExpenseCategory.Parking, ExpenseDate = DateOnly.FromDateTime(DateTime.Today) }));
        Assert.Equal(400, ex.Status);
    }
    [Fact]
    public async Task Gasto_AprobacionNoPuedeRepetirse()
    {
        var (connection, db) = await Database();
        await using (connection) await using (db) {
            var service = new ExpenseService(db);
            var saved = await service.Create(new ExpenseInput { EmployeeId = 1,
                Category = ExpenseCategory.Restaurante, Description = "Comida de equipo",
                ExpenseDate = DateOnly.FromDateTime(DateTime.Today), AmountCents = 1400 }, default);
            Assert.Equal("Pendiente", saved.Status);
            Assert.Equal("Aprobado", (await service.SetStatus(saved.Id, ExpenseStatus.Aprobado, default)).Status);
            var ex = await Assert.ThrowsAsync<ApiError>(() => service.SetStatus(saved.Id, ExpenseStatus.Rechazado, default));
            Assert.Equal(409, ex.Status);
        }
    }
    [Fact]
    public async Task Salida_ConStockInsuficiente_NoModificaExistenciasNiHistorial()
    {
        var (connection, db) = await Database();
        await using (connection) await using (db) {
            var service = new InventoryService(db);
            var ex = await Assert.ThrowsAsync<ApiError>(() => service.Move(1,
                new StockMovementInput { MovementType = MovementType.Salida,
                    Quantity = 3, Reason = "Entrega comercial" }, default));
            Assert.Equal(409, ex.Status);
            Assert.Equal(2, (await service.Get(1, default)).CurrentStock);
            Assert.Empty(await service.Movements(1, default));
        }
    }
    [Fact]
    public async Task MovimientoValido_PersisteStockYRegistro()
    {
        var (connection, db) = await Database();
        await using (connection) await using (db) {
            var service = new InventoryService(db);
            var result = await service.Move(1, new StockMovementInput {
                MovementType = MovementType.Entrada, Quantity = 7, Reason = "Reposición" }, default);
            Assert.Equal(9, result.CurrentStock);
            var movement = Assert.Single(await service.Movements(1, default));
            Assert.Equal(7, movement.Quantity);
            db.ChangeTracker.Clear();
            Assert.Equal(9, (await db.Products.AsNoTracking().SingleAsync()).CurrentStock);
        }
    }
    [Fact]
    public async Task EmailDuplicado_SeRechazaSinDuplicarFila()
    {
        var (connection, db) = await Database();
        await using (connection) await using (db) {
            var service = new EmployeeService(db);
            var error = await Assert.ThrowsAsync<ApiError>(() => service.Create(new EmployeeInput {
                FirstName = "Otra", LastName = "Persona", Email = "EVA@DEMO.LOCAL",
                Department = "Ventas", Position = "Técnica", HireDate = new DateOnly(2024, 1, 1) }, default));
            Assert.Equal(409, error.Status);
            Assert.Equal(1, await db.Employees.CountAsync());
        }
    }
    [Fact]
    public async Task Dashboard_CalculaIndicadoresDeTablasReales()
    {
        var (connection, db) = await Database();
        await using (connection) await using (db) {
            await new ExpenseService(db).Create(new ExpenseInput {
                EmployeeId = 1, AmountCents = 1530, Category = ExpenseCategory.Parking,
                Description = "Parking oficina", ExpenseDate = DateOnly.FromDateTime(DateTime.Today) }, default);
            var dashboard = await new DashboardService(db).Get(default);
            Assert.Equal(1, dashboard.ActiveEmployees);
            Assert.Equal(1530, dashboard.MonthExpensesCents);
            Assert.Equal(1, dashboard.PendingExpenses);
            Assert.Equal(1, dashboard.LowStockProducts);
            Assert.Equal("Eva Prueba", Assert.Single(dashboard.RecentExpenses).EmployeeName);
        }
    }
    [Fact]
    public async Task MigracionInicial_CreaTablasYSeedEsIdempotente()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.MigrateAsync();
        await DemoSeed.SeedAsync(db);
        Assert.Equal(12, await db.Employees.CountAsync());
        Assert.Equal(25, await db.Expenses.CountAsync());
        Assert.Equal(15, await db.Products.CountAsync());
        Assert.Equal(15, await db.StockMovements.CountAsync());
        await DemoSeed.SeedAsync(db);
        Assert.Equal(12, await db.Employees.CountAsync());
        Assert.Equal(25, await db.Expenses.CountAsync());
        Assert.Equal(15, await db.StockMovements.CountAsync());
    }

    [Fact]
    public async Task BusquedaEmpleados_NoDistingueMayusculasEnSQLite()
    {
        var (connection, db) = await Database();
        await using (connection) await using (db) {
            var result = await new EmployeeService(db).List("eva", true, default);
            Assert.Single(result);
            Assert.Equal("Eva", result[0].FirstName);
        }
    }

}
