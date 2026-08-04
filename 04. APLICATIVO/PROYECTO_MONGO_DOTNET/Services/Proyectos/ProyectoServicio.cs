using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;

namespace PROYECTO_MONGO_DOTNET.Services.Proyectos;

public sealed class ProyectoServicio
{
    private readonly IProyectoRepositorio _proyectos;
    private readonly IAsignacionProyectoRepositorio _asignaciones;
    private readonly DepartamentoDAO _departamentos;
    private readonly EmpleadoDAO _empleados;
    private readonly CodigoDAO _codigos;
    private readonly CostoProyectoServicio _costos;
    private readonly IPlanificacionProyectoRepositorio _planes;

    public ProyectoServicio(IProyectoRepositorio proyectos, IAsignacionProyectoRepositorio asignaciones, DepartamentoDAO departamentos,
        EmpleadoDAO empleados, CodigoDAO codigos, CostoProyectoServicio costos, IPlanificacionProyectoRepositorio planes)
    {
        _proyectos = proyectos;
        _asignaciones = asignaciones;
        _departamentos = departamentos;
        _empleados = empleados;
        _codigos = codigos;
        _costos = costos;
        _planes = planes;
    }

    public Task<IReadOnlyCollection<Proyecto>> ListarAsync(bool incluirInactivos = false) => _proyectos.ListarAsync(incluirInactivos);
    public Task<Proyecto?> ObtenerAsync(string codigo) => _proyectos.ObtenerAsync(codigo);

    public async Task<IReadOnlyCollection<ProyectoListadoVm>> ListarVistaAsync(bool incluirInactivos = false)
    {
        var departamentos = _departamentos.Listar().ToDictionary(x => x.Codigo, StringComparer.OrdinalIgnoreCase);
        var empleados = _empleados.Listar().ToDictionary(x => x.PeempCodigo, StringComparer.OrdinalIgnoreCase);
        var resultado = new List<ProyectoListadoVm>();
        foreach (var proyecto in await _proyectos.ListarAsync(incluirInactivos))
        {
            var costo = await _costos.CalcularAsync(proyecto.Codigo);
            departamentos.TryGetValue(proyecto.CodigoDepartamento, out var departamento);
            empleados.TryGetValue(proyecto.CodigoJefeProyecto, out var jefe);
            resultado.Add(new ProyectoListadoVm
            {
                Proyecto = proyecto,
                NombreDepartamento = departamento?.Descripcion ?? "Departamento no disponible",
                NombreJefeProyecto = jefe == null ? "Jefe no disponible" : Nombre(jefe),
                SaldoDisponible = costo.SaldoDisponible
            });
        }
        return resultado;
    }

    public async Task<Proyecto> CrearAsync(GuardarProyectoDto dto, string usuario)
    {
        ReglasProyecto.Validar(dto);
        ValidarReferencias(dto);
        var codigo = string.IsNullOrWhiteSpace(dto.Codigo) ? _codigos.GenerarCodigo("gepro_proyec", "CODIGO") : N(dto.Codigo);
        if (await _proyectos.ExisteCodigoAsync(codigo)) throw ExcepcionNegocio.Conflicto("No se pudo generar el identificador interno del proyecto. Intente nuevamente.");
        var proyecto = Mapear(dto, codigo, usuario, DateTime.UtcNow);
        await _proyectos.CrearAsync(proyecto);
        return proyecto;
    }

    public async Task<Proyecto> ActualizarAsync(string codigo, GuardarProyectoDto dto, string usuario)
    {
        ReglasProyecto.Validar(dto);
        ValidarReferencias(dto);
        var actual = await ObtenerRequerido(codigo);
        if (EstadosProyecto.Cerrados.Contains(actual.Estado)) throw ExcepcionNegocio.Conflicto("Un proyecto cerrado solo puede consultarse.");
        var fases = await _planes.FasesAsync(actual.Codigo);
        if (fases.Any(f => f.FechaInicio.Date < dto.FechaInicio.Date || f.FechaFin.Date > dto.FechaFinPlanificada.Date))
            throw ExcepcionNegocio.Conflicto("Las nuevas fechas del proyecto deben contener todas las fases existentes.");
        actual.Nombre = dto.Nombre.Trim();
        actual.Descripcion = dto.Descripcion.Trim();
        actual.CodigoDepartamento = N(dto.CodigoDepartamento);
        actual.CodigoJefeProyecto = N(dto.CodigoJefeProyecto);
        actual.FechaInicio = Utc(dto.FechaInicio);
        actual.FechaFinPlanificada = Utc(dto.FechaFinPlanificada);
        actual.Prioridad = N(dto.Prioridad);
        actual.Presupuesto = dto.Presupuesto;
        actual.Observacion = dto.Observacion.Trim();
        actual.ModificadoPor = usuario;
        actual.FechaModificacion = DateTime.UtcNow;
        await _proyectos.ActualizarAsync(actual);
        return actual;
    }

    public async Task<Proyecto> CambiarActivoAsync(string codigo, CambiarActivoProyectoDto dto, string usuario)
    {
        var proyecto = await ObtenerRequerido(codigo);
        if (proyecto.Activo == dto.Activo) return proyecto;
        if (!dto.Activo && proyecto.Estado == "EN_CURSO")
            throw ExcepcionNegocio.Conflicto("Pause o cancele el proyecto antes de inactivarlo.");
        proyecto.Activo = dto.Activo;
        proyecto.Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? proyecto.Observacion : dto.Observacion.Trim();
        proyecto.ModificadoPor = usuario;
        proyecto.FechaModificacion = DateTime.UtcNow;
        await _proyectos.ActualizarAsync(proyecto);
        return proyecto;
    }

    public async Task<ResultadoPaginado<ReporteProyectoFila>> ReporteAsync(FiltroProyectosDto filtro)
    {
        IEnumerable<ProyectoListadoVm> consulta = await ListarVistaAsync(true);
        if (!string.IsNullOrWhiteSpace(filtro.TextoBusqueda))
        {
            var texto = filtro.TextoBusqueda.Trim();
            consulta = consulta.Where(x => $"{x.Proyecto.Nombre} {x.NombreJefeProyecto} {x.NombreDepartamento}".Contains(texto, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Estado)) consulta = consulta.Where(x => x.Proyecto.Estado == N(filtro.Estado));
        if (filtro.Activo.HasValue) consulta = consulta.Where(x => x.Proyecto.Activo == filtro.Activo.Value);
        if (!string.IsNullOrWhiteSpace(filtro.CodigoJefe)) consulta = consulta.Where(x => x.Proyecto.CodigoJefeProyecto == N(filtro.CodigoJefe));
        if (!string.IsNullOrWhiteSpace(filtro.CodigoDepartamento)) consulta = consulta.Where(x => x.Proyecto.CodigoDepartamento == N(filtro.CodigoDepartamento));
        if (!string.IsNullOrWhiteSpace(filtro.Prioridad)) consulta = consulta.Where(x => x.Proyecto.Prioridad == N(filtro.Prioridad));
        if (filtro.FechaInicioDesde.HasValue) consulta = consulta.Where(x => x.Proyecto.FechaInicio.Date >= filtro.FechaInicioDesde.Value.Date);
        if (filtro.FechaInicioHasta.HasValue) consulta = consulta.Where(x => x.Proyecto.FechaInicio.Date <= filtro.FechaInicioHasta.Value.Date);
        if (filtro.FechaFinDesde.HasValue) consulta = consulta.Where(x => x.Proyecto.FechaFinPlanificada.Date >= filtro.FechaFinDesde.Value.Date);
        if (filtro.FechaFinHasta.HasValue) consulta = consulta.Where(x => x.Proyecto.FechaFinPlanificada.Date <= filtro.FechaFinHasta.Value.Date);
        if (filtro.AvanceMinimo.HasValue) consulta = consulta.Where(x => x.Proyecto.Avance >= filtro.AvanceMinimo);
        if (filtro.AvanceMaximo.HasValue) consulta = consulta.Where(x => x.Proyecto.Avance <= filtro.AvanceMaximo);
        if (filtro.PresupuestoMinimo.HasValue) consulta = consulta.Where(x => x.Proyecto.Presupuesto >= filtro.PresupuestoMinimo);
        if (filtro.PresupuestoMaximo.HasValue) consulta = consulta.Where(x => x.Proyecto.Presupuesto <= filtro.PresupuestoMaximo);
        if (filtro.SoloAtrasados) consulta = consulta.Where(x => x.Proyecto.Activo && !EstadosProyecto.Cerrados.Contains(x.Proyecto.Estado) && x.Proyecto.FechaFinPlanificada.Date < DateTime.UtcNow.Date);
        if (!string.IsNullOrWhiteSpace(filtro.CodigoEmpleado))
        {
            var codigos = (await _asignaciones.ListarAsync()).Where(x => x.CodigoEmpleado == N(filtro.CodigoEmpleado) && x.Activo).Select(x => x.CodigoProyecto).ToHashSet();
            consulta = consulta.Where(x => codigos.Contains(x.Proyecto.Codigo));
        }
        if (filtro.SoloExcedidos) consulta = consulta.Where(x => x.SaldoDisponible < 0);
        consulta = Ordenar(consulta, filtro.OrdenarPor, filtro.Direccion);
        var lista = consulta.Select(x => new ReporteProyectoFila
        {
            Nombre = x.Proyecto.Nombre,
            Departamento = x.NombreDepartamento,
            Jefe = x.NombreJefeProyecto,
            FechaInicio = x.Proyecto.FechaInicio,
            FechaFinPlanificada = x.Proyecto.FechaFinPlanificada,
            Estado = x.Proyecto.Estado,
            Prioridad = x.Proyecto.Prioridad,
            Avance = x.Proyecto.Avance,
            Presupuesto = x.Proyecto.Presupuesto,
            SaldoDisponible = x.SaldoDisponible,
            Activo = x.Proyecto.Activo
        }).ToList();
        var pagina = Math.Max(1, filtro.Pagina);
        var cantidad = Math.Clamp(filtro.Cantidad, 1, 5000);
        return new ResultadoPaginado<ReporteProyectoFila> { Elementos = lista.Skip((pagina - 1) * cantidad).Take(cantidad).ToList(), Total = lista.Count, Pagina = pagina, Cantidad = cantidad };
    }

    private void ValidarReferencias(GuardarProyectoDto dto)
    {
        if (_departamentos.Buscar(dto.CodigoDepartamento) == null) throw new ExcepcionNegocio("El departamento no existe o no está activo.");
        if (!_empleados.EsJefeProyectoActivo(dto.CodigoJefeProyecto)) throw new ExcepcionNegocio("El empleado seleccionado debe estar activo, tener usuario activo y pertenecer a un cargo designado como jefe de proyecto.");
    }

    private async Task<Proyecto> ObtenerRequerido(string codigo) => await _proyectos.ObtenerAsync(codigo) ?? throw ExcepcionNegocio.NoEncontrado("El proyecto no existe.");
    private static Proyecto Mapear(GuardarProyectoDto x, string codigo, string usuario, DateTime ahora) => new()
    {
        Codigo = codigo,
        Nombre = x.Nombre.Trim(),
        Descripcion = x.Descripcion.Trim(),
        CodigoDepartamento = N(x.CodigoDepartamento),
        CodigoJefeProyecto = N(x.CodigoJefeProyecto),
        FechaInicio = Utc(x.FechaInicio),
        FechaFinPlanificada = Utc(x.FechaFinPlanificada),
        Estado = "PLANIFICADO",
        Prioridad = N(x.Prioridad),
        Avance = 0,
        Presupuesto = x.Presupuesto,
        Activo = true,
        Observacion = x.Observacion.Trim(),
        CreadoPor = usuario,
        FechaCreacion = ahora
    };

    private static IEnumerable<ProyectoListadoVm> Ordenar(IEnumerable<ProyectoListadoVm> origen, string? campo, string? direccion)
    {
        Func<ProyectoListadoVm, object> selector = N(campo) switch
        {
            "NOMBRE" => x => x.Proyecto.Nombre,
            "ESTADO" => x => x.Proyecto.Estado,
            "FECHAINICIO" => x => x.Proyecto.FechaInicio,
            "AVANCE" => x => x.Proyecto.Avance,
            "PRESUPUESTO" => x => x.Proyecto.Presupuesto,
            _ => x => x.Proyecto.Codigo
        };
        return N(direccion) == "DESC" ? origen.OrderByDescending(selector) : origen.OrderBy(selector);
    }

    private static string Nombre(Empleado empleado) => $"{empleado.Persona.Nombres} {empleado.Persona.Apellidos}".Trim();
    private static DateTime Utc(DateTime fecha) => DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);
    private static string N(string? valor) => (valor ?? "").Trim().ToUpperInvariant();
}
