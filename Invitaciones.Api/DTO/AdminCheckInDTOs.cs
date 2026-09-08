namespace Invitaciones.Api.DTO;

// ===== Request/Response DTOs =====

public sealed record QrScanRequest(string QrPayload);

public sealed record CheckInResultDto(
    string Status,           // "success" | "already_checked_in" | "invalid" | "expired" | "not_found"
    string? GuestName,
    string? InvitationName,
    int? TableNumber,
    int? SeatNumber,
    DateTime? CheckedInAt,
    DateTime? OriginalCheckInAt
);

public sealed record CheckInHistoryDto(
    string GuestName,
    string InvitationName,
    int? TableNumber,
    int? SeatNumber,
    DateTime CheckedInAt
);

// ===== Row models for Dapper mapping =====

/// <summary>
/// Row model for check-in: joins Ticket + Invitation + SeatAssignment + VenueTable.
/// </summary>
public sealed record TicketCheckInRow
{
    public Guid Id { get; init; }
    public string Label { get; init; } = "";
    public DateTime? UsedAt { get; init; }
    public string DisplayName { get; init; } = "";
    public int? SeatNumber { get; init; }
    public int? TableNumber { get; init; }
}

/// <summary>
/// Row model for check-in history query results.
/// </summary>
public sealed record CheckInHistoryRow
{
    public string GuestName { get; init; } = "";
    public string InvitationName { get; init; } = "";
    public int? TableNumber { get; init; }
    public int? SeatNumber { get; init; }
    public DateTime CheckedInAt { get; init; }
}
