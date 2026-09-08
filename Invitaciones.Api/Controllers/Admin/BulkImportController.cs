using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/bulk-import")]
[Authorize]
public sealed class BulkImportController : ControllerBase
{
    private readonly IAdminBulkImportService _bulkImportService;

    public BulkImportController(IAdminBulkImportService bulkImportService)
    {
        _bulkImportService = bulkImportService;
    }

    /// <summary>
    /// Import invitations and tickets in bulk from parsed JSON rows.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(BulkImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
        [FromBody] BulkImportRequest request,
        CancellationToken ct)
    {
        if (request.EventId == Guid.Empty)
            return BadRequest(new { message = "El parámetro eventId es requerido" });

        if (request.Rows is null || request.Rows.Count == 0)
            return BadRequest(new { message = "Se requiere al menos una fila para importar" });

        var result = await _bulkImportService.ImportAsync(request.EventId, request, ct);
        return Ok(result);
    }
}
