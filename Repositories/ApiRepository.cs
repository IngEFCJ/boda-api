using ApiConfirmacionAsistenciaInvitacion.Interfaces;
using ApiConfirmacionAsistenciaInvitacion.Models;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;

namespace ApiConfirmacionAsistenciaInvitacion.Repositories
{
    public class ApiRepository : IApiRepository
    {
        private readonly string _connectionString;

        public ApiRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found");
        }

        public async Task<ApiConfig?> GetApiConfigByIdAsync(int apiId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetApiConfigById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ApiId", apiId);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new ApiConfig
                    {
                        FIID = reader.GetInt32("FIID"),
                        FCJSON = reader.GetString("FCJSON")
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving API config: {ex.Message}", ex);
            }

            return null;
        }

        public async Task<UserCredentials?> GetUserCredentialsAsync(int apiId)
        {
            var apiConfig = await GetApiConfigByIdAsync(apiId);
            if (apiConfig == null) return null;

            try
            {
                var configDetail = JsonSerializer.Deserialize<ApiConfigDetail>(apiConfig.FCJSON);
                if (configDetail == null) return null;

                // Aquí podrías implementar la lógica para obtener las credenciales
                // Por ahora, retornamos null ya que el JSON no contiene usuario/contraseña
                // Necesitarías agregar esos campos al JSON o crear otra tabla
                return null;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error deserializing API config JSON: {ex.Message}", ex);
            }
        }

        public async Task<List<ApiConfig>> GetAllApisAsync()
        {
            var apis = new List<ApiConfig>();
            
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT FIID, FCJSON FROM TBApis", connection);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    apis.Add(new ApiConfig
                    {
                        FIID = reader.GetInt32("FIID"),
                        FCJSON = reader.GetString("FCJSON")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all APIs: {ex.Message}", ex);
            }

            return apis;
        }

        public async Task<ApiConfig?> FindApiByCredentialsAsync(string username, string password)
        {
            var allApis = await GetAllApisAsync();
            
            foreach (var api in allApis)
            {
                try
                {
                    var configDetail = JsonSerializer.Deserialize<ApiConfigDetail>(api.FCJSON);
                    if (configDetail != null && 
                        configDetail.Username == username && 
                        configDetail.Password == password)
                    {
                        return api;
                    }
                }
                catch (JsonException)
                {
                    // Si hay error al deserializar, continuar con la siguiente API
                    continue;
                }
            }

            return null;
        }
    }
}
