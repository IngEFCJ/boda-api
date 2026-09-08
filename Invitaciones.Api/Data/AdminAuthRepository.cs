using System.Data;
using Dapper;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.Data;

public sealed class AdminAuthRepository : IAdminAuthRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminAuthRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<DataTable> ValidarCredencialesAsync(string nombreUsuario, string password)
    {
        using var conn = _db.Create();

        var p = new DynamicParameters();
        p.Add("@NombreUsuario", nombreUsuario, DbType.String, ParameterDirection.Input);
        p.Add("@Password", password, DbType.String, ParameterDirection.Input);

        using var reader = await conn.ExecuteReaderAsync(
            sql: "dbo.sp_Usuarios_ValidarCredenciales",
            param: p,
            commandType: CommandType.StoredProcedure
        );

        var dt = new DataTable();
        dt.Load(reader);
        return dt;
    }
}
