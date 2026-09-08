namespace Invitaciones.Api.DTO;

public sealed record InvitationAdminDto(
    Guid Id,
    string DisplayName,
    int TicketCount,
    int ConfirmedCount,
    byte Status,
    string? Token
);

public sealed record CreateInvitationRequest(Guid EventId, string DisplayName);

public sealed record UpdateInvitationRequest(string DisplayName);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
