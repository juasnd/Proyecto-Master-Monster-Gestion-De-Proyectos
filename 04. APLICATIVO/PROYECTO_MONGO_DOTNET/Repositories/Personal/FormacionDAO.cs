using PROYECTO_MONGO_DOTNET.Models;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class FormacionDAO(SqlContexto db,CodigoDAO codigo):SqlDAO(db)
{
 public List<Formacion> PorEmpleado(string e)=>Db.Consultar<Formacion>("SELECT Codigo,CodigoEmpleado,Nivel,Titulo,Institucion,CONVERT(varchar(10),FechaInicio,23) FechaInicio,CONVERT(varchar(10),FechaFin,23) FechaFin,Observacion FROM app.Formacion WHERE CodigoEmpleado=@E",new{E=Mayuscula(e)});
 public void Reemplazar(string e,IEnumerable<Formacion> xs){using var cn=Db.Abrir();using var tx=cn.BeginTransaction();try{Db.Ejecutar("DELETE app.Formacion WHERE CodigoEmpleado=@E",new{E=Mayuscula(e)},tx);foreach(var x in xs.Where(x=>new[]{x.Nivel,x.Titulo,x.Institucion,x.FechaInicio,x.FechaFin,x.Observacion}.Any(y=>!string.IsNullOrWhiteSpace(y)))){x.Codigo=codigo.GenerarCodigo("formacion","PEFOR_CODIGO");x.CodigoEmpleado=Mayuscula(e);Db.Ejecutar("INSERT app.Formacion(Codigo,CodigoEmpleado,Nivel,Titulo,Institucion,FechaInicio,FechaFin,Observacion) VALUES(@Codigo,@CodigoEmpleado,@Nivel,@Titulo,@Institucion,TRY_CONVERT(date,NULLIF(@FechaInicio,'')),TRY_CONVERT(date,NULLIF(@FechaFin,'')),@Observacion)",x,tx);}tx.Commit();}catch{tx.Rollback();throw;}}
 public void EliminarPorEmpleado(string e)=>Db.Ejecutar("DELETE app.Formacion WHERE CodigoEmpleado=@E",new{E=Mayuscula(e)});
}
