using Dapper;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminVenueRepository : IAdminVenueRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminVenueRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<VenueLayoutRow?> GetByEventAsync(Guid eventId, CancellationToken ct)
    {
        using var conn = _db.Create();

        // Defensive: if more than one layout exists for an event (legacy data),
        // return the most recently created one instead of throwing.
        const string sql = """
            SELECT TOP 1 Id, EventId, WidthMeters, HeightMeters, ShapeType, ShapeData, CreatedAt, UpdatedAt
            FROM dbo.VenueLayouts
            WHERE EventId = @EventId
            ORDER BY CreatedAt DESC
            """;

        return await conn.QueryFirstOrDefaultAsync<VenueLayoutRow>(
            new CommandDefinition(sql, new { EventId = eventId }, cancellationToken: ct));
    }

    public async Task<Guid> CreateAsync(VenueLayoutRow row, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            INSERT INTO dbo.VenueLayouts (Id, EventId, WidthMeters, HeightMeters, ShapeType, ShapeData)
            VALUES (@Id, @EventId, @WidthMeters, @HeightMeters, @ShapeType, @ShapeData)
            """;

        await conn.ExecuteAsync(new CommandDefinition(sql, row, cancellationToken: ct));
        return row.Id;
    }

    public async Task<bool> UpdateAsync(VenueLayoutRow row, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            UPDATE dbo.VenueLayouts
            SET WidthMeters = @WidthMeters,
                HeightMeters = @HeightMeters,
                ShapeType = @ShapeType,
                ShapeData = @ShapeData,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id
            """;

        var affected = await conn.ExecuteAsync(new CommandDefinition(sql, row, cancellationToken: ct));
        return affected > 0;
    }

    public async Task<IEnumerable<VenueTableRow>> GetTablesAsync(Guid venueLayoutId, CancellationToken ct)
    {
        using var conn = _db.Create();

        const string sql = """
            SELECT Id, VenueLayoutId, TableNumber, Shape, Capacity, PositionX, PositionY, Rotation, PrimaryMeters, SecondaryMeters, CreatedAt
            FROM dbo.VenueTables
            WHERE VenueLayoutId = @VenueLayoutId
            ORDER BY TableNumber
            """;

        return await conn.QueryAsync<VenueTableRow>(
            new CommandDefinition(sql, new { VenueLayoutId = venueLayoutId }, cancellationToken: ct));
    }

    public async Task SaveTablesAsync(Guid venueLayoutId, IEnumerable<VenueTableRow> tables, CancellationToken ct)
    {
        // Upsert strategy: update tables that still exist, insert new ones, and
        // delete only the tables that were removed. Seat assignments for removed
        // tables are cleared first to satisfy the foreign key; assignments for
        // surviving tables are preserved.
        var incoming = tables.ToList();
        var incomingIds = incoming.Select(t => t.Id).ToHashSet();

        using var conn = _db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        // Current table IDs for this layout
        const string existingSql = """
            SELECT Id FROM dbo.VenueTables WHERE VenueLayoutId = @VenueLayoutId
            """;
        var existingIds = (await conn.QueryAsync<Guid>(
            new CommandDefinition(existingSql, new { VenueLayoutId = venueLayoutId }, transaction: tx, cancellationToken: ct)))
            .ToHashSet();

        // Tables to remove = existing but not in incoming
        var toRemove = existingIds.Where(id => !incomingIds.Contains(id)).ToList();
        if (toRemove.Count > 0)
        {
            // Clear seat assignments for removed tables first (FK safety)
            const string delAssignSql = """
                DELETE FROM dbo.SeatAssignments WHERE VenueTableId IN @Ids
                """;
            await conn.ExecuteAsync(
                new CommandDefinition(delAssignSql, new { Ids = toRemove }, transaction: tx, cancellationToken: ct));

            const string delTablesSql = """
                DELETE FROM dbo.VenueTables WHERE Id IN @Ids
                """;
            await conn.ExecuteAsync(
                new CommandDefinition(delTablesSql, new { Ids = toRemove }, transaction: tx, cancellationToken: ct));
        }

        const string updateSql = """
            UPDATE dbo.VenueTables
            SET TableNumber = @TableNumber,
                Shape = @Shape,
                Capacity = @Capacity,
                PositionX = @PositionX,
                PositionY = @PositionY,
                Rotation = @Rotation,
                PrimaryMeters = @PrimaryMeters,
                SecondaryMeters = @SecondaryMeters
            WHERE Id = @Id
            """;

        const string insertSql = """
            INSERT INTO dbo.VenueTables (Id, VenueLayoutId, TableNumber, Shape, Capacity, PositionX, PositionY, Rotation, PrimaryMeters, SecondaryMeters)
            VALUES (@Id, @VenueLayoutId, @TableNumber, @Shape, @Capacity, @PositionX, @PositionY, @Rotation, @PrimaryMeters, @SecondaryMeters)
            """;

        foreach (var table in incoming)
        {
            if (existingIds.Contains(table.Id))
            {
                await conn.ExecuteAsync(
                    new CommandDefinition(updateSql, table, transaction: tx, cancellationToken: ct));
            }
            else
            {
                await conn.ExecuteAsync(
                    new CommandDefinition(insertSql, table, transaction: tx, cancellationToken: ct));
            }
        }

        tx.Commit();
    }
}
