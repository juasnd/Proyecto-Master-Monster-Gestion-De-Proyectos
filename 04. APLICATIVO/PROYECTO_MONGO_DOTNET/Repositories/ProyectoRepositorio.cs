using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Repositories;

public interface IProyectoRepositorio
{
    Task<IReadOnlyCollection<Proyecto>> ListarAsync(bool incluirInactivos = false);
    Task<Proyecto?> ObtenerAsync(string codigo);
    Task<bool> ExisteCodigoAsync(string codigo);
    Task CrearAsync(Proyecto proyecto);
    Task<bool> ActualizarAsync(Proyecto proyecto);
}

public sealed class ProyectoRepositorio(SqlContexto db) : IProyectoRepositorio
{
    private const string Columnas = "Id,Codigo,Nombre,Descripcion,CodigoDepartamento,CodigoJefeProyecto,FechaInicio,FechaFinPlanificada,FechaFinReal,Estado,Prioridad,Avance,Presupuesto,Activo,Observacion,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion";
    public async Task<IReadOnlyCollection<Proyecto>> ListarAsync(bool incluirInactivos = false) =>
        await db.ConsultarAsync<Proyecto>($"SELECT {Columnas} FROM app.Proyecto {(incluirInactivos ? "" : "WHERE Activo=1")} ORDER BY Codigo");
    public async Task<Proyecto?> ObtenerAsync(string codigo) => (await db.ConsultarAsync<Proyecto>($"SELECT {Columnas} FROM app.Proyecto WHERE Codigo=@Codigo", new { Codigo=N(codigo) })).FirstOrDefault();
    public async Task<bool> ExisteCodigoAsync(string codigo) => await db.EscalarAsync<int>("SELECT COUNT(1) FROM app.Proyecto WHERE Codigo=@Codigo", new { Codigo=N(codigo) }) > 0;
    public async Task CrearAsync(Proyecto x) => await db.EjecutarAsync("INSERT app.Proyecto (Id,Codigo,Nombre,Descripcion,CodigoDepartamento,CodigoJefeProyecto,FechaInicio,FechaFinPlanificada,FechaFinReal,Estado,Prioridad,Avance,Presupuesto,Activo,Observacion,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion) VALUES (@Id,@Codigo,@Nombre,@Descripcion,@CodigoDepartamento,@CodigoJefeProyecto,@FechaInicio,@FechaFinPlanificada,@FechaFinReal,@Estado,@Prioridad,@Avance,@Presupuesto,@Activo,@Observacion,@CreadoPor,@FechaCreacion,@ModificadoPor,@FechaModificacion)", x);
    public async Task<bool> ActualizarAsync(Proyecto x) => await db.EjecutarAsync("UPDATE app.Proyecto SET Nombre=@Nombre,Descripcion=@Descripcion,CodigoDepartamento=@CodigoDepartamento,CodigoJefeProyecto=@CodigoJefeProyecto,FechaInicio=@FechaInicio,FechaFinPlanificada=@FechaFinPlanificada,FechaFinReal=@FechaFinReal,Estado=@Estado,Prioridad=@Prioridad,Avance=@Avance,Presupuesto=@Presupuesto,Activo=@Activo,Observacion=@Observacion,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE Codigo=@Codigo", x) > 0;
    private static string N(string? x) => (x ?? "").Trim().ToUpperInvariant();
}
