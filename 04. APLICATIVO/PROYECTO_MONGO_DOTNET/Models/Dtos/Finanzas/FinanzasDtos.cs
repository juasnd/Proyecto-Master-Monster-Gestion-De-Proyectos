using System.ComponentModel.DataAnnotations;

namespace PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;

public sealed class GuardarContratoDto
{
    public string? CodigoContrato { get; set; }
    [Required] public string CodigoEmpleado { get; set; } = "";
    [Required, StringLength(60)] public string TipoContrato { get; set; } = "";
    [Range(0.01, double.MaxValue)] public decimal SueldoBase { get; set; }
    [Range(0.01, double.MaxValue)] public decimal HorasMensuales { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    [Required] public string Estado { get; set; } = "ACTIVO";
}

public sealed class GuardarCategoriaGastoDto
{
    public string? CodigoCategoria { get; set; }
    [Required, StringLength(60)] public string Nombre { get; set; } = "";
    [StringLength(300)] public string Descripcion { get; set; } = "";
    public bool Activo { get; set; } = true;
}

public sealed class GuardarParametroFinancieroDto
{
    [Required] public string CodigoParametro { get; set; } = "";
    [Required] public string Descripcion { get; set; } = "";
    [Required] public string TipoDato { get; set; } = "TEXTO";
    public string? ValorTexto { get; set; }
    public decimal? ValorDecimal { get; set; }
    public int? ValorEntero { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class GuardarMovimientoDto
{
    public string? CodigoMovimiento { get; set; }
    [Required] public string CodigoProyecto { get; set; } = "";
    [Required] public string Tipo { get; set; } = "GASTO";
    [Required] public string CodigoCategoria { get; set; } = "";
    [Required, StringLength(300)] public string Concepto { get; set; } = "";
    [Range(0.01, double.MaxValue)] public decimal Valor { get; set; }
    public DateTime Fecha { get; set; }
    [StringLength(500)] public string Observacion { get; set; } = "";
}

public sealed class RevisarMovimientoDto
{
    [StringLength(500)] public string Observacion { get; set; } = "";
}
