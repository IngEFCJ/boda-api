using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static Invitaciones.Api.DTO.DTOs;

namespace Invitaciones.Api.Controllers
{
   

    [ApiController]
    [Route("api/public/invitations")]
    public sealed class PublicInvitationsController : ControllerBase
    {
        private readonly IPublicInvitationService _service;

        public PublicInvitationsController(IPublicInvitationService service)
        {
            _service = service;
        }

        [HttpGet("by-token/{token}")]
        [ProducesResponseType(typeof(InvitationByTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<ActionResult<InvitationByTokenResponse>> GetByToken(
            [FromRoute] string token,
            CancellationToken ct)
        {
            var result = await _service.GetInvitationByTokenAsync(token, ct);

            return result.Status switch
            {
                InvitationResolveStatus.Ok => Ok(result.Data),
                InvitationResolveStatus.Expired => StatusCode(StatusCodes.Status410Gone),
                _ => NotFound()
            };
        }

        [HttpPost("confirm")]
        [ProducesResponseType(typeof(InvitationByTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InvitationByTokenResponse>> ConfirmTickets(
                     [FromBody] ConfirmTicketsRequest request,
                     CancellationToken ct)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest("Token requerido.");

            var result = await _service.ConfirmTicketsAsync(request, ct);

            return result.Status switch
            {
                InvitationResolveStatus.Ok => Ok(result.Data),
                InvitationResolveStatus.Expired => StatusCode(StatusCodes.Status410Gone),
                _ => NotFound()
            };
        }



    }

}
