using System.Text.Json;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Middleware;

public sealed class ManejoExcepcionesMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejoExcepcionesMiddleware> _logger;
    public ManejoExcepcionesMiddleware(RequestDelegate siguiente, ILogger<ManejoExcepcionesMiddleware> logger) { _siguiente = siguiente; _logger = logger; }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try { await _siguiente(contexto); }
        catch (ExcepcionNegocio ex)
        {
            if (contexto.Response.HasStarted) throw;
            contexto.Response.StatusCode = ex.CodigoHttp;
            contexto.Response.ContentType = "application/json; charset=utf-8";
            await contexto.Response.WriteAsync(JsonSerializer.Serialize(ApiRespuesta<object>.Fallo(ex.Message, ex.Errores), Opciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado al procesar {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
            if (contexto.Response.HasStarted) throw;
            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            contexto.Response.ContentType = "application/json; charset=utf-8";
            await contexto.Response.WriteAsync(JsonSerializer.Serialize(ApiRespuesta<object>.Fallo("Ocurrió un error interno. Intente nuevamente."), Opciones));
        }
    }
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);
}
