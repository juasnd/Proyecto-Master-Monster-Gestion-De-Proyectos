namespace PROYECTO_MONGO_DOTNET.Models.Entidades;

public sealed class Contrato
{
    public string? Id { get; set; }
    public string CodigoContrato { get; set; } = "";
    public string CodigoEmpleado { get; set; } = "";
    public string TipoContrato { get; set; } = "";
    public decimal SueldoBase { get; set; }
    public decimal HorasMensuales { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    public bool Activo { get; set; } = true;
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class CategoriaGasto
{
    public string? Id { get; set; }
    public string CodigoCategoria { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public bool Activo { get; set; } = true;
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class ParametroFinanciero
{
    public string? Id { get; set; }
    public string CodigoParametro { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string TipoDato { get; set; } = "TEXTO";
    public string? ValorTexto { get; set; }
    public decimal? ValorDecimal { get; set; }
    public int? ValorEntero { get; set; }
    public bool Activo { get; set; } = true;
    public string ModificadoPor { get; set; } = "";
    public DateTime FechaModificacion { get; set; }
}

public sealed class MovimientoProyecto
{
    public string? Id { get; set; }
    public string CodigoMovimiento { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public string Tipo { get; set; } = "GASTO";
    public string CodigoCategoria { get; set; } = "";
    public string Concepto { get; set; } = "";
    public decimal Valor { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public string RegistradoPor { get; set; } = "";
    public DateTime FechaRegistro { get; set; }
    public string? AprobadoPor { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string Observacion { get; set; } = "";
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class CostoProyectoResumen
{
    public string CodigoProyecto { get; set; } = "";
    public string NombreProyecto { get; set; } = "";
    public decimal Presupuesto { get; set; }
    public decimal CostoLaboralPlanificado { get; set; }
    public decimal GastosAprobados { get; set; }
    public decimal CostoComprometidoEstimado { get; set; }
    public decimal SaldoDisponible { get; set; }
    public decimal PorcentajeConsumo { get; set; }
    public bool EnAlerta { get; set; }
    public bool Excedido { get; set; }
}
