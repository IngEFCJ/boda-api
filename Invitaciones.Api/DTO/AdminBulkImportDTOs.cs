namespace Invitaciones.Api.DTO;

public sealed record BulkImportRequest(
    Guid EventId,
    IReadOnlyList<BulkImportRowDto> Rows
);

public sealed record BulkImportRowDto(
    string DisplayName,
    IReadOnlyList<string> TicketLabels
);

public sealed record BulkImportResultDto(
    int InvitationsCreated,
    int TicketsCreated,
    int RowsSkipped,
    IReadOnlyList<string> Errors
);
