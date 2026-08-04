using PROYECTO_MONGO_DOTNET.Models;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class FamiliarDAO(SqlContexto db,CodigoDAO codigo):SqlDAO(db)
{
 public List<Parentesco> ListarParentescos()=>Db.Consultar<Parentesco>("SELECT Codigo,Descripcion FROM app.Parentesco ORDER BY Descripcion");
 public List<Familiar> PorPersona(string p)=>Db.Consultar<Familiar>("SELECT f.Codigo,f.CodigoPersona,f.CodigoParentesco,p.Descripcion DescripcionParentesco,f.Nombre,f.Apellido,CONVERT(varchar(10),f.FechaNacimiento,23) FechaNacimiento,f.Telefono,f.CargaFamiliar,f.Observacion FROM app.Familiar f LEFT JOIN app.Parentesco p ON p.Codigo=f.CodigoParentesco WHERE f.CodigoPersona=@P ORDER BY f.Apellido,f.Nombre",new{P=Mayuscula(p)});
 public void Reemplazar(string p,IEnumerable<Familiar> xs){using var cn=Db.Abrir();using var tx=cn.BeginTransaction();try{Db.Ejecutar("DELETE app.Familiar WHERE CodigoPersona=@P",new{P=Mayuscula(p)},tx);foreach(var x in xs.Where(x=>!string.IsNullOrWhiteSpace(x.Nombre)&&!string.IsNullOrWhiteSpace(x.Apellido))){x.Codigo=codigo.GenerarCodigo("familiar","PEFAM_CODIGO");x.CodigoPersona=Mayuscula(p);x.CodigoParentesco=Mayuscula(x.CodigoParentesco);x.CargaFamiliar=string.IsNullOrWhiteSpace(x.CargaFamiliar)?"S":Mayuscula(x.CargaFamiliar);Db.Ejecutar("INSERT app.Familiar(Codigo,CodigoPersona,CodigoParentesco,Nombre,Apellido,FechaNacimiento,Telefono,CargaFamiliar,Observacion) VALUES(@Codigo,@CodigoPersona,@CodigoParentesco,@Nombre,@Apellido,TRY_CONVERT(date,NULLIF(@FechaNacimiento,'')),@Telefono,@CargaFamiliar,@Observacion)",x,tx);}tx.Commit();}catch{tx.Rollback();throw;}}
 public void EliminarPorPersona(string p)=>Db.Ejecutar("DELETE app.Familiar WHERE CodigoPersona=@P",new{P=Mayuscula(p)});
}
