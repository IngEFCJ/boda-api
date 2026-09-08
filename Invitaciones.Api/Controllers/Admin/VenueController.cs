using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues")]
[Authorize]
public sealed class VenueController : ControllerBase
{
    private readonly IAdminVenueService _venueService;

    public VenueController(IAdminVenueService venueService)
    {
        _venueService = venueService;
    }

    /// <summary>
    /// Get the venue layout with tables for a given event.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(VenueLayoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEvent(
        [FromQuery] Guid eventId,
        CancellationToken ct)
    {
        if (eventId == Guid.Empty)
            return BadRequest(new { message = "El parámetro eventId es requerido" });

        var venue = await _venueService.GetVenueByEventAsync(eventId, ct);
        if (venue is null)
            return NotFound(new { message = "No se encontró un layout para este evento" });

        return Ok(venue);
    }

    /// <summary>
    /// Create a new venue layout for an event.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VenueLayoutDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVenueRequest request,
        CancellationToken ct)
    {
        var venue = await _venueService.CreateVenueAsync(request, ct);
        return CreatedAtAction(nameof(GetByEvent), new { eventId = venue.EventId }, venue);
    }

    /// <summary>
    /// Update venue dimensions and shape.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VenueLayoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateVenueRequest request,
        CancellationToken ct)
    {
        var venue = await _venueService.UpdateVenueAsync(id, request, ct);
        return Ok(venue);
    }

    /// <summary>
    /// Save all tables (positions, shapes, capacities) for a venue.
    /// </summary>
    [HttpPut("{id:guid}/tables")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveTables(
        [FromRoute] Guid id,
        [FromBody] SaveTablesRequest request,
        CancellationToken ct)
    {
        await _venueService.SaveTablesAsync(id, request, ct);
        return NoContent();
    }
}
