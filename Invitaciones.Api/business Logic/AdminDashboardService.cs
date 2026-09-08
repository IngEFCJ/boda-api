using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _repo;

    public AdminDashboardService(IAdminDashboardRepository repo)
    {
        _repo = repo;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid eventId, CancellationToken ct)
    {
        var row = await _repo.GetStatsAsync(eventId, ct);

        int confirmationPercentage = row.TotalTickets > 0
            ? (int)Math.Round((double)row.ConfirmedTickets * 100 / row.TotalTickets)
            : 0;

        int checkInPercentage = row.ConfirmedTickets > 0
            ? (int)Math.Round((double)row.CheckedInTickets * 100 / row.ConfirmedTickets)
            : 0;

        var eventDto = new EventAdminDto(
            row.EventId,
            row.EventTitle,
            row.EventDate,
            row.LocationName,
            row.Address
        );

        return new DashboardStatsDto(
            row.TotalInvitations,
            row.TotalTickets,
            row.ConfirmedTickets,
            row.CheckedInTickets,
            confirmationPercentage,
            checkInPercentage,
            eventDto
        );
    }
}
