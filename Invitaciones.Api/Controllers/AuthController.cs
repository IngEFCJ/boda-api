using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invitaciones.Api.Controllers;

[ApiController]
[Route("api/admin/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAdminAuthService _authService;

    public AuthController(IAdminAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Validate NombreUsuario: required, 3-50 chars
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) ||
            request.NombreUsuario.Length < 3 ||
            request.NombreUsuario.Length > 50)
        {
            return BadRequest(new { message = "El nombre de usuario debe tener entre 3 y 50 caracteres" });
        }

        // Validate Password: required, 6-255 chars
        if (string.IsNullOrWhiteSpace(request.Password) ||
            request.Password.Length < 6 ||
            request.Password.Length > 255)
        {
            return BadRequest(new { message = "La contraseña debe tener entre 6 y 255 caracteres" });
        }

        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
}
