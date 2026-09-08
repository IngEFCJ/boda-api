using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminTicketService
{
    Task<IEnumerable<TicketAdminDto>> GetTicketsAsync(Guid invitationId, CancellationToken ct);
    Task<TicketAdminDto> CreateTicketAsync(Guid invitationId, CreateTicketRequest req, CancellationToken ct);
    Task<TicketAdminDto> UpdateTicketAsync(Guid ticketId, UpdateTicketRequest req, CancellationToken ct);
    Task DeleteTicketAsync(Guid ticketId, CancellationToken ct);
    Task<TicketAdminDto> ConfirmTicketAsync(Guid ticketId, CancellationToken ct);
}
