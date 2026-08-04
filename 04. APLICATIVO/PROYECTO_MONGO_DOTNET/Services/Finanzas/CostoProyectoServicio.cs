using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Finanzas;

public sealed class CostoProyectoServicio
{
    private readonly IProyectoRepositorio _proyectos;
    private readonly IPlanificacionProyectoRepositorio _planes;
    private readonly MovimientoProyectoRepositorio _movimientos;
    private readonly ParametroFinancieroRepositorio _parametros;

    public CostoProyectoServicio(IProyectoRepositorio proyectos, IPlanificacionProyectoRepositorio planes,
        MovimientoProyectoRepositorio movimientos, ParametroFinancieroRepositorio parametros)
    {
        _proyectos = proyectos;
        _planes = planes;
        _movimientos = movimientos;
        _parametros = parametros;
    }

    public async Task<CostoProyectoResumen> CalcularAsync(string codigoProyecto)
    {
        var proyecto = await _proyectos.ObtenerAsync(codigoProyecto) ?? throw ExcepcionNegocio.NoEncontrado("El proyecto no existe.");
        var tareas = await _planes.TareasAsync(proyecto.Codigo);
        var costoLaboralPlanificado = tareas.Sum(x => x.HorasPlanificadas * x.CostoHoraPlanificado);
        var gastosAprobados = await _movimientos.GastosAprobadosAsync(proyecto.Codigo);
        var costoComprometido = decimal.Round(costoLaboralPlanificado * proyecto.Avance / 100m + gastosAprobados, 2);
        var consumo = proyecto.Presupuesto <= 0 ? (costoComprometido > 0 ? 100 : 0) : decimal.Round(costoComprometido / proyecto.Presupuesto * 100, 2);
        var parametro = await _parametros.ObtenerAsync("PORCENTAJE_ALERTA_PRESUPUESTO");
        var alerta = parametro?.ValorDecimal ?? parametro?.ValorEntero ?? 80;
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
        var resultado = new List<CostoProyectoResumen>();
        foreach (var proyecto in await _proyectos.ListarAsync(true)) resultado.Add(await CalcularAsync(proyecto.Codigo));
        return resultado;
    }
}
