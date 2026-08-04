namespace PROYECTO_MONGO_DOTNET.Models.Entidades;

public static class EstadosProyecto
{
    public static readonly string[] Todos = ["PLANIFICADO", "EN_CURSO", "PAUSADO", "FINALIZADO", "CANCELADO"];
    public static readonly string[] Cerrados = ["FINALIZADO", "CANCELADO"];
}

public static class PrioridadesProyecto
{
    public static readonly string[] Todas = ["BAJA", "MEDIA", "ALTA", "CRITICA"];
}

public static class EstadosRegistro
{
    public static readonly string[] Todos = ["PENDIENTE", "APROBADO", "RECHAZADO", "ANULADO"];
}

public static class EstadosTarea
{
    public static readonly string[] Todos = ["PENDIENTE", "EN_PROCESO", "COMPLETADA"];
    public static decimal Factor(string estado) => estado switch { "COMPLETADA" => 1m, "EN_PROCESO" => .5m, _ => 0m };
}

public static class EstadosEntregable
{
    public static readonly string[] Todos = ["PENDIENTE", "ENTREGADO", "JUSTIFICADO"];
}

public sealed class Proyecto
{
    public string? Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string CodigoDepartamento { get; set; } = "";
    public string CodigoJefeProyecto { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFinPlanificada { get; set; }
    public DateTime? FechaFinReal { get; set; }
    public string Estado { get; set; } = "PLANIFICADO";
    public string Prioridad { get; set; } = "MEDIA";
    public decimal Avance { get; set; }
    public decimal Presupuesto { get; set; }
    public bool Activo { get; set; } = true;
    public string Observacion { get; set; } = "";
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string ModificadoPor { get; set; } = "";
    public DateTime? FechaModificacion { get; set; }
}

public sealed class AsignacionProyecto
{
    public string? Id { get; set; }
    public string CodigoAsignacion { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public string CodigoEmpleado { get; set; } = "";
    public string Rol { get; set; } = "";
    public decimal DisponibilidadAsignada { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "ACTIVA";
    public bool Activo { get; set; } = true;
    public string Observacion { get; set; } = "";
    public decimal? CostoHoraSnapshot { get; set; }
    public string AsignadoPor { get; set; } = "";
    public DateTime FechaAsignacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class RegistroHoraProyecto
{
    public string? Id { get; set; }
    public string CodigoRegistro { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public string CodigoEmpleado { get; set; } = "";
    public string? CodigoTarea { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Horas { get; set; }
    public string Actividad { get; set; } = "";
    public string Estado { get; set; } = "PENDIENTE";
    public string RegistradoPor { get; set; } = "";
    public DateTime FechaRegistro { get; set; }
    public string? AprobadoPor { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string ObservacionRevision { get; set; } = "";
    public decimal? CostoHoraAplicado { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class FaseProyecto
{
    public string CodigoFase { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int Orden { get; set; } = 1;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public bool Activo { get; set; } = true;
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class TareaProyecto
{
    public string CodigoTarea { get; set; } = "";
    public string CodigoActividad { get; set; } = "";
    public string CodigoFase { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string CodigoResponsable { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public decimal Peso { get; set; }
    public decimal HorasPlanificadas { get; set; }
    public decimal CostoHoraPlanificado { get; set; }
    public DateTime? FechaCompletada { get; set; }
    public bool Activo { get; set; } = true;
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class DependenciaTarea
{
    public string CodigoProyecto { get; set; } = "";
    public string CodigoTarea { get; set; } = "";
    public string CodigoPredecesora { get; set; } = "";
    public string NombrePredecesora { get; set; } = "";
    public string TipoDependencia { get; set; } = "FS";
    public int DesfaseDias { get; set; }
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
}

public sealed class ActividadProyecto
{
    public string CodigoActividad { get; set; } = "";
    public string CodigoFase { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string CodigoResponsable { get; set; } = "";
    public int Orden { get; set; } = 1;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public bool Activo { get; set; } = true;
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class EntregableProyecto
{
    public string CodigoEntregable { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public string? CodigoTarea { get; set; }
    public string TipoRelacion { get; set; } = "TAREA";
    public string CodigoRelacion { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public DateTime FechaCompromiso { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public string Evidencia { get; set; } = "";
    public string Observacion { get; set; } = "";
    public DateTime? FechaEntrega { get; set; }
    public bool Activo { get; set; } = true;
    public string CreadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public sealed class IndicadoresPlanificacionProyecto
{
    public decimal PesoPlanificado { get; set; }
    public decimal AvancePlanificado { get; set; }
    public decimal AvanceReal { get; set; }
    public decimal DesviacionAvance { get; set; }
    public decimal Presupuesto { get; set; }
    public decimal CostoPlanificadoLaboral { get; set; }
    public decimal GastosAprobados { get; set; }
    public decimal CostoComprometidoEstimado { get; set; }
    public decimal SaldoDisponible { get; set; }
    public decimal HorasPlanificadas { get; set; }
    public int TareasTotales { get; set; }
    public int TareasCompletadas { get; set; }
    public int EntregablesTotales { get; set; }
    public int EntregablesEntregados { get; set; }
    public bool PlanCompleto { get; set; }
}

public sealed class TareaPlanificacionVm
{
    public TareaProyecto Tarea { get; set; } = new();
    public decimal Avance { get; set; }
    public decimal AvancePlanificado { get; set; }
    public bool Bloqueada { get; set; }
    public IReadOnlyCollection<DependenciaTarea> Dependencias { get; set; } = [];
    public IReadOnlyCollection<EntregableProyecto> Entregables { get; set; } = [];
}

public sealed class FasePlanificacionVm
{
    public FaseProyecto Fase { get; set; } = new();
    public decimal Peso { get; set; }
    public decimal Avance { get; set; }
    public IReadOnlyCollection<ActividadPlanificacionVm> Actividades { get; set; } = [];
    public IReadOnlyCollection<TareaPlanificacionVm> Tareas { get; set; } = [];
    public IReadOnlyCollection<EntregableProyecto> Entregables { get; set; } = [];
}

public sealed class ActividadPlanificacionVm
{
    public ActividadProyecto Actividad { get; set; } = new();
    public decimal Peso { get; set; }
    public decimal Avance { get; set; }
    public IReadOnlyCollection<TareaPlanificacionVm> Tareas { get; set; } = [];
    public IReadOnlyCollection<EntregableProyecto> Entregables { get; set; } = [];
}

public sealed class PlanificacionProyectoVm
{
    public Proyecto Proyecto { get; set; } = new();
    public IndicadoresPlanificacionProyecto Indicadores { get; set; } = new();
    public IReadOnlyCollection<FasePlanificacionVm> Fases { get; set; } = [];
    public IReadOnlyCollection<EntregableProyecto> Entregables { get; set; } = [];
    public IReadOnlyCollection<CapacidadEmpleadoProyectoVm> Equipo { get; set; } = [];
}

public sealed class CapacidadEmpleadoProyectoVm
{
    public AsignacionProyecto Asignacion { get; set; } = new();
    public string NombreEmpleado { get; set; } = "";
    public decimal DisponibilidadUsada { get; set; }
    public decimal DisponibilidadRestante { get; set; }
    public decimal HorasDisponiblesEstimadas { get; set; }
    public decimal HorasPlanificadasTareas { get; set; }
    public decimal DiferenciaHoras { get; set; }
    public string EstadoCapacidad { get; set; } = "DISPONIBLE";
    public IReadOnlyCollection<string> Conflictos { get; set; } = [];
}

public sealed class ProyectoListadoVm
{
    public Proyecto Proyecto { get; set; } = new();
    public string NombreDepartamento { get; set; } = "";
    public string NombreJefeProyecto { get; set; } = "";
    public decimal SaldoDisponible { get; set; }
}

public sealed class ReporteProyectoFila
{
    public string Nombre { get; set; } = "";
    public string Departamento { get; set; } = "";
    public string Jefe { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFinPlanificada { get; set; }
    public string Estado { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public decimal Avance { get; set; }
    public decimal Presupuesto { get; set; }
    public decimal SaldoDisponible { get; set; }
    public bool Activo { get; set; }
}

public sealed class ValidacionCierreProyectoVm
{
    public bool PuedeFinalizar { get; set; }
    public IReadOnlyCollection<string> Pendientes { get; set; } = [];
}

public sealed class SeguimientoProyecto
{
    public string? Id { get; set; }
    public string CodigoSeguimiento { get; set; } = "";
    public string CodigoProyecto { get; set; } = "";
    public decimal AvanceAnterior { get; set; }
    public decimal AvanceNuevo { get; set; }
    public string EstadoAnterior { get; set; } = "";
    public string EstadoNuevo { get; set; } = "";
    public string Observacion { get; set; } = "";
    public string RegistradoPor { get; set; } = "";
    public DateTime FechaRegistro { get; set; }
}
