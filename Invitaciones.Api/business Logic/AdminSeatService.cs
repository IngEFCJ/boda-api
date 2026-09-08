using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminSeatService : IAdminSeatService
{
    private readonly IAdminSeatRepository _seatRepo;
    private readonly IAdminVenueRepository _venueRepo;

    public AdminSeatService(IAdminSeatRepository seatRepo, IAdminVenueRepository venueRepo)
    {
        _seatRepo = seatRepo;
        _venueRepo = venueRepo;
    }

    public async Task<SeatAssignmentDataDto> GetAssignmentDataAsync(Guid eventId, CancellationToken ct)
    {
        // Get venue layout for the event to retrieve assignments
        var venue = await _venueRepo.GetByEventAsync(eventId, ct);

        IReadOnlyList<SeatAssignmentDto> assignments;

        if (venue is null)
        {
            assignments = Array.Empty<SeatAssignmentDto>();
        }
        else
        {
            var rows = await _seatRepo.GetByVenueAsync(venue.Id, ct);
            assignments = rows.Select(r => new SeatAssignmentDto(
                r.VenueTableId,
                r.SeatNumber,
                r.TicketId,
                r.GuestName,
                r.InvitationName
            )).ToList();
        }

        var unassigned = await _seatRepo.GetUnassignedConfirmedAsync(eventId, ct);
        var unassignedDtos = unassigned.Select(t => new TicketAdminDto(
            t.Id, t.InvitationId, t.Label, t.ConfirmedAt, t.UsedAt
        )).ToList();

        return new SeatAssignmentDataDto(assignments, unassignedDtos);
    }

    public async Task SaveAssignmentsAsync(Guid venueId, SaveAssignmentsRequest req, CancellationToken ct)
    {
        if (req.Assignments is null || req.Assignments.Count == 0)
        {
            // Save empty assignments (clear all)
            await _seatRepo.SaveAssignmentsAsync(venueId, Enumerable.Empty<SeatAssignmentRow>(), ct);
            return;
        }

        // Validate: no duplicate (VenueTableId, SeatNumber) pairs
        var seatPairs = req.Assignments
            .Select(a => (a.VenueTableId, a.SeatNumber))
            .ToList();

        if (seatPairs.Distinct().Count() != seatPairs.Count)
            throw new ArgumentException("No se permiten asientos duplicados en la misma mesa");

        // Validate: no duplicate TicketId (a ticket cannot be assigned to more than one seat)
        var ticketIds = req.Assignments.Select(a => a.TicketId).ToList();

        if (ticketIds.Distinct().Count() != ticketIds.Count)
            throw new ArgumentException("Un boleto no puede ser asignado a más de un asiento");

        // Validate: seat number does not exceed table capacity
        var tables = await _venueRepo.GetTablesAsync(venueId, ct);
        var tableDict = tables.ToDictionary(t => t.Id, t => t.Capacity);

        foreach (var assignment in req.Assignments)
        {
            if (!tableDict.TryGetValue(assignment.VenueTableId, out var capacity))
                throw new ArgumentException($"La mesa con Id '{assignment.VenueTableId}' no pertenece a este venue");

            if (assignment.SeatNumber < 1 || assignment.SeatNumber > capacity)
                throw new ArgumentException(
                    $"El número de asiento {assignment.SeatNumber} excede la capacidad de la mesa ({capacity})");
        }

        // Map to row models and persist
        var rows = req.Assignments.Select(a => new SeatAssignmentRow
        {
            VenueTableId = a.VenueTableId,
            SeatNumber = a.SeatNumber,
            TicketId = a.TicketId
        });

        await _seatRepo.SaveAssignmentsAsync(venueId, rows, ct);
    }
}
