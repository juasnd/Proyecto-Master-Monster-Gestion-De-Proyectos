using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;
using PROYECTO_MONGO_DOTNET.Models.ViewModels;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;

namespace PROYECTO_MONGO_DOTNET.Controllers.Web;

[Route("Finanzas/Contratos"), PermisoRequerido("FIN_CONTRATOS")]
public sealed class ContratosController : Controller
{
    private readonly ContratoServicio _servicio; private readonly EmpleadoDAO _empleados;
    public ContratosController(ContratoServicio servicio, EmpleadoDAO empleados) { _servicio = servicio; _empleados = empleados; }
    [HttpGet, HttpGet("/contratos.jsp")] public async Task<IActionResult> Index(string? mensaje = null) => View("~/Views/Finanzas/Contratos.cshtml", new ContratosPaginaVm { Contratos = await _servicio.ListarAsync(), Empleados = _empleados.Listar(), Mensaje = mensaje });
    [HttpPost, ValidateAntiForgeryToken, PermisoRequerido("FIN_CONTRATOS", PermisoAccion.Crear)]
    public async Task<IActionResult> Guardar(GuardarContratoDto dto)
    {
        try { await _servicio.GuardarAsync(dto, Usuario); return Redirect("/Finanzas/Contratos?mensaje=guardado"); }
        catch (ExcepcionNegocio ex) { return View("~/Views/Finanzas/Contratos.cshtml", new ContratosPaginaVm { Contratos = await _servicio.ListarAsync(), Empleados = _empleados.Listar(), Error = ex.Message }); }
    }
    [HttpPost("inactivar"), ValidateAntiForgeryToken, PermisoRequerido("FIN_CONTRATOS", PermisoAccion.Eliminar)] public async Task<IActionResult> Inactivar(string codigo) { await _servicio.InactivarAsync(codigo, Usuario); return Redirect("/Finanzas/Contratos?mensaje=inactivado"); }
    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}
