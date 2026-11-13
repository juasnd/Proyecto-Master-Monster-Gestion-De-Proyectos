using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;

namespace PROYECTO_MONGO_DOTNET.Services.Finanzas;

public static class ReglasFinanzas
{
    public static void ValidarMovimiento(GuardarMovimientoDto dto)
    {
        var errores = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto,
                new System.ComponentModel.DataAnnotations.ValidationContext(dto), errores, true))
            throw new ExcepcionNegocio(string.Join(" ", errores.Select(e => e.ErrorMessage)));
        if (dto.Fecha == default) throw new ExcepcionNegocio("La fecha es obligatoria.");
        if (dto.Tipo.Trim().ToUpperInvariant() is not ("GASTO" or "INGRESO"))
            throw new ExcepcionNegocio("El tipo de movimiento no es válido.");
    }

    public static decimal CostoHora(decimal sueldoBase, decimal horasMensuales)
    {
        if (sueldoBase <= 0) throw new ExcepcionNegocio("El sueldo base debe ser mayor que cero.");
        if (horasMensuales <= 0) throw new ExcepcionNegocio("Las horas mensuales deben ser mayores que cero.");
        return decimal.Round(sueldoBase / horasMensuales, 6, MidpointRounding.AwayFromZero);
    }

    public static decimal CostoTotal(decimal costoLaboral, decimal gastosAprobados) => costoLaboral + gastosAprobados;
    public static decimal Saldo(decimal presupuesto, decimal costoTotal) => presupuesto - costoTotal;
}
