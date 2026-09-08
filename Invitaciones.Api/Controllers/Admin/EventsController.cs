using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/events")]
[Authorize]
public sealed class EventsController : ControllerBase
{
    private readonly IAdminEventService _eventService;

    public EventsController(IAdminEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var events = await _eventService.GetAllEventsAsync(ct);
        return Ok(events);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var ev = await _eventService.GetEventByIdAsync(id, ct);
        return Ok(ev);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EventAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var created = await _eventService.CreateEventAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EventAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        var updated = await _eventService.UpdateEventAsync(id, request, ct);
        return Ok(updated);
    }
}
