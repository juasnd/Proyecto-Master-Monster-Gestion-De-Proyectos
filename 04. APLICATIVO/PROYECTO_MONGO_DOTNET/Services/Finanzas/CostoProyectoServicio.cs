using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Finanzas;

public sealed class CostoProyectoServicio
{
    private readonly ICostoProyectoRepositorio _costos;
    private readonly ParametroFinancieroRepositorio _parametros;

    public CostoProyectoServicio(ICostoProyectoRepositorio costos, ParametroFinancieroRepositorio parametros)
    {
        _costos = costos;
        _parametros = parametros;
    }

    public async Task<CostoProyectoResumen> CalcularAsync(string codigoProyecto)
    {
        var proyecto = (await _costos.ConsultarAsync(codigoProyecto)).FirstOrDefault() ?? throw ExcepcionNegocio.NoEncontrado("El proyecto no existe.");
        return Calcular(proyecto, await UmbralAsync());
    }

    public static CostoProyectoResumen Calcular(CostoProyectoFila proyecto, decimal alerta)
    {
        var costoLaboralPlanificado = proyecto.CostoLaboralPlanificado;
        var gastosAprobados = proyecto.GastosAprobados;
        var costoComprometido = decimal.Round(ReglasFinanzas.CostoTotal(costoLaboralPlanificado * proyecto.Avance / 100m, gastosAprobados), 2);
        var consumo = proyecto.Presupuesto <= 0 ? (costoComprometido > 0 ? 100 : 0) : decimal.Round(costoComprometido / proyecto.Presupuesto * 100, 2);
        return new CostoProyectoResumen
        {
            CodigoProyecto = proyecto.Codigo,
            NombreProyecto = proyecto.Nombre,
            Presupuesto = proyecto.Presupuesto,
            CostoLaboralPlanificado = costoLaboralPlanificado,
            GastosAprobados = gastosAprobados,
            CostoComprometidoEstimado = costoComprometido,
            SaldoDisponible = ReglasFinanzas.Saldo(proyecto.Presupuesto, costoComprometido),
            PorcentajeConsumo = consumo,
            EnAlerta = consumo >= alerta,
            Excedido = costoComprometido > proyecto.Presupuesto
        };
    }

    public async Task<IReadOnlyCollection<CostoProyectoResumen>> ListarAsync()
    {
        var proyectos = await _costos.ConsultarAsync();
        var alerta = await UmbralAsync();
        return proyectos.Select(proyecto => Calcular(proyecto, alerta)).ToList();
    }

    private async Task<decimal> UmbralAsync()
    {
        var parametro = await _parametros.ObtenerAsync("PORCENTAJE_ALERTA_PRESUPUESTO");
        return parametro?.ValorDecimal ?? parametro?.ValorEntero ?? 80;
    }
}
