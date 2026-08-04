using PROYECTO_MONGO_DOTNET.Models;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class PersonaDAO(SqlContexto db,CodigoDAO codigo):SqlDAO(db)
{
 private const string Q="SELECT p.Codigo PeperCodigo,p.Tipo,p.CodigoSexo PesexCodigo,p.CodigoEstadoCivil PeescCodigo,p.Nombres,p.Apellidos,p.Cedula,CONVERT(varchar(10),p.FechaNacimiento,23) FechaNacimiento,p.CargasFamiliares,p.Direccion,p.Celular,p.TelefonoDomicilio,p.Email,p.Foto,s.Descripcion SexoDescripcion,e.Descripcion EstadoCivilDescripcion FROM app.Persona p LEFT JOIN app.Sexo s ON s.Codigo=p.CodigoSexo LEFT JOIN app.EstadoCivil e ON e.Codigo=p.CodigoEstadoCivil";
 public List<Persona> Listar()=>Db.Consultar<Persona>(Q+" ORDER BY p.Apellidos,p.Nombres");
 public Persona? Buscar(string c)=>Db.Consultar<Persona>(Q+" WHERE p.Codigo=@C",new{C=Mayuscula(c)}).FirstOrDefault();
 public bool Existe(string c)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.Persona WHERE Codigo=@C",new{C=Mayuscula(c)})>0;
 public bool ExisteCedula(string c)=>!string.IsNullOrWhiteSpace(c)&&Db.Escalar<int>("SELECT COUNT(1) FROM app.Persona WHERE Cedula=@C",new{C=c.Trim()})>0;
 public Persona? BuscarPorCedula(string c)=>string.IsNullOrWhiteSpace(c)?null:Db.Consultar<Persona>(Q+" WHERE p.Cedula=@C",new{C=c.Trim()}).FirstOrDefault();
 public bool ExisteCedulaEnOtraPersona(string c,string p)=>!string.IsNullOrWhiteSpace(c)&&Db.Escalar<int>("SELECT COUNT(1) FROM app.Persona WHERE Cedula=@C AND Codigo<>@P",new{C=c.Trim(),P=Mayuscula(p)})>0;
 public void Insertar(Persona x){if(string.IsNullOrWhiteSpace(x.PeperCodigo))x.PeperCodigo=codigo.GenerarCodigo("persona","PEPER_CODIGO");Db.Ejecutar("INSERT app.Persona(Codigo,Tipo,CodigoSexo,CodigoEstadoCivil,Nombres,Apellidos,Cedula,FechaNacimiento,CargasFamiliares,Direccion,Celular,TelefonoDomicilio,Email,Foto) VALUES(@PeperCodigo,@Tipo,@PesexCodigo,@PeescCodigo,@Nombres,@Apellidos,@Cedula,TRY_CONVERT(date,NULLIF(@FechaNacimiento,'')),@CargasFamiliares,@Direccion,@Celular,@TelefonoDomicilio,@Email,@Foto)",Normalizar(x));}
 public void Actualizar(Persona x)=>Db.Ejecutar("UPDATE app.Persona SET Tipo=@Tipo,CodigoSexo=@PesexCodigo,CodigoEstadoCivil=@PeescCodigo,Nombres=@Nombres,Apellidos=@Apellidos,Cedula=@Cedula,FechaNacimiento=TRY_CONVERT(date,NULLIF(@FechaNacimiento,'')),CargasFamiliares=@CargasFamiliares,Direccion=@Direccion,Celular=@Celular,TelefonoDomicilio=@TelefonoDomicilio,Email=@Email,Foto=@Foto WHERE Codigo=@PeperCodigo",Normalizar(x));
 public void Eliminar(string c)=>Db.Ejecutar("DELETE app.Persona WHERE Codigo=@C",new{C=Mayuscula(c)});
 private static Persona Normalizar(Persona x){x.PeperCodigo=Mayuscula(x.PeperCodigo);x.Tipo=string.IsNullOrWhiteSpace(x.Tipo)?"EMP":Mayuscula(x.Tipo);x.PesexCodigo=Mayuscula(x.PesexCodigo);x.PeescCodigo=Mayuscula(x.PeescCodigo);return x;}
}
