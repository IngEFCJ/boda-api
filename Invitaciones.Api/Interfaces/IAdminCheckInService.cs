using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminCheckInService
{
    Task<CheckInResultDto> ProcessQrScanAsync(QrScanRequest req, CancellationToken ct);
    Task<PagedResult<CheckInHistoryDto>> GetHistoryAsync(Guid eventId, int page, CancellationToken ct);
}
