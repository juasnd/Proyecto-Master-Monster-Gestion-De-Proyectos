namespace PROYECTO_MONGO_DOTNET.Services;

public abstract class SqlDAO
{
    protected readonly SqlContexto Db;
    protected SqlDAO(SqlContexto db) => Db = db;
    protected static string Mayuscula(string? valor) => string.IsNullOrWhiteSpace(valor) ? "" : valor.Trim().ToUpperInvariant();
    protected static object ValorONull(string? valor) => string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
}
