using System.Data;

namespace Invitaciones.Api.Interfaces;

public interface IAdminAuthRepository
{
    Task<DataTable> ValidarCredencialesAsync(string nombreUsuario, string password);
}
