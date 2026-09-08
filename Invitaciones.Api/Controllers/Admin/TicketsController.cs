using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/tickets")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly IAdminTicketService _ticketService;

    public TicketsController(IAdminTicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// List all tickets for a given invitation.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByInvitation(
        [FromQuery] Guid invitationId,
        CancellationToken ct)
    {
        if (invitationId == Guid.Empty)
            return BadRequest(new { message = "El parámetro invitationId es requerido" });

        var tickets = await _ticketService.GetTicketsAsync(invitationId, ct);
        return Ok(tickets);
    }

    /// <summary>
    /// Create a new ticket for an invitation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken ct)
    {
        var ticket = await _ticketService.CreateTicketAsync(request.InvitationId, request, ct);
        return CreatedAtAction(nameof(GetByInvitation), new { invitationId = ticket.InvitationId }, ticket);
    }

    /// <summary>
    /// Update a ticket's label.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TicketAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken ct)
    {
        var ticket = await _ticketService.UpdateTicketAsync(id, request, ct);
        return Ok(ticket);
    }

    /// <summary>
    /// Delete a ticket (only if not confirmed and not used).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        await _ticketService.DeleteTicketAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Manually confirm a ticket.
    /// </summary>
    [HttpPatch("{id:guid}/confirm")]
    [ProducesResponseType(typeof(TicketAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var ticket = await _ticketService.ConfirmTicketAsync(id, ct);
        return Ok(ticket);
    }
}
