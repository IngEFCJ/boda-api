using Dapper;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminCheckInRepository : IAdminCheckInRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminCheckInRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<TicketCheckInRow?> GetTicketForCheckInAsync(Guid ticketId, CancellationToken ct)
    {
        const string sql = """
            SELECT 
                t.Id,
                t.Label,
                t.UsedAt,
                i.DisplayName,
                sa.SeatNumber,
                vt.TableNumber
            FROM dbo.Tickets t
            INNER JOIN dbo.Invitations i ON i.Id = t.InvitationId
            LEFT JOIN dbo.SeatAssignments sa ON sa.TicketId = t.Id
            LEFT JOIN dbo.VenueTables vt ON vt.Id = sa.VenueTableId
            WHERE t.Id = @TicketId
            """;

        using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<TicketCheckInRow>(sql, new { TicketId = ticketId });
    }

    public async Task<bool> MarkUsedAsync(Guid ticketId, CancellationToken ct)
    {
        const string sql = """
            UPDATE dbo.Tickets
            SET UsedAt = SYSUTCDATETIME()
            WHERE Id = @TicketId
              AND UsedAt IS NULL
            """;

        using var conn = _db.Create();
        var affected = await conn.ExecuteAsync(sql, new { TicketId = ticketId });
        return affected > 0;
    }

    public async Task<PagedResult<CheckInHistoryRow>> GetCheckInHistoryAsync(
        Guid eventId, int page, int pageSize, CancellationToken ct)
    {
        const string countSql = """
            SELECT COUNT(*)
            FROM dbo.Tickets t
            INNER JOIN dbo.Invitations i ON i.Id = t.InvitationId
            WHERE i.EventId = @EventId
              AND t.UsedAt IS NOT NULL
            """;

        const string dataSql = """
            SELECT 
                t.Label AS GuestName,
                i.DisplayName AS InvitationName,
                vt.TableNumber,
                sa.SeatNumber,
                t.UsedAt AS CheckedInAt
            FROM dbo.Tickets t
            INNER JOIN dbo.Invitations i ON i.Id = t.InvitationId
            LEFT JOIN dbo.SeatAssignments sa ON sa.TicketId = t.Id
            LEFT JOIN dbo.VenueTables vt ON vt.Id = sa.VenueTableId
            WHERE i.EventId = @EventId
              AND t.UsedAt IS NOT NULL
            ORDER BY t.UsedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        using var conn = _db.Create();

        var totalCount = await conn.ExecuteScalarAsync<int>(countSql, new { EventId = eventId });

        var offset = (page - 1) * pageSize;
        var rows = await conn.QueryAsync<CheckInHistoryRow>(dataSql, new
        {
            EventId = eventId,
            Offset = offset,
            PageSize = pageSize
        });

        return new PagedResult<CheckInHistoryRow>(
            rows.ToList(),
            totalCount,
            page,
            pageSize
        );
    }
}
