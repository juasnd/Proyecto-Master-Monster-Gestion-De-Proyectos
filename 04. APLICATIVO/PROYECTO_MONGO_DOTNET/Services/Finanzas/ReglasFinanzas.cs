using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Finanzas;

public static class ReglasFinanzas
{
    public static decimal CostoHora(decimal sueldoBase, decimal horasMensuales)
    {
        if (sueldoBase <= 0) throw new ExcepcionNegocio("El sueldo base debe ser mayor que cero.");
        if (horasMensuales <= 0) throw new ExcepcionNegocio("Las horas mensuales deben ser mayores que cero.");
        return decimal.Round(sueldoBase / horasMensuales, 6, MidpointRounding.AwayFromZero);
    }

    public static decimal CostoTotal(decimal costoLaboral, decimal gastosAprobados) => costoLaboral + gastosAprobados;
    public static decimal Saldo(decimal presupuesto, decimal costoTotal) => presupuesto - costoTotal;
}
