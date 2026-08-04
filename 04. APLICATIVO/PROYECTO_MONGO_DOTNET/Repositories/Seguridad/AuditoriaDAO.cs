namespace PROYECTO_MONGO_DOTNET.Services;
public sealed class AuditoriaDAO(SqlContexto db):SqlDAO(db){public void Registrar(string login,string tabla,string accion,string detalle)=>Db.Ejecutar("INSERT app.Auditoria(Fecha,Login,Tabla,Accion,Detalle) VALUES(SYSUTCDATETIME(),@Login,@Tabla,@Accion,@Detalle)",new{Login=login,Tabla=tabla,Accion=accion,Detalle=detalle});}
