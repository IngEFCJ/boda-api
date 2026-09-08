using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminVenueRepository
{
    Task<VenueLayoutRow?> GetByEventAsync(Guid eventId, CancellationToken ct);
    Task<Guid> CreateAsync(VenueLayoutRow row, CancellationToken ct);
    Task<bool> UpdateAsync(VenueLayoutRow row, CancellationToken ct);
    Task<IEnumerable<VenueTableRow>> GetTablesAsync(Guid venueLayoutId, CancellationToken ct);
    Task SaveTablesAsync(Guid venueLayoutId, IEnumerable<VenueTableRow> tables, CancellationToken ct);
}
