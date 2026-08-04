namespace PROYECTO_MONGO_DOTNET.Models;

public class ReporteHistorial
{
    public string Codigo { get; set; } = ""; public string Fecha { get; set; } = ""; public string Usuario { get; set; } = ""; public string Perfil { get; set; } = "";
    public string CodigoReporte { get; set; } = ""; public string Modulo { get; set; } = ""; public string TipoReporte { get; set; } = ""; public string Formato { get; set; } = "";
    public int TotalRegistros { get; set; } public string Filtros { get; set; } = "";
}
