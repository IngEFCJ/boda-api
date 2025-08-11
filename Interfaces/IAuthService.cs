using ApiConfirmacionAsistenciaInvitacion.Models;

namespace ApiConfirmacionAsistenciaInvitacion.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateAsync(string username, string password);
        string GenerateJwtToken(string username, int apiId);
        bool ValidateBasicAuthHeader(string authHeader, out string username, out string password);
    }
}
