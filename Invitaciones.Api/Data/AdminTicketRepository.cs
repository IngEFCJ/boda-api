using System.Data;
using Dapper;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminTicketRepository : IAdminTicketRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminTicketRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<TicketAdminRow>> GetByInvitationAsync(Guid invitationId, CancellationToken ct)
    {
        const string sql = """
            SELECT Id, InvitationId, Label, ConfirmedAt, UsedAt
            FROM dbo.Tickets
            WHERE InvitationId = @InvitationId
            ORDER BY Label
            """;

        using var conn = _db.Create();
        return await conn.QueryAsync<TicketAdminRow>(sql, new { InvitationId = invitationId });
    }

    public async Task<TicketAdminRow?> GetByIdAsync(Guid ticketId, CancellationToken ct)
    {
        const string sql = """
            SELECT Id, InvitationId, Label, ConfirmedAt, UsedAt
            FROM dbo.Tickets
            WHERE Id = @Id
            """;

        using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<TicketAdminRow>(sql, new { Id = ticketId });
    }

    public async Task<Guid> CreateAsync(Guid invitationId, string label, CancellationToken ct)
    {
        const string sql = """
            DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
            INSERT INTO dbo.Tickets (Id, InvitationId, Label, ConfirmedAt, UsedAt)
            VALUES (@NewId, @InvitationId, @Label, NULL, NULL);
            SELECT @NewId;
            """;

        using var conn = _db.Create();
        return await conn.QuerySingleAsync<Guid>(sql, new { InvitationId = invitationId, Label = label });
    }

    public async Task<bool> UpdateLabelAsync(Guid ticketId, string label, CancellationToken ct)
    {
        const string sql = """
            UPDATE dbo.Tickets
            SET Label = @Label
            WHERE Id = @Id
            """;

        using var conn = _db.Create();
        var affected = await conn.ExecuteAsync(sql, new { Id = ticketId, Label = label });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid ticketId, CancellationToken ct)
    {
        const string sql = """
            DELETE FROM dbo.Tickets
            WHERE Id = @Id
              AND ConfirmedAt IS NULL
              AND UsedAt IS NULL
            """;

        using var conn = _db.Create();
        var affected = await conn.ExecuteAsync(sql, new { Id = ticketId });
        return affected > 0;
    }

    public async Task<bool> ManualConfirmAsync(Guid ticketId, CancellationToken ct)
    {
        const string sql = """
            UPDATE dbo.Tickets
            SET ConfirmedAt = SYSUTCDATETIME()
            WHERE Id = @Id
              AND ConfirmedAt IS NULL
            """;

        using var conn = _db.Create();
        var affected = await conn.ExecuteAsync(sql, new { Id = ticketId });
        return affected > 0;
    }
}
