using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NexoGest.Api.Configuration;
using NexoGest.Api.Data;
using NexoGest.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "data", "nexogest.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(o => o.AddPolicy("LocalAngular", p => p
    .WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<DashboardService>();
var app = builder.Build();
app.UseExceptionHandler();
app.UseCors("LocalAngular");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DemoSeed.SeedAsync(db);
}
app.Run();

public partial class Program { }
