namespace Invitaciones.Api.DTO;

public sealed record TicketAdminDto(
    Guid Id,
    Guid InvitationId,
    string Label,
    DateTime? ConfirmedAt,
    DateTime? UsedAt
);

public sealed record CreateTicketRequest(Guid InvitationId, string Label);

public sealed record UpdateTicketRequest(string Label);

/// <summary>
/// Row model for mapping Dapper query results from dbo.Tickets.
/// </summary>
public sealed record TicketAdminRow
{
    public Guid Id { get; init; }
    public Guid InvitationId { get; init; }
    public string Label { get; init; } = "";
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? UsedAt { get; init; }
}
