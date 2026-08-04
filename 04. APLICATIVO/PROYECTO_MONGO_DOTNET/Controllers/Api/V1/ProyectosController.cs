using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Proyectos;

namespace PROYECTO_MONGO_DOTNET.Controllers.Api.V1;

[ApiController, Route("api/v1/proyectos")]
public sealed class ProyectosController : ControllerBase
{
    private readonly ProyectoServicio _servicio;
    public ProyectosController(ProyectoServicio servicio) => _servicio = servicio;

    [HttpGet, PermisoRequerido("PRO_PROYECTOS")]
    public async Task<ActionResult<ApiRespuesta<IReadOnlyCollection<ProyectoListadoVm>>>> Listar([FromQuery] bool incluirInactivos = false) =>
        Ok(ApiRespuesta<IReadOnlyCollection<ProyectoListadoVm>>.Correcto(await _servicio.ListarVistaAsync(incluirInactivos)));

    [HttpGet("{codigo}"), PermisoRequerido("PRO_PROYECTOS")]
    public async Task<ActionResult<ApiRespuesta<Proyecto>>> Obtener(string codigo)
    {
        var proyecto = await _servicio.ObtenerAsync(codigo);
        return proyecto == null ? NotFound(ApiRespuesta<Proyecto>.Fallo("El proyecto no existe.")) : Ok(ApiRespuesta<Proyecto>.Correcto(proyecto));
    }

    [HttpPost, ValidateAntiForgeryToken, PermisoRequerido("PRO_PROYECTOS", PermisoAccion.Crear)]
    public async Task<ActionResult<ApiRespuesta<Proyecto>>> Crear(GuardarProyectoDto dto)
    {
        var p = await _servicio.CrearAsync(dto, UsuarioActual);
        return CreatedAtAction(nameof(Obtener), new { codigo = p.Codigo }, ApiRespuesta<Proyecto>.Correcto(p, "Proyecto creado correctamente."));
    }

    [HttpPut("{codigo}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_PROYECTOS", PermisoAccion.Editar)]
    public async Task<ActionResult<ApiRespuesta<Proyecto>>> Actualizar(string codigo, GuardarProyectoDto dto) =>
        Ok(ApiRespuesta<Proyecto>.Correcto(await _servicio.ActualizarAsync(codigo, dto, UsuarioActual), "Proyecto actualizado correctamente."));

    [HttpPatch("{codigo}/activo"), ValidateAntiForgeryToken, PermisoRequerido("PRO_PROYECTOS", PermisoAccion.Eliminar)]
    public async Task<IActionResult> CambiarActivo(string codigo, CambiarActivoProyectoDto dto) =>
        Ok(ApiRespuesta<Proyecto>.Correcto(await _servicio.CambiarActivoAsync(codigo, dto, UsuarioActual), dto.Activo ? "Proyecto reactivado correctamente." : "Proyecto inactivado correctamente."));

    [HttpGet("reporte"), PermisoRequerido("RPRO")]
    public async Task<ActionResult<ApiRespuesta<ResultadoPaginado<ReporteProyectoFila>>>> Reporte([FromQuery] FiltroProyectosDto filtros) =>
        Ok(ApiRespuesta<ResultadoPaginado<ReporteProyectoFila>>.Correcto(await _servicio.ReporteAsync(filtros), "Reporte generado correctamente."));

    private string UsuarioActual => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}
