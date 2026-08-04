using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Seguridad;
using PROYECTO_MONGO_DOTNET.Models.ViewModels;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Seguridad;

namespace PROYECTO_MONGO_DOTNET.Controllers.Web;

public sealed class SeguridadProcesosController : Controller
{
    private readonly SeguridadServicio _seguridad;
    private readonly SeguridadCrudServicio _crud;
    public SeguridadProcesosController(SeguridadServicio seguridad, SeguridadCrudServicio crud)
    {
        _seguridad = seguridad;
        _crud = crud;
    }

    [HttpGet("/AsignacionUsuarioPerfilController"), HttpGet("/asignarUsuariosPerfil.jsp"), PermisoRequerido("SEG_USUPER")]
    public IActionResult UsuariosPerfil(string? perfilCodigo = null, string? mensaje = null) =>
        View("~/Views/Seguridad/UsuariosPerfil.cshtml", _crud.PaginaUsuariosPerfil(perfilCodigo, mensaje));

    [HttpPost("/AsignacionUsuarioPerfilController"), ValidateAntiForgeryToken, PermisoRequerido("SEG_USUPER", PermisoAccion.Editar)]
    public IActionResult UsuariosPerfil(string perfilCodigo, string accion, string[]? usuariosDisponibles, string[]? usuariosAsignados)
    {
        try
        {
            var mensaje = _crud.ProcesarUsuariosPerfil(perfilCodigo, accion, usuariosDisponibles, usuariosAsignados, Usuario);
            return Redirect("/AsignacionUsuarioPerfilController?perfilCodigo=" + Uri.EscapeDataString(perfilCodigo) + "&mensaje=" + mensaje);
        }
        catch (ExcepcionNegocio ex)
        {
            return View("~/Views/Seguridad/UsuariosPerfil.cshtml", _crud.PaginaUsuariosPerfil(perfilCodigo, error: ex.Message));
        }
    }

    [HttpGet("/RestablecerClaveController"), HttpGet("/restablecerClave.jsp"), PermisoRequerido("SEG_RESTABLECER")]
    public IActionResult Restablecer(string? login = null, string? mensaje = null) => View("~/Views/Seguridad/RestablecerClave.cshtml", new RestablecerClavePaginaVm { Usuarios = _seguridad.ListarUsuarios(), LoginSeleccionado = login, Mensaje = mensaje });

    [HttpPost("/RestablecerClaveController"), ValidateAntiForgeryToken, PermisoRequerido("SEG_RESTABLECER", PermisoAccion.Editar)]
    public IActionResult Restablecer(RestablecerClaveDto dto)
    {
        try { _seguridad.Restablecer(dto, Usuario); return Redirect("/RestablecerClaveController?mensaje=restablecida"); }
        catch (ExcepcionNegocio ex) { return View("~/Views/Seguridad/RestablecerClave.cshtml", new RestablecerClavePaginaVm { Usuarios = _seguridad.ListarUsuarios(), LoginSeleccionado = dto.Login, Error = ex.Message }); }
    }

    [HttpPost("/cambiarClave.jsp"), ValidateAntiForgeryToken]
    public IActionResult Cambiar(CambiarClaveDto dto)
    {
        if (string.IsNullOrWhiteSpace(Usuario)) return Redirect("/index.jsp");
        try
        {
            _seguridad.Cambiar(dto, Usuario);
            HttpContext.Session.SetString("requiereCambioClave", "N");
            return Redirect("/pagPrincipal.jsp?mensaje=clave_actualizada");
        }
        catch (ExcepcionNegocio ex) { ViewBag.Error = ex.Message; return View("~/Views/Home/CambiarClave.cshtml", dto); }
    }
    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}
