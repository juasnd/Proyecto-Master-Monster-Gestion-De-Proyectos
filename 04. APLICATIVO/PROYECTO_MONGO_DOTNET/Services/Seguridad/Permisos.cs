using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PROYECTO_MONGO_DOTNET.Models;

namespace PROYECTO_MONGO_DOTNET.Services;

public enum PermisoAccion { Ver, Crear, Editar, Eliminar }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class PermisoRequeridoAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _opcion;
    private readonly PermisoAccion _accion;
    public PermisoRequeridoAttribute(string opcion, PermisoAccion accion = PermisoAccion.Ver) { _opcion = opcion; _accion = accion; }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var sesion = context.HttpContext.Session;
        var usuario = sesion.GetString("usuarioLogueado");
        if (string.IsNullOrWhiteSpace(usuario))
        {
            context.Result = EsApi(context.HttpContext)
                ? new UnauthorizedObjectResult(ApiRespuesta<object>.Fallo("La sesión no es válida."))
                : new RedirectResult("/index.jsp");
            return Task.CompletedTask;
        }
        var perfil = sesion.GetString("perfilCodigo") ?? "";
        var permisos = context.HttpContext.RequestServices.GetRequiredService<PermisoDAO>();
        if (!permisos.TienePermiso(perfil, _opcion, _accion))
        {
            context.Result = EsApi(context.HttpContext)
                ? new ObjectResult(ApiRespuesta<object>.Fallo("No tiene permiso para realizar esta operación.")) { StatusCode = StatusCodes.Status403Forbidden }
                : new ContentResult { StatusCode = StatusCodes.Status403Forbidden, Content = "No tiene permiso para realizar esta operación.", ContentType = "text/plain; charset=utf-8" };
        }
        return Task.CompletedTask;
    }

    private static bool EsApi(HttpContext contexto) => contexto.Request.Path.StartsWithSegments("/api");
}
