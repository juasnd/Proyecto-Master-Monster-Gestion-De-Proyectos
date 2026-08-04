using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;

namespace PROYECTO_MONGO_DOTNET.Controllers.Web;

[Route("Finanzas/Reportes")]
public sealed class ReportesFinanzasController : Controller
{
    private readonly ContratoRepositorio _contratos;
    private readonly MovimientoProyectoRepositorio _movimientos;
    private readonly CategoriaGastoRepositorio _categorias;
    private readonly ParametroFinancieroRepositorio _parametros;
    private readonly CostoProyectoServicio _costos;

    public ReportesFinanzasController(ContratoRepositorio contratos, MovimientoProyectoRepositorio movimientos,
        CategoriaGastoRepositorio categorias, ParametroFinancieroRepositorio parametros, CostoProyectoServicio costos)
    {
        _contratos = contratos; _movimientos = movimientos; _categorias = categorias; _parametros = parametros; _costos = costos;
    }

    [HttpGet("Contratos"), PermisoRequerido("RCON")]
    public async Task<IActionResult> Contratos(string? codigoEmpleado, string? estado, decimal? sueldoMinimo,
        decimal? sueldoMaximo, DateTime? fechaInicioDesde, DateTime? fechaInicioHasta, bool soloPorVencer = false)
    {
        IEnumerable<Contrato> q = await _contratos.ListarAsync();
        if (!string.IsNullOrWhiteSpace(codigoEmpleado)) q = q.Where(x => x.CodigoEmpleado.Equals(codigoEmpleado, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => x.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
        if (sueldoMinimo.HasValue) q = q.Where(x => x.SueldoBase >= sueldoMinimo.Value);
        if (sueldoMaximo.HasValue) q = q.Where(x => x.SueldoBase <= sueldoMaximo.Value);
        if (fechaInicioDesde.HasValue) q = q.Where(x => x.FechaInicio >= fechaInicioDesde.Value);
        if (fechaInicioHasta.HasValue) q = q.Where(x => x.FechaInicio <= fechaInicioHasta.Value);
        if (soloPorVencer) q = q.Where(x => x.FechaFin.HasValue && x.FechaFin.Value >= DateTime.UtcNow && x.FechaFin.Value <= DateTime.UtcNow.AddDays(30));
        return Json(ApiRespuesta<IReadOnlyCollection<Contrato>>.Correcto(q.ToList()));
    }

    [HttpGet("Gastos"), PermisoRequerido("RGAS")]
    public async Task<IActionResult> Gastos(string? codigoProyecto, string? codigoCategoria, string? estado,
        DateTime? fechaInicioDesde, DateTime? fechaInicioHasta, decimal? valorMinimo, decimal? valorMaximo)
    {
        IEnumerable<MovimientoProyecto> q = await _movimientos.ListarAsync();
        if (!string.IsNullOrWhiteSpace(codigoProyecto)) q = q.Where(x => x.CodigoProyecto.Equals(codigoProyecto, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(codigoCategoria)) q = q.Where(x => x.CodigoCategoria.Equals(codigoCategoria, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => x.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
        if (fechaInicioDesde.HasValue) q = q.Where(x => x.Fecha >= fechaInicioDesde.Value);
        if (fechaInicioHasta.HasValue) q = q.Where(x => x.Fecha <= fechaInicioHasta.Value);
        if (valorMinimo.HasValue) q = q.Where(x => x.Valor >= valorMinimo.Value);
        if (valorMaximo.HasValue) q = q.Where(x => x.Valor <= valorMaximo.Value);
        return Json(ApiRespuesta<IReadOnlyCollection<MovimientoProyecto>>.Correcto(q.ToList()));
    }

    [HttpGet("ControlPresupuestario"), PermisoRequerido("RPRE")]
    public async Task<IActionResult> Presupuesto(decimal? consumoMinimo, decimal? consumoMaximo, bool soloExcedidos = false, bool soloEnAlerta = false)
    {
        IEnumerable<CostoProyectoResumen> q = await _costos.ListarAsync();
        if (consumoMinimo.HasValue) q = q.Where(x => x.PorcentajeConsumo >= consumoMinimo.Value);
        if (consumoMaximo.HasValue) q = q.Where(x => x.PorcentajeConsumo <= consumoMaximo.Value);
        if (soloExcedidos) q = q.Where(x => x.Excedido);
        if (soloEnAlerta) q = q.Where(x => x.EnAlerta);
        return Json(ApiRespuesta<IReadOnlyCollection<CostoProyectoResumen>>.Correcto(q.ToList()));
    }

    [HttpGet("Categorias"), PermisoRequerido("RCAT")]
    public async Task<IActionResult> Categorias() => Json(ApiRespuesta<IReadOnlyCollection<CategoriaGasto>>.Correcto(await _categorias.ListarAsync()));
    [HttpGet("Parametros"), PermisoRequerido("RPAR")]
    public async Task<IActionResult> Parametros() => Json(ApiRespuesta<IReadOnlyCollection<ParametroFinanciero>>.Correcto(await _parametros.ListarAsync()));
}
