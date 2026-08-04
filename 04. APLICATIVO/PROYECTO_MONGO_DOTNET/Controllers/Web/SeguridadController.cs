using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Seguridad;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Seguridad;

namespace PROYECTO_MONGO_DOTNET.Controllers;

public sealed class SeguridadController : AppController
{
    private readonly SeguridadCrudServicio _seguridad;
    public SeguridadController(SeguridadCrudServicio seguridad) => _seguridad = seguridad;

    [HttpGet("/UsuarioController")]
    [HttpGet("/usuarios.jsp")]
    [PermisoRequerido("SEG_USU")]
    public IActionResult Usuarios(string? mensaje = null) =>
        View("~/Views/Seguridad/Usuarios.cshtml", _seguridad.PaginaUsuarios(mensaje));

    [HttpPost("/UsuarioController/crear")]
    [ValidateAntiForgeryToken]
    [PermisoRequerido("SEG_USU", PermisoAccion.Crear)]
    public IActionResult CrearUsuario(CrearUsuarioDto dto)
    {
        if (!ModelState.IsValid) return VistaUsuarios(error: PrimerError());
        try
        {
            _seguridad.CrearUsuario(dto, UsuarioSesion);
            return Redirect("/UsuarioController?mensaje=creado");
        }
        catch (ExcepcionNegocio ex) { return VistaUsuarios(error: ex.Message); }
    }

    [HttpPost("/UsuarioController/perfil")]
    [ValidateAntiForgeryToken]
    [PermisoRequerido("SEG_USU", PermisoAccion.Editar)]
    public IActionResult AsignarPerfil(AsignarPerfilUsuarioDto dto)
    {
        if (!ModelState.IsValid) return VistaUsuarios(error: PrimerError());
        try
        {
            _seguridad.AsignarPerfil(dto, UsuarioSesion);
            return Redirect("/UsuarioController?mensaje=perfil");
        }
        catch (ExcepcionNegocio ex) { return VistaUsuarios(error: ex.Message); }
    }

    [HttpPost("/UsuarioController/estado")]
    [ValidateAntiForgeryToken]
    [PermisoRequerido("SEG_USU", PermisoAccion.Eliminar)]
    public IActionResult CambiarEstadoUsuario(CambiarEstadoUsuarioDto dto)
    {
        if (!ModelState.IsValid) return VistaUsuarios(error: PrimerError());
        try
        {
            _seguridad.CambiarEstadoUsuario(dto, UsuarioSesion);
            return Redirect("/UsuarioController?mensaje=estado");
        }
        catch (ExcepcionNegocio ex) { return VistaUsuarios(error: ex.Message); }
    }

    [HttpGet("/PerfilController")]
    [HttpGet("/perfiles.jsp")]
    [PermisoRequerido("SEG_PER")]
    public IActionResult Perfiles(string accion = "listar", string? codigo = null, string? mensaje = null)
    {
        var modo = accion is "nuevo" or "editar" ? accion : "listar";
        var vm = _seguridad.PaginaPerfiles(modo, codigo, mensaje);
        if (modo == "editar" && vm.PerfilEditar == null) vm.Error = "No se encontró el perfil solicitado.";
        return View("~/Views/Seguridad/Perfiles.cshtml", vm);
    }

    [HttpPost("/PerfilController/crear")]
    [ValidateAntiForgeryToken]
    [PermisoRequerido("SEG_PER", PermisoAccion.Crear)]
    public IActionResult CrearPerfil(GuardarPerfilDto dto) => ProcesarPerfil(dto, false);

    [HttpPost("/PerfilController/actualizar")]
    [ValidateAntiForgeryToken]
    [PermisoRequerido("SEG_PER", PermisoAccion.Editar)]
    public IActionResult ActualizarPerfil(GuardarPerfilDto dto) => ProcesarPerfil(dto, true);

    private IActionResult ProcesarPerfil(GuardarPerfilDto dto, bool editar)
    {
        if (!ModelState.IsValid) return VistaPerfiles(editar ? "editar" : "nuevo", dto.Codigo, PrimerError());
        try
        {
            _seguridad.GuardarPerfil(dto, editar, UsuarioSesion);
            return Redirect("/PerfilController?mensaje=" + (editar ? "actualizado" : "creado"));
        }
        catch (ExcepcionNegocio ex) { return VistaPerfiles(editar ? "editar" : "nuevo", dto.Codigo, ex.Message); }
    }

    [HttpPost("/PerfilController/estado")]
    [ValidateAntiForgeryToken]
    [PermisoRequerido("SEG_PER", PermisoAccion.Eliminar)]
    public IActionResult CambiarEstadoPerfil(CambiarEstadoPerfilDto dto)
    {
        if (!ModelState.IsValid) return VistaPerfiles(error: PrimerError());
        try
        {
            _seguridad.CambiarEstadoPerfil(dto, UsuarioSesion);
            return Redirect("/PerfilController?mensaje=estado");
        }
        catch (ExcepcionNegocio ex) { return VistaPerfiles(error: ex.Message); }
    }

    [HttpGet("/PermisoController")]
    [HttpGet("/permisos.jsp")]
    [PermisoRequerido("SEG_OPC")]
    public IActionResult Permisos(string? perfil = null, string? perfilCodigo = null, string? mensaje = null) =>
        View("~/Views/Seguridad/Permisos.cshtml", _seguridad.PaginaPermisos(perfilCodigo ?? perfil, mensaje));

    [HttpPost("/PermisoController")]
    [ValidateAntiForgeryToken]
    [PermisoRequerido("SEG_OPC", PermisoAccion.Editar)]
    public IActionResult GuardarPermisos(string PerfilCodigo, string[]? opciones)
    {
        try
        {
            _seguridad.GuardarOpciones(PerfilCodigo, opciones, UsuarioSesion);
            return Redirect("/PermisoController?perfilCodigo=" + Uri.EscapeDataString(PerfilCodigo) + "&mensaje=guardado");
        }
        catch (ExcepcionNegocio ex) { return VistaPermisos(PerfilCodigo, ex.Message); }
    }

    private IActionResult VistaUsuarios(string? mensaje = null, string? error = null) =>
        View("~/Views/Seguridad/Usuarios.cshtml", _seguridad.PaginaUsuarios(mensaje, error));

    private IActionResult VistaPerfiles(string modo = "listar", string? codigo = null, string? error = null) =>
        View("~/Views/Seguridad/Perfiles.cshtml", _seguridad.PaginaPerfiles(modo, codigo, error: error));

    private IActionResult VistaPermisos(string? perfil, string? error) =>
        View("~/Views/Seguridad/Permisos.cshtml", _seguridad.PaginaPermisos(perfil, error: error));

    private string PrimerError() => ModelState.Values.SelectMany(x => x.Errors)
        .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Revise los datos ingresados." : x.ErrorMessage)
        .FirstOrDefault() ?? "Revise los datos ingresados.";
}
