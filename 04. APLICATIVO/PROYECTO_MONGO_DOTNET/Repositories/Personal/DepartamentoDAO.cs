using PROYECTO_MONGO_DOTNET.Models;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class DepartamentoDAO(SqlContexto db,CodigoDAO codigo):SqlDAO(db)
{
 public List<Departamento> Listar()=>Db.Consultar<Departamento>("SELECT Codigo,Descripcion FROM app.Departamento ORDER BY Descripcion");
 public Departamento? Buscar(string c)=>Db.Consultar<Departamento>("SELECT Codigo,Descripcion FROM app.Departamento WHERE Codigo=@C",new{C=Mayuscula(c)}).FirstOrDefault();
 public bool Insertar(Departamento x){x.Codigo=codigo.GenerarCodigo("departamento","PEDEP_CODIGO");return Db.Ejecutar("INSERT app.Departamento(Codigo,Descripcion,Estado) VALUES(@Codigo,@Descripcion,'A')",x)>0;}
 public bool Actualizar(Departamento x)=>Db.Ejecutar("UPDATE app.Departamento SET Descripcion=@Descripcion WHERE Codigo=@Codigo",x)>0;
 public bool Eliminar(string c)=>Db.Ejecutar("DELETE app.Departamento WHERE Codigo=@C",new{C=Mayuscula(c)})>0;
 public bool EnUso(string c)=>Db.Escalar<int>("SELECT (SELECT COUNT(1) FROM app.Cargo WHERE CodigoDepartamento=@C)+(SELECT COUNT(1) FROM app.Empleado WHERE CodigoDepartamento=@C OR CodigoDepartamentoPago=@C)",new{C=Mayuscula(c)})>0;
}
