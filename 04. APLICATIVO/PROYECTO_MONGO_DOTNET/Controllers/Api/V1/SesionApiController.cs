using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;

namespace PROYECTO_MONGO_DOTNET.Controllers.Api.V1;

[ApiController, Route("api/v1/sesion")]
public sealed class SesionApiController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;
    public SesionApiController(IAntiforgery antiforgery) => _antiforgery = antiforgery;
    [HttpGet("token-antifalsificacion")]
    public IActionResult Token()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(ApiRespuesta<object>.Correcto(new { token = tokens.RequestToken, encabezado = "X-CSRF-TOKEN" }));
    }
}
