using System.Text.Json;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminVenueService : IAdminVenueService
{
    private readonly IAdminVenueRepository _repo;

    private static readonly HashSet<string> ValidShapeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "rectangular",
        "polygonal"
    };

    private static readonly HashSet<string> ValidTableShapes = new(StringComparer.OrdinalIgnoreCase)
    {
        "round",
        "rectangular",
        "square",
        "half-moon"
    };

    public AdminVenueService(IAdminVenueRepository repo)
    {
        _repo = repo;
    }

    public async Task<VenueLayoutDto?> GetVenueByEventAsync(Guid eventId, CancellationToken ct)
    {
        var layout = await _repo.GetByEventAsync(eventId, ct);
        if (layout is null)
            return null;

        var tables = await _repo.GetTablesAsync(layout.Id, ct);
        return MapToDto(layout, tables);
    }

    public async Task<VenueLayoutDto> CreateVenueAsync(CreateVenueRequest req, CancellationToken ct)
    {
        ValidateVenueData(req.WidthMeters, req.HeightMeters, req.ShapeType, req.ShapeData);

        // Idempotent: if a layout already exists for this event, update it instead
        // of creating a duplicate.
        var existing = await _repo.GetByEventAsync(req.EventId, ct);
        if (existing is not null)
        {
            var updateRow = new VenueLayoutRow
            {
                Id = existing.Id,
                EventId = req.EventId,
                WidthMeters = req.WidthMeters,
                HeightMeters = req.HeightMeters,
                ShapeType = req.ShapeType.ToLowerInvariant(),
                ShapeData = req.ShapeData
            };
            await _repo.UpdateAsync(updateRow, ct);
            var existingTables = await _repo.GetTablesAsync(existing.Id, ct);
            return MapToDto(updateRow, existingTables);
        }

        var row = new VenueLayoutRow
        {
            Id = Guid.NewGuid(),
            EventId = req.EventId,
            WidthMeters = req.WidthMeters,
            HeightMeters = req.HeightMeters,
            ShapeType = req.ShapeType.ToLowerInvariant(),
            ShapeData = req.ShapeData
        };

        await _repo.CreateAsync(row, ct);
        return MapToDto(row, Enumerable.Empty<VenueTableRow>());
    }

    public async Task<VenueLayoutDto> UpdateVenueAsync(Guid venueId, UpdateVenueRequest req, CancellationToken ct)
    {
        ValidateVenueData(req.WidthMeters, req.HeightMeters, req.ShapeType, req.ShapeData);

        var row = new VenueLayoutRow
        {
            Id = venueId,
            WidthMeters = req.WidthMeters,
            HeightMeters = req.HeightMeters,
            ShapeType = req.ShapeType.ToLowerInvariant(),
            ShapeData = req.ShapeData
        };

        var success = await _repo.UpdateAsync(row, ct);
        if (!success)
            throw new KeyNotFoundException($"Venue con ID '{venueId}' no encontrado");

        var tables = await _repo.GetTablesAsync(venueId, ct);
        return MapToDto(row, tables);
    }

    public async Task SaveTablesAsync(Guid venueId, SaveTablesRequest req, CancellationToken ct)
    {
        if (req.Tables is null || req.Tables.Count == 0)
        {
            // Save empty tables (clear all)
            await _repo.SaveTablesAsync(venueId, Enumerable.Empty<VenueTableRow>(), ct);
            return;
        }

        ValidateTables(req.Tables);

        var rows = req.Tables.Select(t => new VenueTableRow
        {
            Id = t.Id ?? Guid.NewGuid(),
            VenueLayoutId = venueId,
            TableNumber = t.TableNumber,
            Shape = t.Shape.ToLowerInvariant(),
            Capacity = t.Capacity,
            PositionX = t.PositionX,
            PositionY = t.PositionY,
            Rotation = t.Rotation,
            PrimaryMeters = t.PrimaryMeters,
            SecondaryMeters = t.SecondaryMeters
        });

        await _repo.SaveTablesAsync(venueId, rows, ct);
    }

    // ===== Validation =====

    private static void ValidateVenueData(decimal width, decimal height, string shapeType, string? shapeData)
    {
        ValidateDimension(width, "ancho");
        ValidateDimension(height, "alto");

        if (string.IsNullOrWhiteSpace(shapeType))
            throw new ArgumentException("El tipo de forma es obligatorio");

        if (!ValidShapeTypes.Contains(shapeType))
            throw new ArgumentException("El tipo de forma debe ser 'rectangular' o 'polygonal'");

        if (shapeType.Equals("polygonal", StringComparison.OrdinalIgnoreCase))
        {
            ValidatePolygonData(shapeData);
        }
    }

    private static void ValidateDimension(decimal value, string fieldName)
    {
        if (value < 1 || value > 200)
            throw new ArgumentException($"El {fieldName} debe estar entre 1 y 200 metros");

        // Check max 2 decimal places
        var scaled = value * 100;
        if (scaled != Math.Floor(scaled))
            throw new ArgumentException($"El {fieldName} permite máximo 2 decimales");
    }

    private static void ValidatePolygonData(string? shapeData)
    {
        if (string.IsNullOrWhiteSpace(shapeData))
            throw new ArgumentException("Los datos del polígono son obligatorios para formas poligonales");

        List<PolygonVertex>? vertices;
        try
        {
            vertices = JsonSerializer.Deserialize<List<PolygonVertex>>(shapeData,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            throw new ArgumentException("Los datos del polígono no tienen un formato JSON válido");
        }

        if (vertices is null || vertices.Count < 3)
            throw new ArgumentException("El polígono debe tener al menos 3 vértices");

        if (vertices.Count > 20)
            throw new ArgumentException("El polígono no puede tener más de 20 vértices");

        if (IsSelfIntersecting(vertices))
            throw new ArgumentException("El polígono no puede ser auto-intersectante");
    }

    private static bool IsSelfIntersecting(List<PolygonVertex> vertices)
    {
        int n = vertices.Count;
        for (int i = 0; i < n; i++)
        {
            var a1 = vertices[i];
            var a2 = vertices[(i + 1) % n];

            for (int j = i + 2; j < n; j++)
            {
                // Skip adjacent edges (they share a vertex)
                if (i == 0 && j == n - 1)
                    continue;

                var b1 = vertices[j];
                var b2 = vertices[(j + 1) % n];

                if (SegmentsIntersect(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y))
                    return true;
            }
        }

        return false;
    }

    private static bool SegmentsIntersect(
        decimal ax1, decimal ay1, decimal ax2, decimal ay2,
        decimal bx1, decimal by1, decimal bx2, decimal by2)
    {
        decimal d1 = CrossProduct(bx1, by1, bx2, by2, ax1, ay1);
        decimal d2 = CrossProduct(bx1, by1, bx2, by2, ax2, ay2);
        decimal d3 = CrossProduct(ax1, ay1, ax2, ay2, bx1, by1);
        decimal d4 = CrossProduct(ax1, ay1, ax2, ay2, bx2, by2);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        return false;
    }

    private static decimal CrossProduct(decimal ax, decimal ay, decimal bx, decimal by, decimal cx, decimal cy)
    {
        return (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
    }

    private static void ValidateTables(IReadOnlyList<VenueTableSaveDto> tables)
    {
        foreach (var table in tables)
        {
            if (table.Capacity < 1 || table.Capacity > 20)
                throw new ArgumentException($"La capacidad de la mesa {table.TableNumber} debe estar entre 1 y 20");

            if (string.IsNullOrWhiteSpace(table.Shape))
                throw new ArgumentException($"La forma de la mesa {table.TableNumber} es obligatoria");

            if (!ValidTableShapes.Contains(table.Shape))
                throw new ArgumentException($"La forma de la mesa {table.TableNumber} debe ser 'round', 'rectangular', 'square' o 'half-moon'");

            // Medida física opcional: si viene informada debe ser positiva (medida positiva o nula).
            if (table.PrimaryMeters is <= 0)
                throw new ArgumentException($"La medida de la mesa {table.TableNumber} debe ser mayor que cero");

            if (table.SecondaryMeters is <= 0)
                throw new ArgumentException($"La medida secundaria de la mesa {table.TableNumber} debe ser mayor que cero");
        }
    }

    // ===== Mapping =====

    private static VenueLayoutDto MapToDto(VenueLayoutRow layout, IEnumerable<VenueTableRow> tables)
    {
        var tableDtos = tables.Select(t => new VenueTableDto(
            t.Id,
            t.TableNumber,
            t.Shape,
            t.Capacity,
            t.PositionX,
            t.PositionY,
            t.Rotation,
            t.PrimaryMeters,
            t.SecondaryMeters
        )).ToList();

        return new VenueLayoutDto(
            layout.Id,
            layout.EventId,
            layout.WidthMeters,
            layout.HeightMeters,
            layout.ShapeType,
            layout.ShapeData,
            tableDtos
        );
    }

    // ===== Internal types =====

    private sealed record PolygonVertex
    {
        public decimal X { get; init; }
        public decimal Y { get; init; }
    }
}
