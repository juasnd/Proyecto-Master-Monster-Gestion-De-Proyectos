using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PROYECTO_MONGO_DOTNET.Models;

namespace PROYECTO_MONGO_DOTNET.Services.Seguridad;

/// <summary>Revalida la cuenta y el perfil antes de autorizar cada solicitud MVC.</summary>
public sealed class SesionActivaFiltro(SqlContexto db) : IAsyncAuthorizationFilter, IOrderedFilter
{
    public int Order => -2000;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()) return;
        var http = context.HttpContext;
        var login = http.Session.GetString("usuarioLogueado");
        EstadoSesion? estado = null;
        if (!string.IsNullOrWhiteSpace(login))
        {
            estado = (await db.ConsultarAsync<EstadoSesion>("""
                SELECT u.Estado, u.CambioClave, COALESCE(p.Codigo,'') PerfilCodigo,
                       COALESCE(p.Descripcion,'') PerfilNombre
                FROM app.Usuario u
                OUTER APPLY (SELECT TOP(1) x.CodigoPerfil FROM app.UsuarioPerfil x
                    WHERE x.Login=u.Login AND x.FechaRetiro IS NULL
                    ORDER BY x.FechaAsignacion DESC) vigente
                LEFT JOIN app.Perfil p ON p.Codigo=vigente.CodigoPerfil AND p.Estado='A'
                WHERE u.Login=@Login
                """, new { Login = login }, http.RequestAborted)).FirstOrDefault();
        }

        var api = http.Request.Path.StartsWithSegments("/api") ||
                  http.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
        if (estado == null || !estado.Estado.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            http.Session.Clear();
            context.Result = api
                ? new UnauthorizedObjectResult(ApiRespuesta<object>.Fallo("La sesión no es válida."))
                : new RedirectResult("/index.jsp");
            return;
        }

        http.Session.SetString("perfilCodigo", estado.PerfilCodigo);
        http.Session.SetString("perfilNombre", estado.PerfilNombre);
        http.Session.SetString("requiereCambioClave", estado.CambioClave);
        http.Response.Headers.CacheControl = "no-store";
        if (estado.CambioClave.Equals("S", StringComparison.OrdinalIgnoreCase) &&
            !http.Request.Path.Equals(new PathString("/cambiarClave.jsp")) &&
            !http.Request.Path.Equals(new PathString("/Logout")))
        {
            context.Result = api
                ? new ObjectResult(ApiRespuesta<object>.Fallo("Debe cambiar su contraseña antes de continuar.")) { StatusCode = 403 }
                : new RedirectResult("/cambiarClave.jsp");
        }
    }

    public sealed class EstadoSesion
    {
        public string Estado { get; set; } = "";
        public string CambioClave { get; set; } = "";
        public string PerfilCodigo { get; set; } = "";
        public string PerfilNombre { get; set; } = "";
    }
}
