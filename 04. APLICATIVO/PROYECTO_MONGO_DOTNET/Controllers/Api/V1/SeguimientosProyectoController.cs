using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Proyectos;

namespace PROYECTO_MONGO_DOTNET.Controllers.Api.V1;

[ApiController, Route("api/v1/proyectos/{codigoProyecto}")]
public sealed class SeguimientosProyectoController : ControllerBase
{
    private readonly SeguimientoProyectoServicio _servicio;
    public SeguimientosProyectoController(SeguimientoProyectoServicio servicio) => _servicio = servicio;
    [HttpGet("seguimiento"), PermisoRequerido("PRO_SEGUIMIENTO")]
    public async Task<IActionResult> Listar(string codigoProyecto) => Ok(ApiRespuesta<IReadOnlyCollection<SeguimientoProyecto>>.Correcto(await _servicio.ListarAsync(codigoProyecto)));
    [HttpPatch("estado"), ValidateAntiForgeryToken, PermisoRequerido("PRO_SEGUIMIENTO", PermisoAccion.Editar)]
    public async Task<IActionResult> Estado(string codigoProyecto, ActualizarEstadoDto dto) => Ok(ApiRespuesta<Proyecto>.Correcto(await _servicio.CambiarEstadoAsync(codigoProyecto, dto, Usuario), "Estado actualizado correctamente."));
    [HttpGet("finalizacion/validar"), PermisoRequerido("PRO_CIERRE")]
    public async Task<IActionResult> ValidarFinalizacion(string codigoProyecto) => Ok(ApiRespuesta<ValidacionCierreProyectoVm>.Correcto(await _servicio.ValidarFinalizacionAsync(codigoProyecto)));
    [HttpPost("finalizar"), ValidateAntiForgeryToken, PermisoRequerido("PRO_CIERRE", PermisoAccion.Editar)]
    public async Task<IActionResult> Finalizar(string codigoProyecto, FinalizarProyectoDto dto) => Ok(ApiRespuesta<Proyecto>.Correcto(await _servicio.FinalizarAsync(codigoProyecto, dto, Usuario), "Proyecto finalizado correctamente."));
    [HttpPost("cancelar"), ValidateAntiForgeryToken, PermisoRequerido("PRO_CIERRE", PermisoAccion.Editar)]
    public async Task<IActionResult> Cancelar(string codigoProyecto, MotivoDto dto) => Ok(ApiRespuesta<Proyecto>.Correcto(await _servicio.CancelarAsync(codigoProyecto, dto.Motivo, Usuario), "Proyecto cancelado correctamente."));
    private string Usuario => HttpContext.Session.GetString("usuarioLogueado") ?? "";
}
