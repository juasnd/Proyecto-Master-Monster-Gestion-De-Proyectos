using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_MONGO_DOTNET.Controllers;

public class HomeController : AppController
{
    [HttpGet("/pagPrincipal.jsp")]
    [HttpGet("/Home/Index")]
    public IActionResult Index(string? mensaje = null)
    {
        if (!SesionValida) return LoginRedirect();
        CargarSesionViewBag();
        ViewBag.Mensaje = mensaje;
        return View();
    }

    [HttpGet("/cambiarClave.jsp")]
    public IActionResult CambiarClave()
    {
        if (!SesionValida) return LoginRedirect();
        CargarSesionViewBag();
        return View();
    }
}
