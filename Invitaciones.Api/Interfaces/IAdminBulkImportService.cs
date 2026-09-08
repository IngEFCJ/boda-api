using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminBulkImportService
{
    Task<BulkImportResultDto> ImportAsync(Guid eventId, BulkImportRequest req, CancellationToken ct);
}
