using Microsoft.AspNetCore.Mvc;
using NexoGest.Api.DTOs;
using NexoGest.Api.Services;

namespace NexoGest.Api.Controllers;

[ApiController, Route("api/dashboard")]
public sealed class DashboardController(DashboardService svc) : ControllerBase
{
    [HttpGet("summary")] public Task<DashboardDto> Get(CancellationToken ct) => svc.Get(ct);
}
