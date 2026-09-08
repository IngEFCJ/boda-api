using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminCheckInRepository
{
    Task<TicketCheckInRow?> GetTicketForCheckInAsync(Guid ticketId, CancellationToken ct);
    Task<bool> MarkUsedAsync(Guid ticketId, CancellationToken ct);
    Task<PagedResult<CheckInHistoryRow>> GetCheckInHistoryAsync(Guid eventId, int page, int pageSize, CancellationToken ct);
}
