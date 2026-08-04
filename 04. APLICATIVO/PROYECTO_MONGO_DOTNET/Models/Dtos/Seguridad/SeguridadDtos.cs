using System.ComponentModel.DataAnnotations;

namespace PROYECTO_MONGO_DOTNET.Models.Dtos.Seguridad;

public sealed class RestablecerClaveDto
{
    [Required] public string Login { get; set; } = "";
    [Required, DataType(DataType.Password)] public string ClaveTemporal { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(ClaveTemporal))] public string Confirmacion { get; set; } = "";
}

public sealed class CambiarClaveDto
{
    [Required, DataType(DataType.Password)] public string ClaveActual { get; set; } = "";
    [Required, DataType(DataType.Password)] public string NuevaClave { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(NuevaClave))] public string Confirmacion { get; set; } = "";
}

public sealed class CrearUsuarioDto
{
    [Required] public string TipoRegistro { get; set; } = "existente";
    public string CodigoPersona { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Cedula { get; set; } = "";
    [EmailAddress] public string Email { get; set; } = "";
    [Required, StringLength(60, MinimumLength = 3)] public string Login { get; set; } = "";
    [Required, DataType(DataType.Password)] public string ClaveTemporal { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(ClaveTemporal))] public string Confirmacion { get; set; } = "";
    [Required] public string PerfilCodigo { get; set; } = "";
}

public sealed class AsignarPerfilUsuarioDto
{
    [Required] public string Login { get; set; } = "";
    [Required] public string PerfilCodigo { get; set; } = "";
}

public sealed class CambiarEstadoUsuarioDto
{
    [Required] public string Login { get; set; } = "";
    [Required] public string Estado { get; set; } = "";
}

public sealed class GuardarPerfilDto
{
    [Required, StringLength(20, MinimumLength = 3)] public string Codigo { get; set; } = "";
    [Required, StringLength(80)] public string Descripcion { get; set; } = "";
    [StringLength(300)] public string Observacion { get; set; } = "";
}

public sealed class CambiarEstadoPerfilDto
{
    [Required] public string Codigo { get; set; } = "";
    [Required] public string Estado { get; set; } = "";
}

public sealed class GuardarPermisosPerfilDto
{
    [Required] public string PerfilCodigo { get; set; } = "";
    public List<PermisoOpcionDto> Permisos { get; set; } = [];
}

public sealed class PermisoOpcionDto
{
    [Required] public string CodigoOpcion { get; set; } = "";
    public bool Ver { get; set; }
    public bool Crear { get; set; }
    public bool Editar { get; set; }
    public bool Eliminar { get; set; }
}
