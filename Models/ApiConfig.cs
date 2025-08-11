namespace ApiConfirmacionAsistenciaInvitacion.Models
{
    public class ApiConfig
    {
        public int FIID { get; set; }
        public string FCJSON { get; set; } = string.Empty;
    }

    public class ApiConfigDetail
    {
        public string IdAPI { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Puerto { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty;
        public string EstatusDescripcion { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
