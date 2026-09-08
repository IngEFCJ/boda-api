using Dapper;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminSeatRepository : IAdminSeatRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminSeatRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<SeatAssignmentRow>> GetByVenueAsync(Guid venueLayoutId, CancellationToken ct)
    {
        const string sql = """
            SELECT sa.Id, sa.VenueTableId, sa.SeatNumber, sa.TicketId,
                   t.Label AS GuestName, i.DisplayName AS InvitationName, sa.CreatedAt
            FROM dbo.SeatAssignments sa
            INNER JOIN dbo.VenueTables vt ON vt.Id = sa.VenueTableId
            INNER JOIN dbo.Tickets t ON t.Id = sa.TicketId
            INNER JOIN dbo.Invitations i ON i.Id = t.InvitationId
            WHERE vt.VenueLayoutId = @VenueLayoutId
            ORDER BY vt.TableNumber, sa.SeatNumber
            """;

        using var conn = _db.Create();
        return await conn.QueryAsync<SeatAssignmentRow>(sql, new { VenueLayoutId = venueLayoutId });
    }

    public async Task<IEnumerable<TicketAdminRow>> GetUnassignedConfirmedAsync(Guid eventId, CancellationToken ct)
    {
        const string sql = """
            SELECT t.Id, t.InvitationId, t.Label, t.ConfirmedAt, t.UsedAt
            FROM dbo.Tickets t
            INNER JOIN dbo.Invitations i ON i.Id = t.InvitationId
            WHERE i.EventId = @EventId
              AND t.ConfirmedAt IS NOT NULL
              AND t.Id NOT IN (SELECT TicketId FROM dbo.SeatAssignments)
            ORDER BY t.Label
            """;

        using var conn = _db.Create();
        return await conn.QueryAsync<TicketAdminRow>(sql, new { EventId = eventId });
    }

    public async Task SaveAssignmentsAsync(Guid venueLayoutId, IEnumerable<SeatAssignmentRow> assignments, CancellationToken ct)
    {
        using var conn = _db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            // Delete all existing seat assignments for tables belonging to this venue layout
            const string deleteSql = """
                DELETE sa
                FROM dbo.SeatAssignments sa
                INNER JOIN dbo.VenueTables vt ON vt.Id = sa.VenueTableId
                WHERE vt.VenueLayoutId = @VenueLayoutId
                """;

            await conn.ExecuteAsync(deleteSql, new { VenueLayoutId = venueLayoutId }, tx);

            // Insert new assignments
            const string insertSql = """
                INSERT INTO dbo.SeatAssignments (Id, VenueTableId, SeatNumber, TicketId, CreatedAt)
                VALUES (@Id, @VenueTableId, @SeatNumber, @TicketId, SYSUTCDATETIME())
                """;

            foreach (var assignment in assignments)
            {
                await conn.ExecuteAsync(insertSql, new
                {
                    Id = Guid.NewGuid(),
                    assignment.VenueTableId,
                    assignment.SeatNumber,
                    assignment.TicketId
                }, tx);
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
