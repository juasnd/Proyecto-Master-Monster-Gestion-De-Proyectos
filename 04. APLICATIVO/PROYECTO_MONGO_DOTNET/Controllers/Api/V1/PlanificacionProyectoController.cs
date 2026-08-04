using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Proyectos;

namespace PROYECTO_MONGO_DOTNET.Controllers.Api.V1;

[ApiController, Route("api/v1/proyectos/{codigoProyecto}/planificacion")]
public sealed class PlanificacionProyectoController(PlanificacionProyectoServicio servicio) : ControllerBase
{
    [HttpGet, PermisoRequerido("PRO_SEGUIMIENTO")]
    public async Task<IActionResult> Obtener(string codigoProyecto) => Ok(ApiRespuesta<PlanificacionProyectoVm>.Correcto(await servicio.ObtenerAsync(codigoProyecto)));

    [HttpPost("fases"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Crear)]
    public async Task<IActionResult> CrearFase(string codigoProyecto, GuardarFaseProyectoDto dto) => StatusCode(201, ApiRespuesta<FaseProyecto>.Correcto(await servicio.CrearFaseAsync(codigoProyecto, dto, Usuario), "Fase creada correctamente."));
    [HttpPut("fases/{codigoFase}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Editar)]
    public async Task<IActionResult> ActualizarFase(string codigoProyecto, string codigoFase, GuardarFaseProyectoDto dto) => Ok(ApiRespuesta<FaseProyecto>.Correcto(await servicio.ActualizarFaseAsync(codigoProyecto, codigoFase, dto, Usuario), "Fase actualizada correctamente."));
    [HttpDelete("fases/{codigoFase}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Eliminar)]
    public async Task<IActionResult> EliminarFase(string codigoProyecto, string codigoFase) { await servicio.EliminarFaseAsync(codigoProyecto, codigoFase, Usuario); return NoContent(); }

    [HttpPost("fases/{codigoFase}/actividades"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Crear)]
    public async Task<IActionResult> CrearActividad(string codigoProyecto,string codigoFase,GuardarActividadProyectoDto dto)=>StatusCode(201,ApiRespuesta<ActividadProyecto>.Correcto(await servicio.CrearActividadAsync(codigoProyecto,codigoFase,dto,Usuario),"Actividad creada correctamente."));
    [HttpPut("actividades/{codigoActividad}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Editar)]
    public async Task<IActionResult> ActualizarActividad(string codigoProyecto,string codigoActividad,GuardarActividadProyectoDto dto)=>Ok(ApiRespuesta<ActividadProyecto>.Correcto(await servicio.ActualizarActividadAsync(codigoProyecto,codigoActividad,dto,Usuario),"Actividad actualizada correctamente."));
    [HttpDelete("actividades/{codigoActividad}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Eliminar)]
    public async Task<IActionResult> EliminarActividad(string codigoProyecto,string codigoActividad){await servicio.EliminarActividadAsync(codigoProyecto,codigoActividad,Usuario);return NoContent();}

    [HttpPost("actividades/{codigoActividad}/tareas"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Crear)]
    public async Task<IActionResult> CrearTarea(string codigoProyecto, string codigoActividad, GuardarTareaProyectoDto dto) => StatusCode(201, ApiRespuesta<TareaProyecto>.Correcto(await servicio.CrearTareaAsync(codigoProyecto, codigoActividad, dto, Usuario), "Tarea creada correctamente."));
    [HttpPut("tareas/{codigoTarea}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Editar)]
    public async Task<IActionResult> ActualizarTarea(string codigoProyecto, string codigoTarea, GuardarTareaProyectoDto dto) => Ok(ApiRespuesta<TareaProyecto>.Correcto(await servicio.ActualizarTareaAsync(codigoProyecto, codigoTarea, dto, Usuario), "Tarea actualizada correctamente."));
    [HttpPatch("tareas/{codigoTarea}/estado"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Editar)]
    public async Task<IActionResult> EstadoTarea(string codigoProyecto, string codigoTarea, CambiarEstadoTareaDto dto) => Ok(ApiRespuesta<TareaProyecto>.Correcto(await servicio.CambiarEstadoTareaAsync(codigoProyecto, codigoTarea, dto.Estado, Usuario), "Estado de la tarea actualizado."));
    [HttpDelete("tareas/{codigoTarea}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Eliminar)]
    public async Task<IActionResult> EliminarTarea(string codigoProyecto, string codigoTarea) { await servicio.EliminarTareaAsync(codigoProyecto, codigoTarea, Usuario); return NoContent(); }

    [HttpPost("tareas/{codigoTarea}/entregables"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Crear)]
    public async Task<IActionResult> CrearEntregable(string codigoProyecto, string codigoTarea, GuardarEntregableProyectoDto dto) => StatusCode(201, ApiRespuesta<EntregableProyecto>.Correcto(await servicio.CrearEntregableAsync(codigoProyecto, codigoTarea, dto, Usuario), "Entregable creado correctamente."));
    [HttpPost("entregables"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Crear)]
    public async Task<IActionResult> CrearEntregableGeneral(string codigoProyecto, GuardarEntregableProyectoDto dto) => StatusCode(201, ApiRespuesta<EntregableProyecto>.Correcto(await servicio.CrearEntregableAsync(codigoProyecto, dto, Usuario), "Entregable creado correctamente."));
    [HttpPut("entregables/{codigoEntregable}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Editar)]
    public async Task<IActionResult> ActualizarEntregable(string codigoProyecto, string codigoEntregable, GuardarEntregableProyectoDto dto) => Ok(ApiRespuesta<EntregableProyecto>.Correcto(await servicio.ActualizarEntregableAsync(codigoProyecto, codigoEntregable, dto, Usuario), "Entregable actualizado correctamente."));
    [HttpPatch("entregables/{codigoEntregable}/estado"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Editar)]
    public async Task<IActionResult> EstadoEntregable(string codigoProyecto, string codigoEntregable, CambiarEstadoEntregableDto dto) => Ok(ApiRespuesta<EntregableProyecto>.Correcto(await servicio.CambiarEstadoEntregableAsync(codigoProyecto, codigoEntregable, dto, Usuario), "Estado del entregable actualizado."));
    [HttpDelete("entregables/{codigoEntregable}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Eliminar)]
    public async Task<IActionResult> EliminarEntregable(string codigoProyecto, string codigoEntregable) { await servicio.EliminarEntregableAsync(codigoProyecto, codigoEntregable, Usuario); return NoContent(); }

    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}
