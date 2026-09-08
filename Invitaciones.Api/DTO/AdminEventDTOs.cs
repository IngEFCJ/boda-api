namespace Invitaciones.Api.DTO;

public sealed record EventAdminDto(
    Guid Id,
    string Title,
    DateTime EventDate,
    string LocationName,
    string Address
);

public sealed record CreateEventRequest(
    string Title,
    DateTime EventDate,
    string LocationName,
    string Address
);

public sealed record UpdateEventRequest(
    string Title,
    DateTime EventDate,
    string LocationName,
    string Address
);

public sealed record EventRow
{
    public Guid Id { get; init; }
    public string Title { get; init; } = "";
    public DateTime EventDate { get; init; }
    public string LocationName { get; init; } = "";
    public string Address { get; init; } = "";
}
