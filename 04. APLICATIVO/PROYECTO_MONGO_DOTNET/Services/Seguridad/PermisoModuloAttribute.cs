using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PROYECTO_MONGO_DOTNET.Services.Seguridad;

/// <summary>Resuelve la opción heredada por su URL, sin inventar códigos de catálogo.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PermisoModuloAttribute(string ruta) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var accion = PermisoAccion.Ver;
        if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor { ActionName: "Eliminar" })
            accion = PermisoAccion.Eliminar;
        else if (!HttpMethods.IsGet(http.Request.Method) && !HttpMethods.IsHead(http.Request.Method))
        {
            var form = await http.Request.ReadFormAsync(http.RequestAborted);
            accion = form["accion"].ToString() switch
            {
                "eliminar" => PermisoAccion.Eliminar,
                "actualizar" => PermisoAccion.Editar,
                _ => PermisoAccion.Crear
            };
        }

        var permisos = http.RequestServices.GetRequiredService<PermisoDAO>();
        if (!permisos.TienePermisoPorRuta(http.Session.GetString("perfilCodigo") ?? "", ruta, accion))
            context.Result = new ContentResult { StatusCode = 403, Content = "No tiene permiso para realizar esta operación.", ContentType = "text/plain; charset=utf-8" };
    }
}
