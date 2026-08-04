using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Controllers;

[Route("EmpleadoController")]
public class EmpleadoController : AppController
{
    private const long FotoMaxBytes = 2L * 1024L * 1024L;
    private const string FotoDir = "uploads/empleados";
    private static readonly HashSet<string> FotoExtensiones = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly HashSet<string> FotoMime = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private readonly EmpleadoDAO _empleados;
    private readonly PermisoDAO _permisos;
    private readonly IWebHostEnvironment _env;

    public EmpleadoController(EmpleadoDAO empleados, PermisoDAO permisos, IWebHostEnvironment env)
    {
        _empleados = empleados;
        _permisos = permisos;
        _env = env;
    }

    [HttpGet]
    [HttpGet("/empleados.jsp")]
    [HttpGet("/familiares.jsp")]
    [HttpGet("/formacion.jsp")]
    public IActionResult Index(string accion = "listar", string? codigo = null, string? mensaje = null, string? error = null, string? criterioBusqueda = null, string? terminoBusqueda = null, string? departamento = null)
    {
        if (!SesionValida) return LoginRedirect();
        if (accion == "cargosPorDepartamento")
            return Json(_empleados.CargosPorDepartamento(departamento ?? "").Select(c => new { departamento = c.PedepCodigo, codigo = c.PecarCodigo, descripcion = c.PecarDescri }));

        var vm = CargarVm();
        vm.Mensaje = mensaje;
        vm.Error = TraducirErrorEmpleado(error);

        if (accion == "nuevo")
        {
            vm.Modo = "nuevo";
            vm.EmpleadoForm = new Empleado();
        }
        if (accion == "buscar")
        {
            vm.CriterioBusqueda = criterioBusqueda ?? "codigo";
            vm.TerminoBusqueda = terminoBusqueda ?? "";
            vm.Empleados = _empleados.Buscar(vm.CriterioBusqueda, vm.TerminoBusqueda);
        }
        if ((accion == "editar" || accion == "ver") && codigo != null)
        {
            var emp = _empleados.BuscarPorCodigo(codigo);
            if (emp == null) vm.Error = "No se encontro el empleado solicitado.";
            else { vm.EmpleadoForm = emp; vm.Modo = accion; }
        }
        if (accion == "eliminar" && codigo != null)
        {
            var emp = _empleados.BuscarPorCodigo(codigo);
            if (emp == null) return Redirect("/EmpleadoController?error=no_encontrado");
            if (_empleados.EmpleadoTieneRelaciones(codigo) || _empleados.PersonaTieneRelacionesExternas(emp.Persona.PeperCodigo, codigo))
                return Redirect("/EmpleadoController?error=en_uso");
            return Redirect("/EmpleadoController?mensaje=" + (_empleados.Eliminar(codigo) ? "eliminado" : "no_eliminado"));
        }

        return View("Empleados", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Guardar()
    {
        if (!SesionValida) return LoginRedirect();
        var accion = Valor(Request.Form, "accion");
        var nuevo = accion != "actualizar";
        var emp = ConstruirEmpleado(Request.Form);
        var errores = new List<string>();
        emp.Familiares = ConstruirFamiliares(Request.Form, emp.Persona.PeperCodigo, errores);
        emp.Formaciones = ConstruirFormaciones(Request.Form, emp.PeempCodigo);
        emp.Persona.CargasFamiliares = emp.Familiares.Count(f => string.Equals(f.CargaFamiliar, "S", StringComparison.OrdinalIgnoreCase));

        var fotoPart = Request.Form.Files["fotoEmpleado"];
        var fotoAnterior = Valor(Request.Form, "fotoActual");
        if (!nuevo) emp.Persona.Foto = fotoAnterior;

        errores.AddRange(ValidarEmpleado(emp, nuevo));
        errores.AddRange(ValidarFamiliares(emp.Familiares));
        errores.AddRange(ValidarFormaciones(emp.Formaciones));
        var errorFoto = ValidarFoto(fotoPart);
        if (errorFoto != null) errores.Add(errorFoto);

        if (!nuevo)
        {
            if (!_empleados.ExisteEmpleado(emp.PeempCodigo)) errores.Add("No se encontro el empleado que desea actualizar.");
            if (!_empleados.ExistePersona(emp.Persona.PeperCodigo)) errores.Add("No se encontro la persona vinculada al empleado.");
        }

        if (nuevo && _empleados.ExisteEmpleadoConCedula(emp.Persona.Cedula))
            errores.Add("Ya existe un empleado registrado con esta cedula.");
        if (!nuevo && _empleados.ExisteCedulaEnOtraPersona(emp.Persona.Cedula, emp.Persona.PeperCodigo))
            errores.Add("Ya existe otra persona registrada con esta cedula.");

        if (errores.Any()) return VolverAlFormulario(nuevo ? "nuevo" : "editar", emp, string.Join(" ", errores));

        string? fotoGuardada = null;
        try
        {
            if (FotoSeleccionada(fotoPart))
            {
                fotoGuardada = await GuardarFoto(fotoPart!, nuevo ? emp.Persona.Cedula : emp.Persona.PeperCodigo);
                emp.Persona.Foto = fotoGuardada;
            }

            if (nuevo) _empleados.Guardar(emp); else _empleados.Actualizar(emp);
            if (!nuevo && fotoGuardada != null) EliminarFotoSiExiste(fotoAnterior, fotoGuardada);

            return Redirect("/EmpleadoController?mensaje=" + (nuevo ? "guardado" : "actualizado"));
        }
        catch (Exception ex)
        {
            EliminarFotoSiExiste(fotoGuardada, null);
            return VolverAlFormulario(nuevo ? "nuevo" : "editar", emp, MensajeErrorGuardarEmpleado(ex));
        }
    }

    private EmpleadosVm CargarVm() => new()
    {
        Empleados = _empleados.Listar(),
        Departamentos = _empleados.ListarDepartamentos(),
        Cargos = _empleados.ListarCargos(),
        Sexos = _empleados.ListarSexos(),
        EstadosCiviles = _empleados.ListarEstadosCiviles(),
        Parentescos = _empleados.ListarParentescos(),
        PuedeReporteEmpleados = _permisos.TienePermiso(PerfilSesion, "REM")
    };

    private IActionResult VolverAlFormulario(string modo, Empleado emp, string error)
    {
        var vm = CargarVm();
        vm.Modo = modo;
        vm.EmpleadoForm = emp;
        vm.Error = error;
        return View("Empleados", vm);
    }

    private Empleado ConstruirEmpleado(IFormCollection form)
    {
        var dep = Valor(form, "departamentoCodigo").ToUpperInvariant();
        return new Empleado
        {
            PeempCodigo = Valor(form, "codigoEmpleado").ToUpperInvariant(),
            PedepCodigo = dep,
            PecarCodigo = Valor(form, "cargoCodigo").ToUpperInvariant(),
            PedPedepCodigo = dep,
            Persona = new Persona
            {
                PeperCodigo = Valor(form, "codigoPersona").ToUpperInvariant(),
                Cedula = Valor(form, "cedula"),
                Nombres = Valor(form, "nombres"),
                Apellidos = Valor(form, "apellidos"),
                FechaNacimiento = Valor(form, "fechaNacimiento"),
                PesexCodigo = Valor(form, "sexoCodigo"),
                PeescCodigo = Valor(form, "estadoCivilCodigo"),
                CargasFamiliares = int.TryParse(Valor(form, "cargasFamiliares"), out var cargas) ? cargas : 0,
                Direccion = Valor(form, "direccion"),
                Celular = Valor(form, "celular"),
                TelefonoDomicilio = Valor(form, "telefonoDomicilio"),
                Email = Valor(form, "email"),
                Foto = Valor(form, "fotoActual"),
                Tipo = "EMP"
            }
        };
    }

    private static List<Familiar> ConstruirFamiliares(IFormCollection form, string codigoPersona, List<string> errores)
    {
        var json = Valor(form, "familiaresJson");
        if (string.IsNullOrWhiteSpace(json)) return new List<Familiar>();

        try
        {
            var datos = JsonSerializer.Deserialize<List<FamiliarEntrada>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            return datos.Select(item => new Familiar
            {
                Codigo = (item.Codigo ?? "").Trim().ToUpperInvariant(),
                CodigoPersona = codigoPersona,
                CodigoParentesco = (item.CodigoParentesco ?? "").Trim().ToUpperInvariant(),
                DescripcionParentesco = (item.DescripcionParentesco ?? "").Trim(),
                Nombre = (item.Nombre ?? "").Trim(),
                Apellido = (item.Apellido ?? "").Trim(),
                FechaNacimiento = (item.FechaNacimiento ?? "").Trim(),
                Telefono = (item.Telefono ?? "").Trim(),
                CargaFamiliar = string.IsNullOrWhiteSpace(item.CargaFamiliar) ? "S" : item.CargaFamiliar.Trim().ToUpperInvariant(),
                Observacion = (item.Observacion ?? "").Trim()
            }).Where(f => !string.IsNullOrWhiteSpace(f.Nombre) || !string.IsNullOrWhiteSpace(f.Apellido) || !string.IsNullOrWhiteSpace(f.CodigoParentesco)).ToList();
        }
        catch
        {
            errores.Add("La informacion familiar enviada no es valida.");
            return new List<Familiar>();
        }
    }

    private static List<Formacion> ConstruirFormaciones(IFormCollection form, string codigoEmpleado)
    {
        var formacion = new Formacion
        {
            CodigoEmpleado = codigoEmpleado,
            Nivel = Valor(form, "formacionNivel"),
            Titulo = Valor(form, "formacionTitulo"),
            Institucion = Valor(form, "formacionInstitucion"),
            FechaInicio = Valor(form, "formacionInicio"),
            FechaFin = Valor(form, "formacionFin"),
            Observacion = Valor(form, "formacionObservacion")
        };

        return FormacionTieneDatos(formacion) ? new List<Formacion> { formacion } : new List<Formacion>();
    }

    private List<string> ValidarEmpleado(Empleado empleado, bool nuevo)
    {
        var errores = new List<string>();
        var p = empleado.Persona;

        if (!nuevo)
        {
            Requerido(errores, p.PeperCodigo, "El codigo de persona es obligatorio.");
            Requerido(errores, empleado.PeempCodigo, "El codigo de empleado es obligatorio.");
        }
        Requerido(errores, p.Cedula, "La cedula es obligatoria.");
        Requerido(errores, p.Nombres, "Los nombres son obligatorios.");
        Requerido(errores, p.Apellidos, "Los apellidos son obligatorios.");
        Requerido(errores, p.FechaNacimiento, "La fecha de nacimiento es obligatoria.");
        Requerido(errores, p.PesexCodigo, "Debe seleccionar el sexo.");
        Requerido(errores, p.Direccion, "La direccion es obligatoria.");
        Requerido(errores, p.Email, "El email es obligatorio.");
        Requerido(errores, empleado.PedepCodigo, "Debe seleccionar el departamento.");
        Requerido(errores, empleado.PecarCodigo, "Debe seleccionar el cargo.");

        Longitud(errores, p.PeperCodigo, 10, "El codigo de persona no debe superar 10 caracteres.");
        Longitud(errores, empleado.PeempCodigo, 10, "El codigo de empleado no debe superar 10 caracteres.");
        Longitud(errores, p.Nombres, 30, "Los nombres no deben superar 30 caracteres.");
        Longitud(errores, p.Apellidos, 30, "Los apellidos no deben superar 30 caracteres.");
        Longitud(errores, p.Direccion, 100, "La direccion no debe superar 100 caracteres.");
        Longitud(errores, p.Email, 100, "El email no debe superar 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(p.Cedula) && !CedulaEcuatorianaValida(p.Cedula)) errores.Add("La cedula ingresada no es valida.");
        if (!string.IsNullOrWhiteSpace(p.Nombres) && !Regex.IsMatch(p.Nombres, @"^[\p{L} ]{2,30}$")) errores.Add("Los nombres solo deben contener letras y espacios.");
        if (!string.IsNullOrWhiteSpace(p.Apellidos) && !Regex.IsMatch(p.Apellidos, @"^[\p{L} ]{2,30}$")) errores.Add("Los apellidos solo deben contener letras y espacios.");
        if (!string.IsNullOrWhiteSpace(p.FechaNacimiento)) ValidarFechaNacimiento(errores, p.FechaNacimiento);
        if (!string.IsNullOrWhiteSpace(p.Celular) && !Regex.IsMatch(p.Celular, @"^09\d{8}$")) errores.Add("El celular debe tener 10 digitos e iniciar con 09.");
        if (!string.IsNullOrWhiteSpace(p.TelefonoDomicilio) && !Regex.IsMatch(p.TelefonoDomicilio, @"^0[2-7]\d{8}$")) errores.Add("El telefono de domicilio debe tener 10 digitos e iniciar con 02, 03, 04, 05, 06 o 07.");
        if (!string.IsNullOrWhiteSpace(p.Email) && !Regex.IsMatch(p.Email, @"^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$")) errores.Add("Ingrese un email valido.");
        if (p.CargasFamiliares < 0 || p.CargasFamiliares > 99) errores.Add("Las cargas familiares deben estar entre 0 y 99.");
        if (!string.IsNullOrWhiteSpace(empleado.PedepCodigo) && !string.IsNullOrWhiteSpace(empleado.PecarCodigo) && !_empleados.CargoPerteneceDepartamento(empleado.PedepCodigo, empleado.PecarCodigo))
            errores.Add("El cargo seleccionado no pertenece al departamento indicado.");

        return errores;
    }

    private List<string> ValidarFamiliares(List<Familiar> familiares)
    {
        var errores = new List<string>();
        if (!familiares.Any()) return errores;
        var parentescos = _empleados.ListarParentescos().Select(p => p.Codigo.ToUpperInvariant()).ToHashSet();

        for (var i = 0; i < familiares.Count; i++)
        {
            var f = familiares[i];
            var prefijo = "Familiar " + (i + 1) + ": ";
            Requerido(errores, f.Nombre, prefijo + "los nombres son obligatorios.");
            Requerido(errores, f.Apellido, prefijo + "los apellidos son obligatorios.");
            Requerido(errores, f.CodigoParentesco, prefijo + "debe seleccionar el parentesco.");
            Requerido(errores, f.FechaNacimiento, prefijo + "la fecha de nacimiento es obligatoria.");
            Requerido(errores, f.Telefono, prefijo + "el telefono es obligatorio.");
            Longitud(errores, f.Nombre, 30, prefijo + "los nombres no deben superar 30 caracteres.");
            Longitud(errores, f.Apellido, 30, prefijo + "los apellidos no deben superar 30 caracteres.");
            Longitud(errores, f.Observacion, 200, prefijo + "la observacion no debe superar 200 caracteres.");
            if (!string.IsNullOrWhiteSpace(f.CodigoParentesco) && !parentescos.Contains(f.CodigoParentesco.ToUpperInvariant()))
                errores.Add(prefijo + "el parentesco seleccionado no existe.");
            if (!string.IsNullOrWhiteSpace(f.Telefono) && !Regex.IsMatch(f.Telefono, @"^(09|0[2-7])\d{8}$"))
                errores.Add(prefijo + "el telefono debe iniciar con 09, 02, 03, 04, 05, 06 o 07.");
        }

        return errores;
    }

    private static List<string> ValidarFormaciones(List<Formacion> formaciones)
    {
        var errores = new List<string>();
        foreach (var f in formaciones)
        {
            Requerido(errores, f.Nivel, "El nivel de formacion es obligatorio.");
            Requerido(errores, f.Titulo, "El titulo obtenido es obligatorio.");
            Requerido(errores, f.Institucion, "La institucion es obligatoria.");
            Requerido(errores, f.FechaInicio, "La fecha de inicio de formacion es obligatoria.");
            Longitud(errores, f.Observacion, 200, "La observacion de formacion no debe superar 200 caracteres.");
            if (!string.IsNullOrWhiteSpace(f.FechaInicio) && !DateOnly.TryParse(f.FechaInicio, out _))
                errores.Add("La fecha de inicio de formacion debe tener formato AAAA-MM-DD.");
            if (!string.IsNullOrWhiteSpace(f.FechaFin) && !DateOnly.TryParse(f.FechaFin, out _))
                errores.Add("La fecha de finalizacion de formacion debe tener formato AAAA-MM-DD.");
            if (DateOnly.TryParse(f.FechaInicio, out var inicio) && DateOnly.TryParse(f.FechaFin, out var fin) && fin < inicio)
                errores.Add("La fecha de finalizacion no puede ser anterior a la fecha de inicio.");
        }
        return errores;
    }

    private static string? ValidarFoto(IFormFile? foto)
    {
        if (!FotoSeleccionada(foto)) return null;
        var extension = Path.GetExtension(foto!.FileName);
        if (!FotoExtensiones.Contains(extension)) return "La foto debe ser jpg, jpeg, png o webp.";
        if (!FotoMime.Contains(foto.ContentType)) return "El tipo de archivo no es una imagen valida.";
        if (foto.Length > FotoMaxBytes) return "La foto no debe superar 2 MB.";
        return null;
    }

    private async Task<string> GuardarFoto(IFormFile foto, string codigo)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var directorio = Path.GetFullPath(Path.Combine(webRoot, FotoDir));
        Directory.CreateDirectory(directorio);

        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
        var limpio = Regex.Replace(codigo ?? "", @"[^A-Za-z0-9_-]", "");
        if (string.IsNullOrWhiteSpace(limpio)) limpio = "empleado";
        var nombre = limpio + "_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + extension;
        var destino = Path.GetFullPath(Path.Combine(directorio, nombre));
        if (!destino.StartsWith(directorio, StringComparison.OrdinalIgnoreCase))
            throw new IOException("Ruta de foto no valida.");

        await using var stream = System.IO.File.Create(destino);
        await foto.CopyToAsync(stream);
        return FotoDir.Replace('\\', '/') + "/" + nombre;
    }

    private void EliminarFotoSiExiste(string? rutaRelativa, string? excepto)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa) || rutaRelativa == excepto || !rutaRelativa.StartsWith(FotoDir + "/", StringComparison.OrdinalIgnoreCase)) return;
        var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var root = Path.GetFullPath(webRoot);
        var carpetaFotos = Path.GetFullPath(Path.Combine(root, FotoDir));
        var archivo = Path.GetFullPath(Path.Combine(root, rutaRelativa));
        if (archivo.StartsWith(carpetaFotos, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(archivo))
            System.IO.File.Delete(archivo);
    }

    private static bool FotoSeleccionada(IFormFile? foto) => foto != null && foto.Length > 0;
    private static bool FormacionTieneDatos(Formacion f) => new[] { f.Nivel, f.Titulo, f.Institucion, f.FechaInicio, f.FechaFin, f.Observacion }.Any(v => !string.IsNullOrWhiteSpace(v));
    private static void Requerido(List<string> errores, string? valor, string mensaje) { if (string.IsNullOrWhiteSpace(valor)) errores.Add(mensaje); }
    private static void Longitud(List<string> errores, string? valor, int maximo, string mensaje) { if (valor != null && valor.Length > maximo) errores.Add(mensaje); }

    private static void ValidarFechaNacimiento(List<string> errores, string fechaNacimiento)
    {
        if (!DateOnly.TryParse(fechaNacimiento, out var fecha))
        {
            errores.Add("La fecha de nacimiento debe tener formato AAAA-MM-DD.");
            return;
        }
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (fecha > hoy)
        {
            errores.Add("La fecha de nacimiento no puede ser futura.");
            return;
        }
        var edad = hoy.Year - fecha.Year;
        if (fecha > hoy.AddYears(-edad)) edad--;
        if (edad < 18) errores.Add("El empleado debe tener al menos 18 anios.");
    }

    private static bool CedulaEcuatorianaValida(string cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula) || !Regex.IsMatch(cedula, @"^\d{10}$")) return false;
        var provincia = int.Parse(cedula[..2]);
        var tercerDigito = cedula[2] - '0';
        if (provincia is < 1 or > 24 || tercerDigito >= 6) return false;
        int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        var suma = 0;
        for (var i = 0; i < coeficientes.Length; i++)
        {
            var valor = (cedula[i] - '0') * coeficientes[i];
            if (valor > 9) valor -= 9;
            suma += valor;
        }
        var digito = ((suma + 9) / 10) * 10 - suma;
        if (digito == 10) digito = 0;
        return digito == cedula[9] - '0';
    }

    private static string TraducirErrorEmpleado(string? error) => error switch
    {
        "en_uso" => "No se puede eliminar el empleado porque tiene registros relacionados.",
        "no_eliminado" => "No se pudo eliminar el empleado.",
        "no_encontrado" => "No se encontro el empleado solicitado.",
        _ => error ?? ""
    };

    private static string MensajeErrorGuardarEmpleado(Exception ex)
    {
        var detalle = ex.Message ?? "";
        if (detalle.Contains("cedula", StringComparison.OrdinalIgnoreCase)) return detalle;
        if (detalle.Contains("codigo", StringComparison.OrdinalIgnoreCase)) return detalle;
        return string.IsNullOrWhiteSpace(detalle)
            ? "No se pudo guardar el empleado. Revise los datos e intente nuevamente."
            : "No se pudo guardar el empleado. Detalle: " + detalle;
    }

    private sealed class FamiliarEntrada
    {
        public string? Codigo { get; set; }
        public string? CodigoParentesco { get; set; }
        public string? DescripcionParentesco { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? CargaFamiliar { get; set; }
        public string? Observacion { get; set; }
    }
}
