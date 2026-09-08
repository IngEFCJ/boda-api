using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminEventService
{
    Task<IEnumerable<EventAdminDto>> GetAllEventsAsync(CancellationToken ct);
    Task<EventAdminDto> GetEventByIdAsync(Guid id, CancellationToken ct);
    Task<EventAdminDto> CreateEventAsync(CreateEventRequest req, CancellationToken ct);
    Task<EventAdminDto> UpdateEventAsync(Guid id, UpdateEventRequest req, CancellationToken ct);
}
