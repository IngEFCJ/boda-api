using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminDashboardRepository
{
    Task<DashboardStatsRow> GetStatsAsync(Guid eventId, CancellationToken ct);
}
