using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;

namespace PROYECTO_MONGO_DOTNET.Services.Proyectos;

public sealed class PlanificacionProyectoServicio
{
    private readonly IPlanificacionProyectoRepositorio _planes;
    private readonly IProyectoRepositorio _proyectos;
    private readonly IAsignacionProyectoRepositorio _asignaciones;
    private readonly ISeguimientoProyectoRepositorio _seguimientos;
    private readonly CostoProyectoServicio _costos;
    private readonly AsignacionProyectoServicio _equipo;
    private readonly CodigoDAO _codigos;

    public PlanificacionProyectoServicio(IPlanificacionProyectoRepositorio planes, IProyectoRepositorio proyectos,
        IAsignacionProyectoRepositorio asignaciones, ISeguimientoProyectoRepositorio seguimientos,
        CostoProyectoServicio costos, AsignacionProyectoServicio equipo, CodigoDAO codigos)
    {
        _planes = planes; _proyectos = proyectos; _asignaciones = asignaciones;
        _seguimientos = seguimientos; _costos = costos; _equipo = equipo; _codigos = codigos;
    }

    public async Task<PlanificacionProyectoVm> ObtenerAsync(string codigoProyecto)
    {
        var proyecto = await ProyectoRequerido(codigoProyecto);
        var fases = (await _planes.FasesAsync(proyecto.Codigo)).ToList();
        var actividades = (await _planes.ActividadesAsync(proyecto.Codigo)).ToList();
        var tareas = (await _planes.TareasAsync(proyecto.Codigo)).ToList();
        var dependencias = (await _planes.DependenciasAsync(proyecto.Codigo)).ToList();
        var entregables = (await _planes.EntregablesAsync(proyecto.Codigo)).ToList();
        var costo = await _costos.CalcularAsync(proyecto.Codigo);
        var hoy = DateTime.UtcNow.Date;
        var modelosTareas = tareas.ToDictionary(t => t.CodigoTarea, t =>
        {
            return new TareaPlanificacionVm
            {
                Tarea = t, Avance = decimal.Round(EstadosTarea.Factor(t.Estado) * 100, 2), AvancePlanificado = AvancePlanificado(t, hoy),
                Dependencias = dependencias.Where(d => d.CodigoTarea == t.CodigoTarea).ToList(),
                Bloqueada = EstaBloqueada(t, dependencias, tareas),
                Entregables = entregables.Where(e => e.CodigoTarea == t.CodigoTarea).ToList()
            };
        });
        var grupos = fases.Select(f =>
        {
            var tareasFase = tareas.Where(t => t.CodigoFase == f.CodigoFase).ToList();
            var peso = tareasFase.Sum(t => t.Peso);
            var actividadesFase = actividades.Where(a => a.CodigoFase == f.CodigoFase).Select(a =>
            {
                var tareasActividad = tareas.Where(t => t.CodigoActividad == a.CodigoActividad).ToList();
                var pesoActividad = tareasActividad.Sum(t => t.Peso);
                return new ActividadPlanificacionVm
                {
                    Actividad = a, Peso = pesoActividad,
                    Avance = pesoActividad == 0 ? 0 : decimal.Round(tareasActividad.Sum(t => t.Peso * EstadosTarea.Factor(t.Estado)) / pesoActividad * 100, 2),
                    Tareas = tareasActividad.Select(t => modelosTareas[t.CodigoTarea]).ToList(),
                    Entregables = entregables.Where(e => e.TipoRelacion == "ACTIVIDAD" && e.CodigoRelacion == a.CodigoActividad).ToList()
                };
            }).ToList();
            return new FasePlanificacionVm
            {
                Fase = f,
                Peso = peso,
                Avance = peso == 0 ? 0 : decimal.Round(tareasFase.Sum(t => t.Peso * EstadosTarea.Factor(t.Estado)) / peso * 100, 2),
                Actividades = actividadesFase,
                Tareas = tareasFase.Select(t => modelosTareas[t.CodigoTarea]).ToList(),
                Entregables = entregables.Where(e => e.TipoRelacion == "FASE" && e.CodigoRelacion == f.CodigoFase).ToList()
            };
        }).ToList();
        var pesoTotal = tareas.Sum(t => t.Peso);
        var avanceReal = decimal.Round(tareas.Sum(t => t.Peso * EstadosTarea.Factor(t.Estado)), 2);
        var avancePlanificado = decimal.Round(tareas.Sum(t => t.Peso * AvancePlanificado(t, hoy) / 100), 2);
        var indicadores = new IndicadoresPlanificacionProyecto
        {
            PesoPlanificado = pesoTotal,
            AvancePlanificado = avancePlanificado,
            AvanceReal = avanceReal,
            DesviacionAvance = decimal.Round(avanceReal - avancePlanificado, 2),
            Presupuesto = proyecto.Presupuesto,
            CostoPlanificadoLaboral = tareas.Sum(t => t.HorasPlanificadas * t.CostoHoraPlanificado),
            GastosAprobados = costo.GastosAprobados,
            CostoComprometidoEstimado = costo.CostoComprometidoEstimado,
            SaldoDisponible = costo.SaldoDisponible,
            HorasPlanificadas = tareas.Sum(t => t.HorasPlanificadas),
            TareasTotales = tareas.Count,
            TareasCompletadas = tareas.Count(t => t.Estado == "COMPLETADA"),
            EntregablesTotales = entregables.Count,
            EntregablesEntregados = entregables.Count(e => e.Estado is "ENTREGADO" or "JUSTIFICADO"),
            PlanCompleto = tareas.Count > 0 && pesoTotal == 100
        };
        return new PlanificacionProyectoVm { Proyecto = proyecto, Indicadores = indicadores, Fases = grupos, Entregables = entregables, Equipo = await _equipo.EquipoAsync(proyecto.Codigo) };
    }

    public Task<IReadOnlyCollection<TareaProyecto>> TareasAsync(string proyecto) => _planes.TareasAsync(N(proyecto));

    public async Task<FaseProyecto> CrearFaseAsync(string proyecto, GuardarFaseProyectoDto dto, string usuario)
    {
        var p = await ProyectoEditable(proyecto); ValidarFase(dto, p);
        var x = new FaseProyecto { CodigoFase = _codigos.GenerarCodigo("gepro_fase", "CODIGOFASE"), CodigoProyecto = p.Codigo,
            Nombre = dto.Nombre.Trim(), Descripcion = dto.Descripcion.Trim(), Orden = dto.Orden, FechaInicio = Utc(dto.FechaInicio),
            FechaFin = Utc(dto.FechaFin), CreadoPor = usuario, FechaCreacion = DateTime.UtcNow };
        await _planes.CrearFaseAsync(x); return x;
    }

    public async Task<FaseProyecto> ActualizarFaseAsync(string proyecto, string fase, GuardarFaseProyectoDto dto, string usuario)
    {
        var p = await ProyectoEditable(proyecto); ValidarFase(dto, p);
        var x = await FaseRequerida(p.Codigo, fase);
        var actividades = (await _planes.ActividadesAsync(p.Codigo)).Where(a => a.CodigoFase == x.CodigoFase).ToList();
        if (actividades.Any(a => a.FechaInicio.Date < dto.FechaInicio.Date || a.FechaFin.Date > dto.FechaFin.Date))
            throw ExcepcionNegocio.Conflicto("Las fechas de la fase deben contener todas sus actividades.");
        x.Nombre = dto.Nombre.Trim(); x.Descripcion = dto.Descripcion.Trim(); x.Orden = dto.Orden;
        x.FechaInicio = Utc(dto.FechaInicio); x.FechaFin = Utc(dto.FechaFin); Marcar(x, usuario);
        await _planes.ActualizarFaseAsync(x); return x;
    }

    public async Task EliminarFaseAsync(string proyecto, string fase, string usuario)
    {
        await ProyectoEditable(proyecto); var x = await FaseRequerida(proyecto, fase);
        if ((await _planes.ActividadesAsync(proyecto)).Any(a => a.CodigoFase == x.CodigoFase)) throw ExcepcionNegocio.Conflicto("No se puede eliminar una fase que contiene actividades activas.");
        x.Activo = false; Marcar(x, usuario); await _planes.ActualizarFaseAsync(x);
    }

    public async Task<ActividadProyecto> CrearActividadAsync(string proyecto, string fase, GuardarActividadProyectoDto dto, string usuario)
    {
        var p=await ProyectoEditable(proyecto); var f=await FaseRequerida(p.Codigo,fase); await ValidarActividad(dto,p,f);
        var x=new ActividadProyecto { CodigoActividad=_codigos.GenerarCodigo("gepro_actividad","CODIGOACTIVIDAD"),CodigoFase=f.CodigoFase,CodigoProyecto=p.Codigo,
            Nombre=dto.Nombre.Trim(),Descripcion=dto.Descripcion.Trim(),CodigoResponsable=N(dto.CodigoResponsable),Orden=dto.Orden,FechaInicio=Utc(dto.FechaInicio),
            FechaFin=Utc(dto.FechaFin),CreadoPor=usuario,FechaCreacion=DateTime.UtcNow };
        await _planes.CrearActividadAsync(x); return x;
    }

    public async Task<ActividadProyecto> ActualizarActividadAsync(string proyecto,string actividad,GuardarActividadProyectoDto dto,string usuario)
    {
        var p=await ProyectoEditable(proyecto); var x=await ActividadRequerida(p.Codigo,actividad); var f=await FaseRequerida(p.Codigo,x.CodigoFase); await ValidarActividad(dto,p,f);
        var tareas=(await _planes.TareasAsync(p.Codigo)).Where(t=>t.CodigoActividad==x.CodigoActividad).ToList();
        if(tareas.Any(t=>t.FechaInicio.Date<dto.FechaInicio.Date||t.FechaFin.Date>dto.FechaFin.Date)) throw ExcepcionNegocio.Conflicto("Las fechas de la actividad deben contener todas sus tareas.");
        x.Nombre=dto.Nombre.Trim();x.Descripcion=dto.Descripcion.Trim();x.CodigoResponsable=N(dto.CodigoResponsable);x.Orden=dto.Orden;x.FechaInicio=Utc(dto.FechaInicio);x.FechaFin=Utc(dto.FechaFin);Marcar(x,usuario);
        await _planes.ActualizarActividadAsync(x);return x;
    }

    public async Task EliminarActividadAsync(string proyecto,string actividad,string usuario)
    {
        await ProyectoEditable(proyecto);var x=await ActividadRequerida(proyecto,actividad);
        if((await _planes.TareasAsync(proyecto)).Any(t=>t.CodigoActividad==x.CodigoActividad)) throw ExcepcionNegocio.Conflicto("No se puede eliminar una actividad que contiene tareas activas.");
        x.Activo=false;Marcar(x,usuario);await _planes.ActualizarActividadAsync(x);
    }

    public async Task<TareaProyecto> CrearTareaAsync(string proyecto, string actividad, GuardarTareaProyectoDto dto, string usuario)
    {
        var p = await ProyectoEditable(proyecto); var a = await ActividadRequerida(p.Codigo, actividad); var f=await FaseRequerida(p.Codigo,a.CodigoFase);
        var asignacion = await ValidarTarea(dto, p, a, null);
        var codigoTarea = _codigos.GenerarCodigo("gepro_tarea", "CODIGOTAREA");
        var dependencias = await ValidarDependenciasAsync(p, codigoTarea, dto, usuario);
        var x = new TareaProyecto { CodigoTarea = codigoTarea, CodigoProyecto = p.Codigo,
            CodigoActividad=a.CodigoActividad,CodigoFase = f.CodigoFase, Nombre = dto.Nombre.Trim(), Descripcion = dto.Descripcion.Trim(), CodigoResponsable = N(dto.CodigoResponsable),
            FechaInicio = Utc(dto.FechaInicio), FechaFin = Utc(dto.FechaFin), Peso = dto.Peso, HorasPlanificadas = dto.HorasPlanificadas,
            CostoHoraPlanificado = asignacion.CostoHoraSnapshot ?? 0, CreadoPor = usuario, FechaCreacion = DateTime.UtcNow };
        await _planes.CrearTareaAsync(x); await _planes.ReemplazarDependenciasAsync(p.Codigo, x.CodigoTarea, dependencias);
        await RecalcularAsync(p, usuario, "Tarea incorporada a la planificación."); return x;
    }

    public async Task<TareaProyecto> ActualizarTareaAsync(string proyecto, string tarea, GuardarTareaProyectoDto dto, string usuario)
    {
        var p = await ProyectoEditable(proyecto); var x = await TareaRequerida(p.Codigo, tarea); var actividadAnterior=x.CodigoActividad;
        var a=await ActividadRequerida(p.Codigo,string.IsNullOrWhiteSpace(dto.CodigoActividad)?x.CodigoActividad:dto.CodigoActividad);
        var asignacion = await ValidarTarea(dto, p, a, x.CodigoTarea);
        var dependencias = await ValidarDependenciasAsync(p, x.CodigoTarea, dto, usuario);
        x.CodigoActividad=a.CodigoActividad;x.CodigoFase = a.CodigoFase; x.Nombre = dto.Nombre.Trim(); x.Descripcion = dto.Descripcion.Trim(); x.CodigoResponsable = N(dto.CodigoResponsable);
        x.FechaInicio = Utc(dto.FechaInicio); x.FechaFin = Utc(dto.FechaFin); x.Peso = dto.Peso;
        x.HorasPlanificadas = dto.HorasPlanificadas; x.CostoHoraPlanificado = asignacion.CostoHoraSnapshot ?? 0; Marcar(x, usuario);
        await _planes.ActualizarTareaAsync(x); await _planes.ReemplazarDependenciasAsync(p.Codigo, x.CodigoTarea, dependencias);
        if(actividadAnterior!=x.CodigoActividad) await ActualizarEstadoActividadAsync(p.Codigo,actividadAnterior,usuario);
        await ActualizarEstadoActividadAsync(p.Codigo,x.CodigoActividad,usuario);await ActualizarEstadoFaseAsync(p.Codigo,x.CodigoFase,usuario);
        await RecalcularAsync(p, usuario, "Tarea actualizada en la planificación."); return x;
    }

    public async Task<TareaProyecto> CambiarEstadoTareaAsync(string proyecto, string tarea, string estado, string usuario)
    {
        var p = await ProyectoEditable(proyecto); var x = await TareaRequerida(p.Codigo, tarea); estado = N(estado);
        if (!EstadosTarea.Todos.Contains(estado)) throw new ExcepcionNegocio("El estado de la tarea no es válido.");
        var dependencias = (await _planes.DependenciasAsync(p.Codigo)).Where(d => d.CodigoTarea == x.CodigoTarea).ToList();
        var tareas = (await _planes.TareasAsync(p.Codigo)).ToDictionary(t => t.CodigoTarea);
        if (estado is "EN_PROCESO" or "COMPLETADA")
        {
            var bloqueantes = dependencias.Where(d => !DependenciaCumplidaParaEstado(d, estado, tareas)).Select(d => d.NombrePredecesora).ToList();
            if (bloqueantes.Count > 0) throw ExcepcionNegocio.Conflicto($"La tarea continúa bloqueada por: {string.Join(", ", bloqueantes)}.");
        }
        var entregables = (await _planes.EntregablesAsync(p.Codigo)).Where(e => e.CodigoTarea == x.CodigoTarea).ToList();
        if (estado == "COMPLETADA" && entregables.Any(e => e.Estado != "ENTREGADO"))
            throw ExcepcionNegocio.Conflicto("Antes de completar la tarea deben marcarse como entregados todos sus entregables.");
        x.Estado = estado; x.FechaCompletada = estado == "COMPLETADA" ? DateTime.UtcNow : null; Marcar(x, usuario);
        await _planes.ActualizarTareaAsync(x); await ActualizarEstadoActividadAsync(p.Codigo,x.CodigoActividad,usuario); await ActualizarEstadoFaseAsync(p.Codigo, x.CodigoFase, usuario);
        await RecalcularAsync(p, usuario, $"La tarea {x.CodigoTarea} cambió a {estado}."); return x;
    }

    public async Task EliminarTareaAsync(string proyecto, string tarea, string usuario)
    {
        var p = await ProyectoEditable(proyecto); var x = await TareaRequerida(p.Codigo, tarea);
        var dependencias = await _planes.DependenciasAsync(p.Codigo);
        if (dependencias.Any(d => d.CodigoPredecesora == x.CodigoTarea)) throw ExcepcionNegocio.Conflicto("No se puede eliminar la tarea porque otras tareas dependen de ella.");
        await _planes.ReemplazarDependenciasAsync(p.Codigo, x.CodigoTarea, []);
        x.Activo = false; Marcar(x, usuario); await _planes.ActualizarTareaAsync(x);
        await ActualizarEstadoActividadAsync(p.Codigo,x.CodigoActividad,usuario);await ActualizarEstadoFaseAsync(p.Codigo, x.CodigoFase, usuario); await RecalcularAsync(p, usuario, "Tarea retirada de la planificación.");
    }

    public Task<EntregableProyecto> CrearEntregableAsync(string proyecto, string tarea, GuardarEntregableProyectoDto dto, string usuario)
    {
        dto.TipoRelacion = "TAREA";
        dto.CodigoRelacion = tarea;
        return CrearEntregableAsync(proyecto, dto, usuario);
    }

    public async Task<EntregableProyecto> CrearEntregableAsync(string proyecto, GuardarEntregableProyectoDto dto, string usuario)
    {
        var p = await ProyectoEditable(proyecto);
        var relacion = await ValidarEntregableAsync(p.Codigo, dto);
        var x = new EntregableProyecto
        {
            CodigoEntregable = _codigos.GenerarCodigo("gepro_entregable", "CODIGOENTREGABLE"),
            CodigoProyecto = p.Codigo,
            CodigoTarea = relacion.CodigoTarea,
            TipoRelacion = N(dto.TipoRelacion),
            CodigoRelacion = N(dto.CodigoRelacion),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            FechaCompromiso = Utc(dto.FechaCompromiso),
            Evidencia = dto.Evidencia.Trim(),
            Observacion = dto.Observacion.Trim(),
            CreadoPor = usuario,
            FechaCreacion = DateTime.UtcNow
        };
        await _planes.CrearEntregableAsync(x);
        return x;
    }

    public async Task<EntregableProyecto> ActualizarEntregableAsync(string proyecto, string entregable, GuardarEntregableProyectoDto dto, string usuario)
    {
        await ProyectoEditable(proyecto); var x = await EntregableRequerido(proyecto, entregable); var relacion = await ValidarEntregableAsync(proyecto, dto);
        x.CodigoTarea = relacion.CodigoTarea; x.TipoRelacion = N(dto.TipoRelacion); x.CodigoRelacion = N(dto.CodigoRelacion);
        x.Nombre = dto.Nombre.Trim(); x.Descripcion = dto.Descripcion.Trim(); x.FechaCompromiso = Utc(dto.FechaCompromiso);
        x.Evidencia = dto.Evidencia.Trim(); x.Observacion = dto.Observacion.Trim(); Marcar(x, usuario); await _planes.ActualizarEntregableAsync(x); return x;
    }

    public async Task<EntregableProyecto> CambiarEstadoEntregableAsync(string proyecto, string entregable, CambiarEstadoEntregableDto dto, string usuario)
    {
        await ProyectoEditable(proyecto); var x = await EntregableRequerido(proyecto, entregable); var estado = N(dto.Estado);
        if (!EstadosEntregable.Todos.Contains(estado)) throw new ExcepcionNegocio("El estado del entregable no es válido.");
        var tarea = x.CodigoTarea == null ? null : await TareaRequerida(proyecto, x.CodigoTarea);
        if (estado == "PENDIENTE" && tarea?.Estado == "COMPLETADA") throw ExcepcionNegocio.Conflicto("No se puede reabrir un entregable de una tarea completada.");
        if (estado == "JUSTIFICADO" && string.IsNullOrWhiteSpace(dto.Observacion)) throw new ExcepcionNegocio("La justificación es obligatoria.");
        x.Estado = estado; x.Evidencia = dto.Evidencia.Trim(); x.Observacion = dto.Observacion.Trim();
        x.FechaEntrega = estado is "ENTREGADO" or "JUSTIFICADO" ? DateTime.UtcNow : null; Marcar(x, usuario); await _planes.ActualizarEntregableAsync(x); return x;
    }

    public async Task EliminarEntregableAsync(string proyecto, string entregable, string usuario)
    {
        await ProyectoEditable(proyecto); var x = await EntregableRequerido(proyecto, entregable);
        if (x.CodigoTarea != null && (await TareaRequerida(proyecto, x.CodigoTarea)).Estado == "COMPLETADA") throw ExcepcionNegocio.Conflicto("No se puede eliminar un entregable de una tarea completada.");
        x.Activo = false; Marcar(x, usuario); await _planes.ActualizarEntregableAsync(x);
    }

    public async Task<decimal> RecalcularAvanceAsync(string proyecto, string usuario)
    {
        var p = await ProyectoRequerido(proyecto); return await RecalcularAsync(p, usuario, "Avance recalculado desde la planificación.");
    }

    private async Task<AsignacionProyecto> ValidarTarea(GuardarTareaProyectoDto dto, Proyecto p, ActividadProyecto actividad, string? excluir)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ExcepcionNegocio("El nombre de la tarea es obligatorio.");
        if (dto.FechaInicio == default || dto.FechaFin == default || dto.FechaFin.Date < dto.FechaInicio.Date) throw new ExcepcionNegocio("Las fechas de la tarea no son válidas.");
        if (dto.FechaInicio.Date < actividad.FechaInicio.Date || dto.FechaFin.Date > actividad.FechaFin.Date) throw new ExcepcionNegocio("La tarea debe estar dentro de las fechas de la actividad.");
        var total = (await _planes.TareasAsync(p.Codigo)).Where(t => t.CodigoTarea != excluir).Sum(t => t.Peso) + dto.Peso;
        if (total > 100) throw ExcepcionNegocio.Conflicto($"El peso total de las tareas no puede superar 100 %. El nuevo total sería {total:0.##} %.");
        var asignacion = await _asignaciones.ObtenerActivaAsync(p.Codigo, dto.CodigoResponsable) ?? throw ExcepcionNegocio.Conflicto("El responsable debe estar asignado al proyecto.");
        if (dto.FechaInicio.Date < asignacion.FechaInicio.Date || dto.FechaFin.Date > asignacion.FechaFin.Date) throw ExcepcionNegocio.Conflicto("La asignación del responsable debe cubrir toda la tarea.");
        return asignacion;
    }

    private async Task<List<DependenciaTarea>> ValidarDependenciasAsync(Proyecto proyecto, string codigoTarea, GuardarTareaProyectoDto dto, string usuario)
    {
        var tareas = (await _planes.TareasAsync(proyecto.Codigo)).ToDictionary(t => t.CodigoTarea);
        var solicitudes = (dto.Dependencias ?? []).Where(d => !string.IsNullOrWhiteSpace(d.CodigoPredecesora)).ToList();
        if (solicitudes.GroupBy(d => N(d.CodigoPredecesora)).Any(g => g.Count() > 1))
            throw ExcepcionNegocio.Conflicto("Una tarea predecesora no puede agregarse más de una vez.");

        var nuevas = new List<DependenciaTarea>();
        foreach (var solicitud in solicitudes)
        {
            var predecesora = N(solicitud.CodigoPredecesora);
            var tipo = N(solicitud.TipoDependencia);
            if (predecesora == codigoTarea) throw ExcepcionNegocio.Conflicto("Una tarea no puede depender de sí misma.");
            if (!tareas.ContainsKey(predecesora)) throw ExcepcionNegocio.Conflicto("La tarea predecesora no pertenece al proyecto o está inactiva.");
            if (tipo is not ("FS" or "SS" or "FF" or "SF")) throw new ExcepcionNegocio("El tipo de dependencia no es válido.");
            nuevas.Add(new DependenciaTarea
            {
                CodigoProyecto = proyecto.Codigo, CodigoTarea = codigoTarea, CodigoPredecesora = predecesora,
                NombrePredecesora = tareas[predecesora].Nombre, TipoDependencia = tipo, DesfaseDias = solicitud.DesfaseDias,
                CreadoPor = usuario, FechaCreacion = DateTime.UtcNow
            });
        }

        tareas[codigoTarea] = new TareaProyecto
        {
            CodigoTarea = codigoTarea, CodigoProyecto = proyecto.Codigo, Nombre = dto.Nombre.Trim(),
            FechaInicio = Utc(dto.FechaInicio), FechaFin = Utc(dto.FechaFin), Activo = true
        };
        var existentes = (await _planes.DependenciasAsync(proyecto.Codigo)).Where(d => d.CodigoTarea != codigoTarea).ToList();
        var relaciones = existentes.Concat(nuevas).ToList();
        if (TieneCiclo(relaciones)) throw ExcepcionNegocio.Conflicto("La relación crea un ciclo entre tareas. Revise las predecesoras seleccionadas.");

        foreach (var relacion in relaciones)
        {
            if (!tareas.TryGetValue(relacion.CodigoTarea, out var sucesora) || !tareas.TryGetValue(relacion.CodigoPredecesora, out var predecesora)) continue;
            if (!CumpleFechas(relacion, sucesora, predecesora))
                throw ExcepcionNegocio.Conflicto($"Las fechas de '{sucesora.Nombre}' no cumplen la dependencia {relacion.TipoDependencia} con '{predecesora.Nombre}' y su desfase de {relacion.DesfaseDias} día(s).");
        }
        return nuevas;
    }

    private static bool CumpleFechas(DependenciaTarea d, TareaProyecto sucesora, TareaProyecto predecesora)
    {
        var desfase = d.DesfaseDias;
        return d.TipoDependencia switch
        {
            "FS" => sucesora.FechaInicio.Date >= predecesora.FechaFin.Date.AddDays(desfase),
            "SS" => sucesora.FechaInicio.Date >= predecesora.FechaInicio.Date.AddDays(desfase),
            "FF" => sucesora.FechaFin.Date >= predecesora.FechaFin.Date.AddDays(desfase),
            "SF" => sucesora.FechaFin.Date >= predecesora.FechaInicio.Date.AddDays(desfase),
            _ => false
        };
    }

    private static bool TieneCiclo(IReadOnlyCollection<DependenciaTarea> dependencias)
    {
        var grafo = dependencias.GroupBy(d => d.CodigoTarea).ToDictionary(g => g.Key, g => g.Select(d => d.CodigoPredecesora).ToList());
        var visitando = new HashSet<string>();
        var visitadas = new HashSet<string>();
        bool Visitar(string tarea)
        {
            if (visitando.Contains(tarea)) return true;
            if (visitadas.Contains(tarea)) return false;
            visitando.Add(tarea);
            if (grafo.TryGetValue(tarea, out var anteriores) && anteriores.Any(Visitar)) return true;
            visitando.Remove(tarea); visitadas.Add(tarea); return false;
        }
        return grafo.Keys.Any(Visitar);
    }

    private static bool DependenciaCumplidaParaEstado(DependenciaTarea dependencia, string estadoDestino, IReadOnlyDictionary<string, TareaProyecto> tareas)
    {
        if (!tareas.TryGetValue(dependencia.CodigoPredecesora, out var predecesora)) return false;
        if (estadoDestino == "EN_PROCESO")
            return dependencia.TipoDependencia == "FS" ? predecesora.Estado == "COMPLETADA" : dependencia.TipoDependencia is "FF" or "SF" || predecesora.Estado != "PENDIENTE";
        return dependencia.TipoDependencia is "FS" or "FF" ? predecesora.Estado == "COMPLETADA" : predecesora.Estado != "PENDIENTE";
    }

    private static bool EstaBloqueada(TareaProyecto tarea, IReadOnlyCollection<DependenciaTarea> dependencias, IReadOnlyCollection<TareaProyecto> tareas)
    {
        var diccionario = tareas.ToDictionary(t => t.CodigoTarea);
        return dependencias.Where(d => d.CodigoTarea == tarea.CodigoTarea).Any(d => !DependenciaCumplidaParaEstado(d, "EN_PROCESO", diccionario));
    }

    private async Task<AsignacionProyecto> ValidarActividad(GuardarActividadProyectoDto dto,Proyecto p,FaseProyecto f)
    {
        if(string.IsNullOrWhiteSpace(dto.Nombre)) throw new ExcepcionNegocio("El nombre de la actividad es obligatorio.");
        if(dto.FechaInicio==default||dto.FechaFin==default||dto.FechaFin.Date<dto.FechaInicio.Date) throw new ExcepcionNegocio("Las fechas de la actividad no son válidas.");
        if(dto.FechaInicio.Date<f.FechaInicio.Date||dto.FechaFin.Date>f.FechaFin.Date) throw new ExcepcionNegocio("La actividad debe estar dentro de las fechas de la fase.");
        var asignacion=await _asignaciones.ObtenerActivaAsync(p.Codigo,dto.CodigoResponsable)??throw ExcepcionNegocio.Conflicto("El responsable de la actividad debe estar asignado al proyecto.");
        if(dto.FechaInicio.Date<asignacion.FechaInicio.Date||dto.FechaFin.Date>asignacion.FechaFin.Date) throw ExcepcionNegocio.Conflicto("La asignación del responsable debe cubrir toda la actividad.");
        return asignacion;
    }

    private static void ValidarFase(GuardarFaseProyectoDto dto, Proyecto p)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ExcepcionNegocio("El nombre de la fase es obligatorio.");
        if (dto.FechaInicio == default || dto.FechaFin == default || dto.FechaFin.Date < dto.FechaInicio.Date) throw new ExcepcionNegocio("Las fechas de la fase no son válidas.");
        if (dto.FechaInicio.Date < p.FechaInicio.Date || dto.FechaFin.Date > p.FechaFinPlanificada.Date) throw new ExcepcionNegocio("La fase debe estar dentro de las fechas planificadas del proyecto.");
    }
    private async Task<(string? CodigoTarea, DateTime Inicio, DateTime Fin)> ValidarEntregableAsync(string proyecto, GuardarEntregableProyectoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ExcepcionNegocio("El nombre del entregable es obligatorio.");
        var tipo = N(dto.TipoRelacion);
        var codigo = N(dto.CodigoRelacion);
        string? codigoTarea = null;
        DateTime inicio;
        DateTime fin;
        switch (tipo)
        {
            case "FASE":
                var fase = await FaseRequerida(proyecto, codigo);
                inicio = fase.FechaInicio; fin = fase.FechaFin;
                break;
            case "ACTIVIDAD":
                var actividad = await ActividadRequerida(proyecto, codigo);
                inicio = actividad.FechaInicio; fin = actividad.FechaFin;
                break;
            case "TAREA":
                var tarea = await TareaRequerida(proyecto, codigo);
                codigoTarea = tarea.CodigoTarea; inicio = tarea.FechaInicio; fin = tarea.FechaFin;
                break;
            default:
                throw new ExcepcionNegocio("Seleccione una fase, actividad o tarea válida.");
        }
        if (dto.FechaCompromiso == default || dto.FechaCompromiso.Date < inicio.Date || dto.FechaCompromiso.Date > fin.Date)
            throw new ExcepcionNegocio("La fecha del entregable debe estar dentro de las fechas del elemento relacionado.");
        return (codigoTarea, inicio, fin);
    }
    private async Task ActualizarEstadoFaseAsync(string p, string f, string usuario)
    {
        var fase = await FaseRequerida(p, f); var tareas = (await _planes.TareasAsync(p)).Where(t => t.CodigoFase == f).ToList();
        fase.Estado = tareas.Count > 0 && tareas.All(t => t.Estado == "COMPLETADA") ? "COMPLETADA" : tareas.Any(t => t.Estado != "PENDIENTE") ? "EN_PROCESO" : "PENDIENTE";
        Marcar(fase, usuario); await _planes.ActualizarFaseAsync(fase);
    }
    private async Task ActualizarEstadoActividadAsync(string p,string a,string usuario)
    {
        var actividad=await ActividadRequerida(p,a);var tareas=(await _planes.TareasAsync(p)).Where(t=>t.CodigoActividad==a).ToList();
        actividad.Estado=tareas.Count>0&&tareas.All(t=>t.Estado=="COMPLETADA")?"COMPLETADA":tareas.Any(t=>t.Estado!="PENDIENTE")?"EN_PROCESO":"PENDIENTE";
        Marcar(actividad,usuario);await _planes.ActualizarActividadAsync(actividad);
    }
    private async Task<decimal> RecalcularAsync(Proyecto p, string usuario, string observacion)
    {
        var tareas = await _planes.TareasAsync(p.Codigo); var nuevo = decimal.Round(tareas.Sum(t => t.Peso * EstadosTarea.Factor(t.Estado)), 2);
        if (nuevo == p.Avance) return nuevo;
        var anterior = p.Avance; var estadoAnterior = p.Estado; p.Avance = nuevo;
        p.ModificadoPor = usuario; p.FechaModificacion = DateTime.UtcNow; await _proyectos.ActualizarAsync(p);
        await _seguimientos.CrearAsync(new SeguimientoProyecto { CodigoSeguimiento = _codigos.GenerarCodigo("gepro_seguimiento", "CODIGOSEGUIMIENTO"),
            CodigoProyecto = p.Codigo, AvanceAnterior = anterior, AvanceNuevo = nuevo, EstadoAnterior = estadoAnterior, EstadoNuevo = p.Estado,
            Observacion = observacion, RegistradoPor = usuario, FechaRegistro = DateTime.UtcNow });
        return nuevo;
    }
    private async Task<Proyecto> ProyectoRequerido(string p) => await _proyectos.ObtenerAsync(p) ?? throw ExcepcionNegocio.NoEncontrado("El proyecto no existe.");
    private async Task<Proyecto> ProyectoEditable(string p) { var x = await ProyectoRequerido(p); if (!x.Activo || EstadosProyecto.Cerrados.Contains(x.Estado)) throw ExcepcionNegocio.Conflicto("El proyecto está cerrado o inactivo."); return x; }
    private async Task<FaseProyecto> FaseRequerida(string p, string f) { var x = await _planes.FaseAsync(N(p), N(f)); return x is { Activo: true } ? x : throw ExcepcionNegocio.NoEncontrado("La fase no existe."); }
    private async Task<ActividadProyecto> ActividadRequerida(string p,string a){var x=await _planes.ActividadAsync(N(p),N(a));return x is {Activo:true}?x:throw ExcepcionNegocio.NoEncontrado("La actividad no existe.");}
    private async Task<TareaProyecto> TareaRequerida(string p, string t) { var x = await _planes.TareaAsync(N(p), N(t)); return x is { Activo: true } ? x : throw ExcepcionNegocio.NoEncontrado("La tarea no existe."); }
    private async Task<EntregableProyecto> EntregableRequerido(string p, string e) { var x = await _planes.EntregableAsync(N(p), N(e)); return x is { Activo: true } ? x : throw ExcepcionNegocio.NoEncontrado("El entregable no existe."); }
    private static decimal AvancePlanificado(TareaProyecto t, DateTime hoy) { if (hoy < t.FechaInicio.Date) return 0; if (hoy >= t.FechaFin.Date) return 100; var dias = Math.Max(1, (t.FechaFin.Date - t.FechaInicio.Date).TotalDays); return decimal.Round((decimal)((hoy - t.FechaInicio.Date).TotalDays / dias) * 100, 2); }
    private static void Marcar(FaseProyecto x, string u) { x.ModificadoPor = u; x.FechaModificacion = DateTime.UtcNow; }
    private static void Marcar(ActividadProyecto x,string u){x.ModificadoPor=u;x.FechaModificacion=DateTime.UtcNow;}
    private static void Marcar(TareaProyecto x, string u) { x.ModificadoPor = u; x.FechaModificacion = DateTime.UtcNow; }
    private static void Marcar(EntregableProyecto x, string u) { x.ModificadoPor = u; x.FechaModificacion = DateTime.UtcNow; }
    private static DateTime Utc(DateTime x) => DateTime.SpecifyKind(x.Date, DateTimeKind.Utc);
    private static string N(string? x) => (x ?? "").Trim().ToUpperInvariant();
}
