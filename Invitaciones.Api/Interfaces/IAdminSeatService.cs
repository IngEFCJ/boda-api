using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminSeatService
{
    Task<SeatAssignmentDataDto> GetAssignmentDataAsync(Guid eventId, CancellationToken ct);
    Task SaveAssignmentsAsync(Guid venueId, SaveAssignmentsRequest req, CancellationToken ct);
}
