using Dapper;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminDashboardRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<DashboardStatsRow> GetStatsAsync(Guid eventId, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            SELECT
                e.Id AS EventId,
                e.Title AS EventTitle,
                e.EventDate,
                e.LocationName,
                e.Address,
                COUNT(DISTINCT i.Id) AS TotalInvitations,
                COUNT(DISTINCT t.Id) AS TotalTickets,
                COUNT(DISTINCT CASE WHEN t.ConfirmedAt IS NOT NULL THEN t.Id END) AS ConfirmedTickets,
                COUNT(DISTINCT CASE WHEN t.UsedAt IS NOT NULL THEN t.Id END) AS CheckedInTickets
            FROM dbo.Events e
            LEFT JOIN dbo.Invitations i ON i.EventId = e.Id
            LEFT JOIN dbo.Tickets t ON t.InvitationId = i.Id
            WHERE e.Id = @EventId
            GROUP BY e.Id, e.Title, e.EventDate, e.LocationName, e.Address
            """;

        var result = await conn.QuerySingleOrDefaultAsync<DashboardStatsRow>(
            new CommandDefinition(sql, new { EventId = eventId }, cancellationToken: ct));

        return result ?? throw new KeyNotFoundException($"Evento con ID '{eventId}' no encontrado");
    }
}
