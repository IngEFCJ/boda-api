using Invitaciones.Api.DTO;
using Invitaciones.Api.business_Logic;
using Xunit;

namespace Invitaciones.Api.Tests;

/// <summary>
/// Example-based tests for the physical measurement mapping in
/// <see cref="AdminVenueService"/>: a <see cref="VenueTableSaveDto"/> carrying
/// PrimaryMeters/SecondaryMeters must map to a <see cref="VenueTableRow"/> and
/// back to a <see cref="VenueTableDto"/> preserving both columns; a save DTO
/// without measurement must map to null on both sides.
///
/// Validates: Requirements 5.1, 5.2, 5.3, 6.1
/// </summary>
public sealed class AdminVenueMeasurementMappingTests
{
    private static readonly Guid EventId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VenueId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static (AdminVenueService service, FakeAdminVenueRepository repo) CreateSut()
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
        return (new AdminVenueService(repo), repo);
    }

    [Fact]
    public async Task SaveTables_WithRectangularMeasurement_MapsSaveDtoToRowPreservingBothColumns()
    {
        var (service, repo) = CreateSut();

        var save = new SaveTablesRequest(new[]
        {
            new VenueTableSaveDto(
                Id: null,
                TableNumber: 1,
                Shape: "rectangular",
                Capacity: 6,
                PositionX: 1m,
                PositionY: 2m,
                Rotation: 0m,
                PrimaryMeters: 2.00m,
                SecondaryMeters: 1.00m)
        });

        await service.SaveTablesAsync(VenueId, save, CancellationToken.None);

        var row = Assert.Single(repo.LastSavedRows);
        Assert.Equal(2.00m, row.PrimaryMeters);
        Assert.Equal(1.00m, row.SecondaryMeters);
    }

    [Fact]
    public async Task SaveTables_WithSingleMeasurement_MapsPrimaryAndLeavesSecondaryNull()
    {
        var (service, repo) = CreateSut();

        var save = new SaveTablesRequest(new[]
        {
            new VenueTableSaveDto(
                Id: null,
                TableNumber: 1,
                Shape: "round",
                Capacity: 8,
                PositionX: 0m,
                PositionY: 0m,
                Rotation: 0m,
                PrimaryMeters: 1.50m,
                SecondaryMeters: null)
        });

        await service.SaveTablesAsync(VenueId, save, CancellationToken.None);

        var row = Assert.Single(repo.LastSavedRows);
        Assert.Equal(1.50m, row.PrimaryMeters);
        Assert.Null(row.SecondaryMeters);
    }

    [Fact]
    public async Task SaveTables_WithoutMeasurement_MapsBothColumnsToNull()
    {
        var (service, repo) = CreateSut();

        var save = new SaveTablesRequest(new[]
        {
            new VenueTableSaveDto(
                Id: null,
                TableNumber: 1,
                Shape: "square",
                Capacity: 4,
                PositionX: 0m,
                PositionY: 0m,
                Rotation: 0m,
                PrimaryMeters: null,
                SecondaryMeters: null)
        });

        await service.SaveTablesAsync(VenueId, save, CancellationToken.None);

        var row = Assert.Single(repo.LastSavedRows);
        Assert.Null(row.PrimaryMeters);
        Assert.Null(row.SecondaryMeters);
    }

    [Fact]
    public async Task GetVenueByEvent_MapsRowMeasurementToDtoPreservingBothColumns()
    {
        var (service, _) = CreateSut();

        // Persist a rectangular table with both measurements, then read it back
        // through the service's MapToDto path.
        var save = new SaveTablesRequest(new[]
        {
            new VenueTableSaveDto(
                Id: null,
                TableNumber: 1,
                Shape: "rectangular",
                Capacity: 6,
                PositionX: 3m,
                PositionY: 4m,
                Rotation: 90m,
                PrimaryMeters: 2.25m,
                SecondaryMeters: 1.10m)
        });
        await service.SaveTablesAsync(VenueId, save, CancellationToken.None);

        var layout = await service.GetVenueByEventAsync(EventId, CancellationToken.None);

        Assert.NotNull(layout);
        var dto = Assert.Single(layout!.Tables);
        Assert.Equal(2.25m, dto.PrimaryMeters);
        Assert.Equal(1.10m, dto.SecondaryMeters);
    }

    [Fact]
    public async Task GetVenueByEvent_LegacyTableWithoutMeasurement_MapsToNullDto()
    {
        var (service, _) = CreateSut();

        var save = new SaveTablesRequest(new[]
        {
            new VenueTableSaveDto(
                Id: null,
                TableNumber: 1,
                Shape: "round",
                Capacity: 8,
                PositionX: 0m,
                PositionY: 0m,
                Rotation: 0m,
                PrimaryMeters: null,
                SecondaryMeters: null)
        });
        await service.SaveTablesAsync(VenueId, save, CancellationToken.None);

        var layout = await service.GetVenueByEventAsync(EventId, CancellationToken.None);

        Assert.NotNull(layout);
        var dto = Assert.Single(layout!.Tables);
        Assert.Null(dto.PrimaryMeters);
        Assert.Null(dto.SecondaryMeters);
    }

    [Fact]
    public async Task SaveTables_RoundTrip_PreservesMeasurementThroughRowToDto()
    {
        var (service, _) = CreateSut();

        var save = new SaveTablesRequest(new[]
        {
            new VenueTableSaveDto(
                Id: null,
                TableNumber: 1,
                Shape: "half-moon",
                Capacity: 6,
                PositionX: 5m,
                PositionY: 6m,
                Rotation: 45m,
                PrimaryMeters: 1.75m,
                SecondaryMeters: null)
        });

        await service.SaveTablesAsync(VenueId, save, CancellationToken.None);
        var layout = await service.GetVenueByEventAsync(EventId, CancellationToken.None);

        var dto = Assert.Single(layout!.Tables);
        var original = save.Tables[0];
        Assert.Equal(original.PrimaryMeters, dto.PrimaryMeters);
        Assert.Equal(original.SecondaryMeters, dto.SecondaryMeters);
    }
}
