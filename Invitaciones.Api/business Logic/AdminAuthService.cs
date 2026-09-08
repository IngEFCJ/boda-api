using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminAuthService : IAdminAuthService
{
    private readonly IAdminAuthRepository _repo;
    private readonly IConfiguration _config;

    public AdminAuthService(IAdminAuthRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var dt = await _repo.ValidarCredencialesAsync(request.NombreUsuario, request.Password);

        if (dt.Rows.Count == 0)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        var row = dt.Rows[0];
        var resultado = row["Resultado"]?.ToString();

        switch (resultado)
        {
            case "EXITO":
                break;

            case "BLOQUEADO":
                var bloqueadoHasta = row["BloqueadoHasta"] as DateTime?;
                var mensaje = bloqueadoHasta.HasValue
                    ? $"Cuenta bloqueada hasta {bloqueadoHasta.Value:HH:mm:ss} UTC"
                    : "Cuenta bloqueada temporalmente";
                throw new InvalidOperationException(mensaje);

            case "FALLIDO":
            default:
                throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        var usuarioId = Convert.ToInt32(row["UsuarioID"]);
        var nombreUsuario = row["NombreUsuario"]?.ToString() ?? "";
        var email = row["Email"]?.ToString() ?? "";
        var rol = row["Rol"]?.ToString() ?? "";

        var token = GenerateJwtToken(usuarioId, nombreUsuario, email, rol);
        var expirationMinutes = int.Parse(_config["Jwt:ExpirationMinutes"] ?? "30");
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        return new LoginResponse(
            UsuarioID: usuarioId,
            Usuario: nombreUsuario,
            Email: email,
            Rol: rol,
            Token: token,
            ExpiresIn: expirationMinutes * 60,
            ExpiresAt: expiresAt
        );
    }

    private string GenerateJwtToken(int usuarioId, string nombreUsuario, string email, string rol)
    {
        var jwtSection = _config.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("Missing configuration: Jwt:SecretKey");
        var issuer = jwtSection["Issuer"] ?? "InvitacionesApi";
        var audience = jwtSection["Audience"] ?? "AdminPanel";
        var expirationMinutes = int.Parse(jwtSection["ExpirationMinutes"] ?? "30");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim("nameid", usuarioId.ToString()),
            new Claim("unique_name", nombreUsuario),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", rol)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
