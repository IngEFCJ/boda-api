namespace Invitaciones.Api.DTO;

public sealed record DashboardStatsDto(
    int TotalInvitations,
    int TotalTickets,
    int ConfirmedTickets,
    int CheckedInTickets,
    int ConfirmationPercentage,
    int CheckInPercentage,
    EventAdminDto Event
);

public sealed record DashboardStatsRow
{
    public int TotalInvitations { get; init; }
    public int TotalTickets { get; init; }
    public int ConfirmedTickets { get; init; }
    public int CheckedInTickets { get; init; }
    public Guid EventId { get; init; }
    public string EventTitle { get; init; } = "";
    public DateTime EventDate { get; init; }
    public string LocationName { get; init; } = "";
    public string Address { get; init; } = "";
}
