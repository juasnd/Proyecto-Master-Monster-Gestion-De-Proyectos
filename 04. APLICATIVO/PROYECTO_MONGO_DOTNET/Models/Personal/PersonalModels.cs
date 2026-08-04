namespace PROYECTO_MONGO_DOTNET.Models;

public class Departamento { public string Codigo { get; set; } = ""; public string Descripcion { get; set; } = ""; }
public class Cargo { public string PedepCodigo { get; set; } = ""; public string PecarCodigo { get; set; } = ""; public string PecarDescri { get; set; } = ""; public string NombreDepartamento { get; set; } = ""; public bool EsJefeProyecto { get; set; } }
public class Catalogo { public string Codigo { get; set; } = ""; public string Descripcion { get; set; } = ""; }
public class Parentesco { public string Codigo { get; set; } = ""; public string Descripcion { get; set; } = ""; }

public class Persona
{
    public string PeperCodigo { get; set; } = ""; public string Tipo { get; set; } = ""; public string PesexCodigo { get; set; } = ""; public string PeescCodigo { get; set; } = "";
    public string Nombres { get; set; } = ""; public string Apellidos { get; set; } = ""; public string Cedula { get; set; } = ""; public string FechaNacimiento { get; set; } = "";
    public int CargasFamiliares { get; set; } public string Direccion { get; set; } = ""; public string Celular { get; set; } = ""; public string TelefonoDomicilio { get; set; } = "";
    public string Email { get; set; } = ""; public string Foto { get; set; } = ""; public string SexoDescripcion { get; set; } = ""; public string EstadoCivilDescripcion { get; set; } = "";
}

public class Familiar
{
    public string Codigo { get; set; } = ""; public string CodigoPersona { get; set; } = ""; public string CodigoParentesco { get; set; } = ""; public string DescripcionParentesco { get; set; } = "";
    public string Nombre { get; set; } = ""; public string Apellido { get; set; } = ""; public string FechaNacimiento { get; set; } = ""; public string Telefono { get; set; } = "";
    public string CargaFamiliar { get; set; } = "S"; public string Observacion { get; set; } = "";
}

public class Formacion
{
    public string Codigo { get; set; } = ""; public string CodigoEmpleado { get; set; } = ""; public string Nivel { get; set; } = ""; public string Titulo { get; set; } = "";
    public string Institucion { get; set; } = ""; public string FechaInicio { get; set; } = ""; public string FechaFin { get; set; } = ""; public string Observacion { get; set; } = "";
}

public class Empleado
{
    public string PeempCodigo { get; set; } = ""; public string PedepCodigo { get; set; } = ""; public string PecarCodigo { get; set; } = ""; public string PedPedepCodigo { get; set; } = "";
    public string NombreDepartamento { get; set; } = ""; public string NombreCargo { get; set; } = ""; public Persona Persona { get; set; } = new();
    public List<Familiar> Familiares { get; set; } = new(); public List<Formacion> Formaciones { get; set; } = new();
}
