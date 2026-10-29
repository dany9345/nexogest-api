using Microsoft.AspNetCore.Mvc;
using NexoGest.Api.DTOs;
using NexoGest.Api.Entities;
using NexoGest.Api.Services;

namespace NexoGest.Api.Controllers;

[ApiController, Route("api/expenses")]
public sealed class ExpensesController(ExpenseService svc) : ControllerBase
{
    [HttpGet] public Task<List<ExpenseDto>> List(int? employeeId, ExpenseStatus? status,
        ExpenseCategory? category, DateOnly? from, DateOnly? to, CancellationToken ct)
        => svc.List(employeeId, status, category, from, to, ct);
    [HttpGet("{id:int}")] public Task<ExpenseDto> Get(int id, CancellationToken ct) => svc.Get(id, ct);
    [HttpPost] public async Task<ActionResult<ExpenseDto>> Create(ExpenseInput input, CancellationToken ct)
    {
        var saved = await svc.Create(input, ct);
        return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
    }
    [HttpPatch("{id:int}/status")] public Task<ExpenseDto> SetStatus(int id,
        ExpenseStatusInput input, CancellationToken ct) => svc.SetStatus(id, input.Status, ct);
}
