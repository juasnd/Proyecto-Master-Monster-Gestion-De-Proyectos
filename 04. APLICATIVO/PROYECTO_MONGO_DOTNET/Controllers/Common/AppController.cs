using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_MONGO_DOTNET.Controllers;

public abstract class AppController : Controller
{
    protected bool SesionValida => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("usuarioLogueado"));
    protected IActionResult LoginRedirect() => Redirect("/index.jsp");
    protected string UsuarioSesion => HttpContext.Session.GetString("usuarioLogueado") ?? "";
    protected string PerfilSesion => HttpContext.Session.GetString("perfilCodigo") ?? "";

    protected void CargarSesionViewBag()
    {
        ViewBag.UsuarioNombre = HttpContext.Session.GetString("usuarioNombre") ?? UsuarioSesion;
        ViewBag.PerfilNombre = HttpContext.Session.GetString("perfilNombre") ?? "Perfil";
    }

    protected static string Valor(IFormCollection form, string nombre) =>
        form[nombre].ToString().Trim();
}
