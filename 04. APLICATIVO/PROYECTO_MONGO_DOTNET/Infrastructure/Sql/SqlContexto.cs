using System.Data;
using System.Globalization;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace PROYECTO_MONGO_DOTNET.Services;

public sealed class SqlContexto
{
    public string CadenaConexion { get; }

    public SqlContexto(IConfiguration configuracion)
    {
        CadenaConexion = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING")
            ?? configuracion.GetConnectionString("SqlServer")
            ?? "Server=localhost;Database=PROYECTO_MONSTER;Integrated Security=True;TrustServerCertificate=True;Encrypt=True";
    }

    public SqlConnection Abrir()
    {
        var conexion = new SqlConnection(CadenaConexion);
        conexion.Open();
        return conexion;
    }

    public int Ejecutar(string sql, object? parametros = null, SqlTransaction? transaccion = null)
    {
        var propia = transaccion == null ? Abrir() : null;
        try
        {
            using var comando = CrearComando(transaccion?.Connection ?? propia!, sql, parametros, transaccion);
            return comando.ExecuteNonQuery();
        }
        finally { propia?.Dispose(); }
    }

    public async Task<int> EjecutarAsync(string sql, object? parametros = null)
    {
        await using var conexion = new SqlConnection(CadenaConexion);
        await conexion.OpenAsync();
        await using var comando = CrearComando(conexion, sql, parametros, null);
        return await comando.ExecuteNonQueryAsync();
    }

    public T? Escalar<T>(string sql, object? parametros = null)
    {
        using var conexion = Abrir();
        using var comando = CrearComando(conexion, sql, parametros, null);
        var valor = comando.ExecuteScalar();
        return valor is null or DBNull ? default : (T)Convertir(valor, typeof(T));
    }

    public async Task<T?> EscalarAsync<T>(string sql, object? parametros = null)
    {
        await using var conexion = new SqlConnection(CadenaConexion);
        await conexion.OpenAsync();
        await using var comando = CrearComando(conexion, sql, parametros, null);
        var valor = await comando.ExecuteScalarAsync();
        return valor is null or DBNull ? default : (T)Convertir(valor, typeof(T));
    }

    public List<T> Consultar<T>(string sql, object? parametros = null) where T : new()
    {
        using var conexion = Abrir();
        using var comando = CrearComando(conexion, sql, parametros, null);
        using var lector = comando.ExecuteReader();
        return Mapear<T>(lector);
    }

    public async Task<List<T>> ConsultarAsync<T>(string sql, object? parametros = null) where T : new()
    {
        await using var conexion = new SqlConnection(CadenaConexion);
        await conexion.OpenAsync();
        await using var comando = CrearComando(conexion, sql, parametros, null);
        await using var lector = await comando.ExecuteReaderAsync();
        return Mapear<T>(lector);
    }

    private static SqlCommand CrearComando(SqlConnection conexion, string sql, object? parametros, SqlTransaction? transaccion)
    {
        var comando = new SqlCommand(sql, conexion, transaccion) { CommandTimeout = 30 };
        if (parametros == null) return comando;
        foreach (var propiedad in parametros.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            comando.Parameters.AddWithValue("@" + propiedad.Name, propiedad.GetValue(parametros) ?? DBNull.Value);
        return comando;
    }

    private static List<T> Mapear<T>(IDataReader lector) where T : new()
    {
        var propiedades = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.CanWrite).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var resultado = new List<T>();
        while (lector.Read())
        {
            var fila = new T();
            for (var i = 0; i < lector.FieldCount; i++)
            {
                if (!propiedades.TryGetValue(lector.GetName(i), out var propiedad) || lector.IsDBNull(i)) continue;
                propiedad.SetValue(fila, Convertir(lector.GetValue(i), propiedad.PropertyType));
            }
            resultado.Add(fila);
        }
        return resultado;
    }

    private static object Convertir(object valor, Type destino)
    {
        var tipo = Nullable.GetUnderlyingType(destino) ?? destino;
        if (tipo == typeof(string)) return Convert.ToString(valor, CultureInfo.InvariantCulture)?.Trim() ?? "";
        if (tipo == typeof(bool) && valor is string texto) return texto.Equals("S", StringComparison.OrdinalIgnoreCase) || texto == "1";
        if (tipo.IsEnum) return Enum.Parse(tipo, valor.ToString()!, true);
        return Convert.ChangeType(valor, tipo, CultureInfo.InvariantCulture);
    }
}
