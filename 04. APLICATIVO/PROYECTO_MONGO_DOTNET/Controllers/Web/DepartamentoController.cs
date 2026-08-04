using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Controllers;

[Route("DepartamentoController")]
public class DepartamentoController : AppController
{
    private readonly DepartamentoDAO _departamentos;
    private readonly PermisoDAO _permisos;

    public DepartamentoController(DepartamentoDAO departamentos, PermisoDAO permisos)
    {
        _departamentos = departamentos;
        _permisos = permisos;
    }

    [HttpGet]
    [HttpGet("/departamentos.jsp")]
    public IActionResult Index(string accion = "listar", string? codigo = null, string? mensaje = null, string? error = null)
    {
        if (!SesionValida) return LoginRedirect();
        var vm = CargarVm();
        vm.Mensaje = mensaje;
        vm.Error = error;

        if (accion == "nuevo") vm.Modo = "nuevo";
        if (accion == "editar" && codigo != null)
        {
            vm.DepartamentoEditar = _departamentos.Buscar(codigo);
            vm.Modo = vm.DepartamentoEditar == null ? "listar" : "editar";
            if (vm.DepartamentoEditar == null) vm.Error = "no_encontrado";
        }
        if (accion == "ver" && codigo != null)
        {
            vm.DepartamentoVer = _departamentos.Buscar(codigo);
            vm.Modo = vm.DepartamentoVer == null ? "listar" : "ver";
            if (vm.DepartamentoVer == null) vm.Error = "no_encontrado";
        }
        if (accion == "eliminar" && codigo != null)
        {
            if (_departamentos.EnUso(codigo)) return Redirect("/DepartamentoController?error=en_uso");
            return Redirect("/DepartamentoController?mensaje=" + (_departamentos.Eliminar(codigo) ? "eliminado" : "no_eliminado"));
        }

        return View("Departamentos", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Guardar()
    {
        if (!SesionValida) return LoginRedirect();
        var accion = Valor(Request.Form, "accion");
        var descripcion = Valor(Request.Form, "descripcion");
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            var vm = CargarVm();
            vm.Modo = accion == "actualizar" ? "editar" : "nuevo";
            vm.Error = "Debe ingresar el nombre del departamento.";
            vm.DepartamentoEditar = accion == "actualizar"
                ? new Departamento { Codigo = Valor(Request.Form, "codigo"), Descripcion = descripcion }
                : null;
            return View("Departamentos", vm);
        }

        var ok = accion == "actualizar"
            ? _departamentos.Actualizar(new Departamento { Codigo = Valor(Request.Form, "codigo"), Descripcion = descripcion })
            : _departamentos.Insertar(new Departamento { Descripcion = descripcion });

        if (!ok)
        {
            var vm = CargarVm();
            vm.Modo = accion == "actualizar" ? "editar" : "nuevo";
            vm.Error = accion == "actualizar" ? "No se pudo actualizar el departamento." : "No se pudo guardar el departamento.";
            return View("Departamentos", vm);
        }

        return Redirect("/DepartamentoController?mensaje=" + (accion == "actualizar" ? "actualizado" : "guardado"));
    }

    private DepartamentosVm CargarVm() => new()
    {
        Departamentos = _departamentos.Listar(),
        PuedeReporteDepartamentos = _permisos.TienePermiso(PerfilSesion, "RDE")
    };
}
