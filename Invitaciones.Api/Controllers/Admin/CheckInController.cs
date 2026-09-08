using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/checkin")]
[Authorize]
public sealed class CheckInController : ControllerBase
{
    private readonly IAdminCheckInService _checkInService;

    public CheckInController(IAdminCheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    /// <summary>
    /// Process a QR scan payload and perform check-in.
    /// </summary>
    [HttpPost("scan")]
    [ProducesResponseType(typeof(CheckInResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Scan(
        [FromBody] QrScanRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.QrPayload))
            return BadRequest(new { message = "El payload del QR es requerido" });

        var result = await _checkInService.ProcessQrScanAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated check-in history for an event.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedResult<CheckInHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] Guid eventId,
        [FromQuery] int page = 1,
        CancellationToken ct = default)
    {
        if (eventId == Guid.Empty)
            return BadRequest(new { message = "El parámetro eventId es requerido" });

        if (page < 1)
            page = 1;

        var history = await _checkInService.GetHistoryAsync(eventId, page, ct);
        return Ok(history);
    }
}
