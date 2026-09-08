using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminInvitationService
{
    Task<PagedResult<InvitationAdminDto>> GetInvitationsAsync(Guid eventId, int page, string? search, CancellationToken ct);
    Task<InvitationAdminDto> CreateInvitationAsync(Guid eventId, CreateInvitationRequest req, CancellationToken ct);
    Task<InvitationAdminDto> UpdateInvitationAsync(Guid id, UpdateInvitationRequest req, CancellationToken ct);
    Task DeactivateInvitationAsync(Guid id, CancellationToken ct);
}
