using Microsoft.Data.SqlClient;
using PROYECTO_MONGO_DOTNET.Models.ViewModels;

namespace PROYECTO_MONGO_DOTNET.Services;

public sealed class AsignacionUsuarioPerfilDAO : SqlDAO
{
    public AsignacionUsuarioPerfilDAO(SqlContexto db) : base(db) { }

    private const string ConsultaUsuarios = """
        SELECT u.CodigoPersona,u.Login,COALESCE(p.Cedula,'') Cedula,COALESCE(p.Nombres,'') Nombres,
               COALESCE(p.Apellidos,'') Apellidos,COALESCE(actual.CodigoPerfil,'') PerfilCodigo,
               COALESCE(actual.Descripcion,'Sin perfil') PerfilDescripcion
        FROM app.Usuario u
        LEFT JOIN app.Persona p ON p.Codigo=u.CodigoPersona
        OUTER APPLY (
            SELECT TOP(1) up.CodigoPerfil,per.Descripcion
            FROM app.UsuarioPerfil up
            LEFT JOIN app.Perfil per ON per.Codigo=up.CodigoPerfil
            WHERE up.Login=u.Login AND up.FechaRetiro IS NULL
            ORDER BY up.FechaAsignacion DESC,up.Id DESC
        ) actual
        """;

    public List<UsuarioPerfilProcesoVm> ListarDisponibles(string perfil) => Db.Consultar<UsuarioPerfilProcesoVm>(ConsultaUsuarios + """

        WHERE NOT EXISTS(SELECT 1 FROM app.UsuarioPerfil x WHERE x.Login=u.Login AND x.CodigoPerfil=@Perfil AND x.FechaRetiro IS NULL)
        ORDER BY p.Apellidos,p.Nombres,u.Login
        """, new { Perfil = Mayuscula(perfil) });

    public List<UsuarioPerfilProcesoVm> ListarAsignados(string perfil) => Db.Consultar<UsuarioPerfilProcesoVm>(ConsultaUsuarios + """

        WHERE EXISTS(SELECT 1 FROM app.UsuarioPerfil x WHERE x.Login=u.Login AND x.CodigoPerfil=@Perfil AND x.FechaRetiro IS NULL)
        ORDER BY p.Apellidos,p.Nombres,u.Login
        """, new { Perfil = Mayuscula(perfil) });

    public int Asignar(string perfil, IEnumerable<string> logins, string usuario)
    {
        var unicos = Limpiar(logins);
        using var conexion = Db.Abrir();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            var procesados = 0;
            foreach (var login in unicos)
            {
                if (AsignarUno(conexion, transaccion, Mayuscula(perfil), login, usuario)) procesados++;
            }
            transaccion.Commit();
            return procesados;
        }
        catch { transaccion.Rollback(); throw; }
    }

    public int Retirar(string perfil, IEnumerable<string> logins, string usuario)
    {
        var unicos = Limpiar(logins);
        if (unicos.Count == 0) return 0;
        using var conexion = Db.Abrir();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            var total = 0;
            foreach (var login in unicos)
                total += Db.Ejecutar("UPDATE app.UsuarioPerfil SET FechaRetiro=SYSUTCDATETIME(),ModificadoPor=@Usuario WHERE CodigoPerfil=@Perfil AND Login=@Login AND FechaRetiro IS NULL", new { Perfil = Mayuscula(perfil), Login = login, Usuario = usuario }, transaccion);
            transaccion.Commit();
            return total;
        }
        catch { transaccion.Rollback(); throw; }
    }

    public int AsignarTodos(string perfil, string usuario) => Asignar(perfil, ListarDisponibles(perfil).Select(x => x.Login), usuario);
    public int RetirarTodos(string perfil, string usuario) => Retirar(perfil, ListarAsignados(perfil).Select(x => x.Login), usuario);

    private bool AsignarUno(SqlConnection conexion, SqlTransaction transaccion, string perfil, string login, string usuario)
    {
        using var buscarUsuario = new SqlCommand("SELECT CodigoPersona FROM app.Usuario WHERE Login=@Login", conexion, transaccion);
        buscarUsuario.Parameters.AddWithValue("@Login", login);
        var persona = Convert.ToString(buscarUsuario.ExecuteScalar())?.Trim();
        if (string.IsNullOrWhiteSpace(persona)) return false;
        Db.Ejecutar("UPDATE app.UsuarioPerfil SET FechaRetiro=SYSUTCDATETIME(),ModificadoPor=@Usuario WHERE Login=@Login AND FechaRetiro IS NULL", new { Login = login, Usuario = usuario }, transaccion);

        using var buscar = new SqlCommand("SELECT TOP(1) Id FROM app.UsuarioPerfil WHERE Login=@Login AND CodigoPerfil=@Perfil ORDER BY FechaAsignacion DESC,Id DESC", conexion, transaccion);
        buscar.Parameters.AddWithValue("@Login", login);
        buscar.Parameters.AddWithValue("@Perfil", perfil);
        var id = buscar.ExecuteScalar();
        if (id != null)
            Db.Ejecutar("UPDATE app.UsuarioPerfil SET CodigoPersona=@Persona,FechaAsignacion=SYSUTCDATETIME(),FechaRetiro=NULL,AsignadoPor=@Usuario,ModificadoPor=NULL WHERE Id=@Id", new { Persona = persona, Usuario = usuario, Id = Convert.ToInt64(id) }, transaccion);
        else
            Db.Ejecutar("INSERT app.UsuarioPerfil(CodigoPersona,Login,CodigoPerfil,FechaAsignacion,AsignadoPor) VALUES(@Persona,@Login,@Perfil,SYSUTCDATETIME(),@Usuario)", new { Persona = persona, Login = login, Perfil = perfil, Usuario = usuario }, transaccion);
        return true;
    }

    private static List<string> Limpiar(IEnumerable<string>? logins) => (logins ?? [])
        .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}
