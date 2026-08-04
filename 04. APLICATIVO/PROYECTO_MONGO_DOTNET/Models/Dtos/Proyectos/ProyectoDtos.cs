using System.ComponentModel.DataAnnotations;

namespace PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;

public sealed class GuardarProyectoDto
{
    public string? Codigo { get; set; }
    [Required, StringLength(150)] public string Nombre { get; set; } = "";
    [StringLength(1000)] public string Descripcion { get; set; } = "";
    [Required] public string CodigoDepartamento { get; set; } = "";
    [Required] public string CodigoJefeProyecto { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFinPlanificada { get; set; }
    [Required] public string Prioridad { get; set; } = "MEDIA";
    [Range(0, double.MaxValue)] public decimal Presupuesto { get; set; }
    [StringLength(1000)] public string Observacion { get; set; } = "";
}

public sealed class GuardarAsignacionDto
{
    [Required] public string CodigoEmpleado { get; set; } = "";
    [Required, StringLength(100)] public string Rol { get; set; } = "";
    [Range(1, 100)] public decimal DisponibilidadAsignada { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    [StringLength(500)] public string Observacion { get; set; } = "";
}

public sealed class GuardarFaseProyectoDto
{
    [Required, StringLength(150)] public string Nombre { get; set; } = "";
    [StringLength(1000)] public string Descripcion { get; set; } = "";
    [Range(1, 999)] public int Orden { get; set; } = 1;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}

public sealed class GuardarTareaProyectoDto
{
    public string? CodigoFase { get; set; }
    [Required] public string CodigoActividad { get; set; } = "";
    [Required, StringLength(180)] public string Nombre { get; set; } = "";
    [StringLength(1000)] public string Descripcion { get; set; } = "";
    [Required] public string CodigoResponsable { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    [Range(0.01, 100)] public decimal Peso { get; set; }
    [Range(0, double.MaxValue)] public decimal HorasPlanificadas { get; set; }
    public List<GuardarDependenciaTareaDto> Dependencias { get; set; } = [];
}

public sealed class GuardarDependenciaTareaDto
{
    [Required] public string CodigoPredecesora { get; set; } = "";
    [Required, RegularExpression("^(FS|SS|FF|SF)$")] public string TipoDependencia { get; set; } = "FS";
    [Range(-3650, 3650)] public int DesfaseDias { get; set; }
}

public sealed class GuardarActividadProyectoDto
{
    [Required, StringLength(180)] public string Nombre { get; set; } = "";
    [StringLength(1000)] public string Descripcion { get; set; } = "";
    [Required] public string CodigoResponsable { get; set; } = "";
    [Range(1, 999)] public int Orden { get; set; } = 1;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}

public sealed class CambiarEstadoTareaDto
{
    [Required] public string Estado { get; set; } = "";
}

public sealed class GuardarEntregableProyectoDto
{
    [Required, RegularExpression("^(FASE|ACTIVIDAD|TAREA)$")] public string TipoRelacion { get; set; } = "TAREA";
    [Required] public string CodigoRelacion { get; set; } = "";
    [Required, StringLength(180)] public string Nombre { get; set; } = "";
    [StringLength(1000)] public string Descripcion { get; set; } = "";
    public DateTime FechaCompromiso { get; set; }
    [StringLength(500)] public string Evidencia { get; set; } = "";
    [StringLength(1000)] public string Observacion { get; set; } = "";
}

public sealed class CambiarEstadoEntregableDto
{
    [Required] public string Estado { get; set; } = "";
    [StringLength(500)] public string Evidencia { get; set; } = "";
    [StringLength(1000)] public string Observacion { get; set; } = "";
}

public sealed class ActualizarEstadoDto
{
    [Required] public string Estado { get; set; } = "";
    [StringLength(1000)] public string Observacion { get; set; } = "";
}

public sealed class CambiarActivoProyectoDto
{
    public bool Activo { get; set; }
    [StringLength(500)] public string Observacion { get; set; } = "";
}

public sealed class FinalizarProyectoDto
{
    public DateTime FechaFinReal { get; set; }
    [Required, StringLength(1000)] public string ObservacionFinal { get; set; } = "";
}

public sealed class MotivoDto
{
    [Required, StringLength(1000)] public string Motivo { get; set; } = "";
}

public sealed class FiltroProyectosDto
{
    public string? TextoBusqueda { get; set; }
    public string? Estado { get; set; }
    public bool? Activo { get; set; }
    public string? CodigoJefe { get; set; }
    public string? CodigoDepartamento { get; set; }
    public string? CodigoEmpleado { get; set; }
    public string? Prioridad { get; set; }
    public DateTime? FechaInicioDesde { get; set; }
    public DateTime? FechaInicioHasta { get; set; }
    public DateTime? FechaFinDesde { get; set; }
    public DateTime? FechaFinHasta { get; set; }
    public decimal? AvanceMinimo { get; set; }
    public decimal? AvanceMaximo { get; set; }
    public decimal? PresupuestoMinimo { get; set; }
    public decimal? PresupuestoMaximo { get; set; }
    public bool SoloAtrasados { get; set; }
    public bool SoloExcedidos { get; set; }
    public string NivelDetalle { get; set; } = "RESUMEN";
    public int Pagina { get; set; } = 1;
    public int Cantidad { get; set; } = 20;
    public string OrdenarPor { get; set; } = "Codigo";
    public string Direccion { get; set; } = "ASC";
}
