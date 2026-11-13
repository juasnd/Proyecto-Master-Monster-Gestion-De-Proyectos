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
        try
        {
            conexion.Open();
            return conexion;
        }
        catch
        {
            conexion.Dispose();
            throw;
        }
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

    public async Task<int> EjecutarAsync(string sql, object? parametros = null, CancellationToken cancellationToken = default, SqlTransaction? transaccion = null)
    {
        await using var conexion = transaccion == null ? new SqlConnection(CadenaConexion) : null;
        if (conexion != null) await conexion.OpenAsync(cancellationToken);
        await using var comando = CrearComando(transaccion?.Connection ?? conexion!, sql, parametros, transaccion);
        return await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    public T? Escalar<T>(string sql, object? parametros = null, SqlTransaction? transaccion = null)
    {
        using var conexion = transaccion == null ? Abrir() : null;
        using var comando = CrearComando(transaccion?.Connection ?? conexion!, sql, parametros, transaccion);
        var valor = comando.ExecuteScalar();
        return valor is null or DBNull ? default : (T)Convertir(valor, typeof(T));
    }

    public async Task<T?> EscalarAsync<T>(string sql, object? parametros = null, CancellationToken cancellationToken = default, SqlTransaction? transaccion = null)
    {
        await using var conexion = transaccion == null ? new SqlConnection(CadenaConexion) : null;
        if (conexion != null) await conexion.OpenAsync(cancellationToken);
        await using var comando = CrearComando(transaccion?.Connection ?? conexion!, sql, parametros, transaccion);
        var valor = await comando.ExecuteScalarAsync(cancellationToken);
        return valor is null or DBNull ? default : (T)Convertir(valor, typeof(T));
    }

    public List<T> Consultar<T>(string sql, object? parametros = null, SqlTransaction? transaccion = null) where T : new()
    {
        using var conexion = transaccion == null ? Abrir() : null;
        using var comando = CrearComando(transaccion?.Connection ?? conexion!, sql, parametros, transaccion);
        using var lector = comando.ExecuteReader();
        return Mapear<T>(lector);
    }

    public async Task<List<T>> ConsultarAsync<T>(string sql, object? parametros = null, CancellationToken cancellationToken = default) where T : new()
    {
        await using var conexion = new SqlConnection(CadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var comando = CrearComando(conexion, sql, parametros, null);
        await using var lector = await comando.ExecuteReaderAsync(cancellationToken);
        var columnas = Columnas<T>(lector);
        var resultado = new List<T>();
        while (await lector.ReadAsync(cancellationToken)) resultado.Add(MapearFila<T>(lector, columnas));
        return resultado;
    }

    private static SqlCommand CrearComando(SqlConnection conexion, string sql, object? parametros, SqlTransaction? transaccion)
    {
        var comando = new SqlCommand(sql, conexion, transaccion) { CommandTimeout = 30 };
        if (parametros == null) return comando;
        foreach (var propiedad in parametros.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var valor = propiedad.GetValue(parametros) ?? DBNull.Value;
            var tipo = Nullable.GetUnderlyingType(propiedad.PropertyType) ?? propiedad.PropertyType;
            if (tipo == typeof(DateTime))
                comando.Parameters.Add("@" + propiedad.Name, SqlDbType.DateTime2).Value = valor;
            else
                comando.Parameters.AddWithValue("@" + propiedad.Name, valor);
        }
        return comando;
    }

    private static List<T> Mapear<T>(IDataReader lector) where T : new()
    {
        var columnas = Columnas<T>(lector);
        var resultado = new List<T>();
        while (lector.Read()) resultado.Add(MapearFila<T>(lector, columnas));
        return resultado;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> Propiedades = new();

    private static (int Ordinal, PropertyInfo Propiedad)[] Columnas<T>(IDataReader lector)
    {
        var propiedades = Propiedades.GetOrAdd(typeof(T), tipo => tipo.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase));
        return Enumerable.Range(0, lector.FieldCount).Where(i => propiedades.ContainsKey(lector.GetName(i)))
            .Select(i => (i, propiedades[lector.GetName(i)])).ToArray();
    }

    private static T MapearFila<T>(IDataReader lector, (int Ordinal, PropertyInfo Propiedad)[] columnas) where T : new()
    {
        var fila = new T();
        foreach (var (ordinal, propiedad) in columnas)
            if (!lector.IsDBNull(ordinal)) propiedad.SetValue(fila, Convertir(lector.GetValue(ordinal), propiedad.PropertyType));
        return fila;
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
