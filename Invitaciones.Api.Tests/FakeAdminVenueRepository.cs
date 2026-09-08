using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Tests;

/// <summary>
/// In-memory fake of <see cref="IAdminVenueRepository"/> that captures the
/// <see cref="VenueTableRow"/> instances handed to <c>SaveTablesAsync</c> and
/// returns them from <c>GetTablesAsync</c>. This lets the mapping tests exercise
/// the full <c>VenueTableSaveDto -> VenueTableRow -> VenueTableDto</c> path
/// through the real <c>AdminVenueService</c> without a database.
/// </summary>
public sealed class FakeAdminVenueRepository : IAdminVenueRepository
{
    private readonly Dictionary<Guid, VenueLayoutRow> _layouts = new();
    private readonly Dictionary<Guid, List<VenueTableRow>> _tables = new();

    /// <summary>The last set of rows persisted via <see cref="SaveTablesAsync"/>.</summary>
    public List<VenueTableRow> LastSavedRows { get; private set; } = new();

    public void SeedLayout(VenueLayoutRow layout)
    {
        _layouts[layout.EventId] = layout;
        if (!_tables.ContainsKey(layout.Id))
            _tables[layout.Id] = new List<VenueTableRow>();
    }

    public Task<VenueLayoutRow?> GetByEventAsync(Guid eventId, CancellationToken ct)
        => Task.FromResult(_layouts.TryGetValue(eventId, out var row) ? row : null);

    public Task<Guid> CreateAsync(VenueLayoutRow row, CancellationToken ct)
    {
        _layouts[row.EventId] = row;
        _tables[row.Id] = new List<VenueTableRow>();
        return Task.FromResult(row.Id);
    }

    public Task<bool> UpdateAsync(VenueLayoutRow row, CancellationToken ct)
    {
        _layouts[row.EventId] = row;
        return Task.FromResult(true);
    }

    public Task<IEnumerable<VenueTableRow>> GetTablesAsync(Guid venueLayoutId, CancellationToken ct)
        => Task.FromResult<IEnumerable<VenueTableRow>>(
            _tables.TryGetValue(venueLayoutId, out var list) ? list : new List<VenueTableRow>());

    public Task SaveTablesAsync(Guid venueLayoutId, IEnumerable<VenueTableRow> tables, CancellationToken ct)
    {
        LastSavedRows = tables.ToList();
        _tables[venueLayoutId] = LastSavedRows;
        return Task.CompletedTask;
    }
}
