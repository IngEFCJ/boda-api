using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/invitations")]
[Authorize]
public sealed class InvitationsController : ControllerBase
{
    private readonly IAdminInvitationService _service;

    public InvitationsController(IAdminInvitationService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvitationAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations(
        [FromQuery] Guid eventId,
        [FromQuery] int page = 1,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetInvitationsAsync(eventId, page, search, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(InvitationAdminDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInvitation(
        [FromBody] CreateInvitationRequest request,
        CancellationToken ct = default)
    {
        var created = await _service.CreateInvitationAsync(request.EventId, request, ct);
        return CreatedAtAction(nameof(GetInvitations), new { eventId = request.EventId }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InvitationAdminDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInvitation(
        Guid id,
        [FromBody] UpdateInvitationRequest request,
        CancellationToken ct = default)
    {
        var updated = await _service.UpdateInvitationAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateInvitation(
        Guid id,
        CancellationToken ct = default)
    {
        await _service.DeactivateInvitationAsync(id, ct);
        return NoContent();
    }
}
