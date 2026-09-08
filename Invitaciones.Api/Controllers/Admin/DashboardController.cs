using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public DashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats/{eventId:guid}")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStats(Guid eventId, CancellationToken ct)
    {
        var stats = await _dashboardService.GetDashboardStatsAsync(eventId, ct);
        return Ok(stats);
    }
}
