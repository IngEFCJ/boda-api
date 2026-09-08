using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminSeatRepository
{
    Task<IEnumerable<SeatAssignmentRow>> GetByVenueAsync(Guid venueLayoutId, CancellationToken ct);
    Task<IEnumerable<TicketAdminRow>> GetUnassignedConfirmedAsync(Guid eventId, CancellationToken ct);
    Task SaveAssignmentsAsync(Guid venueLayoutId, IEnumerable<SeatAssignmentRow> assignments, CancellationToken ct);
}
