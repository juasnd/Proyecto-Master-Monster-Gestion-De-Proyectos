using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Proyectos;

public sealed class AsignacionProyectoServicio
{
    private readonly IAsignacionProyectoRepositorio _asignaciones;
    private readonly IProyectoRepositorio _proyectos;
    private readonly EmpleadoDAO _empleados;
    private readonly ContratoRepositorio _contratos;
    private readonly ParametroFinancieroRepositorio _parametros;
    private readonly CodigoDAO _codigos;
    private readonly IPlanificacionProyectoRepositorio _planes;

    public AsignacionProyectoServicio(IAsignacionProyectoRepositorio asignaciones, IProyectoRepositorio proyectos,
        EmpleadoDAO empleados, ContratoRepositorio contratos, ParametroFinancieroRepositorio parametros,
        CodigoDAO codigos, IPlanificacionProyectoRepositorio planes)
    {
        _asignaciones = asignaciones;
        _proyectos = proyectos;
        _empleados = empleados;
        _contratos = contratos;
        _parametros = parametros;
        _codigos = codigos;
        _planes = planes;
    }

    public Task<IReadOnlyCollection<AsignacionProyecto>> PorProyectoAsync(string proyecto) => _asignaciones.PorProyectoAsync(proyecto);
    public Task<IReadOnlyCollection<AsignacionProyecto>> ListarAsync() => _asignaciones.ListarAsync();

    public async Task<IReadOnlyCollection<CapacidadEmpleadoProyectoVm>> EquipoAsync(string codigoProyecto)
    {
        _ = await ProyectoRequerido(codigoProyecto);
        var asignaciones = await _asignaciones.PorProyectoAsync(codigoProyecto);
        var todas = await _asignaciones.ListarAsync();
        var tareas = await _planes.TareasAsync(codigoProyecto);
        var empleados = _empleados.Listar().ToDictionary(x => x.PeempCodigo, StringComparer.OrdinalIgnoreCase);
        var parametro = await _parametros.ObtenerAsync("HORAS_MENSUALES_REFERENCIA");
        var horasReferencia = parametro?.ValorDecimal ?? parametro?.ValorEntero ?? 160m;
        var resultado = new List<CapacidadEmpleadoProyectoVm>();
        foreach (var asignacion in asignaciones)
        {
            empleados.TryGetValue(asignacion.CodigoEmpleado, out var empleado);
            var otros = todas.Where(x => x.Activo && x.CodigoAsignacion != asignacion.CodigoAsignacion &&
                x.CodigoEmpleado == asignacion.CodigoEmpleado && x.FechaInicio <= asignacion.FechaFin && x.FechaFin >= asignacion.FechaInicio).ToList();
            var usada = otros.Sum(x => x.DisponibilidadAsignada) + (asignacion.Activo ? asignacion.DisponibilidadAsignada : 0);
            var contrato = await _contratos.VigenteAsync(asignacion.CodigoEmpleado, asignacion.FechaInicio);
            var horasMensuales = contrato?.HorasMensuales is > 0 ? contrato.HorasMensuales : horasReferencia;
            var dias = Math.Max(1, (asignacion.FechaFin.Date - asignacion.FechaInicio.Date).Days + 1);
            var disponibles = decimal.Round(horasMensuales * dias / 30m * asignacion.DisponibilidadAsignada / 100m, 2);
            var planificadas = tareas.Where(x => x.CodigoResponsable == asignacion.CodigoEmpleado && x.FechaInicio <= asignacion.FechaFin && x.FechaFin >= asignacion.FechaInicio).Sum(x => x.HorasPlanificadas);
            var porcentaje = disponibles <= 0 ? (planificadas > 0 ? 101 : 0) : planificadas / disponibles * 100m;
            resultado.Add(new CapacidadEmpleadoProyectoVm
            {
                Asignacion = asignacion,
                NombreEmpleado = empleado == null ? "Empleado no disponible" : $"{empleado.Persona.Nombres} {empleado.Persona.Apellidos}".Trim(),
                DisponibilidadUsada = usada,
                DisponibilidadRestante = Math.Max(0, 100m - usada),
                HorasDisponiblesEstimadas = disponibles,
                HorasPlanificadasTareas = planificadas,
                DiferenciaHoras = disponibles - planificadas,
                EstadoCapacidad = porcentaje > 100 ? "SOBRECARGADO" : porcentaje >= 85 ? "CERCA_DEL_LIMITE" : "DISPONIBLE",
                Conflictos = otros.Select(x => $"{x.DisponibilidadAsignada:0.##} % asignado del {x.FechaInicio:dd/MM/yyyy} al {x.FechaFin:dd/MM/yyyy}").ToList()
            });
        }
        return resultado;
    }

    public async Task<AsignacionProyecto> CrearAsync(string codigoProyecto, GuardarAsignacionDto dto, string usuario)
    {
        var proyecto = await ProyectoAbierto(codigoProyecto);
        if (!_empleados.TieneUsuarioActivo(dto.CodigoEmpleado))
            throw new ExcepcionNegocio("El empleado debe estar activo, tener una persona asociada y un usuario activo.");
        ReglasProyecto.ValidarAsignacionDisponible(await _asignaciones.ObtenerActivaAsync(proyecto.Codigo, dto.CodigoEmpleado) != null);
        ValidarFechasProyecto(proyecto, dto);
        var disponibilidadUsada = await _asignaciones.DedicacionSolapadaAsync(dto.CodigoEmpleado, Utc(dto.FechaInicio), Utc(dto.FechaFin));
        ReglasProyecto.ValidarAsignacion(dto, disponibilidadUsada);
        var contrato = await _contratos.VigenteAsync(dto.CodigoEmpleado, Utc(dto.FechaInicio));
        var asignacion = new AsignacionProyecto
        {
            CodigoAsignacion = _codigos.GenerarCodigo("ge_peemp_gepro", "CODIGOASIGNACION"),
            CodigoProyecto = proyecto.Codigo,
            CodigoEmpleado = N(dto.CodigoEmpleado),
            Rol = dto.Rol.Trim(),
            DisponibilidadAsignada = dto.DisponibilidadAsignada,
            FechaInicio = Utc(dto.FechaInicio),
            FechaFin = Utc(dto.FechaFin),
            Estado = "ACTIVA",
            Activo = true,
            Observacion = dto.Observacion.Trim(),
            AsignadoPor = usuario,
            FechaAsignacion = DateTime.UtcNow,
            CostoHoraSnapshot = contrato == null || contrato.HorasMensuales <= 0 ? null : decimal.Round(contrato.SueldoBase / contrato.HorasMensuales, 6)
        };
        await _asignaciones.CrearAsync(asignacion);
        return asignacion;
    }

    public async Task<AsignacionProyecto> ActualizarAsync(string proyectoCodigo, string empleadoCodigo, GuardarAsignacionDto dto, string usuario)
    {
        var proyecto = await ProyectoAbierto(proyectoCodigo);
        var actual = await _asignaciones.ObtenerActivaAsync(proyecto.Codigo, empleadoCodigo) ?? throw ExcepcionNegocio.NoEncontrado("La asignación activa no existe.");
        if (N(dto.CodigoEmpleado) != actual.CodigoEmpleado) throw new ExcepcionNegocio("No se puede cambiar el empleado de una asignación existente.");
        if (!_empleados.TieneUsuarioActivo(actual.CodigoEmpleado)) throw ExcepcionNegocio.Conflicto("El usuario del empleado ya no está activo.");
        ValidarFechasProyecto(proyecto, dto);
        var disponibilidadUsada = await _asignaciones.DedicacionSolapadaAsync(actual.CodigoEmpleado, Utc(dto.FechaInicio), Utc(dto.FechaFin), actual.CodigoAsignacion);
        ReglasProyecto.ValidarAsignacion(dto, disponibilidadUsada);
        var tareas = (await _planes.TareasAsync(proyecto.Codigo)).Where(x => x.CodigoResponsable == actual.CodigoEmpleado).ToList();
        if (tareas.Any(x => x.FechaInicio.Date < dto.FechaInicio.Date || x.FechaFin.Date > dto.FechaFin.Date))
            throw ExcepcionNegocio.Conflicto("La vigencia de la asignación debe cubrir todas las tareas del responsable.");
        actual.Rol = dto.Rol.Trim();
        actual.DisponibilidadAsignada = dto.DisponibilidadAsignada;
        actual.FechaInicio = Utc(dto.FechaInicio);
        actual.FechaFin = Utc(dto.FechaFin);
        actual.Observacion = dto.Observacion.Trim();
        actual.ModificadoPor = usuario;
        actual.FechaModificacion = DateTime.UtcNow;
        await _asignaciones.ActualizarAsync(actual);
        return actual;
    }

    public async Task RetirarAsync(string proyecto, string empleado, string usuario)
    {
        var actual = await _asignaciones.ObtenerActivaAsync(proyecto, empleado) ?? throw ExcepcionNegocio.NoEncontrado("La asignación activa no existe.");
        var tareas = (await _planes.TareasAsync(proyecto)).Where(x => x.CodigoResponsable == actual.CodigoEmpleado && x.Estado != "COMPLETADA").ToList();
        if (tareas.Count > 0)
            throw ExcepcionNegocio.Conflicto($"Antes de retirar al integrante debe reasignar {tareas.Count} tarea(s) activa(s): {string.Join(", ", tareas.Take(5).Select(x => x.Nombre))}.");
        actual.Activo = false;
        actual.Estado = "RETIRADA";
        if (actual.FechaFin > DateTime.UtcNow) actual.FechaFin = DateTime.UtcNow;
        actual.ModificadoPor = usuario;
        actual.FechaModificacion = DateTime.UtcNow;
        await _asignaciones.ActualizarAsync(actual);
    }

    private async Task<Proyecto> ProyectoRequerido(string codigo) => await _proyectos.ObtenerAsync(codigo) ?? throw ExcepcionNegocio.NoEncontrado("El proyecto no existe.");
    private async Task<Proyecto> ProyectoAbierto(string codigo)
    {
        var proyecto = await ProyectoRequerido(codigo);
        if (!proyecto.Activo || EstadosProyecto.Cerrados.Contains(proyecto.Estado)) throw ExcepcionNegocio.Conflicto("El proyecto no admite asignaciones.");
        return proyecto;
    }

    private static void ValidarFechasProyecto(Proyecto proyecto, GuardarAsignacionDto dto)
    {
        if (Utc(dto.FechaInicio) < proyecto.FechaInicio || Utc(dto.FechaFin) > proyecto.FechaFinPlanificada)
            throw new ExcepcionNegocio("Las fechas de participación deben estar dentro de las fechas planificadas del proyecto.");
    }

    private static DateTime Utc(DateTime fecha) => DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);
    private static string N(string? valor) => (valor ?? "").Trim().ToUpperInvariant();
}
