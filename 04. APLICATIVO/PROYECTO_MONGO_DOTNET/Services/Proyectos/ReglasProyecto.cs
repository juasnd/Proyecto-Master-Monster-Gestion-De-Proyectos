using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Proyectos;

public static class ReglasProyecto
{
    public static void Validar(GuardarProyectoDto dto)
    {
        var errores = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(dto.Nombre)) errores["nombre"] = ["El nombre es obligatorio."];
        if (string.IsNullOrWhiteSpace(dto.CodigoDepartamento)) errores["codigoDepartamento"] = ["El departamento es obligatorio."];
        if (string.IsNullOrWhiteSpace(dto.CodigoJefeProyecto)) errores["codigoJefeProyecto"] = ["El jefe del proyecto es obligatorio."];
        if (dto.FechaInicio == default) errores["fechaInicio"] = ["La fecha inicial es obligatoria."];
        if (dto.FechaFinPlanificada == default) errores["fechaFinPlanificada"] = ["La fecha final planificada es obligatoria."];
        if (dto.FechaFinPlanificada.Date < dto.FechaInicio.Date) errores["fechaFinPlanificada"] = ["La fecha final no puede ser anterior a la fecha inicial."];
        if (dto.Presupuesto < 0) errores["presupuesto"] = ["El presupuesto no puede ser negativo."];
        if (!PrioridadesProyecto.Todas.Contains(N(dto.Prioridad))) errores["prioridad"] = ["La prioridad indicada no es válida."];
        if (errores.Count > 0) throw new ExcepcionNegocio("Revise los campos señalados.", errores: errores);
    }

    public static void ValidarAsignacion(GuardarAsignacionDto dto, decimal disponibilidadUsada)
    {
        var errores = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(dto.Rol)) errores["rol"] = ["El rol dentro del proyecto es obligatorio."];
        if (dto.DisponibilidadAsignada is < 1 or > 100) errores["disponibilidadAsignada"] = ["La disponibilidad asignada debe estar entre 1 y 100 %."];
        if (disponibilidadUsada + dto.DisponibilidadAsignada > 100)
            errores["disponibilidadAsignada"] = [$"La disponibilidad usada sería {disponibilidadUsada + dto.DisponibilidadAsignada:0.##} %. El máximo es 100 %. Actualmente tiene {disponibilidadUsada:0.##} % comprometido en proyectos activos con fechas coincidentes."];
        if (dto.FechaInicio == default) errores["fechaInicio"] = ["La fecha inicial es obligatoria."];
        if (dto.FechaFin == default || dto.FechaFin.Date < dto.FechaInicio.Date) errores["fechaFin"] = ["La fecha final no puede ser anterior a la inicial."];
        if (errores.Count > 0) throw new ExcepcionNegocio("Revise la asignación.", errores: errores);
    }

    public static void ValidarAsignacionDisponible(bool yaExisteAsignacionActiva)
    {
        if (yaExisteAsignacionActiva) throw ExcepcionNegocio.Conflicto("El empleado ya tiene una asignación activa en el proyecto.");
    }

    public static void ValidarCambioAvance(decimal anterior, decimal nuevo, string? observacion)
    {
        if (nuevo is < 0 or > 100) throw new ExcepcionNegocio("El avance debe estar entre 0 y 100.");
        if (nuevo < anterior && string.IsNullOrWhiteSpace(observacion))
            throw new ExcepcionNegocio("Debe justificar la reducción del avance.");
    }

    private static string N(string? valor) => (valor ?? "").Trim().ToUpperInvariant();
}
