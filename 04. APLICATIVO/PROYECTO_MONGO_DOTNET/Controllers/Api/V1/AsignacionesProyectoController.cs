using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Proyectos;

namespace PROYECTO_MONGO_DOTNET.Controllers.Api.V1;

[ApiController, Route("api/v1/proyectos/{codigoProyecto}/empleados")]
public sealed class AsignacionesProyectoController : ControllerBase
{
    private readonly AsignacionProyectoServicio _servicio;
    public AsignacionesProyectoController(AsignacionProyectoServicio servicio) => _servicio = servicio;
    [HttpGet, PermisoRequerido("PRO_ASIGNAR")]
    public async Task<IActionResult> Listar(string codigoProyecto) => Ok(ApiRespuesta<IReadOnlyCollection<AsignacionProyecto>>.Correcto(await _servicio.PorProyectoAsync(codigoProyecto)));
    [HttpGet("capacidad"), PermisoRequerido("PRO_ASIGNAR")]
    public async Task<IActionResult> Capacidad(string codigoProyecto) => Ok(ApiRespuesta<IReadOnlyCollection<CapacidadEmpleadoProyectoVm>>.Correcto(await _servicio.EquipoAsync(codigoProyecto)));
    [HttpPost, ValidateAntiForgeryToken, PermisoRequerido("PRO_ASIGNAR", PermisoAccion.Crear)]
    public async Task<IActionResult> Crear(string codigoProyecto, GuardarAsignacionDto dto) => StatusCode(StatusCodes.Status201Created, ApiRespuesta<AsignacionProyecto>.Correcto(await _servicio.CrearAsync(codigoProyecto, dto, Usuario), "Empleado asignado correctamente."));
    [HttpPut("{codigoEmpleado}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_ASIGNAR", PermisoAccion.Editar)]
    public async Task<IActionResult> Actualizar(string codigoProyecto, string codigoEmpleado, GuardarAsignacionDto dto) => Ok(ApiRespuesta<AsignacionProyecto>.Correcto(await _servicio.ActualizarAsync(codigoProyecto, codigoEmpleado, dto, Usuario), "Asignación actualizada correctamente."));
    [HttpDelete("{codigoEmpleado}"), ValidateAntiForgeryToken, PermisoRequerido("PRO_ASIGNAR", PermisoAccion.Eliminar)]
    public async Task<IActionResult> Retirar(string codigoProyecto, string codigoEmpleado) { await _servicio.RetirarAsync(codigoProyecto, codigoEmpleado, Usuario); return NoContent(); }
    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}

[ApiController, Route("api/v1/asignaciones-proyecto")]
public sealed class ReporteAsignacionesProyectoController : ControllerBase
{
    private readonly AsignacionProyectoServicio _servicio;
    public ReporteAsignacionesProyectoController(AsignacionProyectoServicio servicio) => _servicio = servicio;
    [HttpGet("reporte"), PermisoRequerido("RASI")]
    public async Task<IActionResult> Reporte([FromQuery] string? codigoProyecto, [FromQuery] string? codigoEmpleado, [FromQuery] string? rol,
        [FromQuery] string? estado, [FromQuery] decimal? dedicacionMinima, [FromQuery] decimal? dedicacionMaxima,
        [FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta, [FromQuery] bool soloSobrecargados = false)
    {
        IEnumerable<AsignacionProyecto> q = await _servicio.ListarAsync();
        if (!string.IsNullOrWhiteSpace(codigoProyecto)) q = q.Where(x => x.CodigoProyecto.Equals(codigoProyecto, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(codigoEmpleado)) q = q.Where(x => x.CodigoEmpleado.Equals(codigoEmpleado, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(rol)) q = q.Where(x => x.Rol.Contains(rol, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => x.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
        if (dedicacionMinima.HasValue) q = q.Where(x => x.DisponibilidadAsignada >= dedicacionMinima.Value);
        if (dedicacionMaxima.HasValue) q = q.Where(x => x.DisponibilidadAsignada <= dedicacionMaxima.Value);
        if (fechaDesde.HasValue) q = q.Where(x => x.FechaInicio >= fechaDesde.Value);
        if (fechaHasta.HasValue) q = q.Where(x => x.FechaFin <= fechaHasta.Value);
        if (soloSobrecargados) q = q.GroupBy(x => x.CodigoEmpleado).Where(g => g.Where(x => x.Activo).Sum(x => x.DisponibilidadAsignada) > 100).SelectMany(x => x);
        return Ok(ApiRespuesta<IReadOnlyCollection<AsignacionProyecto>>.Correcto(q.ToList(), "Reporte generado correctamente."));
    }
}
