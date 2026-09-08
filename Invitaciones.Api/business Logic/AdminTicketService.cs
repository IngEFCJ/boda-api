using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminTicketService : IAdminTicketService
{
    private readonly IAdminTicketRepository _repo;

    public AdminTicketService(IAdminTicketRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<TicketAdminDto>> GetTicketsAsync(Guid invitationId, CancellationToken ct)
    {
        var rows = await _repo.GetByInvitationAsync(invitationId, ct);
        return rows.Select(MapToDto);
    }

    public async Task<TicketAdminDto> CreateTicketAsync(Guid invitationId, CreateTicketRequest req, CancellationToken ct)
    {
        var label = ValidateLabel(req.Label);

        var newId = await _repo.CreateAsync(invitationId, label, ct);

        return new TicketAdminDto(newId, invitationId, label, null, null);
    }

    public async Task<TicketAdminDto> UpdateTicketAsync(Guid ticketId, UpdateTicketRequest req, CancellationToken ct)
    {
        var label = ValidateLabel(req.Label);

        var updated = await _repo.UpdateLabelAsync(ticketId, label, ct);
        if (!updated)
            throw new KeyNotFoundException($"Ticket con Id '{ticketId}' no encontrado");

        var row = await _repo.GetByIdAsync(ticketId, ct);
        return MapToDto(row!);
    }

    public async Task DeleteTicketAsync(Guid ticketId, CancellationToken ct)
    {
        var deleted = await _repo.DeleteAsync(ticketId, ct);
        if (!deleted)
            throw new InvalidOperationException("No se puede eliminar un boleto confirmado o utilizado");
    }

    public async Task<TicketAdminDto> ConfirmTicketAsync(Guid ticketId, CancellationToken ct)
    {
        var confirmed = await _repo.ManualConfirmAsync(ticketId, ct);
        if (!confirmed)
            throw new KeyNotFoundException($"Ticket con Id '{ticketId}' no encontrado o ya fue confirmado");

        var row = await _repo.GetByIdAsync(ticketId, ct);
        return MapToDto(row!);
    }

    private static string ValidateLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("La etiqueta del boleto es requerida");

        var trimmed = label.Trim();

        if (trimmed.Length > 100)
            throw new ArgumentException("La etiqueta del boleto no puede exceder 100 caracteres");

        return trimmed;
    }

    private static TicketAdminDto MapToDto(TicketAdminRow row) =>
        new(row.Id, row.InvitationId, row.Label, row.ConfirmedAt, row.UsedAt);
}
