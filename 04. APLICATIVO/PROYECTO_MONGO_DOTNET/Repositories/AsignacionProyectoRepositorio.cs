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
    public async Task CrearAsync(AsignacionProyecto x)=>await db.EjecutarAsync("INSERT app.AsignacionProyecto (Id,CodigoAsignacion,CodigoProyecto,CodigoEmpleado,Rol,PorcentajeDedicacion,FechaInicio,FechaFin,Estado,Activo,Observacion,CostoHoraSnapshot,AsignadoPor,FechaAsignacion,ModificadoPor,FechaModificacion) VALUES (@Id,@CodigoAsignacion,@CodigoProyecto,@CodigoEmpleado,@Rol,@DisponibilidadAsignada,@FechaInicio,@FechaFin,@Estado,@Activo,@Observacion,@CostoHoraSnapshot,@AsignadoPor,@FechaAsignacion,@ModificadoPor,@FechaModificacion)",x);
    public async Task<bool> ActualizarAsync(AsignacionProyecto x)=>await db.EjecutarAsync("UPDATE app.AsignacionProyecto SET CodigoProyecto=@CodigoProyecto,CodigoEmpleado=@CodigoEmpleado,Rol=@Rol,PorcentajeDedicacion=@DisponibilidadAsignada,FechaInicio=@FechaInicio,FechaFin=@FechaFin,Estado=@Estado,Activo=@Activo,Observacion=@Observacion,CostoHoraSnapshot=@CostoHoraSnapshot,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoAsignacion=@CodigoAsignacion",x)>0;
    public async Task CerrarActivasAsync(string p,string u,DateTime f)=>await db.EjecutarAsync("UPDATE app.AsignacionProyecto SET Activo=0,Estado='CERRADA',ModificadoPor=@U,FechaModificacion=@F WHERE CodigoProyecto=@P AND Activo=1",new{P=N(p),U=u,F=f});
    private static string N(string? x)=>(x??"").Trim().ToUpperInvariant();
}
