using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminVenueService
{
    Task<VenueLayoutDto?> GetVenueByEventAsync(Guid eventId, CancellationToken ct);
    Task<VenueLayoutDto> CreateVenueAsync(CreateVenueRequest req, CancellationToken ct);
    Task<VenueLayoutDto> UpdateVenueAsync(Guid venueId, UpdateVenueRequest req, CancellationToken ct);
    Task SaveTablesAsync(Guid venueId, SaveTablesRequest req, CancellationToken ct);
}
