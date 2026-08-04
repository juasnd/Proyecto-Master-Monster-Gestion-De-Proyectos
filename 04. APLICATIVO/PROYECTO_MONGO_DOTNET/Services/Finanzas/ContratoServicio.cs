using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Finanzas;

public sealed class ContratoServicio
{
    private readonly ContratoRepositorio _repositorio;
    private readonly EmpleadoDAO _empleados;
    private readonly CodigoDAO _codigos;
    public ContratoServicio(ContratoRepositorio repositorio, EmpleadoDAO empleados, CodigoDAO codigos) { _repositorio = repositorio; _empleados = empleados; _codigos = codigos; }
    public Task<List<Contrato>> ListarAsync() => _repositorio.ListarAsync();

    public async Task<Contrato> GuardarAsync(GuardarContratoDto dto, string usuario)
    {
        if (_empleados.BuscarPorCodigo(dto.CodigoEmpleado) == null) throw new ExcepcionNegocio("El empleado no existe o no está activo.");
        if (dto.SueldoBase <= 0) throw new ExcepcionNegocio("El sueldo base debe ser mayor que cero.");
        if (dto.HorasMensuales <= 0) throw new ExcepcionNegocio("Las horas mensuales deben ser mayores que cero.");
        if (dto.FechaFin.HasValue && dto.FechaFin < dto.FechaInicio) throw new ExcepcionNegocio("La fecha final no puede ser anterior a la inicial.");
        var codigo = N(dto.CodigoContrato);
        if (await _repositorio.HaySolapamientoAsync(dto.CodigoEmpleado, Utc(dto.FechaInicio), dto.FechaFin.HasValue ? Utc(dto.FechaFin.Value) : null, codigo))
            throw ExcepcionNegocio.Conflicto("El empleado ya tiene un contrato principal activo que se solapa con las fechas indicadas.");
        var existente = string.IsNullOrWhiteSpace(codigo) ? null : await _repositorio.ObtenerAsync(codigo);
        var contrato = existente ?? new Contrato { CodigoContrato = _codigos.GenerarCodigo("pecon_contrato", "CODIGOCONTRATO"), CreadoPor = usuario, FechaCreacion = DateTime.UtcNow };
        contrato.CodigoEmpleado = N(dto.CodigoEmpleado); contrato.TipoContrato = N(dto.TipoContrato); contrato.SueldoBase = dto.SueldoBase;
        contrato.HorasMensuales = dto.HorasMensuales; contrato.FechaInicio = Utc(dto.FechaInicio); contrato.FechaFin = dto.FechaFin.HasValue ? Utc(dto.FechaFin.Value) : null;
        contrato.Estado = N(dto.Estado); contrato.Activo = contrato.Estado != "INACTIVO";
        if (existente == null) await _repositorio.CrearAsync(contrato); else { contrato.ModificadoPor = usuario; contrato.FechaModificacion = DateTime.UtcNow; await _repositorio.ActualizarAsync(contrato); }
        return contrato;
    }

    public async Task InactivarAsync(string codigo, string usuario)
    {
        var contrato = await _repositorio.ObtenerAsync(codigo) ?? throw ExcepcionNegocio.NoEncontrado("El contrato no existe.");
        contrato.Activo = false; contrato.Estado = "INACTIVO"; contrato.ModificadoPor = usuario; contrato.FechaModificacion = DateTime.UtcNow;
        await _repositorio.ActualizarAsync(contrato);
    }
    private static DateTime Utc(DateTime x) => DateTime.SpecifyKind(x, DateTimeKind.Utc);
    private static string N(string? x) => (x ?? "").Trim().ToUpperInvariant();
}
