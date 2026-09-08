using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminBulkImportService : IAdminBulkImportService
{
    private readonly IAdminInvitationRepository _invitationRepo;
    private readonly IAdminTicketRepository _ticketRepo;

    public AdminBulkImportService(
        IAdminInvitationRepository invitationRepo,
        IAdminTicketRepository ticketRepo)
    {
        _invitationRepo = invitationRepo;
        _ticketRepo = ticketRepo;
    }

    public async Task<BulkImportResultDto> ImportAsync(Guid eventId, BulkImportRequest req, CancellationToken ct)
    {
        var errors = new List<string>();
        int invitationsCreated = 0;
        int ticketsCreated = 0;
        int rowsSkipped = 0;

        for (int i = 0; i < req.Rows.Count; i++)
        {
            var row = req.Rows[i];
            int rowNumber = i + 1;

            var rowErrors = ValidateRow(row, rowNumber);

            if (rowErrors.Count > 0)
            {
                errors.AddRange(rowErrors);
                rowsSkipped++;
                continue;
            }

            var displayName = row.DisplayName.Trim();

            // Create invitation
            var invitationId = await _invitationRepo.CreateAsync(eventId, displayName, ct);

            // Generate token for the invitation
            await _invitationRepo.GenerateTokenAsync(invitationId, ct);

            invitationsCreated++;

            // Create tickets for each label
            foreach (var label in row.TicketLabels)
            {
                var trimmedLabel = label.Trim();
                await _ticketRepo.CreateAsync(invitationId, trimmedLabel, ct);
                ticketsCreated++;
            }
        }

        return new BulkImportResultDto(invitationsCreated, ticketsCreated, rowsSkipped, errors);
    }

    private static List<string> ValidateRow(BulkImportRowDto row, int rowNumber)
    {
        var errors = new List<string>();

        // Validate DisplayName
        var displayName = row.DisplayName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors.Add($"Fila {rowNumber}: nombre de invitación vacío");
        }
        else if (displayName.Length > 150)
        {
            errors.Add($"Fila {rowNumber}: nombre excede 150 caracteres");
        }

        // Validate TicketLabels existence
        if (row.TicketLabels == null || row.TicketLabels.Count == 0)
        {
            errors.Add($"Fila {rowNumber}: debe tener al menos una etiqueta de boleto");
        }
        else
        {
            // Validate max 20 labels per row
            if (row.TicketLabels.Count > 20)
            {
                errors.Add($"Fila {rowNumber}: no puede tener más de 20 etiquetas de boleto");
            }

            // Validate each label
            for (int j = 0; j < row.TicketLabels.Count; j++)
            {
                var label = row.TicketLabels[j]?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(label))
                {
                    errors.Add($"Fila {rowNumber}: etiqueta {j + 1} está vacía");
                }
                else if (label.Length > 100)
                {
                    errors.Add($"Fila {rowNumber}: etiqueta {j + 1} excede 100 caracteres");
                }
            }
        }

        return errors;
    }
}
