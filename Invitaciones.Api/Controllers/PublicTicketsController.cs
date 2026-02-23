using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static Invitaciones.Api.DTO.DTOs;

namespace Invitaciones.Api.Controllers
{
    [ApiController]
    [Route("api/public/tickets")]
    public sealed class PublicTicketsController : ControllerBase
    {
        private readonly IPublicInvitationService _service;

        public PublicTicketsController(IPublicInvitationService service)
        {
            _service = service;
        }

        [HttpGet("by-token/{token}")]
        [ProducesResponseType(typeof(TicketsWithQrResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<ActionResult<TicketsWithQrResponse>> GetTicketsWithQrByToken(
            [FromRoute] string token,
            CancellationToken ct)
        {
            var result = await _service.GetTicketsWithQrByTokenAsync(token, ct);

            return result.Status switch
            {
                InvitationResolveStatus.Ok => Ok(result.Data),
                InvitationResolveStatus.Expired => StatusCode(StatusCodes.Status410Gone),
                _ => NotFound()
            };
        }
    }
}