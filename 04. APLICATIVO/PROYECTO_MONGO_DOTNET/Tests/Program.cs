using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Proyectos;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;
using PROYECTO_MONGO_DOTNET.Services.Personal;
using PROYECTO_MONGO_DOTNET.Services.Proyectos;
using PROYECTO_MONGO_DOTNET.Services.Seguridad;

var pruebas = new List<(string Nombre, Func<Task> Ejecutar)>();
void Caso(string nombre, Action accion) => pruebas.Add((nombre, () => { accion(); return Task.CompletedTask; }));
void Igual<T>(T esperado, T actual) { if (!EqualityComparer<T>.Default.Equals(esperado, actual)) throw new Exception($"Esperado {esperado}, recibido {actual}"); }
void Rechaza(Action accion) { try { accion(); } catch (ExcepcionNegocio) { return; } throw new Exception("Se aceptó una entrada inválida"); }

Caso("Costo horario usa decimal y redondea seis decimales", () => Igual(6.666667m, ReglasFinanzas.CostoHora(1000m, 150m)));
Caso("Costo horario rechaza divisor cero", () => Rechaza(() => ReglasFinanzas.CostoHora(1000, 0)));
Caso("Guardar exige permiso de editar incluso con prefijo del DTO", () => Igual(PermisoAccion.Editar,
    PermisoRequeridoAttribute.AccionGuardar(new Dictionary<string,string?> { ["dto.CodigoContrato"]="CON1" }, "CodigoContrato")));
Caso("Guardar sin identificador exige permiso de crear", () => Igual(PermisoAccion.Crear,
    PermisoRequeridoAttribute.AccionGuardar(new Dictionary<string,string?> { ["CodigoContrato"]="" }, "CodigoContrato")));
Caso("Costo y saldo conservan el cálculo existente", () => {
    var resumen = CostoProyectoServicio.Calcular(new CostoProyectoFila { Presupuesto = 1000, Avance = 50, CostoLaboralPlanificado = 1200, GastosAprobados = 250 }, 80);
    Igual(850m, resumen.CostoComprometidoEstimado); Igual(150m, resumen.SaldoDisponible); Igual(true, resumen.EnAlerta);
});
Caso("Presupuesto cero no divide por cero", () => Igual(100m, CostoProyectoServicio.Calcular(new CostoProyectoFila { GastosAprobados = 1 }, 80).PorcentajeConsumo));
Caso("Movimiento rechaza tipo desconocido y fecha vacía", () => {
    var dto = new GuardarMovimientoDto { CodigoProyecto="PRO1", CodigoCategoria="CAT1", Concepto="Compra", Valor=1, Tipo="OTRO", Fecha=DateTime.Today };
    Rechaza(() => ReglasFinanzas.ValidarMovimiento(dto)); dto.Tipo="GASTO"; dto.Fecha=default; Rechaza(() => ReglasFinanzas.ValidarMovimiento(dto));
});
Caso("Disponibilidad acepta 100 y rechaza exceso", () => {
    var dto = new GuardarAsignacionDto { Rol="Desarrollo", DisponibilidadAsignada=40, FechaInicio=DateTime.Today, FechaFin=DateTime.Today.AddDays(1) };
    ReglasProyecto.ValidarAsignacion(dto,60); Rechaza(() => ReglasProyecto.ValidarAsignacion(dto,61));
});
Caso("Transiciones guiadas preservan inicio, pausa y reanudación", () => {
    var permitidas = new HashSet<(string,string)> { ("PLANIFICADO","EN_CURSO"), ("EN_CURSO","PAUSADO"), ("PAUSADO","EN_CURSO") };
    foreach (var origen in new[] { "PLANIFICADO","EN_CURSO","PAUSADO","FINALIZADO","CANCELADO" })
        foreach (var destino in new[] { "PLANIFICADO","EN_CURSO","PAUSADO","FINALIZADO","CANCELADO" })
            Igual(permitidas.Contains((origen,destino)), ReglasProyecto.PuedeCambiarEstado(origen,destino));
});
Caso("Proyecto rechaza fechas invertidas", () => Rechaza(() => ReglasProyecto.Validar(new GuardarProyectoDto { Nombre="Proyecto", CodigoDepartamento="DEP1", CodigoJefeProyecto="EMP1", Prioridad="MEDIA", FechaInicio=DateTime.Today, FechaFinPlanificada=DateTime.Today.AddDays(-1) })));
Caso("Avance ponderado no renormaliza un plan incompleto", () => Igual(30m, ReglasPlanificacion.AvanceReal(new[] { new TareaProyecto { Peso=20, Estado="COMPLETADA" }, new TareaProyecto { Peso=20, Estado="EN_PROCESO" } })));
Caso("Detecta ciclos, autociclos y acepta un grafo acíclico", () => {
    var a = new DependenciaTarea { CodigoTarea="B", CodigoPredecesora="A" };
    Igual(false, ReglasPlanificacion.TieneCiclo(new[] { a }));
    Igual(true, ReglasPlanificacion.TieneCiclo(new[] { a, new DependenciaTarea { CodigoTarea="A", CodigoPredecesora="B" } }));
    Igual(true, ReglasPlanificacion.TieneCiclo(new[] { new DependenciaTarea { CodigoTarea="A", CodigoPredecesora="A" } }));
});
Caso("Dependencias extensas no desbordan la pila", () => Igual(false, ReglasPlanificacion.TieneCiclo(Enumerable.Range(1,20000).Select(i => new DependenciaTarea { CodigoTarea=i.ToString(), CodigoPredecesora=(i-1).ToString() }))));
Caso("Firma de foto no acepta HTML renombrado", () => {
    Igual(false, FotoEmpleadoServicio.FirmaValida("<html>"u8, ".png"));
    Igual(true, FotoEmpleadoServicio.FirmaValida(new byte[] {137,80,78,71,13,10,26,10}, ".png"));
    Igual(false, FotoEmpleadoServicio.FirmaValida(new byte[] {137,80,78,71,13,10,26,10}, ".jpg"));
});
pruebas.Add(("Permisos devuelven 401 sin sesión, sin consultar SQL", async () => {
    var http = new DefaultHttpContext(); http.Request.Path="/api/v1/proyectos";
    http.Features.Set<ISessionFeature>(new SesionFeaturePruebas { Session = new SesionMemoria() });
    var contexto = new AuthorizationFilterContext(new ActionContext(http,new RouteData(),new ActionDescriptor()), new List<IFilterMetadata>());
    await new PermisoRequeridoAttribute("PRO_PROYECTOS").OnAuthorizationAsync(contexto);
    Igual(true, contexto.Result is UnauthorizedObjectResult);
}));
pruebas.Add(("Filtro global rechaza sesión ausente antes de acceder a datos", async () => {
    var http = new DefaultHttpContext(); http.Request.Path="/api/v1/proyectos";
    http.Features.Set<ISessionFeature>(new SesionFeaturePruebas { Session = new SesionMemoria() });
    var contexto = new AuthorizationFilterContext(new ActionContext(http,new RouteData(),new ActionDescriptor()), new List<IFilterMetadata>());
    await new SesionActivaFiltro(new SqlContexto(new ConfigurationBuilder().Build())).OnAuthorizationAsync(contexto);
    Igual(true, contexto.Result is UnauthorizedObjectResult);
}));
pruebas.Add(("Revisión concurrente fallida devuelve conflicto", async () => {
    var servicio = new MovimientoProyectoServicio(new MovimientosMemoria(), null!, null!, null!);
    try { await servicio.RevisarAsync("MOV1","APROBADO","","admin"); }
    catch (ExcepcionNegocio ex) { Igual(409,ex.CodigoHttp); return; }
    throw new Exception("La revisión ignoró el conflicto");
}));
var fallos=0;
foreach (var (nombre,ejecutar) in pruebas)
{
    try { await ejecutar(); Console.WriteLine($"OK {nombre}"); }
    catch (Exception ex) { fallos++; Console.Error.WriteLine($"FALLO {nombre}: {ex.Message}"); }
}
Console.WriteLine($"{pruebas.Count-fallos}/{pruebas.Count} pruebas correctas");
return fallos==0 ? 0 : 1;

sealed class SesionFeaturePruebas : ISessionFeature
{
    public ISession Session { get; set; } = null!;
}

sealed class SesionMemoria : ISession
{
    private readonly Dictionary<string,byte[]> datos=new();
    public bool IsAvailable=>true;
    public string Id=>"pruebas";
    public IEnumerable<string> Keys=>datos.Keys;
    public void Clear()=>datos.Clear();
    public Task CommitAsync(CancellationToken cancellationToken=default)=>Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken=default)=>Task.CompletedTask;
    public void Remove(string key)=>datos.Remove(key);
    public void Set(string key,byte[] value)=>datos[key]=value;
    public bool TryGetValue(string key,out byte[] value)=>datos.TryGetValue(key,out value!);
}

sealed class MovimientosMemoria : IMovimientoProyectoRepositorio
{
    public Task<MovimientoProyecto?> ObtenerAsync(string codigo)=>Task.FromResult<MovimientoProyecto?>(new() { CodigoMovimiento=codigo });
    public Task<bool> RevisarPendienteAsync(MovimientoProyecto movimiento)=>Task.FromResult(false);
    public Task<List<MovimientoProyecto>> ListarAsync()=>throw new NotSupportedException();
    public Task CrearAsync(MovimientoProyecto movimiento)=>throw new NotSupportedException();
    public Task<bool> ActualizarAsync(MovimientoProyecto movimiento)=>throw new NotSupportedException();
    public Task<decimal> GastosAprobadosAsync(string proyecto)=>throw new NotSupportedException();
}
