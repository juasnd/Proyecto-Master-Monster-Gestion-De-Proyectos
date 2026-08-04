using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;
using PROYECTO_MONGO_DOTNET.Models.ViewModels;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;

namespace PROYECTO_MONGO_DOTNET.Controllers.Web;

public sealed class CategoriasGastoController : Controller
{
    private readonly CatalogosFinancierosServicio _servicio; public CategoriasGastoController(CatalogosFinancierosServicio servicio) => _servicio = servicio;
    [HttpGet("/Finanzas/Categorias"), HttpGet("/categoriasGasto.jsp"), PermisoRequerido("FIN_CATEGORIAS")] public async Task<IActionResult> Index(string? mensaje = null) => View("~/Views/Finanzas/Categorias.cshtml", new CategoriasPaginaVm { Categorias = await _servicio.ListarCategoriasAsync(), Mensaje = mensaje });
    [HttpPost("/Finanzas/Categorias"), ValidateAntiForgeryToken, PermisoRequerido("FIN_CATEGORIAS", PermisoAccion.Crear)] public async Task<IActionResult> Guardar(GuardarCategoriaGastoDto dto) { try { await _servicio.GuardarCategoriaAsync(dto, Usuario); return Redirect("/Finanzas/Categorias?mensaje=guardada"); } catch (ExcepcionNegocio ex) { return View("~/Views/Finanzas/Categorias.cshtml", new CategoriasPaginaVm { Categorias = await _servicio.ListarCategoriasAsync(), Error = ex.Message }); } }
    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}

public sealed class ParametrosFinancierosController : Controller
{
    private readonly CatalogosFinancierosServicio _servicio; public ParametrosFinancierosController(CatalogosFinancierosServicio servicio) => _servicio = servicio;
    [HttpGet("/Finanzas/Parametros"), HttpGet("/parametrosFinancieros.jsp"), PermisoRequerido("FIN_PARAMETROS")] public async Task<IActionResult> Index(string? mensaje = null) => View("~/Views/Finanzas/Parametros.cshtml", new ParametrosPaginaVm { Parametros = await _servicio.ListarParametrosAsync(), Mensaje = mensaje });
    [HttpPost("/Finanzas/Parametros"), ValidateAntiForgeryToken, PermisoRequerido("FIN_PARAMETROS", PermisoAccion.Editar)] public async Task<IActionResult> Guardar(GuardarParametroFinancieroDto dto) { try { await _servicio.GuardarParametroAsync(dto, Usuario); return Redirect("/Finanzas/Parametros?mensaje=guardado"); } catch (ExcepcionNegocio ex) { return View("~/Views/Finanzas/Parametros.cshtml", new ParametrosPaginaVm { Parametros = await _servicio.ListarParametrosAsync(), Error = ex.Message }); } }
    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}
