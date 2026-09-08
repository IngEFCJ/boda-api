using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminEventRepository
{
    Task<IEnumerable<EventRow>> GetAllAsync(CancellationToken ct);
    Task<EventRow?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Guid> CreateAsync(EventRow row, CancellationToken ct);
    Task<bool> UpdateAsync(EventRow row, CancellationToken ct);
}
