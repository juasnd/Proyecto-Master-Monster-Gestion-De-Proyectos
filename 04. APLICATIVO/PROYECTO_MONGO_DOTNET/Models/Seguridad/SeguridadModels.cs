namespace PROYECTO_MONGO_DOTNET.Models;

public class Usuario
{
    public string Codigo { get; set; } = "";
    public string CodigoPersona { get; set; } = "";
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string EstadoCodigo { get; set; } = "";
    public int IntentosFallidos { get; set; }
    public string CambioClave { get; set; } = "";
    public string NombrePersona { get; set; } = "";
    public string Cedula { get; set; } = "";
    public string Email { get; set; } = "";
    public string TipoPersona { get; set; } = "";
    public string CodigoEmpleado { get; set; } = "";
    public string Observacion { get; set; } = "";
    public DateTime? UltimoAcceso { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public Perfil? Perfil { get; set; }
    public bool EstaActivo => string.Equals(EstadoCodigo, "A", StringComparison.OrdinalIgnoreCase);
    public bool EstaBloqueado => string.Equals(EstadoCodigo, "B", StringComparison.OrdinalIgnoreCase);
    public bool RequiereCambioClave => string.Equals(CambioClave, "S", StringComparison.OrdinalIgnoreCase);
}

public class Perfil
{
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Observacion { get; set; } = "";
    public string EstadoCodigo { get; set; } = "A";
    public int UsuariosAsignados { get; set; }
    public bool EstaActivo => string.Equals(EstadoCodigo, "A", StringComparison.OrdinalIgnoreCase);
}

public class MenuOpcion
{
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Url { get; set; } = "";
    public string CodigoPadre { get; set; } = "";
    public int Nivel { get; set; }
    public int Orden { get; set; }
    public List<MenuOpcion> Hijos { get; set; } = new();
}

public sealed class PermisoPerfil
{
    public string CodigoOpcion { get; set; } = "";
    public string DescripcionOpcion { get; set; } = "";
    public string CodigoPadre { get; set; } = "";
    public int Nivel { get; set; }
    public int Orden { get; set; }
    public bool Ver { get; set; }
    public bool Crear { get; set; }
    public bool Editar { get; set; }
    public bool Eliminar { get; set; }
}
