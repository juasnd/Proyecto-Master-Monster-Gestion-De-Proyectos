namespace PROYECTO_MONGO_DOTNET.Services.Compartidos;

public sealed class ExcepcionNegocio : Exception
{
    public int CodigoHttp { get; }
    public IDictionary<string, string[]> Errores { get; }

    public ExcepcionNegocio(string mensaje, int codigoHttp = StatusCodes.Status422UnprocessableEntity, IDictionary<string, string[]>? errores = null)
        : base(mensaje)
    {
        CodigoHttp = codigoHttp;
        Errores = errores ?? new Dictionary<string, string[]>();
    }

    public static ExcepcionNegocio NoEncontrado(string mensaje) => new(mensaje, StatusCodes.Status404NotFound);
    public static ExcepcionNegocio Conflicto(string mensaje) => new(mensaje, StatusCodes.Status409Conflict);
}
