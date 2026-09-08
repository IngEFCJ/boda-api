using Invitaciones.Api.DTO;

namespace Invitaciones.Api.Interfaces;

public interface IAdminAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
