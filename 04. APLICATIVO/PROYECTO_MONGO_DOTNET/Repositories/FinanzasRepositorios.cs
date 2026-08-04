using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;
namespace PROYECTO_MONGO_DOTNET.Repositories;

public sealed class ContratoRepositorio(SqlContexto db)
{
    private const string C="Id,CodigoContrato,CodigoEmpleado,TipoContrato,SueldoBase,HorasMensuales,FechaInicio,FechaFin,Estado,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion";
    public Task<List<Contrato>> ListarAsync()=>db.ConsultarAsync<Contrato>($"SELECT {C} FROM app.Contrato ORDER BY CodigoEmpleado,FechaInicio DESC");
    public async Task<Contrato?> ObtenerAsync(string c)=>(await db.ConsultarAsync<Contrato>($"SELECT {C} FROM app.Contrato WHERE CodigoContrato=@C",new{C=N(c)})).FirstOrDefault();
    public async Task<Contrato?> VigenteAsync(string e,DateTime f)=>(await db.ConsultarAsync<Contrato>($"SELECT TOP(1) {C} FROM app.Contrato WHERE CodigoEmpleado=@E AND Activo=1 AND Estado='ACTIVO' AND FechaInicio<=@F AND (FechaFin IS NULL OR FechaFin>=@F) ORDER BY FechaInicio DESC",new{E=N(e),F=f})).FirstOrDefault();
    public async Task<bool> HaySolapamientoAsync(string e,DateTime i,DateTime? f,string? x)=>await db.EscalarAsync<int>("SELECT COUNT(1) FROM app.Contrato WHERE CodigoEmpleado=@E AND Activo=1 AND Estado='ACTIVO' AND FechaInicio<=@F AND (FechaFin IS NULL OR FechaFin>=@I) AND (@X='' OR CodigoContrato<>@X)",new{E=N(e),I=i,F=f??DateTime.MaxValue,X=N(x)})>0;
    public async Task CrearAsync(Contrato x)=>await db.EjecutarAsync("INSERT app.Contrato (Id,CodigoContrato,CodigoEmpleado,TipoContrato,SueldoBase,HorasMensuales,FechaInicio,FechaFin,Estado,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion) VALUES (@Id,@CodigoContrato,@CodigoEmpleado,@TipoContrato,@SueldoBase,@HorasMensuales,@FechaInicio,@FechaFin,@Estado,@Activo,@CreadoPor,@FechaCreacion,@ModificadoPor,@FechaModificacion)",x);
    public async Task<bool> ActualizarAsync(Contrato x)=>await db.EjecutarAsync("UPDATE app.Contrato SET CodigoEmpleado=@CodigoEmpleado,TipoContrato=@TipoContrato,SueldoBase=@SueldoBase,HorasMensuales=@HorasMensuales,FechaInicio=@FechaInicio,FechaFin=@FechaFin,Estado=@Estado,Activo=@Activo,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoContrato=@CodigoContrato",x)>0;
    private static string N(string? x)=>(x??"").Trim().ToUpperInvariant();
}

public sealed class CategoriaGastoRepositorio(SqlContexto db)
{
    private const string C="Id,CodigoCategoria,Nombre,Descripcion,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion";
    public Task<List<CategoriaGasto>> ListarAsync()=>db.ConsultarAsync<CategoriaGasto>($"SELECT {C} FROM app.CategoriaGasto ORDER BY Nombre");
    public async Task<CategoriaGasto?> ObtenerAsync(string c)=>(await db.ConsultarAsync<CategoriaGasto>($"SELECT {C} FROM app.CategoriaGasto WHERE CodigoCategoria=@C",new{C=N(c)})).FirstOrDefault();
    public async Task CrearAsync(CategoriaGasto x)=>await db.EjecutarAsync("INSERT app.CategoriaGasto (Id,CodigoCategoria,Nombre,Descripcion,Activo,CreadoPor,FechaCreacion,ModificadoPor,FechaModificacion) VALUES (@Id,@CodigoCategoria,@Nombre,@Descripcion,@Activo,@CreadoPor,@FechaCreacion,@ModificadoPor,@FechaModificacion)",x);
    public async Task<bool> ActualizarAsync(CategoriaGasto x)=>await db.EjecutarAsync("UPDATE app.CategoriaGasto SET Nombre=@Nombre,Descripcion=@Descripcion,Activo=@Activo,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoCategoria=@CodigoCategoria",x)>0;
    private static string N(string? x)=>(x??"").Trim().ToUpperInvariant();
}

public sealed class ParametroFinancieroRepositorio(SqlContexto db)
{
    private const string C="Id,CodigoParametro,Descripcion,TipoDato,ValorTexto,ValorDecimal,ValorEntero,Activo,ModificadoPor,FechaModificacion";
    public Task<List<ParametroFinanciero>> ListarAsync()=>db.ConsultarAsync<ParametroFinanciero>($"SELECT {C} FROM app.ParametroFinanciero ORDER BY CodigoParametro");
    public async Task<ParametroFinanciero?> ObtenerAsync(string c)=>(await db.ConsultarAsync<ParametroFinanciero>($"SELECT {C} FROM app.ParametroFinanciero WHERE CodigoParametro=@C",new{C=N(c)})).FirstOrDefault();
    public async Task UpsertAsync(ParametroFinanciero x)=>await db.EjecutarAsync("UPDATE app.ParametroFinanciero SET Descripcion=@Descripcion,TipoDato=@TipoDato,ValorTexto=@ValorTexto,ValorDecimal=@ValorDecimal,ValorEntero=@ValorEntero,Activo=@Activo,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoParametro=@CodigoParametro; IF @@ROWCOUNT=0 INSERT app.ParametroFinanciero (Id,CodigoParametro,Descripcion,TipoDato,ValorTexto,ValorDecimal,ValorEntero,Activo,ModificadoPor,FechaModificacion) VALUES (@Id,@CodigoParametro,@Descripcion,@TipoDato,@ValorTexto,@ValorDecimal,@ValorEntero,@Activo,@ModificadoPor,@FechaModificacion)",x);
    private static string N(string? x)=>(x??"").Trim().ToUpperInvariant();
}

public sealed class MovimientoProyectoRepositorio(SqlContexto db)
{
    private const string C="Id,CodigoMovimiento,CodigoProyecto,Tipo,CodigoCategoria,Concepto,Valor,Fecha,Estado,RegistradoPor,FechaRegistro,AprobadoPor,FechaAprobacion,Observacion,ModificadoPor,FechaModificacion";
    public Task<List<MovimientoProyecto>> ListarAsync()=>db.ConsultarAsync<MovimientoProyecto>($"SELECT {C} FROM app.MovimientoProyecto ORDER BY Fecha DESC");
    public async Task<MovimientoProyecto?> ObtenerAsync(string c)=>(await db.ConsultarAsync<MovimientoProyecto>($"SELECT {C} FROM app.MovimientoProyecto WHERE CodigoMovimiento=@C",new{C=N(c)})).FirstOrDefault();
    public async Task CrearAsync(MovimientoProyecto x)=>await db.EjecutarAsync("INSERT app.MovimientoProyecto (Id,CodigoMovimiento,CodigoProyecto,Tipo,CodigoCategoria,Concepto,Valor,Fecha,Estado,RegistradoPor,FechaRegistro,AprobadoPor,FechaAprobacion,Observacion,ModificadoPor,FechaModificacion) VALUES (@Id,@CodigoMovimiento,@CodigoProyecto,@Tipo,@CodigoCategoria,@Concepto,@Valor,@Fecha,@Estado,@RegistradoPor,@FechaRegistro,@AprobadoPor,@FechaAprobacion,@Observacion,@ModificadoPor,@FechaModificacion)",x);
    public async Task<bool> ActualizarAsync(MovimientoProyecto x)=>await db.EjecutarAsync("UPDATE app.MovimientoProyecto SET CodigoProyecto=@CodigoProyecto,Tipo=@Tipo,CodigoCategoria=@CodigoCategoria,Concepto=@Concepto,Valor=@Valor,Fecha=@Fecha,Estado=@Estado,RegistradoPor=@RegistradoPor,FechaRegistro=@FechaRegistro,AprobadoPor=@AprobadoPor,FechaAprobacion=@FechaAprobacion,Observacion=@Observacion,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion WHERE CodigoMovimiento=@CodigoMovimiento",x)>0;
    public async Task<decimal> GastosAprobadosAsync(string p)=>await db.EscalarAsync<decimal>("SELECT COALESCE(SUM(Valor),0) FROM app.MovimientoProyecto WHERE CodigoProyecto=@P AND Estado='APROBADO'",new{P=N(p)});
    private static string N(string? x)=>(x??"").Trim().ToUpperInvariant();
}
