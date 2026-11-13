using PROYECTO_MONGO_DOTNET.Services;

namespace PROYECTO_MONGO_DOTNET.Repositories;

public interface ICostoProyectoRepositorio
{
    Task<List<CostoProyectoFila>> ConsultarAsync(string? codigo = null);
}

public sealed class CostoProyectoFila
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Presupuesto { get; set; }
    public decimal Avance { get; set; }
    public decimal CostoLaboralPlanificado { get; set; }
    public decimal GastosAprobados { get; set; }
}

public sealed class CostoProyectoRepositorio(SqlContexto db) : ICostoProyectoRepositorio
{
    public Task<List<CostoProyectoFila>> ConsultarAsync(string? codigo = null) => db.ConsultarAsync<CostoProyectoFila>("""
        SELECT p.Codigo,p.Nombre,p.Presupuesto,p.Avance,
               COALESCE(t.Costo,0) CostoLaboralPlanificado,COALESCE(m.Total,0) GastosAprobados
        FROM app.Proyecto p
        OUTER APPLY (SELECT SUM(HorasPlanificadas*CostoHoraPlanificado) Costo
                     FROM app.TareaProyecto WHERE CodigoProyecto=p.Codigo AND Activo=1) t
        OUTER APPLY (SELECT SUM(Valor) Total FROM app.MovimientoProyecto
                     WHERE CodigoProyecto=p.Codigo AND Tipo='GASTO' AND Estado='APROBADO') m
        WHERE (@Codigo IS NULL OR p.Codigo=@Codigo)
        ORDER BY p.Codigo
        """, new { Codigo = codigo?.Trim().ToUpperInvariant() });
}
