using PROYECTO_MONGO_DOTNET.Models.Entidades;

namespace PROYECTO_MONGO_DOTNET.Services.Proyectos;

public static class ReglasPlanificacion
{
    // Los pesos se expresan como porcentaje del proyecto; un plan incompleto no se renormaliza a 100.
    public static decimal AvanceReal(IEnumerable<TareaProyecto> tareas) =>
        decimal.Round(tareas.Sum(t => t.Peso * EstadosTarea.Factor(t.Estado)), 2);

    public static bool TieneCiclo(IEnumerable<DependenciaTarea> dependencias)
    {
        var salidas = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var entradas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var dependencia in dependencias)
        {
            var origen = dependencia.CodigoPredecesora;
            var destino = dependencia.CodigoTarea;
            entradas.TryAdd(origen, 0);
            entradas.TryAdd(destino, 0);
            if (!salidas.TryGetValue(origen, out var siguientes)) salidas[origen] = siguientes = new(StringComparer.OrdinalIgnoreCase);
            if (siguientes.Add(destino)) entradas[destino]++;
        }
        var disponibles = new Queue<string>(entradas.Where(x => x.Value == 0).Select(x => x.Key));
        var visitadas = 0;
        while (disponibles.TryDequeue(out var tarea))
        {
            visitadas++;
            if (!salidas.TryGetValue(tarea, out var siguientes)) continue;
            foreach (var siguiente in siguientes)
                if (--entradas[siguiente] == 0) disponibles.Enqueue(siguiente);
        }
        return visitadas != entradas.Count;
    }
}
