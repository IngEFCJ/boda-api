using System.Security.Cryptography;
using System.Text;
using Dapper;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminInvitationRepository : IAdminInvitationRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminInvitationRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<PagedResult<InvitationAdminDto>> GetPagedAsync(
        Guid eventId, int page, int pageSize, string? search, CancellationToken ct)
    {
        using var conn = _db.Create();

        var whereClause = "WHERE i.EventId = @EventId AND i.Status = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@EventId", eventId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause += " AND i.DisplayName LIKE @Search";
            parameters.Add("@Search", $"%{search.Trim()}%");
        }

        // Count query
        var countSql = $@"
            SELECT COUNT(*)
            FROM dbo.Invitations i
            {whereClause}";

        var totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

        // Paged data query with LEFT JOIN to count tickets and confirmed tickets
        var offset = (page - 1) * pageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var dataSql = $@"
            SELECT
                i.Id,
                i.DisplayName,
                COUNT(t.Id) AS TicketCount,
                COUNT(CASE WHEN t.ConfirmedAt IS NOT NULL THEN 1 END) AS ConfirmedCount,
                i.Status,
                (SELECT TOP 1 it.TokenHash FROM dbo.InvitationTokens it WHERE it.InvitationId = i.Id ORDER BY it.CreatedAt DESC) AS Token
            FROM dbo.Invitations i
            LEFT JOIN dbo.Tickets t ON t.InvitationId = i.Id
            {whereClause}
            GROUP BY i.Id, i.DisplayName, i.Status
            ORDER BY i.DisplayName
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var rows = await conn.QueryAsync<InvitationAdminRow>(dataSql, parameters);

        var items = rows.Select(r => new InvitationAdminDto(
            r.Id,
            r.DisplayName,
            r.TicketCount,
            r.ConfirmedCount,
            r.Status,
            r.Token
        )).ToList();

        return new PagedResult<InvitationAdminDto>(items, totalCount, page, pageSize);
    }

    public async Task<Guid> CreateAsync(Guid eventId, string displayName, CancellationToken ct)
    {
        using var conn = _db.Create();

        var id = Guid.NewGuid();

        const string sql = @"
            INSERT INTO dbo.Invitations (Id, EventId, DisplayName, Status)
            VALUES (@Id, @EventId, @DisplayName, 1)";

        await conn.ExecuteAsync(sql, new { Id = id, EventId = eventId, DisplayName = displayName });

        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, string displayName, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = @"
            UPDATE dbo.Invitations
            SET DisplayName = @DisplayName
            WHERE Id = @Id";

        var affected = await conn.ExecuteAsync(sql, new { Id = id, DisplayName = displayName });
        return affected > 0;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = @"
            UPDATE dbo.Invitations
            SET Status = 0
            WHERE Id = @Id";

        var affected = await conn.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<string> GenerateTokenAsync(Guid invitationId, CancellationToken ct)
    {
        // Generate 32 random bytes
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var plainToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        // Compute SHA-256 hash of the plain token
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        var tokenHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        // Store the hash in InvitationTokens
        using var conn = _db.Create();

        const string sql = @"
            INSERT INTO dbo.InvitationTokens (Id, InvitationId, TokenHash, CreatedAt)
            VALUES (NEWID(), @InvitationId, @TokenHash, SYSUTCDATETIME())";

        await conn.ExecuteAsync(sql, new { InvitationId = invitationId, TokenHash = tokenHash });

        // Return the plain-text hex token for sharing
        return plainToken;
    }

    private sealed record InvitationAdminRow
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; } = "";
        public int TicketCount { get; init; }
        public int ConfirmedCount { get; init; }
        public byte Status { get; init; }
        public string? Token { get; init; }
    }
}
