using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models.ViewModels;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Proyectos;

namespace PROYECTO_MONGO_DOTNET.Controllers.Web;

public sealed class ProyectosViewController : Controller
{
    private readonly DepartamentoDAO _departamentos;
    private readonly EmpleadoDAO _empleados;
    private readonly PermisoDAO _permisos;
    private readonly ProyectoServicio _proyectos;

    public ProyectosViewController(DepartamentoDAO departamentos, EmpleadoDAO empleados, PermisoDAO permisos, ProyectoServicio proyectos)
    {
        _departamentos = departamentos;
        _empleados = empleados;
        _permisos = permisos;
        _proyectos = proyectos;
    }

    [HttpGet("/Proyectos"), HttpGet("/proyectos.jsp"), PermisoRequerido("PRO_PROYECTOS")]
    public IActionResult Proyectos() => View("~/Views/Proyectos/Index.cshtml", Modelo());

    [HttpGet("/Proyectos/Planificacion"), HttpGet("/seguimientoProyectos.jsp"), PermisoRequerido("PRO_SEGUIMIENTO")]
    public IActionResult ListaPlanificacion() => View("~/Views/Proyectos/PlanificacionLista.cshtml", Modelo());

    [HttpGet("/Proyectos/{codigoProyecto}/Planificacion"), PermisoRequerido("PRO_SEGUIMIENTO")]
    public async Task<IActionResult> Planificacion(string codigoProyecto)
    {
        var proyecto = await _proyectos.ObtenerAsync(codigoProyecto);
        if (proyecto == null) return NotFound();
        var modelo = Modelo();
        modelo.CodigoProyecto = codigoProyecto.Trim().ToUpperInvariant();
        var jefe = modelo.JefesProyecto.FirstOrDefault(x => x.PeempCodigo.Equals(proyecto.CodigoJefeProyecto, StringComparison.OrdinalIgnoreCase));
        modelo.NombreJefeProyecto = jefe == null
            ? proyecto.CodigoJefeProyecto
            : $"{jefe.Persona.Nombres} {jefe.Persona.Apellidos}".Trim();
        return View("~/Views/Proyectos/Planificacion.cshtml", modelo);
    }

    [HttpGet("/Proyectos/Asignaciones"), HttpGet("/asignarEmpleados.jsp")]
    public IActionResult AsignacionesHeredadas() => Redirect("/Proyectos");

    [HttpGet("/Proyectos/Seguimiento")]
    public IActionResult SeguimientoHeredado() => Redirect("/Proyectos/Planificacion");

    [HttpGet("/Proyectos/Cierre"), HttpGet("/cierreProyectos.jsp")]
    public IActionResult CierreHeredado() => Redirect("/Proyectos");

    private ProyectosPaginaVm Modelo()
    {
        var perfil = HttpContext.Session.GetString("perfilCodigo") ?? "";
        return new ProyectosPaginaVm
        {
            Departamentos = _departamentos.Listar(),
            Empleados = _empleados.ListarAsignablesProyecto(),
            JefesProyecto = _empleados.ListarJefesProyecto(),
            PuedeReportar = _permisos.TienePermiso(perfil, "RPRO"),
            PuedeCrear = _permisos.TienePermiso(perfil, "PRO_PROYECTOS", PermisoAccion.Crear),
            PuedeEditar = _permisos.TienePermiso(perfil, "PRO_PROYECTOS", PermisoAccion.Editar),
            PuedeInactivar = _permisos.TienePermiso(perfil, "PRO_PROYECTOS", PermisoAccion.Eliminar),
            PuedeGestionarEquipo = _permisos.TienePermiso(perfil, "PRO_ASIGNAR"),
            PuedePlanificar = _permisos.TienePermiso(perfil, "PRO_SEGUIMIENTO"),
            PuedeCambiarEstado = _permisos.TienePermiso(perfil, "PRO_CIERRE", PermisoAccion.Editar),
            PuedeFinalizar = _permisos.TienePermiso(perfil, "PRO_CIERRE", PermisoAccion.Editar)
        };
    }
}
