using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminEventService : IAdminEventService
{
    private readonly IAdminEventRepository _repo;

    public AdminEventService(IAdminEventRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<EventAdminDto>> GetAllEventsAsync(CancellationToken ct)
    {
        var rows = await _repo.GetAllAsync(ct);
        return rows.Select(MapToDto);
    }

    public async Task<EventAdminDto> GetEventByIdAsync(Guid id, CancellationToken ct)
    {
        var row = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Evento con ID '{id}' no encontrado");

        return MapToDto(row);
    }

    public async Task<EventAdminDto> CreateEventAsync(CreateEventRequest req, CancellationToken ct)
    {
        ValidateEventData(req.Title, req.EventDate, req.LocationName, req.Address);

        var row = new EventRow
        {
            Id = Guid.NewGuid(),
            Title = req.Title.Trim(),
            EventDate = req.EventDate,
            LocationName = req.LocationName.Trim(),
            Address = req.Address.Trim()
        };

        await _repo.CreateAsync(row, ct);
        return MapToDto(row);
    }

    public async Task<EventAdminDto> UpdateEventAsync(Guid id, UpdateEventRequest req, CancellationToken ct)
    {
        ValidateEventData(req.Title, req.EventDate, req.LocationName, req.Address);

        var existing = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Evento con ID '{id}' no encontrado");

        var updated = new EventRow
        {
            Id = id,
            Title = req.Title.Trim(),
            EventDate = req.EventDate,
            LocationName = req.LocationName.Trim(),
            Address = req.Address.Trim()
        };

        var success = await _repo.UpdateAsync(updated, ct);
        if (!success)
            throw new KeyNotFoundException($"Evento con ID '{id}' no encontrado");

        return MapToDto(updated);
    }

    private static void ValidateEventData(string title, DateTime eventDate, string locationName, string address)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título es obligatorio");

        if (title.Trim().Length > 150)
            throw new ArgumentException("El título no puede exceder 150 caracteres");

        if (string.IsNullOrWhiteSpace(locationName))
            throw new ArgumentException("El nombre del lugar es obligatorio");

        if (locationName.Trim().Length > 150)
            throw new ArgumentException("El nombre del lugar no puede exceder 150 caracteres");

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("La dirección es obligatoria");

        if (address.Trim().Length > 250)
            throw new ArgumentException("La dirección no puede exceder 250 caracteres");

        if (eventDate <= DateTime.UtcNow)
            throw new ArgumentException("La fecha del evento debe ser en el futuro");
    }

    private static EventAdminDto MapToDto(EventRow row) =>
        new(row.Id, row.Title, row.EventDate, row.LocationName, row.Address);
}
