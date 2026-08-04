using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Repositories;

public interface IPlanificacionProyectoRepositorio
{
    Task<IReadOnlyCollection<FaseProyecto>> FasesAsync(string codigoProyecto);
    Task<FaseProyecto?> FaseAsync(string codigoProyecto, string codigoFase);
    Task<IReadOnlyCollection<ActividadProyecto>> ActividadesAsync(string codigoProyecto);
    Task<ActividadProyecto?> ActividadAsync(string codigoProyecto, string codigoActividad);
    Task<IReadOnlyCollection<TareaProyecto>> TareasAsync(string codigoProyecto);
    Task<TareaProyecto?> TareaAsync(string codigoProyecto, string codigoTarea);
    Task<IReadOnlyCollection<DependenciaTarea>> DependenciasAsync(string codigoProyecto);
    Task<IReadOnlyCollection<EntregableProyecto>> EntregablesAsync(string codigoProyecto);
    Task<EntregableProyecto?> EntregableAsync(string codigoProyecto, string codigoEntregable);
    Task CrearFaseAsync(FaseProyecto fase);
    Task ActualizarFaseAsync(FaseProyecto fase);
    Task CrearActividadAsync(ActividadProyecto actividad);
    Task ActualizarActividadAsync(ActividadProyecto actividad);
    Task CrearTareaAsync(TareaProyecto tarea);
    Task ActualizarTareaAsync(TareaProyecto tarea);
    Task ReemplazarDependenciasAsync(string codigoProyecto, string codigoTarea, IReadOnlyCollection<DependenciaTarea> dependencias);
    Task CrearEntregableAsync(EntregableProyecto entregable);
    Task ActualizarEntregableAsync(EntregableProyecto entregable);
}

public sealed class PlanificacionProyectoRepositorio(SqlContexto db) : IPlanificacionProyectoRepositorio
{
    private const string Fases = "CodigoFase,CodigoProyecto,Nombre,Descripcion,Orden,FechaInicio,FechaFin,Estado,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion";
    private const string Actividades = "CodigoActividad,CodigoFase,CodigoProyecto,Nombre,Descripcion,CodigoResponsable,Orden,FechaInicio,FechaFin,Estado,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion";
    private const string Tareas = "CodigoTarea,CodigoActividad,CodigoFase,CodigoProyecto,Nombre,Descripcion,CodigoResponsable,FechaInicio,FechaFin,Estado,Peso,HorasPlanificadas,CostoHoraPlanificado,FechaCompletada,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion";
    private const string Entregables = "e.CodigoEntregable,COALESCE(t.CodigoProyecto,a.CodigoProyecto,f.CodigoProyecto) CodigoProyecto,e.CodigoTarea,e.TipoRelacion,e.CodigoRelacion,e.Nombre,e.Descripcion,e.FechaCompromiso,e.Estado,e.Evidencia,e.Observacion,e.FechaEntrega,e.Activo,e.CreadoPor,e.FechaCreacion,e.ModificadoPor,e.FechaModificacion";

    public async Task<IReadOnlyCollection<FaseProyecto>> FasesAsync(string p) =>
        await db.ConsultarAsync<FaseProyecto>($"SELECT {Fases} FROM app.FaseProyecto WHERE CodigoProyecto=@P AND Activo=1 ORDER BY Orden,CodigoFase", new { P = N(p) });
    public async Task<FaseProyecto?> FaseAsync(string p, string f) =>
        (await db.ConsultarAsync<FaseProyecto>($"SELECT {Fases} FROM app.FaseProyecto WHERE CodigoProyecto=@P AND CodigoFase=@F", new { P = N(p), F = N(f) })).FirstOrDefault();
    public async Task<IReadOnlyCollection<ActividadProyecto>> ActividadesAsync(string p) =>
        await db.ConsultarAsync<ActividadProyecto>($"SELECT {Actividades} FROM app.ActividadProyecto WHERE CodigoProyecto=@P AND Activo=1 ORDER BY Orden,CodigoActividad", new { P=N(p) });
    public async Task<ActividadProyecto?> ActividadAsync(string p,string a) =>
        (await db.ConsultarAsync<ActividadProyecto>($"SELECT {Actividades} FROM app.ActividadProyecto WHERE CodigoProyecto=@P AND CodigoActividad=@A",new{P=N(p),A=N(a)})).FirstOrDefault();
    public async Task<IReadOnlyCollection<TareaProyecto>> TareasAsync(string p) =>
        await db.ConsultarAsync<TareaProyecto>($"SELECT {Tareas} FROM app.TareaProyecto WHERE CodigoProyecto=@P AND Activo=1 ORDER BY FechaInicio,CodigoTarea", new { P = N(p) });
    public async Task<TareaProyecto?> TareaAsync(string p, string t) =>
        (await db.ConsultarAsync<TareaProyecto>($"SELECT {Tareas} FROM app.TareaProyecto WHERE CodigoProyecto=@P AND CodigoTarea=@T", new { P = N(p), T = N(t) })).FirstOrDefault();
    public async Task<IReadOnlyCollection<DependenciaTarea>> DependenciasAsync(string p) =>
        await db.ConsultarAsync<DependenciaTarea>("SELECT d.CodigoProyecto,d.CodigoTarea,d.CodigoPredecesora,pre.Nombre NombrePredecesora,d.TipoDependencia,d.DesfaseDias,d.CreadoPor,d.FechaCreacion FROM app.DependenciaTarea d INNER JOIN app.TareaProyecto tarea ON tarea.CodigoTarea=d.CodigoTarea AND tarea.Activo=1 INNER JOIN app.TareaProyecto pre ON pre.CodigoTarea=d.CodigoPredecesora AND pre.Activo=1 WHERE d.CodigoProyecto=@P ORDER BY d.CodigoTarea,pre.FechaInicio,d.CodigoPredecesora", new { P = N(p) });
    public async Task<IReadOnlyCollection<EntregableProyecto>> EntregablesAsync(string p) =>
        await db.ConsultarAsync<EntregableProyecto>($"SELECT {Entregables} FROM app.EntregableProyecto e LEFT JOIN app.TareaProyecto t ON t.CodigoTarea=e.CodigoTarea LEFT JOIN app.ActividadProyecto a ON e.TipoRelacion='ACTIVIDAD' AND a.CodigoActividad=e.CodigoRelacion LEFT JOIN app.FaseProyecto f ON e.TipoRelacion='FASE' AND f.CodigoFase=e.CodigoRelacion WHERE COALESCE(t.CodigoProyecto,a.CodigoProyecto,f.CodigoProyecto)=@P AND e.Activo=1 ORDER BY e.FechaCompromiso,e.CodigoEntregable", new { P = N(p) });
    public async Task<EntregableProyecto?> EntregableAsync(string p, string e) =>
        (await db.ConsultarAsync<EntregableProyecto>($"SELECT {Entregables} FROM app.EntregableProyecto e LEFT JOIN app.TareaProyecto t ON t.CodigoTarea=e.CodigoTarea LEFT JOIN app.ActividadProyecto a ON e.TipoRelacion='ACTIVIDAD' AND a.CodigoActividad=e.CodigoRelacion LEFT JOIN app.FaseProyecto f ON e.TipoRelacion='FASE' AND f.CodigoFase=e.CodigoRelacion WHERE COALESCE(t.CodigoProyecto,a.CodigoProyecto,f.CodigoProyecto)=@P AND e.CodigoEntregable=@E", new { P = N(p), E = N(e) })).FirstOrDefault();

    public async Task CrearFaseAsync(FaseProyecto x) => await db.EjecutarAsync("INSERT app.FaseProyecto(CodigoFase,CodigoProyecto,Nombre,Descripcion,Orden,FechaInicio,FechaFin,Estado,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion) VALUES(@CodigoFase,@CodigoProyecto,@Nombre,@Descripcion,@Orden,@FechaInicio,@FechaFin,@Estado,@Activo,@CreadoPor,@FechaCreacion,@ModificadoPor,@FechaModificacion)", x);
    public async Task ActualizarFaseAsync(FaseProyecto x) => await db.EjecutarAsync("UPDATE app.FaseProyecto SET Nombre=@Nombre,Descripcion=@Descripcion,Orden=@Orden,FechaInicio=@FechaInicio,FechaFin=@FechaFin,Estado=@Estado,Activo=@Activo,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoFase=@CodigoFase AND CodigoProyecto=@CodigoProyecto", x);
    public async Task CrearActividadAsync(ActividadProyecto x)=>await db.EjecutarAsync("INSERT app.ActividadProyecto(CodigoActividad,CodigoFase,CodigoProyecto,Nombre,Descripcion,CodigoResponsable,Orden,FechaInicio,FechaFin,Estado,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion) VALUES(@CodigoActividad,@CodigoFase,@CodigoProyecto,@Nombre,@Descripcion,@CodigoResponsable,@Orden,@FechaInicio,@FechaFin,@Estado,@Activo,@CreadoPor,@FechaCreacion,@ModificadoPor,@FechaModificacion)",x);
    public async Task ActualizarActividadAsync(ActividadProyecto x)=>await db.EjecutarAsync("UPDATE app.ActividadProyecto SET CodigoFase=@CodigoFase,Nombre=@Nombre,Descripcion=@Descripcion,CodigoResponsable=@CodigoResponsable,Orden=@Orden,FechaInicio=@FechaInicio,FechaFin=@FechaFin,Estado=@Estado,Activo=@Activo,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoActividad=@CodigoActividad AND CodigoProyecto=@CodigoProyecto",x);
    public async Task CrearTareaAsync(TareaProyecto x) => await db.EjecutarAsync("INSERT app.TareaProyecto(CodigoTarea,CodigoActividad,CodigoFase,CodigoProyecto,Nombre,Descripcion,CodigoResponsable,FechaInicio,FechaFin,Estado,Peso,HorasPlanificadas,CostoHoraPlanificado,FechaCompletada,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion) VALUES(@CodigoTarea,@CodigoActividad,@CodigoFase,@CodigoProyecto,@Nombre,@Descripcion,@CodigoResponsable,@FechaInicio,@FechaFin,@Estado,@Peso,@HorasPlanificadas,@CostoHoraPlanificado,@FechaCompletada,@Activo,@CreadoPor,@FechaCreacion,@ModificadoPor,@FechaModificacion)", x);
    public async Task ActualizarTareaAsync(TareaProyecto x) => await db.EjecutarAsync("UPDATE app.TareaProyecto SET CodigoActividad=@CodigoActividad,CodigoFase=@CodigoFase,Nombre=@Nombre,Descripcion=@Descripcion,CodigoResponsable=@CodigoResponsable,FechaInicio=@FechaInicio,FechaFin=@FechaFin,Estado=@Estado,Peso=@Peso,HorasPlanificadas=@HorasPlanificadas,CostoHoraPlanificado=@CostoHoraPlanificado,FechaCompletada=@FechaCompletada,Activo=@Activo,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoTarea=@CodigoTarea AND CodigoProyecto=@CodigoProyecto", x);
    public Task ReemplazarDependenciasAsync(string p, string t, IReadOnlyCollection<DependenciaTarea> dependencias)
    {
        using var conexion = db.Abrir();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            db.Ejecutar("DELETE app.DependenciaTarea WHERE CodigoProyecto=@P AND CodigoTarea=@T", new { P = N(p), T = N(t) }, transaccion);
            foreach (var x in dependencias)
                db.Ejecutar("INSERT app.DependenciaTarea(CodigoProyecto,CodigoTarea,CodigoPredecesora,TipoDependencia,DesfaseDias,CreadoPor,FechaCreacion) VALUES(@CodigoProyecto,@CodigoTarea,@CodigoPredecesora,@TipoDependencia,@DesfaseDias,@CreadoPor,@FechaCreacion)", x, transaccion);
            transaccion.Commit();
            return Task.CompletedTask;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }
    public async Task CrearEntregableAsync(EntregableProyecto x) => await db.EjecutarAsync("INSERT app.EntregableProyecto(CodigoEntregable,CodigoTarea,TipoRelacion,CodigoRelacion,Nombre,Descripcion,FechaCompromiso,Estado,Evidencia,Observacion,FechaEntrega,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion) VALUES(@CodigoEntregable,@CodigoTarea,@TipoRelacion,@CodigoRelacion,@Nombre,@Descripcion,@FechaCompromiso,@Estado,@Evidencia,@Observacion,@FechaEntrega,@Activo,@CreadoPor,@FechaCreacion,@ModificadoPor,@FechaModificacion)", x);
    public async Task ActualizarEntregableAsync(EntregableProyecto x) => await db.EjecutarAsync("UPDATE app.EntregableProyecto SET CodigoTarea=@CodigoTarea,TipoRelacion=@TipoRelacion,CodigoRelacion=@CodigoRelacion,Nombre=@Nombre,Descripcion=@Descripcion,FechaCompromiso=@FechaCompromiso,Estado=@Estado,Evidencia=@Evidencia,Observacion=@Observacion,FechaEntrega=@FechaEntrega,Activo=@Activo,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoEntregable=@CodigoEntregable", x);
    private static string N(string? x) => (x ?? "").Trim().ToUpperInvariant();
}
