using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Proyectos;

public sealed class SeguimientoProyectoServicio
{
    private readonly IProyectoRepositorio _proyectos;
    private readonly ISeguimientoProyectoRepositorio _seguimientos;
    private readonly IAsignacionProyectoRepositorio _asignaciones;
    private readonly IPlanificacionProyectoRepositorio _planes;
    private readonly CodigoDAO _codigos;
    private readonly PlanificacionProyectoServicio _planificacion;

    public SeguimientoProyectoServicio(IProyectoRepositorio proyectos, ISeguimientoProyectoRepositorio seguimientos,
        IAsignacionProyectoRepositorio asignaciones, IPlanificacionProyectoRepositorio planes,
        CodigoDAO codigos, PlanificacionProyectoServicio planificacion)
    {
        _proyectos = proyectos;
        _seguimientos = seguimientos;
        _asignaciones = asignaciones;
        _planes = planes;
        _codigos = codigos;
        _planificacion = planificacion;
    }

    public Task<IReadOnlyCollection<SeguimientoProyecto>> ListarAsync(string proyecto) => _seguimientos.PorProyectoAsync(proyecto);

    public async Task<Proyecto> CambiarEstadoAsync(string codigo, ActualizarEstadoDto dto, string usuario)
    {
        var proyecto = await Requerido(codigo);
        var destino = N(dto.Estado);
        var permitida = (proyecto.Estado, destino) switch
        {
            ("PLANIFICADO", "EN_CURSO") => true,
            ("EN_CURSO", "PAUSADO") => true,
            ("PAUSADO", "EN_CURSO") => true,
            _ => false
        };
        if (!permitida) throw ExcepcionNegocio.Conflicto($"No se permite cambiar de {proyecto.Estado} a {destino}. Use las acciones guiadas disponibles.");
        if (destino == "EN_CURSO" && proyecto.Estado == "PLANIFICADO")
        {
            var tareas = await _planes.TareasAsync(proyecto.Codigo);
            if (tareas.Count == 0 || tareas.Sum(x => x.Peso) != 100)
                throw ExcepcionNegocio.Conflicto("Para iniciar, el plan debe contener tareas con una cobertura total de 100 %.");
        }
        var observacion = string.IsNullOrWhiteSpace(dto.Observacion)
            ? destino == "PAUSADO" ? "Proyecto pausado." : proyecto.Estado == "PAUSADO" ? "Proyecto reanudado." : "Proyecto iniciado."
            : dto.Observacion.Trim();
        return await Cambiar(proyecto, destino, observacion, usuario);
    }

    public async Task<ValidacionCierreProyectoVm> ValidarFinalizacionAsync(string codigo)
    {
        var proyecto = await Requerido(codigo);
        var plan = await _planificacion.ObtenerAsync(codigo);
        var actividades = await _planes.ActividadesAsync(codigo);
        var tareas = await _planes.TareasAsync(codigo);
        var entregables = await _planes.EntregablesAsync(codigo);
        var pendientes = new List<string>();
        if (proyecto.Estado is not ("EN_CURSO" or "PAUSADO")) pendientes.Add("El proyecto debe estar En curso o Pausado.");
        if (!plan.Indicadores.PlanCompleto) pendientes.Add($"La cobertura del plan es {plan.Indicadores.PesoPlanificado:0.##} % y debe llegar a 100 %.");
        var tareasPendientes = tareas.Where(x => x.Estado != "COMPLETADA").ToList();
        if (tareasPendientes.Count > 0) pendientes.Add($"Hay {tareasPendientes.Count} tarea(s) pendiente(s): {string.Join(", ", tareasPendientes.Take(5).Select(x => x.Nombre))}.");
        var actividadesPendientes = actividades.Where(x => x.Estado != "COMPLETADA").ToList();
        if (actividadesPendientes.Count > 0) pendientes.Add($"Hay {actividadesPendientes.Count} actividad(es) sin cerrar.");
        var entregablesPendientes = entregables.Where(x => x.Estado is not ("ENTREGADO" or "JUSTIFICADO")).ToList();
        if (entregablesPendientes.Count > 0) pendientes.Add($"Hay {entregablesPendientes.Count} entregable(s) pendiente(s): {string.Join(", ", entregablesPendientes.Take(5).Select(x => x.Nombre))}.");
        return new ValidacionCierreProyectoVm { PuedeFinalizar = pendientes.Count == 0, Pendientes = pendientes };
    }

    public async Task<Proyecto> FinalizarAsync(string codigo, FinalizarProyectoDto dto, string usuario)
    {
        if (string.IsNullOrWhiteSpace(dto.ObservacionFinal)) throw new ExcepcionNegocio("La observación final es obligatoria.");
        var proyecto = await Requerido(codigo);
        if (dto.FechaFinReal == default || dto.FechaFinReal.Date < proyecto.FechaInicio.Date || dto.FechaFinReal.Date > DateTime.UtcNow.Date)
            throw new ExcepcionNegocio("La fecha real de finalización debe estar entre el inicio del proyecto y hoy.");
        await _planificacion.RecalcularAvanceAsync(codigo, usuario);
        proyecto = await Requerido(codigo);
        var validacion = await ValidarFinalizacionAsync(codigo);
        if (!validacion.PuedeFinalizar) throw ExcepcionNegocio.Conflicto(string.Join(" ", validacion.Pendientes));
        proyecto.Avance = 100;
        proyecto.FechaFinReal = DateTime.SpecifyKind(dto.FechaFinReal.Date, DateTimeKind.Utc);
        var actualizado = await Cambiar(proyecto, "FINALIZADO", dto.ObservacionFinal.Trim(), usuario);
        actualizado.FechaFinReal = proyecto.FechaFinReal;
        await _proyectos.ActualizarAsync(actualizado);
        await _asignaciones.CerrarActivasAsync(proyecto.Codigo, usuario, DateTime.UtcNow);
        return actualizado;
    }

    public async Task<Proyecto> CancelarAsync(string codigo, string motivo, string usuario)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new ExcepcionNegocio("El motivo de cancelación es obligatorio.");
        var proyecto = await Requerido(codigo);
        if (!proyecto.Activo || EstadosProyecto.Cerrados.Contains(proyecto.Estado)) throw ExcepcionNegocio.Conflicto("El proyecto ya está cerrado o inactivo.");
        var actualizado = await Cambiar(proyecto, "CANCELADO", motivo.Trim(), usuario);
        actualizado.FechaFinReal = DateTime.UtcNow.Date;
        await _proyectos.ActualizarAsync(actualizado);
        await _asignaciones.CerrarActivasAsync(proyecto.Codigo, usuario, DateTime.UtcNow);
        return actualizado;
    }

    private async Task<Proyecto> Cambiar(Proyecto proyecto, string estado, string observacion, string usuario)
    {
        if (!proyecto.Activo || EstadosProyecto.Cerrados.Contains(proyecto.Estado)) throw ExcepcionNegocio.Conflicto("El proyecto está cerrado o inactivo.");
        var seguimiento = new SeguimientoProyecto
        {
            CodigoSeguimiento = _codigos.GenerarCodigo("gepro_seguimiento", "CODIGOSEGUIMIENTO"),
            CodigoProyecto = proyecto.Codigo,
            AvanceAnterior = proyecto.Avance,
            AvanceNuevo = proyecto.Avance,
            EstadoAnterior = proyecto.Estado,
            EstadoNuevo = estado,
            Observacion = observacion,
            RegistradoPor = usuario,
            FechaRegistro = DateTime.UtcNow
        };
        proyecto.Estado = estado;
        proyecto.Observacion = observacion;
        proyecto.ModificadoPor = usuario;
        proyecto.FechaModificacion = seguimiento.FechaRegistro;
        await _proyectos.ActualizarAsync(proyecto);
        await _seguimientos.CrearAsync(seguimiento);
        return proyecto;
    }

    private async Task<Proyecto> Requerido(string codigo) => await _proyectos.ObtenerAsync(codigo) ?? throw ExcepcionNegocio.NoEncontrado("El proyecto no existe.");
    private static string N(string? valor) => (valor ?? "").Trim().ToUpperInvariant();
}
