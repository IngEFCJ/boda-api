using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid eventId, CancellationToken ct);
}
