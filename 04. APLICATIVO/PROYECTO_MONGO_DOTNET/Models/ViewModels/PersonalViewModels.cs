namespace PROYECTO_MONGO_DOTNET.Models;

public class DepartamentosVm
{
    public string Modo { get; set; } = "listar"; public string? Mensaje { get; set; } public string? Error { get; set; }
    public List<Departamento> Departamentos { get; set; } = new(); public Departamento? DepartamentoEditar { get; set; } public Departamento? DepartamentoVer { get; set; }
    public bool PuedeReporteDepartamentos { get; set; }
}
public class CargosVm
{
    public string Modo { get; set; } = "listar"; public string? Mensaje { get; set; } public string? Error { get; set; } public List<Cargo> Cargos { get; set; } = new();
    public List<Departamento> Departamentos { get; set; } = new(); public Cargo? CargoEditar { get; set; } public Cargo? CargoVer { get; set; } public bool PuedeReporteCargos { get; set; }
}
public class EmpleadosVm
{
    public string Modo { get; set; } = "listar"; public string? Mensaje { get; set; } public string? Error { get; set; } public string CriterioBusqueda { get; set; } = "codigo";
    public string TerminoBusqueda { get; set; } = ""; public List<Empleado> Empleados { get; set; } = new(); public Empleado EmpleadoForm { get; set; } = new();
    public List<Departamento> Departamentos { get; set; } = new(); public List<Cargo> Cargos { get; set; } = new(); public List<Catalogo> Sexos { get; set; } = new();
    public List<Catalogo> EstadosCiviles { get; set; } = new(); public List<Parentesco> Parentescos { get; set; } = new(); public bool PuedeReporteEmpleados { get; set; }
}
public class ReportesVm { public string? Mensaje { get; set; } public List<ReporteHistorial> Historial { get; set; } = new(); public ReporteHistorial? Detalle { get; set; } }
