using static Invitaciones.Api.DTO.DTOs;

namespace Invitaciones.Api.Interfaces
{
    public interface IInvitationRepository
    {
        Task<InvitationBundleResult> GetInvitationBundleByTokenAsync(string token, CancellationToken ct);

        Task<int> ConfirmTicketsByTokenAsync(string tokenHash, IReadOnlyList<Guid>? ticketIds, CancellationToken ct);

    }
}
