namespace Invitaciones.Api.DTO;

// ===== DTOs =====

public sealed record VenueLayoutDto(
    Guid Id,
    Guid EventId,
    decimal WidthMeters,
    decimal HeightMeters,
    string ShapeType,
    string? ShapeData,
    IReadOnlyList<VenueTableDto> Tables
);

public sealed record VenueTableDto(
    Guid Id,
    int TableNumber,
    string Shape,
    int Capacity,
    decimal PositionX,
    decimal PositionY,
    decimal Rotation,
    decimal? PrimaryMeters = null,
    decimal? SecondaryMeters = null
);

public sealed record CreateVenueRequest(
    Guid EventId,
    decimal WidthMeters,
    decimal HeightMeters,
    string ShapeType,
    string? ShapeData
);

public sealed record UpdateVenueRequest(
    decimal WidthMeters,
    decimal HeightMeters,
    string ShapeType,
    string? ShapeData
);

public sealed record SaveTablesRequest(IReadOnlyList<VenueTableSaveDto> Tables);

public sealed record VenueTableSaveDto(
    Guid? Id,
    int TableNumber,
    string Shape,
    int Capacity,
    decimal PositionX,
    decimal PositionY,
    decimal Rotation,
    decimal? PrimaryMeters = null,
    decimal? SecondaryMeters = null
);

// ===== Row Types (DB mapping) =====

public sealed record VenueLayoutRow
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public decimal WidthMeters { get; init; }
    public decimal HeightMeters { get; init; }
    public string ShapeType { get; init; } = "rectangular";
    public string? ShapeData { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record VenueTableRow
{
    public Guid Id { get; init; }
    public Guid VenueLayoutId { get; init; }
    public int TableNumber { get; init; }
    public string Shape { get; init; } = "round";
    public int Capacity { get; init; }
    public decimal PositionX { get; init; }
    public decimal PositionY { get; init; }
    public decimal Rotation { get; init; }
    public decimal? PrimaryMeters { get; init; }
    public decimal? SecondaryMeters { get; init; }
    public DateTime CreatedAt { get; init; }
}
