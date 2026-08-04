using PROYECTO_MONGO_DOTNET.Models.Entidades;

namespace PROYECTO_MONGO_DOTNET.Models.ViewModels;

public sealed class ProyectosPaginaVm
{
    public string? CodigoProyecto { get; set; }
    public string? NombreJefeProyecto { get; set; }
    public IReadOnlyCollection<Departamento> Departamentos { get; set; } = [];
    public IReadOnlyCollection<Empleado> Empleados { get; set; } = [];
    public IReadOnlyCollection<Empleado> JefesProyecto { get; set; } = [];
    public bool PuedeReportar { get; set; }
    public bool PuedeCrear { get; set; }
    public bool PuedeEditar { get; set; }
    public bool PuedeInactivar { get; set; }
    public bool PuedeGestionarEquipo { get; set; }
    public bool PuedePlanificar { get; set; }
    public bool PuedeCambiarEstado { get; set; }
    public bool PuedeFinalizar { get; set; }
}

public sealed class ContratosPaginaVm
{
    public IReadOnlyCollection<Contrato> Contratos { get; set; } = [];
    public IReadOnlyCollection<Empleado> Empleados { get; set; } = [];
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class CategoriasPaginaVm
{
    public IReadOnlyCollection<CategoriaGasto> Categorias { get; set; } = [];
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class ParametrosPaginaVm
{
    public IReadOnlyCollection<ParametroFinanciero> Parametros { get; set; } = [];
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class MovimientosPaginaVm
{
    public IReadOnlyCollection<MovimientoProyecto> Movimientos { get; set; } = [];
    public IReadOnlyCollection<Proyecto> Proyectos { get; set; } = [];
    public IReadOnlyCollection<CategoriaGasto> Categorias { get; set; } = [];
    public bool ModoAprobacion { get; set; }
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class ControlPresupuestarioPaginaVm
{
    public IReadOnlyCollection<CostoProyectoResumen> Proyectos { get; set; } = [];
}

public sealed class RestablecerClavePaginaVm
{
    public IReadOnlyCollection<Usuario> Usuarios { get; set; } = [];
    public string? LoginSeleccionado { get; set; }
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class PersonaUsuarioVm
{
    public string Codigo { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
    public string Cedula { get; set; } = "";
    public string Email { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string CodigoEmpleado { get; set; } = "";
}

public sealed class UsuariosPaginaVm
{
    public IReadOnlyCollection<Usuario> Usuarios { get; set; } = [];
    public IReadOnlyCollection<Perfil> Perfiles { get; set; } = [];
    public IReadOnlyCollection<PersonaUsuarioVm> PersonasDisponibles { get; set; } = [];
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class PerfilesPaginaVm
{
    public IReadOnlyCollection<Perfil> Perfiles { get; set; } = [];
    public string Modo { get; set; } = "listar";
    public Perfil? PerfilEditar { get; set; }
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class PermisosPaginaVm
{
    public IReadOnlyCollection<Perfil> Perfiles { get; set; } = [];
    public Perfil? PerfilSeleccionado { get; set; }
    public IReadOnlyCollection<PermisoPerfil> Permisos { get; set; } = [];
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public sealed class UsuarioPerfilProcesoVm
{
    public string CodigoPersona { get; set; } = "";
    public string Login { get; set; } = "";
    public string Cedula { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string PerfilCodigo { get; set; } = "";
    public string PerfilDescripcion { get; set; } = "Sin perfil";
    public string NombreCompleto => string.IsNullOrWhiteSpace((Apellidos + " " + Nombres).Trim()) ? Login : (Apellidos + " " + Nombres).Trim();
}

public sealed class UsuariosPerfilPaginaVm
{
    public IReadOnlyCollection<Perfil> Perfiles { get; set; } = [];
    public Perfil? PerfilSeleccionado { get; set; }
    public IReadOnlyCollection<UsuarioPerfilProcesoVm> UsuariosDisponibles { get; set; } = [];
    public IReadOnlyCollection<UsuarioPerfilProcesoVm> UsuariosAsignados { get; set; } = [];
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}
