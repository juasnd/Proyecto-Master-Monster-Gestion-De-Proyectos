using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Services.Personal;
using PROYECTO_MONGO_DOTNET.Services.Seguridad;

namespace PROYECTO_MONGO_DOTNET.Controllers;

[PermisoModulo("empleados.jsp")]
public sealed class FotoEmpleadoController(FotoEmpleadoServicio fotos) : Controller
{
    [HttpGet("/uploads/empleados/{nombre}")]
    public IActionResult Obtener(string nombre)
    {
        var ruta = fotos.Resolver(FotoEmpleadoServicio.Directorio + "/" + nombre);
        if (ruta == null || !System.IO.File.Exists(ruta)) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(ruta, FotoEmpleadoServicio.TipoContenido(Path.GetExtension(nombre))!);
    }
}
