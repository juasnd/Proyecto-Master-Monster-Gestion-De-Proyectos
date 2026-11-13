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
    private readonly string? _campoCodigo;
    public PermisoRequeridoAttribute(string opcion, PermisoAccion accion = PermisoAccion.Ver, string? campoCodigo = null)
    { _opcion = opcion; _accion = accion; _campoCodigo = campoCodigo; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var sesion = context.HttpContext.Session;
        var usuario = sesion.GetString("usuarioLogueado");
        if (string.IsNullOrWhiteSpace(usuario))
        {
            context.Result = EsApi(context.HttpContext)
                ? new UnauthorizedObjectResult(ApiRespuesta<object>.Fallo("La sesión no es válida."))
                : new RedirectResult("/index.jsp");
            return;
        }
        var perfil = sesion.GetString("perfilCodigo") ?? "";
        var permisos = context.HttpContext.RequestServices.GetRequiredService<PermisoDAO>();
        var accion = _accion;
        if (_campoCodigo != null)
        {
            var formulario = await context.HttpContext.Request.ReadFormAsync(context.HttpContext.RequestAborted);
            // Se incluyen las fuentes de MVC y sus prefijos de parámetro (por ejemplo dto.CodigoContrato).
            var valores = formulario.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value.ToString()))
                .Concat(context.HttpContext.Request.Query.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value.ToString())))
                .Concat(context.RouteData.Values.Select(x => new KeyValuePair<string, string?>(x.Key, Convert.ToString(x.Value))));
            accion = AccionGuardar(valores, _campoCodigo);
        }
        if (!permisos.TienePermiso(perfil, _opcion, accion))
        {
            context.Result = EsApi(context.HttpContext)
                ? new ObjectResult(ApiRespuesta<object>.Fallo("No tiene permiso para realizar esta operación.")) { StatusCode = StatusCodes.Status403Forbidden }
                : new ContentResult { StatusCode = StatusCodes.Status403Forbidden, Content = "No tiene permiso para realizar esta operación.", ContentType = "text/plain; charset=utf-8" };
        }
    }

    private static bool EsApi(HttpContext contexto) => contexto.Request.Path.StartsWithSegments("/api");

    public static PermisoAccion AccionGuardar(IEnumerable<KeyValuePair<string, string?>> valores, string campoCodigo) =>
        valores.Any(x => (x.Key.Equals(campoCodigo, StringComparison.OrdinalIgnoreCase) ||
                         x.Key.EndsWith("." + campoCodigo, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(x.Value))
            ? PermisoAccion.Editar : PermisoAccion.Crear;
}
