using PROYECTO_MONGO_DOTNET.Models;
namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class ReporteHistorialDAO(SqlContexto db,CodigoDAO codigo):SqlDAO(db)
{
 private const string Q="SELECT Codigo,CONVERT(varchar(33),Fecha,127) Fecha,Usuario,Perfil,CodigoReporte,Modulo,TipoReporte,Formato,TotalRegistros,Filtros FROM app.ReporteHistorial";
 public List<ReporteHistorial> Listar()=>Db.Consultar<ReporteHistorial>(Q+" ORDER BY Fecha DESC"); public ReporteHistorial? Buscar(string c)=>Db.Consultar<ReporteHistorial>(Q+" WHERE Codigo=@C",new{C=Mayuscula(c)}).FirstOrDefault();
 public void Registrar(ReporteHistorial x)=>Db.Ejecutar("INSERT app.ReporteHistorial(Codigo,Fecha,Usuario,Perfil,CodigoReporte,Modulo,TipoReporte,Formato,TotalRegistros,Filtros) VALUES(@Codigo,SYSUTCDATETIME(),@Usuario,@Perfil,@CodigoReporte,@Modulo,@TipoReporte,@Formato,@TotalRegistros,@Filtros)",new{Codigo=codigo.GenerarCodigo("reportehistorial","XERHI_CODIGO"),x.Usuario,x.Perfil,x.CodigoReporte,x.Modulo,x.TipoReporte,x.Formato,x.TotalRegistros,x.Filtros});
 public bool Eliminar(string c)=>Db.Ejecutar("DELETE app.ReporteHistorial WHERE Codigo=@C",new{C=Mayuscula(c)})>0;
}
