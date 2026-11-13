using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Finanzas;

public sealed class MovimientoProyectoServicio
{
    private readonly IMovimientoProyectoRepositorio _movimientos;
    private readonly ICategoriaGastoRepositorio _categorias;
    private readonly IProyectoRepositorio _proyectos;
    private readonly CodigoDAO _codigos;
    public MovimientoProyectoServicio(IMovimientoProyectoRepositorio movimientos, ICategoriaGastoRepositorio categorias, IProyectoRepositorio proyectos, CodigoDAO codigos) { _movimientos = movimientos; _categorias = categorias; _proyectos = proyectos; _codigos = codigos; }
    public Task<List<MovimientoProyecto>> ListarAsync() => _movimientos.ListarAsync();

    public async Task<MovimientoProyecto> GuardarAsync(GuardarMovimientoDto dto, string usuario)
    {
        ReglasFinanzas.ValidarMovimiento(dto);
        if (await _proyectos.ObtenerAsync(dto.CodigoProyecto) == null) throw new ExcepcionNegocio("El proyecto no existe.");
        var categoria = await _categorias.ObtenerAsync(dto.CodigoCategoria);
        if (categoria == null || !categoria.Activo) throw new ExcepcionNegocio("La categoría no existe o está inactiva.");
        var codigo = N(dto.CodigoMovimiento);
        var existente = string.IsNullOrWhiteSpace(codigo) ? null : await _movimientos.ObtenerAsync(codigo);
        if (!string.IsNullOrWhiteSpace(codigo) && existente == null) throw ExcepcionNegocio.NoEncontrado("El movimiento no existe.");
        if (existente != null && existente.Estado != "PENDIENTE") throw ExcepcionNegocio.Conflicto("Solo se pueden editar movimientos pendientes.");
        var x = existente ?? new MovimientoProyecto { CodigoMovimiento = _codigos.GenerarCodigo("fin_movimiento_proyecto", "CODIGOMOVIMIENTO"), RegistradoPor = usuario, FechaRegistro = DateTime.UtcNow };
        x.CodigoProyecto = N(dto.CodigoProyecto); x.Tipo = N(dto.Tipo); x.CodigoCategoria = categoria.CodigoCategoria;
        x.Concepto = dto.Concepto.Trim(); x.Valor = dto.Valor; x.Fecha = Utc(dto.Fecha.Date); x.Observacion = (dto.Observacion ?? "").Trim();
        if (existente == null) await _movimientos.CrearAsync(x); else { x.ModificadoPor = usuario; x.FechaModificacion = DateTime.UtcNow; if (!await _movimientos.ActualizarAsync(x)) throw ExcepcionNegocio.Conflicto("El movimiento ya fue revisado. Recargue los datos."); }
        return x;
    }

    public async Task<MovimientoProyecto> RevisarAsync(string codigo, string estado, string observacion, string usuario)
    {
        if (observacion?.Length > 500) throw new ExcepcionNegocio("La observación no debe superar 500 caracteres.");
        var x = await _movimientos.ObtenerAsync(codigo) ?? throw ExcepcionNegocio.NoEncontrado("El movimiento no existe.");
        if (x.Estado != "PENDIENTE") throw ExcepcionNegocio.Conflicto("El movimiento ya fue revisado.");
        estado = N(estado);
        if (estado is not ("APROBADO" or "RECHAZADO" or "ANULADO")) throw new ExcepcionNegocio("El estado de revisión no es válido.");
        x.Estado = estado; x.AprobadoPor = usuario; x.FechaAprobacion = DateTime.UtcNow; x.Observacion = (observacion ?? "").Trim();
        if (!await _movimientos.RevisarPendienteAsync(x)) throw ExcepcionNegocio.Conflicto("El movimiento ya fue revisado por otra solicitud.");
        return x;
    }
    private static DateTime Utc(DateTime x) => DateTime.SpecifyKind(x, DateTimeKind.Utc);
    private static string N(string? x) => (x ?? "").Trim().ToUpperInvariant();
}
