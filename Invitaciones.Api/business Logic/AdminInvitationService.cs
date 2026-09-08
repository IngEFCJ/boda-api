using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminInvitationService : IAdminInvitationService
{
    private const int DefaultPageSize = 20;
    private readonly IAdminInvitationRepository _repo;

    public AdminInvitationService(IAdminInvitationRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<InvitationAdminDto>> GetInvitationsAsync(
        Guid eventId, int page, string? search, CancellationToken ct)
    {
        if (page < 1) page = 1;

        return await _repo.GetPagedAsync(eventId, page, DefaultPageSize, search, ct);
    }

    public async Task<InvitationAdminDto> CreateInvitationAsync(
        Guid eventId, CreateInvitationRequest req, CancellationToken ct)
    {
        var displayName = ValidateDisplayName(req.DisplayName);

        var id = await _repo.CreateAsync(eventId, displayName, ct);
        var token = await _repo.GenerateTokenAsync(id, ct);

        return new InvitationAdminDto(id, displayName, 0, 0, 1, token);
    }

    public async Task<InvitationAdminDto> UpdateInvitationAsync(
        Guid id, UpdateInvitationRequest req, CancellationToken ct)
    {
        var displayName = ValidateDisplayName(req.DisplayName);

        var updated = await _repo.UpdateAsync(id, displayName, ct);
        if (!updated)
            throw new KeyNotFoundException($"Invitación con ID '{id}' no encontrada.");

        // Return updated DTO — ticket counts unchanged, token not affected
        return new InvitationAdminDto(id, displayName, 0, 0, 1, null);
    }

    public async Task DeactivateInvitationAsync(Guid id, CancellationToken ct)
    {
        var deactivated = await _repo.DeactivateAsync(id, ct);
        if (!deactivated)
            throw new KeyNotFoundException($"Invitación con ID '{id}' no encontrada.");
    }

    private static string ValidateDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("El nombre de la invitación es requerido.");

        var trimmed = displayName.Trim();

        if (trimmed.Length > 150)
            throw new ArgumentException("El nombre de la invitación no puede exceder 150 caracteres.");

        return trimmed;
    }
}
