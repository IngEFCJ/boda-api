namespace Invitaciones.Api.DTO;

public sealed record LoginRequest(string NombreUsuario, string Password);

public sealed record LoginResponse(
    int UsuarioID,
    string Usuario,
    string Email,
    string Rol,
    string Token,
    int ExpiresIn,
    DateTime ExpiresAt
);
