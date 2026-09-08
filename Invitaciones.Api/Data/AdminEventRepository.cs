using Dapper;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminEventRepository : IAdminEventRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminEventRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<EventRow>> GetAllAsync(CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            SELECT Id, Title, EventDate, LocationName, Address
            FROM dbo.Events
            ORDER BY EventDate DESC
            """;

        return await conn.QueryAsync<EventRow>(new CommandDefinition(sql, cancellationToken: ct));
    }

    public async Task<EventRow?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            SELECT Id, Title, EventDate, LocationName, Address
            FROM dbo.Events
            WHERE Id = @Id
            """;

        return await conn.QuerySingleOrDefaultAsync<EventRow>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<Guid> CreateAsync(EventRow row, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            INSERT INTO dbo.Events (Id, Title, EventDate, LocationName, Address)
            VALUES (@Id, @Title, @EventDate, @LocationName, @Address)
            """;

        await conn.ExecuteAsync(new CommandDefinition(sql, row, cancellationToken: ct));
        return row.Id;
    }

    public async Task<bool> UpdateAsync(EventRow row, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            UPDATE dbo.Events
            SET Title = @Title,
                EventDate = @EventDate,
                LocationName = @LocationName,
                Address = @Address
            WHERE Id = @Id
            """;

        var affected = await conn.ExecuteAsync(new CommandDefinition(sql, row, cancellationToken: ct));
        return affected > 0;
    }
}
