using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;
using PROYECTO_MONGO_DOTNET.Models.ViewModels;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;

namespace PROYECTO_MONGO_DOTNET.Controllers.Web;

public sealed class MovimientosFinancierosController : Controller
{
    private readonly MovimientoProyectoServicio _servicio; private readonly IProyectoRepositorio _proyectos; private readonly CategoriaGastoRepositorio _categorias;
    public MovimientosFinancierosController(MovimientoProyectoServicio servicio, IProyectoRepositorio proyectos, CategoriaGastoRepositorio categorias) { _servicio = servicio; _proyectos = proyectos; _categorias = categorias; }
    [HttpGet("/Finanzas/Gastos"), HttpGet("/gastosProyectos.jsp"), PermisoRequerido("FIN_GASTOS")] public Task<IActionResult> Gastos(string? mensaje = null) => Vista(false, mensaje, null);
    [HttpGet("/Finanzas/AprobarGastos"), HttpGet("/aprobarGastos.jsp"), PermisoRequerido("FIN_APROBAR_GASTOS")] public Task<IActionResult> Aprobar(string? mensaje = null) => Vista(true, mensaje, null);
    [HttpPost("/Finanzas/Gastos"), ValidateAntiForgeryToken, PermisoRequerido("FIN_GASTOS", PermisoAccion.Crear)] public async Task<IActionResult> Guardar(GuardarMovimientoDto dto) { try { await _servicio.GuardarAsync(dto, Usuario); return Redirect("/Finanzas/Gastos?mensaje=guardado"); } catch (ExcepcionNegocio ex) { return await Vista(false, null, ex.Message); } }
    [HttpPost("/Finanzas/AprobarGastos/{codigo}/{estado}"), ValidateAntiForgeryToken, PermisoRequerido("FIN_APROBAR_GASTOS", PermisoAccion.Editar)] public async Task<IActionResult> Revisar(string codigo, string estado, RevisarMovimientoDto dto) { await _servicio.RevisarAsync(codigo, estado, dto.Observacion, Usuario); return Redirect("/Finanzas/AprobarGastos?mensaje=procesado"); }
    private async Task<IActionResult> Vista(bool aprobacion, string? mensaje, string? error) => View("~/Views/Finanzas/Movimientos.cshtml", new MovimientosPaginaVm { Movimientos = await _servicio.ListarAsync(), Proyectos = await _proyectos.ListarAsync(), Categorias = await _categorias.ListarAsync(), ModoAprobacion = aprobacion, Mensaje = mensaje, Error = error });
    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}

public sealed class ControlPresupuestarioController : Controller
{
    private readonly CostoProyectoServicio _servicio; public ControlPresupuestarioController(CostoProyectoServicio servicio) => _servicio = servicio;
    [HttpGet("/Finanzas/ControlPresupuestario"), HttpGet("/controlPresupuestario.jsp"), HttpGet("/costoLaboral.jsp"), PermisoRequerido("FIN_PRESUPUESTO")] public async Task<IActionResult> Index() => View("~/Views/Finanzas/ControlPresupuestario.cshtml", new ControlPresupuestarioPaginaVm { Proyectos = await _servicio.ListarAsync() });
}
