using System.Data;

namespace Invitaciones.Api.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();

    }
}
