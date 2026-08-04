using PROYECTO_MONGO_DOTNET.Models;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class CargoDAO(SqlContexto db,CodigoDAO codigo):SqlDAO(db)
{
 private const string Q="SELECT c.CodigoDepartamento PedepCodigo,c.Codigo PecarCodigo,c.Descripcion PecarDescri,d.Descripcion NombreDepartamento,c.EsJefeProyecto FROM app.Cargo c JOIN app.Departamento d ON d.Codigo=c.CodigoDepartamento";
 public List<Cargo> Listar()=>Db.Consultar<Cargo>(Q+" ORDER BY d.Descripcion,c.Descripcion");
 public List<Cargo> PorDepartamento(string d)=>Db.Consultar<Cargo>(Q+" WHERE c.CodigoDepartamento=@D ORDER BY c.Descripcion",new{D=Mayuscula(d)});
 public Cargo? Buscar(string d,string c)=>Db.Consultar<Cargo>(Q+" WHERE c.CodigoDepartamento=@D AND c.Codigo=@C",new{D=Mayuscula(d),C=Mayuscula(c)}).FirstOrDefault();
 public bool Insertar(Cargo x){x.PecarCodigo=codigo.GenerarCodigo("cargo","PECAR_CODIGO");return Db.Ejecutar("INSERT app.Cargo(CodigoDepartamento,Codigo,Descripcion,Estado) VALUES(@PedepCodigo,@PecarCodigo,@PecarDescri,'A')",x)>0;}
 public bool Actualizar(Cargo x)=>Db.Ejecutar("UPDATE app.Cargo SET Descripcion=@PecarDescri WHERE CodigoDepartamento=@PedepCodigo AND Codigo=@PecarCodigo",x)>0;
 public bool Eliminar(string d,string c)=>Db.Ejecutar("DELETE app.Cargo WHERE CodigoDepartamento=@D AND Codigo=@C",new{D=Mayuscula(d),C=Mayuscula(c)})>0;
 public bool EnUso(string d,string c)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.Empleado WHERE CodigoDepartamento=@D AND CodigoCargo=@C",new{D=Mayuscula(d),C=Mayuscula(c)})>0;
}
