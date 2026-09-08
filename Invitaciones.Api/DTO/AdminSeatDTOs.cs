namespace Invitaciones.Api.DTO;

// ===== DTOs =====

public sealed record SeatAssignmentDataDto(
    IReadOnlyList<SeatAssignmentDto> Assignments,
    IReadOnlyList<TicketAdminDto> Unassigned
);

public sealed record SeatAssignmentDto(
    Guid VenueTableId,
    int SeatNumber,
    Guid TicketId,
    string GuestName,
    string InvitationName
);

public sealed record SaveAssignmentsRequest(IReadOnlyList<SeatAssignmentSaveDto> Assignments);

public sealed record SeatAssignmentSaveDto(Guid VenueTableId, int SeatNumber, Guid TicketId);

// ===== Row Type (DB mapping) =====

public sealed record SeatAssignmentRow
{
    public Guid Id { get; init; }
    public Guid VenueTableId { get; init; }
    public int SeatNumber { get; init; }
    public Guid TicketId { get; init; }
    public string GuestName { get; init; } = "";
    public string InvitationName { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}
