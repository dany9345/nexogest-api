using Microsoft.AspNetCore.Mvc;
using NexoGest.Api.DTOs;
using NexoGest.Api.Services;

namespace NexoGest.Api.Controllers;

[ApiController, Route("api/employees")]
public sealed class EmployeesController(EmployeeService svc) : ControllerBase
{
    [HttpGet] public Task<List<EmployeeDto>> List([FromQuery] string? search, [FromQuery] bool? active, CancellationToken ct)
        => svc.List(search, active, ct);
    [HttpGet("{id:int}")] public Task<EmployeeDto> Get(int id, CancellationToken ct) => svc.Get(id, ct);
    [HttpPost] public async Task<ActionResult<EmployeeDto>> Create(EmployeeInput input, CancellationToken ct)
    {
        var saved = await svc.Create(input, ct);
        return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
    }
    [HttpPut("{id:int}")] public Task<EmployeeDto> Update(int id, EmployeeInput input, CancellationToken ct) => svc.Update(id, input, ct);
    [HttpPatch("{id:int}/deactivate")] public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await svc.Deactivate(id, ct); return NoContent();
    }
}
