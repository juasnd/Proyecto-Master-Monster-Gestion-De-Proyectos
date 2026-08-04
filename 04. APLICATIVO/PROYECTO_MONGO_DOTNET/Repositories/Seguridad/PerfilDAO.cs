using PROYECTO_MONGO_DOTNET.Models;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class PerfilDAO(SqlContexto db):SqlDAO(db)
{
 private const string Q="SELECT p.Codigo,p.Descripcion,p.Observacion,p.Estado EstadoCodigo,(SELECT COUNT(1) FROM app.UsuarioPerfil u WHERE u.CodigoPerfil=p.Codigo AND u.FechaRetiro IS NULL) UsuariosAsignados FROM app.Perfil p";
 public List<Perfil> Listar(bool incluirInactivos=true)=>Db.Consultar<Perfil>(Q+(incluirInactivos?"":" WHERE p.Estado='A'")+" ORDER BY p.Descripcion");
 public Perfil? Buscar(string? c)=>string.IsNullOrWhiteSpace(c)?null:Db.Consultar<Perfil>(Q+" WHERE p.Codigo=@C",new{C=Mayuscula(c)}).FirstOrDefault();
 public bool Existe(string? c)=>Buscar(c)!=null;
 public void Insertar(Perfil x,string u)=>Db.Ejecutar("INSERT app.Perfil(Codigo,Descripcion,Observacion,Estado,CreadoPor,FechaCreacion) VALUES(@Codigo,@Descripcion,@Observacion,'A',@U,SYSUTCDATETIME())",new{x.Codigo,x.Descripcion,x.Observacion,U=u});
 public bool Actualizar(Perfil x,string u)=>Db.Ejecutar("UPDATE app.Perfil SET Descripcion=@Descripcion,Observacion=@Observacion,ModificadoPor=@U,FechaModificacion=SYSUTCDATETIME() WHERE Codigo=@Codigo",new{x.Codigo,x.Descripcion,x.Observacion,U=u})>0;
 public bool CambiarEstado(string c,string e,string u)=>Db.Ejecutar("UPDATE app.Perfil SET Estado=@E,ModificadoPor=@U,FechaModificacion=SYSUTCDATETIME() WHERE Codigo=@C",new{C=Mayuscula(c),E=Mayuscula(e),U=u})>0;
 public bool EnUso(string c)=>Db.Escalar<int>("SELECT COUNT(1) FROM app.UsuarioPerfil WHERE CodigoPerfil=@C AND FechaRetiro IS NULL",new{C=Mayuscula(c)})>0;
}
