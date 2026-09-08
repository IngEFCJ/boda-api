using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminInvitationRepository
{
    Task<PagedResult<InvitationAdminDto>> GetPagedAsync(Guid eventId, int page, int pageSize, string? search, CancellationToken ct);
    Task<Guid> CreateAsync(Guid eventId, string displayName, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, string displayName, CancellationToken ct);
    Task<bool> DeactivateAsync(Guid id, CancellationToken ct);
    Task<string> GenerateTokenAsync(Guid invitationId, CancellationToken ct);
}
