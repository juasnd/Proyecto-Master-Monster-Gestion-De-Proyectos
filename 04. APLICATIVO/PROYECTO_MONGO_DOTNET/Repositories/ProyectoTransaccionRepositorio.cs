using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Repositories;

public interface IProyectoTransaccionRepositorio
{
    Task CambiarEstadoAsync(Proyecto proyecto, SeguimientoProyecto seguimiento);
}

public sealed class ProyectoTransaccionRepositorio(SqlContexto db) : IProyectoTransaccionRepositorio
{
    public async Task CambiarEstadoAsync(Proyecto proyecto, SeguimientoProyecto seguimiento)
    {
        using var conexion = db.Abrir();
        using var transaccion = conexion.BeginTransaction();
        // El estado esperado evita que dos transiciones aprobadas sobre la misma lectura se sobrescriban.
        var filas = await db.EjecutarAsync("""
            UPDATE app.Proyecto SET Estado=@Estado,Observacion=@Observacion,Avance=@Avance,
                FechaFinReal=@FechaFinReal,ModificadoPor=@ModificadoPor,FechaModificacion=@FechaModificacion
            WHERE Codigo=@Codigo AND Activo=1 AND Estado=@EstadoAnterior
            """, new { proyecto.Estado, proyecto.Observacion, proyecto.Avance, proyecto.FechaFinReal,
                proyecto.ModificadoPor, proyecto.FechaModificacion, proyecto.Codigo, seguimiento.EstadoAnterior }, transaccion: transaccion);
        if (filas != 1) throw ExcepcionNegocio.Conflicto("El proyecto cambió mientras se procesaba la solicitud. Recargue los datos.");
        await db.EjecutarAsync("""
            INSERT app.SeguimientoProyecto(Id,CodigoSeguimiento,CodigoProyecto,AvanceAnterior,AvanceNuevo,
                EstadoAnterior,EstadoNuevo,Observacion,RegistradoPor,FechaRegistro)
            VALUES(@Id,@CodigoSeguimiento,@CodigoProyecto,@AvanceAnterior,@AvanceNuevo,@EstadoAnterior,
                @EstadoNuevo,@Observacion,@RegistradoPor,@FechaRegistro)
            """, seguimiento, transaccion: transaccion);
        if (EstadosProyecto.Cerrados.Contains(proyecto.Estado))
            await db.EjecutarAsync("""
                UPDATE app.AsignacionProyecto SET Activo=0,Estado='CERRADA',ModificadoPor=@Usuario,
                    FechaModificacion=@Fecha WHERE CodigoProyecto=@Codigo AND Activo=1
                """, new { Usuario = proyecto.ModificadoPor, Fecha = proyecto.FechaModificacion, proyecto.Codigo }, transaccion: transaccion);
        transaccion.Commit();
    }
}
