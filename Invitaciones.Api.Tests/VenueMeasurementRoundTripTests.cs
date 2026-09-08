using FsCheck;
using FsCheck.Xunit;
using Invitaciones.Api.DTO;
using Invitaciones.Api.business_Logic;
using Xunit;

namespace Invitaciones.Api.Tests;

// Feature: venue-planner-refinements, Property 6: Round-trip de la medida fisica en la persistencia.
// Mapear VenueTableSaveDto -> VenueTableRow -> VenueTableDto conserva PrimaryMeters/SecondaryMeters
// (dentro de la precision DECIMAL(6,2)) y una mesa sin medida vuelve a leerse como null.
// El round-trip se ejerce a traves del AdminVenueService real (SaveTablesAsync mapea SaveDto -> Row,
// GetVenueByEventAsync mapea Row -> Dto) con un repositorio en memoria que captura las filas.
// Validates: Requirements 5.1, 5.2, 5.3, 6.1
public sealed class VenueMeasurementRoundTripTests
{
    private static readonly Guid EventId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid VenueId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static AdminVenueService BuildService()
    {
        var repo = new FakeAdminVenueRepository();
        repo.SeedLayout(new VenueLayoutRow
        {
            Id = VenueId,
            EventId = EventId,
            WidthMeters = 10m,
            HeightMeters = 8m,
            ShapeType = "rectangular",
            ShapeData = null
        });
        return new AdminVenueService(repo);
    }

    private static VenueTableDto RoundTrip(VenueTableSaveDto saveDto)
    {
        var service = BuildService();
        var ct = CancellationToken.None;

        // SaveDto -> Row (SaveTablesAsync mapping) captured by the fake repo,
        // then Row -> Dto (MapToDto) on read-back.
        service.SaveTablesAsync(VenueId, new SaveTablesRequest(new[] { saveDto }), ct)
            .GetAwaiter().GetResult();
        var layout = service.GetVenueByEventAsync(EventId, ct).GetAwaiter().GetResult();
        Assert.NotNull(layout);
        return Assert.Single(layout!.Tables);
    }

    // DECIMAL(6,2): up to 6 total digits, 2 decimals => magnitude <= 9999.99.
    // The service requires measurements > 0, constrained here to a realistic table range.
    private static Gen<decimal> PositiveMeasurementGen()
        => from cents in Gen.Choose(1, 100_000) // 0.01 .. 1000.00 m
           select cents / 100m;

    private static Gen<string> ShapeGen()
        => Gen.Elements("round", "rectangular", "square", "half-moon");

    private static Gen<int> CapacityGen()
        => Gen.Choose(1, 20);

    private static Gen<VenueTableSaveDto> TableWithMeasurementGen()
        => from shape in ShapeGen()
           from capacity in CapacityGen()
           from tableNumber in Gen.Choose(1, 500)
           from primary in PositiveMeasurementGen()
           from includeSecondary in Gen.Elements(true, false)
           from secondary in PositiveMeasurementGen()
           select new VenueTableSaveDto(
               Id: Guid.NewGuid(),
               TableNumber: tableNumber,
               Shape: shape,
               Capacity: capacity,
               PositionX: 1.0m,
               PositionY: 1.0m,
               Rotation: 0m,
               PrimaryMeters: primary,
               SecondaryMeters: includeSecondary ? secondary : null);

    private static Gen<VenueTableSaveDto> TableWithoutMeasurementGen()
        => from shape in ShapeGen()
           from capacity in CapacityGen()
           from tableNumber in Gen.Choose(1, 500)
           select new VenueTableSaveDto(
               Id: Guid.NewGuid(),
               TableNumber: tableNumber,
               Shape: shape,
               Capacity: capacity,
               PositionX: 1.0m,
               PositionY: 1.0m,
               Rotation: 0m,
               PrimaryMeters: null,
               SecondaryMeters: null);

    [Property(MaxTest = 200)]
    public Property Measurement_is_preserved_through_save_and_read_roundtrip()
        => Prop.ForAll(Arb.From(TableWithMeasurementGen()), saveDto =>
        {
            var dto = RoundTrip(saveDto);

            var primaryOk = dto.PrimaryMeters == saveDto.PrimaryMeters;
            var secondaryOk = dto.SecondaryMeters == saveDto.SecondaryMeters;

            return (primaryOk && secondaryOk)
                .Label($"expected primary={saveDto.PrimaryMeters}, secondary={saveDto.SecondaryMeters}; " +
                       $"got primary={dto.PrimaryMeters}, secondary={dto.SecondaryMeters}");
        });

    [Property(MaxTest = 200)]
    public Property Absent_measurement_roundtrips_as_null()
        => Prop.ForAll(Arb.From(TableWithoutMeasurementGen()), saveDto =>
        {
            var dto = RoundTrip(saveDto);

            return (dto.PrimaryMeters is null && dto.SecondaryMeters is null)
                .Label($"expected both null; got primary={dto.PrimaryMeters}, secondary={dto.SecondaryMeters}");
        });
}
