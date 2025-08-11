using ApiConfirmacionAsistenciaInvitacion.Interfaces;
using ApiConfirmacionAsistenciaInvitacion.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiConfirmacionAsistenciaInvitacion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
         _authService = authService;
        }

        /// <summary>
        /// Genera un bearer token usando autenticación Basic
        /// </summary>
        /// <returns>Token JWT si la autenticación es exitosa</returns>
        [HttpPost("token")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(LoginResponse), 401)]
        [ProducesResponseType(typeof(LoginResponse), 400)]
        public async Task<IActionResult> GenerateToken()
        {
            try
            {
                // Obtener el header de autorización
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(authHeader))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Header de autorización requerido"
                    });
                }

                // Validar y extraer credenciales del Basic Auth
                if (!_authService.ValidateBasicAuthHeader(authHeader, out string username, out string password))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Formato de autorización inválido. Use Basic Auth"
                    });
                }

                // Autenticar usuario
                var result = await _authService.AuthenticateAsync(username, password);
                
                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = $"Error interno: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        /// <param name="token">Token JWT a validar</param>
        /// <returns>Información del token si es válido</returns>
        [HttpPost("validate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult ValidateToken([FromBody] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token requerido");
                }

                // Aquí podrías implementar la validación del token
                // Por ahora solo retornamos un mensaje de éxito
                return Ok(new { message = "Token válido", token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
