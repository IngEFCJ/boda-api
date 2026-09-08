using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/seats")]
[Authorize]
public sealed class SeatsController : ControllerBase
{
    private readonly IAdminSeatService _seatService;

    public SeatsController(IAdminSeatService seatService)
    {
        _seatService = seatService;
    }

    /// <summary>
    /// Get seat assignments and unassigned confirmed tickets for an event.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SeatAssignmentDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAssignmentData(
        [FromQuery] Guid eventId,
        CancellationToken ct)
    {
        if (eventId == Guid.Empty)
            return BadRequest(new { message = "El parámetro eventId es requerido" });

        var data = await _seatService.GetAssignmentDataAsync(eventId, ct);
        return Ok(data);
    }

    /// <summary>
    /// Save all seat assignments for a venue.
    /// </summary>
    [HttpPut("{venueId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveAssignments(
        [FromRoute] Guid venueId,
        [FromBody] SaveAssignmentsRequest request,
        CancellationToken ct)
    {
        if (venueId == Guid.Empty)
            return BadRequest(new { message = "El parámetro venueId es requerido" });

        await _seatService.SaveAssignmentsAsync(venueId, request, ct);
        return NoContent();
    }
}
