using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;
namespace PROYECTO_MONGO_DOTNET.Repositories;
public interface ISeguimientoProyectoRepositorio { Task<IReadOnlyCollection<SeguimientoProyecto>> PorProyectoAsync(string codigoProyecto); Task CrearAsync(SeguimientoProyecto seguimiento); }
public sealed class SeguimientoProyectoRepositorio(SqlContexto db):ISeguimientoProyectoRepositorio
{
    private const string C="Id,CodigoSeguimiento,CodigoProyecto,AvanceAnterior,AvanceNuevo,EstadoAnterior,EstadoNuevo,Observacion,RegistradoPor,FechaRegistro";
    public async Task<IReadOnlyCollection<SeguimientoProyecto>> PorProyectoAsync(string p)=>await db.ConsultarAsync<SeguimientoProyecto>($"SELECT {C} FROM app.SeguimientoProyecto WHERE CodigoProyecto=@P ORDER BY FechaRegistro DESC",new{P=(p??"").Trim().ToUpperInvariant()});
    public async Task CrearAsync(SeguimientoProyecto x)=>await db.EjecutarAsync("INSERT app.SeguimientoProyecto (Id,CodigoSeguimiento,CodigoProyecto,AvanceAnterior,AvanceNuevo,EstadoAnterior,EstadoNuevo,Observacion,RegistradoPor,FechaRegistro) VALUES (@Id,@CodigoSeguimiento,@CodigoProyecto,@AvanceAnterior,@AvanceNuevo,@EstadoAnterior,@EstadoNuevo,@Observacion,@RegistradoPor,@FechaRegistro)",x);
}
