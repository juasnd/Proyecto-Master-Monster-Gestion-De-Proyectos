namespace PROYECTO_MONGO_DOTNET.Models;

public sealed class ApiRespuesta<T>
{
    public bool Exito { get; init; }
    public string Mensaje { get; init; } = "";
    public T? Datos { get; init; }
    public IDictionary<string, string[]>? Errores { get; init; }

    public static ApiRespuesta<T> Correcto(T datos, string mensaje = "Operación completada correctamente.") =>
        new() { Exito = true, Mensaje = mensaje, Datos = datos };

    public static ApiRespuesta<T> Fallo(string mensaje, IDictionary<string, string[]>? errores = null) =>
        new() { Exito = false, Mensaje = mensaje, Errores = errores };
}

public sealed class ResultadoPaginado<T>
{
    public IReadOnlyCollection<T> Elementos { get; init; } = [];
    public long Total { get; init; }
    public int Pagina { get; init; }
    public int Cantidad { get; init; }
    public int TotalPaginas => Cantidad <= 0 ? 0 : (int)Math.Ceiling(Total / (double)Cantidad);
}
