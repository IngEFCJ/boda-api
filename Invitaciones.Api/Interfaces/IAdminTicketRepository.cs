using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminTicketRepository
{
    Task<IEnumerable<TicketAdminRow>> GetByInvitationAsync(Guid invitationId, CancellationToken ct);
    Task<TicketAdminRow?> GetByIdAsync(Guid ticketId, CancellationToken ct);
    Task<Guid> CreateAsync(Guid invitationId, string label, CancellationToken ct);
    Task<bool> UpdateLabelAsync(Guid ticketId, string label, CancellationToken ct);
    Task<bool> DeleteAsync(Guid ticketId, CancellationToken ct);
    Task<bool> ManualConfirmAsync(Guid ticketId, CancellationToken ct);
}
