using static Invitaciones.Api.DTO.DTOs;

namespace Invitaciones.Api.Interfaces
{
    public interface IPublicInvitationService
    {
        Task<InvitationResolveResult<InvitationByTokenResponse>> GetInvitationByTokenAsync(
      string token,
      CancellationToken ct);

        Task<InvitationResolveResult<InvitationByTokenResponse>> ConfirmTicketsAsync(
             ConfirmTicketsRequest request,
             CancellationToken ct);

        Task<InvitationResolveResult<TicketsWithQrResponse>> GetTicketsWithQrByTokenAsync(
             string token,
             CancellationToken ct);
    }

    public enum InvitationResolveStatus { Ok, NotFound, Expired }

    public sealed record InvitationResolveResult<T>(
        InvitationResolveStatus Status,
        T? Data = default)
    {
        public static InvitationResolveResult<T> Ok(T data) => new(InvitationResolveStatus.Ok, data);
        public static InvitationResolveResult<T> NotFound() => new(InvitationResolveStatus.NotFound);
        public static InvitationResolveResult<T> Expired() => new(InvitationResolveStatus.Expired);
    }



}
