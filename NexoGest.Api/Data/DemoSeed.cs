using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Entities;

namespace NexoGest.Api.Data;

public static class DemoSeed
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Employees.AnyAsync())
        {
            var people = new (string First, string Last, string Dept, string Position)[] {
                ("Laura", "Martín", "Comercial", "Responsable comercial"),
                ("Carlos", "Ruiz", "Administración", "Técnico administrativo"),
                ("Ana", "Sánchez", "RR. HH.", "Responsable de personas"),
                ("David", "López", "Comercial", "Ejecutivo de cuentas"),
                ("Elena", "García", "Operaciones", "Coordinadora"),
                ("Marcos", "Fernández", "Comercial", "Ejecutivo de cuentas"),
                ("Paula", "Torres", "Administración", "Contabilidad"),
                ("Javier", "Moreno", "Logística", "Técnico de almacén"),
                ("Lucía", "Díaz", "Comercial", "Ejecutiva de cuentas"),
                ("Sergio", "Navarro", "Operaciones", "Analista"),
                ("Marta", "Ortega", "RR. HH.", "Generalista"),
                ("Pablo", "Castro", "Logística", "Responsable de almacén")
            };
            for (int n = 0; n < people.Length; n++)
            {
                var x = people[n];
                db.Employees.Add(new Employee {
                    FirstName = x.First, LastName = x.Last,
                    Email = $"{RemoveAccents(x.First)}.{RemoveAccents(x.Last)}@demo.nexogest.local".ToLowerInvariant(),
                    Department = x.Dept, Position = x.Position,
                    HireDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-(150 + n * 68))),
                    IsActive = n != 10
                });
            }
            await db.SaveChangesAsync();
        }
        if (!await db.Expenses.AnyAsync())
        {
            var employees = await db.Employees.OrderBy(x => x.Id).ToListAsync();
            var kinds = Enum.GetValues<ExpenseCategory>();
            var descriptions = new[] { "Aparcamiento visita cliente", "Almuerzo con equipo comercial",
                "Billete de tren a reunión", "Desplazamiento regional", "Material de oficina",
                "Gastos de atención al cliente", "Parking de estación", "Dietas jornada de formación" };
            for (int i = 0; i < 25; i++)
            {
                var day = DateOnly.FromDateTime(DateTime.Today.AddDays(-((i * 7) % 84)));
                db.Expenses.Add(new Expense {
                    EmployeeId = employees[i % 10].Id, Category = kinds[i % kinds.Length],
                    Description = descriptions[i % descriptions.Length],
                    AmountCents = 890 + ((i * 1397) % 17900), ExpenseDate = day,
                    Status = i % 5 == 0 ? ExpenseStatus.Rechazado : i % 3 == 0
                        ? ExpenseStatus.Pendiente : ExpenseStatus.Aprobado,
                    CreatedAt = DateTime.UtcNow.AddHours(-(i * 7))
                });
            }
            await db.SaveChangesAsync();
        }
        if (!await db.Products.AnyAsync())
        {
            var items = new (string Name, string Sku, int Stock, int Min)[] {
                ("Portátil oficina 14 pulgadas", "INF-001", 7, 3),
                ("Monitor 24 pulgadas", "INF-002", 4, 5),
                ("Teclado inalámbrico", "INF-003", 16, 6),
                ("Ratón ergonómico", "INF-004", 13, 5),
                ("Auriculares USB", "INF-005", 2, 4),
                ("Dock USB-C", "INF-006", 9, 3),
                ("Maletín comercial", "COM-001", 12, 6),
                ("Tarjetas de visita (pack)", "COM-002", 18, 8),
                ("Material publicitario (kit)", "COM-003", 3, 5),
                ("Adaptador HDMI", "INF-007", 7, 3),
                ("Webcam 1080p", "INF-008", 5, 3),
                ("Agenda de empresa", "OFI-001", 25, 10),
                ("Pack bolígrafos", "OFI-002", 40, 12),
                ("Batería externa USB", "COM-004", 1, 3),
                ("Soporte portátil", "INF-009", 8, 4)
            };
            foreach (var x in items)
            {
                var p = new Product { Name = x.Name, Sku = x.Sku, CurrentStock = x.Stock,
                    MinimumStock = x.Min, Description = "Material corporativo · uso interno" };
                db.Products.Add(p);
                db.StockMovements.Add(new StockMovement { Product = p,
                    MovementType = MovementType.Entrada, Quantity = x.Stock,
                    Reason = "Inventario inicial de demostración" });
            }
            await db.SaveChangesAsync();
        }
    }
    private static string RemoveAccents(string value) => value
        .Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u');
}
