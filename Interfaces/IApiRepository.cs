using ApiConfirmacionAsistenciaInvitacion.Models;

namespace ApiConfirmacionAsistenciaInvitacion.Interfaces
{
    public interface IApiRepository
    {
        Task<ApiConfig?> GetApiConfigByIdAsync(int apiId);
        Task<UserCredentials?> GetUserCredentialsAsync(int apiId);
        Task<List<ApiConfig>> GetAllApisAsync();
        Task<ApiConfig?> FindApiByCredentialsAsync(string username, string password);
    }
}
