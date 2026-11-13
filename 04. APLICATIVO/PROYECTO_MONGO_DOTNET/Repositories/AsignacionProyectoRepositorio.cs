using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Repositories;

public interface IAsignacionProyectoRepositorio
{
    Task<IReadOnlyCollection<AsignacionProyecto>> PorProyectoAsync(string codigoProyecto, bool incluirInactivas = true);
    Task<IReadOnlyCollection<AsignacionProyecto>> ListarAsync();
    Task<AsignacionProyecto?> ObtenerActivaAsync(string proyecto, string empleado);
    Task<decimal> DedicacionSolapadaAsync(string empleado, DateTime inicio, DateTime fin, string? excluirCodigo = null);
    Task CrearAsync(AsignacionProyecto asignacion);
    Task<bool> ActualizarAsync(AsignacionProyecto asignacion);
    Task CerrarActivasAsync(string codigoProyecto, string usuario, DateTime fechaUtc);
}

public sealed class AsignacionProyectoRepositorio(SqlContexto db) : IAsignacionProyectoRepositorio
{
    private const string C = "Id,CodigoAsignacion,CodigoProyecto,CodigoEmpleado,Rol,PorcentajeDedicacion DisponibilidadAsignada,FechaInicio,FechaFin,Estado,Activo,Observacion,CostoHoraSnapshot,AsignadoPor,FechaAsignacion,ModificadoPor,FechaModificacion";
    public async Task<IReadOnlyCollection<AsignacionProyecto>> PorProyectoAsync(string codigo, bool inactivas=true) => await db.ConsultarAsync<AsignacionProyecto>($"SELECT {C} FROM app.AsignacionProyecto WHERE CodigoProyecto=@Codigo {(inactivas?"":"AND Activo=1")} ORDER BY CodigoEmpleado",new{Codigo=N(codigo)});
    public async Task<IReadOnlyCollection<AsignacionProyecto>> ListarAsync()=>await db.ConsultarAsync<AsignacionProyecto>($"SELECT {C} FROM app.AsignacionProyecto");
    public async Task<AsignacionProyecto?> ObtenerActivaAsync(string p,string e)=>(await db.ConsultarAsync<AsignacionProyecto>($"SELECT {C} FROM app.AsignacionProyecto WHERE CodigoProyecto=@P AND CodigoEmpleado=@E AND Activo=1",new{P=N(p),E=N(e)})).FirstOrDefault();
    public async Task<decimal> DedicacionSolapadaAsync(string e,DateTime inicio,DateTime fin,string? excluirCodigo=null)=>await db.EscalarAsync<decimal>("SELECT COALESCE(SUM(a.PorcentajeDedicacion),0) FROM app.AsignacionProyecto a INNER JOIN app.Proyecto p ON p.Codigo=a.CodigoProyecto WHERE a.CodigoEmpleado=@E AND a.Activo=1 AND p.Activo=1 AND p.Estado NOT IN('FINALIZADO','CANCELADO') AND a.FechaInicio<=@Fin AND a.FechaFin>=@Inicio AND (@Excluir='' OR a.CodigoAsignacion<>@Excluir)",new{E=N(e),Inicio=inicio,Fin=fin,Excluir=N(excluirCodigo)});
    public async Task CrearAsync(AsignacionProyecto x) => await GuardarAsync(x, "INSERT app.AsignacionProyecto (Id,CodigoAsignacion,CodigoProyecto,CodigoEmpleado,Rol,PorcentajeDedicacion,FechaInicio,FechaFin,Estado,Activo,Observacion,CostoHoraSnapshot,AsignadoPor,FechaAsignacion,ModificadoPor,FechaModificacion) VALUES (@Id,@CodigoAsignacion,@CodigoProyecto,@CodigoEmpleado,@Rol,@DisponibilidadAsignada,@FechaInicio,@FechaFin,@Estado,@Activo,@Observacion,@CostoHoraSnapshot,@AsignadoPor,@FechaAsignacion,@ModificadoPor,@FechaModificacion)");
    public Task<bool> ActualizarAsync(AsignacionProyecto x) => GuardarAsync(x, "UPDATE app.AsignacionProyecto SET CodigoProyecto=@CodigoProyecto,CodigoEmpleado=@CodigoEmpleado,Rol=@Rol,PorcentajeDedicacion=@DisponibilidadAsignada,FechaInicio=@FechaInicio,FechaFin=@FechaFin,Estado=@Estado,Activo=@Activo,Observacion=@Observacion,CostoHoraSnapshot=@CostoHoraSnapshot,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoAsignacion=@CodigoAsignacion AND Activo=1");
    public async Task CerrarActivasAsync(string p,string u,DateTime f)=>await db.EjecutarAsync("UPDATE app.AsignacionProyecto SET Activo=0,Estado='CERRADA',ModificadoPor=@U,FechaModificacion=@F WHERE CodigoProyecto=@P AND Activo=1",new{P=N(p),U=u,F=f});
    private async Task<bool> GuardarAsync(AsignacionProyecto asignacion, string sql)
    {
        using var conexion = db.Abrir();
        using var transaccion = conexion.BeginTransaction(System.Data.IsolationLevel.Serializable);
        // Altas, ediciones y retiros individuales comparten el bloqueo del empleado.
        await db.EscalarAsync<int>("SELECT COUNT(1) FROM app.Empleado WITH (UPDLOCK,HOLDLOCK) WHERE Codigo=@CodigoEmpleado", asignacion, transaccion: transaccion);
        if (asignacion.Activo)
        {
            var abierto = await db.EscalarAsync<int>("SELECT COUNT(1) FROM app.Proyecto WITH (UPDLOCK,HOLDLOCK) WHERE Codigo=@CodigoProyecto AND Activo=1 AND Estado NOT IN ('FINALIZADO','CANCELADO') AND FechaInicio<=@FechaInicio AND FechaFinPlanificada>=@FechaFin", asignacion, transaccion: transaccion);
            if (abierto != 1) throw ExcepcionNegocio.Conflicto("El proyecto ya no admite esta asignación o sus fechas cambiaron.");
            var duplicadas = await db.EscalarAsync<int>("SELECT COUNT(1) FROM app.AsignacionProyecto WHERE CodigoProyecto=@CodigoProyecto AND CodigoEmpleado=@CodigoEmpleado AND Activo=1 AND CodigoAsignacion<>@CodigoAsignacion", asignacion, transaccion: transaccion);
            if (duplicadas > 0) throw ExcepcionNegocio.Conflicto("El empleado ya tiene una asignación activa en el proyecto.");
            var usada = await db.EscalarAsync<decimal>("SELECT COALESCE(SUM(a.PorcentajeDedicacion),0) FROM app.AsignacionProyecto a JOIN app.Proyecto p ON p.Codigo=a.CodigoProyecto WHERE a.CodigoEmpleado=@CodigoEmpleado AND a.Activo=1 AND p.Activo=1 AND p.Estado NOT IN ('FINALIZADO','CANCELADO') AND a.FechaInicio<=@FechaFin AND a.FechaFin>=@FechaInicio AND a.CodigoAsignacion<>@CodigoAsignacion", asignacion, transaccion: transaccion);
            if (asignacion.DisponibilidadAsignada is < 1 or > 100 || usada + asignacion.DisponibilidadAsignada > 100)
                throw ExcepcionNegocio.Conflicto("La disponibilidad del empleado cambió y supera el 100 %. Recargue los datos.");
        }
        var filas = await db.EjecutarAsync(sql, asignacion, transaccion: transaccion);
        if (filas != 1) throw ExcepcionNegocio.Conflicto("La asignación cambió mientras se procesaba la solicitud. Recargue los datos.");
        transaccion.Commit();
        return filas > 0;
    }

    private static string N(string? x)=>(x??"").Trim().ToUpperInvariant();
}
