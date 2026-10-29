using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NexoGest.Api.Configuration;

public sealed class ApiError(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
    public static ApiError NotFound(string what) => new(404, $"{what} no encontrado.");
    public static ApiError Conflict(string what) => new(409, what);
    public static ApiError Invalid(string what) => new(400, what);
}

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        var status = ex is ApiError a ? a.Status : ex is DbUpdateException ? 409 : 500;
        if (status == 500) logger.LogError(ex, "Error inesperado en {Path}", context.Request.Path);
        else logger.LogWarning("Error HTTP {Status}: {Message}", status, ex.Message);
        context.Response.StatusCode = status;
        var problem = new ProblemDetails {
            Status = status,
            Title = status switch { 400 => "Datos no válidos", 404 => "No encontrado", 409 => "Conflicto", _ => "Error interno" },
            Detail = status == 500 ? "Error inesperado del servidor." : status == 409 && ex is DbUpdateException
                ? "Los datos entran en conflicto con una restricción de la base de datos." : ex.Message,
            Instance = context.Request.Path
        };
        await context.Response.WriteAsJsonAsync(problem, cancellationToken: ct);
        return true;
    }
}
