using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Controllers;

[Route("ReporteController")]
public class ReporteController : AppController
{
    private readonly ReporteHistorialDAO _reportes;
    private readonly PermisoDAO _permisos;

    public ReporteController(ReporteHistorialDAO reportes, PermisoDAO permisos)
    {
        _reportes = reportes;
        _permisos = permisos;
    }

    [HttpGet]
    [HttpGet("/reportes.jsp")]
    public IActionResult Index(string accion = "listar", string? codigo = null, string? mensaje = null)
    {
        if (!SesionValida) return LoginRedirect();
        var vm = new ReportesVm { Historial = _reportes.Listar(), Mensaje = mensaje };
        if (accion == "detalle" && codigo != null) vm.Detalle = _reportes.Buscar(codigo);
        return View("~/Views/Reportes/Index.cshtml", vm);
    }

    [HttpPost]
    public IActionResult Post()
    {
        if (!SesionValida) return Unauthorized(new { ok = false, mensaje = "Sesion no valida." });
        var accion = Valor(Request.Form, "accion");
        if (accion == "registrar")
        {
            var codigoReporte = Valor(Request.Form, "codigoReporte").ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigoReporte) || !_permisos.TienePermiso(PerfilSesion, codigoReporte))
                return StatusCode(StatusCodes.Status403Forbidden, new { ok = false, mensaje = "No tiene permiso para registrar este reporte." });

            _reportes.Registrar(new ReporteHistorial
            {
                CodigoReporte = codigoReporte,
                Modulo = Valor(Request.Form, "modulo"),
                TipoReporte = Valor(Request.Form, "tipoReporte"),
                Formato = Valor(Request.Form, "formato"),
                Filtros = Valor(Request.Form, "filtros"),
                TotalRegistros = int.TryParse(Request.Form["totalRegistros"], out var total) ? total : 0,
                Usuario = UsuarioSesion,
                Perfil = PerfilSesion
            });
            return Json(new { ok = true, mensaje = "Historial registrado." });
        }
        if (accion == "eliminar")
        {
            var ok = _reportes.Eliminar(Valor(Request.Form, "codigo"));
            return Redirect("/ReporteController?mensaje=" + (ok ? "eliminado" : "no_eliminado"));
        }
        return Redirect("/ReporteController");
    }
}
