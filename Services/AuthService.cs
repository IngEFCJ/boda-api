using ApiConfirmacionAsistenciaInvitacion.Interfaces;
using ApiConfirmacionAsistenciaInvitacion.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiConfirmacionAsistenciaInvitacion.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiRepository _apiRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IApiRepository apiRepository, IConfiguration configuration)
        {
            _apiRepository = apiRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse> AuthenticateAsync(string username, string password)
        {
            try
            {
                // Buscar la API que coincida con las credenciales
                var apiConfig = await _apiRepository.FindApiByCredentialsAsync(username, password);
                
                if (apiConfig == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Generar token JWT con el apiId encontrado
                var token = GenerateJwtToken(username, apiConfig.FIID);
                var expiresAt = DateTime.UtcNow.AddHours(24); // Token válido por 24 horas

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    Message = "Autenticación exitosa",
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Error en la autenticación: {ex.Message}"
                };
            }
        }

        public string GenerateJwtToken(string username, int apiId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyHere12345678901234567890";
            var issuer = jwtSettings["Issuer"] ?? "ApiConfirmacionAsistenciaInvitacion";
            var audience = jwtSettings["Audience"] ?? "ApiConfirmacionAsistenciaInvitacion";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("ApiId", apiId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateBasicAuthHeader(string authHeader, out string username, out string password)
        {
            username = string.Empty;
            password = string.Empty;

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
            {
                return false;
            }

            try
            {
                var encodedCredentials = authHeader.Substring("Basic ".Length);
                var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                var separatorIndex = decodedCredentials.IndexOf(':');

                if (separatorIndex == -1)
                {
                    return false;
                }

                username = decodedCredentials.Substring(0, separatorIndex);
                password = decodedCredentials.Substring(separatorIndex + 1);

                return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
            }
            catch
            {
                return false;
            }
        }
    }
}
