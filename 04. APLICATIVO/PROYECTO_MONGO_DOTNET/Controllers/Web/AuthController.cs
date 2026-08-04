using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Controllers;

public class AuthController : Controller
{
    private readonly UsuarioDAO _usuarios;
    public AuthController(UsuarioDAO usuarios) => _usuarios = usuarios;

    [HttpGet("/")]
    [HttpGet("/index.jsp")]
    public IActionResult Index(string? error, string? num)
    {
        ViewBag.Error = error;
        ViewBag.Num = num;
        return View();
    }

    [HttpPost("/Controller")]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string usuario, string password)
    {
        var user = _usuarios.BuscarPorLogin(usuario);
        if (user == null) return Redirect("/index.jsp?error=usuario_no_existe");
        if (user.EstaBloqueado) return Redirect("/index.jsp?error=bloqueado");
        if (!user.EstaActivo) return Redirect("/index.jsp?error=inactivo");

        if (_usuarios.ValidarPassword(user, password))
        {
            _usuarios.ReiniciarIntentos(user.Login);
            _usuarios.ActualizarUltimoAcceso(user.Login);
            var nombre = _usuarios.ObtenerNombrePersona(user.CodigoPersona);

            HttpContext.Session.SetString("usuarioLogueado", user.Login);
            HttpContext.Session.SetString("usuarioNombre", string.IsNullOrWhiteSpace(nombre) ? user.Login : nombre);
            HttpContext.Session.SetString("perfilCodigo", user.Perfil?.Codigo ?? "");
            HttpContext.Session.SetString("perfilNombre", user.Perfil?.Descripcion ?? "");
            HttpContext.Session.SetString("requiereCambioClave", user.RequiereCambioClave ? "S" : "N");

            if (user.RequiereCambioClave) return Redirect("/cambiarClave.jsp");
            return Redirect("/pagPrincipal.jsp");
        }

        var intentos = _usuarios.SumarIntentosFallidos(user.Login);
        return intentos >= 3
            ? Redirect("/index.jsp?error=bloqueado")
            : Redirect("/index.jsp?error=intentos&num=" + intentos);
    }

    [HttpGet("/Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";
        return Redirect("/index.jsp");
    }
}
