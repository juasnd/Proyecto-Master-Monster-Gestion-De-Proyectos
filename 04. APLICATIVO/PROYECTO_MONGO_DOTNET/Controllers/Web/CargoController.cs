using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Controllers;

[Route("CargoController")]
public class CargoController : AppController
{
    private readonly CargoDAO _cargos;
    private readonly DepartamentoDAO _departamentos;
    private readonly PermisoDAO _permisos;

    public CargoController(CargoDAO cargos, DepartamentoDAO departamentos, PermisoDAO permisos)
    {
        _cargos = cargos;
        _departamentos = departamentos;
        _permisos = permisos;
    }

    [HttpGet]
    [HttpGet("/cargos.jsp")]
    public IActionResult Index(string accion = "listar", string? depCode = null, string? carCode = null, string? mensaje = null, string? error = null)
    {
        if (!SesionValida) return LoginRedirect();
        var vm = CargarVm();
        vm.Mensaje = mensaje;
        vm.Error = error;

        if (accion == "nuevo") vm.Modo = "nuevo";
        if (accion == "editar" && depCode != null && carCode != null)
        {
            vm.CargoEditar = _cargos.Buscar(depCode, carCode);
            vm.Modo = vm.CargoEditar == null ? "listar" : "editar";
            if (vm.CargoEditar == null) vm.Error = "no_encontrado";
        }
        if (accion == "ver" && depCode != null && carCode != null)
        {
            vm.CargoVer = _cargos.Buscar(depCode, carCode);
            vm.Modo = vm.CargoVer == null ? "listar" : "ver";
            if (vm.CargoVer == null) vm.Error = "no_encontrado";
        }
        if (accion == "eliminar" && depCode != null && carCode != null)
        {
            if (_cargos.EnUso(depCode, carCode)) return Redirect("/CargoController?error=en_uso");
            return Redirect("/CargoController?mensaje=" + (_cargos.Eliminar(depCode, carCode) ? "eliminado" : "no_eliminado"));
        }

        return View("Cargos", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Guardar()
    {
        if (!SesionValida) return LoginRedirect();
        var accion = Valor(Request.Form, "accion");
        var dep = Valor(Request.Form, "pedepCodigo");
        var codigo = Valor(Request.Form, "pecarCodigo");
        var descripcion = Valor(Request.Form, "pecarDescri");
        if (string.IsNullOrWhiteSpace(dep) || string.IsNullOrWhiteSpace(descripcion))
        {
            var vm = CargarVm();
            vm.Modo = accion == "actualizar" ? "editar" : "nuevo";
            vm.Error = "Debe seleccionar el departamento e ingresar el nombre del cargo.";
            vm.CargoEditar = accion == "actualizar"
                ? new Cargo { PedepCodigo = dep, PecarCodigo = codigo, PecarDescri = descripcion }
                : null;
            return View("Cargos", vm);
        }

        var ok = accion == "actualizar"
            ? _cargos.Actualizar(new Cargo { PedepCodigo = dep, PecarCodigo = codigo, PecarDescri = descripcion })
            : _cargos.Insertar(new Cargo { PedepCodigo = dep, PecarDescri = descripcion });

        if (!ok)
        {
            var vm = CargarVm();
            vm.Modo = accion == "actualizar" ? "editar" : "nuevo";
            vm.Error = accion == "actualizar" ? "No se pudo actualizar el cargo." : "No se pudo guardar el cargo.";
            return View("Cargos", vm);
        }

        return Redirect("/CargoController?mensaje=" + (accion == "actualizar" ? "actualizado" : "guardado"));
    }

    private CargosVm CargarVm() => new()
    {
        Cargos = _cargos.Listar(),
        Departamentos = _departamentos.Listar(),
        PuedeReporteCargos = _permisos.TienePermiso(PerfilSesion, "RCA")
    };
}
