using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class EmpleadoDAO(SqlContexto db,CodigoDAO codigo,PersonaDAO personas,DepartamentoDAO departamentos,CargoDAO cargos,FamiliarDAO familiares,FormacionDAO formaciones):SqlDAO(db)
{
 public List<Empleado> Listar()=>MapearListado(Db.Consultar<FilaEmpleado>("SELECT Codigo,CodigoDepartamento,CodigoCargo,CodigoPersona,CodigoDepartamentoPago FROM app.Empleado ORDER BY Codigo")).OrderBy(x=>x.Persona.Apellidos).ThenBy(x=>x.Persona.Nombres).ToList();
 public List<Empleado> ListarActivos()=>ListarPorCondicion("e.Estado='A' AND p.Tipo='EMP'");
 public List<Empleado> ListarAsignablesProyecto()=>ListarPorCondicion("e.Estado='A' AND p.Tipo='EMP' AND EXISTS(SELECT 1 FROM app.Usuario u WHERE u.CodigoPersona=p.Codigo AND u.Estado='A')");
 public List<Empleado> ListarJefesProyecto()=>ListarPorCondicion("e.Estado='A' AND p.Tipo='EMP' AND c.Estado='A' AND c.EsJefeProyecto=1 AND EXISTS(SELECT 1 FROM app.Usuario u WHERE u.CodigoPersona=p.Codigo AND u.Estado='A')");
 public List<Empleado> Buscar(string criterio,string termino){if(string.IsNullOrWhiteSpace(termino))return Listar();var t=termino.Trim();return Listar().Where(e=>(criterio switch{"nombres"=>e.Persona.Nombres,"apellidos"=>e.Persona.Apellidos,"cedula"=>e.Persona.Cedula,_=>e.PeempCodigo}).Contains(t,StringComparison.OrdinalIgnoreCase)).ToList();}
 public Empleado? BuscarPorCodigo(string c){var x=Db.Consultar<FilaEmpleado>("SELECT Codigo,CodigoDepartamento,CodigoCargo,CodigoPersona,CodigoDepartamentoPago FROM app.Empleado WHERE Codigo=@C",new{C=Mayuscula(c)}).FirstOrDefault();return x==null?null:Mapear(x,true);}
 public Empleado? BuscarActivoPorCodigo(string c)
 {
     var fila=Db.Consultar<FilaEmpleado>("SELECT e.Codigo,e.CodigoDepartamento,e.CodigoCargo,e.CodigoPersona,e.CodigoDepartamentoPago FROM app.Empleado e JOIN app.Persona p ON p.Codigo=e.CodigoPersona WHERE e.Codigo=@C AND e.Estado='A' AND p.Tipo='EMP'",new{C=Mayuscula(c)}).FirstOrDefault();
     return fila==null?null:Mapear(fila,false);
 }
 public bool TieneUsuarioActivo(string c)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.Empleado e JOIN app.Persona p ON p.Codigo=e.CodigoPersona JOIN app.Usuario u ON u.CodigoPersona=p.Codigo WHERE e.Codigo=@C AND e.Estado='A' AND p.Tipo='EMP' AND u.Estado='A'",new{C=Mayuscula(c)})>0;
 public bool EsJefeProyectoActivo(string c)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.Empleado e JOIN app.Persona p ON p.Codigo=e.CodigoPersona JOIN app.Cargo ca ON ca.Codigo=e.CodigoCargo AND ca.CodigoDepartamento=e.CodigoDepartamento JOIN app.Usuario u ON u.CodigoPersona=p.Codigo WHERE e.Codigo=@C AND e.Estado='A' AND p.Tipo='EMP' AND ca.Estado='A' AND ca.EsJefeProyecto=1 AND u.Estado='A'",new{C=Mayuscula(c)})>0;
 public bool ExisteEmpleado(string c)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.Empleado WHERE Codigo=@C",new{C=Mayuscula(c)})>0;
 public bool ExisteEmpleadoPorPersona(string p)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.Empleado WHERE CodigoPersona=@P",new{P=Mayuscula(p)})>0;
 public bool ExisteEmpleadoConCedula(string c){var p=personas.BuscarPorCedula(c);return p!=null&&ExisteEmpleadoPorPersona(p.PeperCodigo);}
 public bool ExistePersona(string p)=>personas.Existe(p); public bool ExisteCedula(string c)=>personas.ExisteCedula(c); public bool ExisteCedulaEnOtraPersona(string c,string p)=>personas.ExisteCedulaEnOtraPersona(c,p);
 public void Guardar(Empleado e)
 {
     using var conexion = Db.Abrir();
     using var transaccion = conexion.BeginTransaction();
     e.PeempCodigo = string.IsNullOrWhiteSpace(e.PeempCodigo) ? codigo.GenerarCodigo("empleado", "PEEMP_CODIGO") : Mayuscula(e.PeempCodigo);
     if (ExisteEmpleado(e.PeempCodigo)) throw new ExcepcionNegocio("Ya existe un registro con el código indicado.");
     var persona = personas.BuscarPorCedula(e.Persona.Cedula, transaccion);
     e.Persona.Tipo = "EMP";
     if (persona != null)
     {
         if (ExisteEmpleadoPorPersona(persona.PeperCodigo)) throw new ExcepcionNegocio("Ya existe un empleado registrado con esta cédula.");
         e.Persona.PeperCodigo = persona.PeperCodigo;
         personas.Actualizar(e.Persona, transaccion);
     }
     else
     {
         e.Persona.PeperCodigo = string.IsNullOrWhiteSpace(e.Persona.PeperCodigo) ? codigo.GenerarCodigo("persona", "PEPER_CODIGO") : Mayuscula(e.Persona.PeperCodigo);
         if (personas.Existe(e.Persona.PeperCodigo, transaccion)) throw new ExcepcionNegocio("Ya existe un registro con el código indicado.");
         personas.Insertar(e.Persona, transaccion);
     }
     Db.Ejecutar("INSERT app.Empleado(Codigo,CodigoDepartamento,CodigoCargo,CodigoPersona,CodigoDepartamentoPago,Estado) VALUES(@Codigo,@Departamento,@Cargo,@Persona,@Pago,'A')",
         new { Codigo=e.PeempCodigo, Departamento=Mayuscula(e.PedepCodigo), Cargo=Mayuscula(e.PecarCodigo), Persona=Mayuscula(e.Persona.PeperCodigo),
             Pago=Mayuscula(string.IsNullOrWhiteSpace(e.PedPedepCodigo) ? e.PedepCodigo : e.PedPedepCodigo) }, transaccion);
     familiares.Reemplazar(e.Persona.PeperCodigo, e.Familiares, transaccion);
     formaciones.Reemplazar(e.PeempCodigo, e.Formaciones, transaccion);
     transaccion.Commit();
 }

 public void Actualizar(Empleado e)
 {
     using var conexion = Db.Abrir();
     using var transaccion = conexion.BeginTransaction();
     personas.Actualizar(e.Persona, transaccion);
     var filas = Db.Ejecutar("UPDATE app.Empleado SET CodigoDepartamento=@D,CodigoCargo=@C,CodigoDepartamentoPago=@P WHERE Codigo=@E AND CodigoPersona=@Persona",
         new { D=Mayuscula(e.PedepCodigo), C=Mayuscula(e.PecarCodigo), P=Mayuscula(e.PedPedepCodigo), Persona=Mayuscula(e.Persona.PeperCodigo), E=Mayuscula(e.PeempCodigo) }, transaccion);
     if (filas != 1) throw new ExcepcionNegocio("La persona no corresponde al empleado indicado.");
     familiares.Reemplazar(e.Persona.PeperCodigo, e.Familiares, transaccion);
     formaciones.Reemplazar(e.PeempCodigo, e.Formaciones, transaccion);
     transaccion.Commit();
 }

 public bool Eliminar(string codigoEmpleado)
 {
     var empleado = BuscarPorCodigo(codigoEmpleado);
     if (empleado == null) return false;
     using var conexion = Db.Abrir();
     using var transaccion = conexion.BeginTransaction();
     formaciones.EliminarPorEmpleado(codigoEmpleado, transaccion);
     Db.Ejecutar("DELETE app.Empleado WHERE Codigo=@Codigo", new { Codigo=Mayuscula(codigoEmpleado) }, transaccion);
     familiares.EliminarPorPersona(empleado.Persona.PeperCodigo, transaccion);
     personas.Eliminar(empleado.Persona.PeperCodigo, transaccion);
     transaccion.Commit();
     return true;
 }

 public bool EmpleadoTieneRelaciones(string e)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.AsignacionProyecto WHERE CodigoEmpleado=@E",new{E=Mayuscula(e)})>0;
 public bool PersonaTieneRelacionesExternas(string p,string e)=>Db.Escalar<int>("SELECT (SELECT COUNT(1) FROM app.Usuario WHERE CodigoPersona=@P)+(SELECT COUNT(1) FROM app.Empleado WHERE CodigoPersona=@P AND Codigo<>@E)",new{P=Mayuscula(p),E=Mayuscula(e)})>0;
 public bool CargoPerteneceDepartamento(string d,string c)=>cargos.Buscar(d,c)!=null; public List<Departamento> ListarDepartamentos()=>departamentos.Listar(); public List<Cargo> ListarCargos()=>cargos.Listar(); public List<Cargo> CargosPorDepartamento(string d)=>cargos.PorDepartamento(d); public List<Catalogo> ListarSexos()=>Db.Consultar<Catalogo>("SELECT Codigo,Descripcion FROM app.Sexo ORDER BY Descripcion"); public List<Catalogo> ListarEstadosCiviles()=>Db.Consultar<Catalogo>("SELECT Codigo,Descripcion FROM app.EstadoCivil ORDER BY Descripcion"); public List<Parentesco> ListarParentescos()=>familiares.ListarParentescos();
 private List<Empleado> ListarPorCondicion(string condicion)=>MapearListado(Db.Consultar<FilaEmpleado>($"SELECT e.Codigo,e.CodigoDepartamento,e.CodigoCargo,e.CodigoPersona,e.CodigoDepartamentoPago FROM app.Empleado e JOIN app.Persona p ON p.Codigo=e.CodigoPersona JOIN app.Cargo c ON c.Codigo=e.CodigoCargo AND c.CodigoDepartamento=e.CodigoDepartamento WHERE {condicion} ORDER BY p.Apellidos,p.Nombres"));
 private List<Empleado> MapearListado(List<FilaEmpleado> filas)
 {
     if(filas.Count==0)return [];
     var personasPorCodigo=personas.Listar().ToDictionary(p=>p.PeperCodigo,StringComparer.OrdinalIgnoreCase);
     var departamentosPorCodigo=departamentos.Listar().ToDictionary(d=>d.Codigo,StringComparer.OrdinalIgnoreCase);
     var cargosPorCodigo=cargos.Listar().ToDictionary(c=>(Mayuscula(c.PedepCodigo),Mayuscula(c.PecarCodigo)));
     var resultado=new List<Empleado>();
     foreach(var fila in filas)
     {
         if(!personasPorCodigo.TryGetValue(fila.CodigoPersona,out var persona))continue;
         departamentosPorCodigo.TryGetValue(fila.CodigoDepartamento,out var departamento);
         cargosPorCodigo.TryGetValue((Mayuscula(fila.CodigoDepartamento),Mayuscula(fila.CodigoCargo)),out var cargo);
         resultado.Add(new Empleado { PeempCodigo=fila.Codigo,PedepCodigo=fila.CodigoDepartamento,PecarCodigo=fila.CodigoCargo,
             PedPedepCodigo=fila.CodigoDepartamentoPago,Persona=persona,NombreDepartamento=departamento?.Descripcion??fila.CodigoDepartamento,
             NombreCargo=cargo?.PecarDescri??fila.CodigoCargo });
     }
     return resultado;
 }
 private Empleado? Mapear(FilaEmpleado x,bool completo){var p=personas.Buscar(x.CodigoPersona);if(p==null)return null;var e=new Empleado{PeempCodigo=x.Codigo,PedepCodigo=x.CodigoDepartamento,PecarCodigo=x.CodigoCargo,PedPedepCodigo=x.CodigoDepartamentoPago,Persona=p,NombreDepartamento=departamentos.Buscar(x.CodigoDepartamento)?.Descripcion??x.CodigoDepartamento,NombreCargo=cargos.Buscar(x.CodigoDepartamento,x.CodigoCargo)?.PecarDescri??x.CodigoCargo};if(completo){e.Familiares=familiares.PorPersona(p.PeperCodigo);e.Formaciones=formaciones.PorEmpleado(e.PeempCodigo);}return e;}
 private sealed class FilaEmpleado{public string Codigo{get;set;}="";public string CodigoDepartamento{get;set;}="";public string CodigoCargo{get;set;}="";public string CodigoPersona{get;set;}="";public string CodigoDepartamentoPago{get;set;}="";}
}
