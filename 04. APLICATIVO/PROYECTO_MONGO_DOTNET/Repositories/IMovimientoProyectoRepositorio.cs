using PROYECTO_MONGO_DOTNET.Models.Entidades;

namespace PROYECTO_MONGO_DOTNET.Repositories;

public interface IMovimientoProyectoRepositorio
{
    Task<List<MovimientoProyecto>> ListarAsync();
    Task<MovimientoProyecto?> ObtenerAsync(string codigo);
    Task CrearAsync(MovimientoProyecto movimiento);
    Task<bool> ActualizarAsync(MovimientoProyecto movimiento);
    Task<bool> RevisarPendienteAsync(MovimientoProyecto movimiento);
    Task<decimal> GastosAprobadosAsync(string proyecto);
}

public interface ICategoriaGastoRepositorio
{
    Task<List<CategoriaGasto>> ListarAsync();
    Task<CategoriaGasto?> ObtenerAsync(string codigo);
    Task CrearAsync(CategoriaGasto categoria);
    Task<bool> ActualizarAsync(CategoriaGasto categoria);
}
