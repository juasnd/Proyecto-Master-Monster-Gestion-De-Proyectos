using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Personal;

public sealed class FotoEmpleadoServicio(IWebHostEnvironment entorno, ILogger<FotoEmpleadoServicio> logger)
{
    public const string Directorio = "uploads/empleados";
    public const long MaximoBytes = 2L * 1024 * 1024;
    private string Carpeta => Path.GetFullPath(Path.Combine(entorno.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), Directorio));

    public static string? TipoContenido(string extension) => extension.ToLowerInvariant() switch
    {
        ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".webp" => "image/webp", _ => null
    };

    public static bool FirmaValida(ReadOnlySpan<byte> bytes, string extension) => extension.ToLowerInvariant() switch
    {
        ".png" => bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        ".jpg" or ".jpeg" => bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255,
        ".webp" => bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8),
        _ => false
    };

    public async Task<string> GuardarAsync(IFormFile foto, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
        var tipo = TipoContenido(extension);
        if (foto.Length <= 0 || foto.Length > MaximoBytes || tipo == null || !tipo.Equals(foto.ContentType, StringComparison.OrdinalIgnoreCase))
            throw new ExcepcionNegocio("La foto debe ser jpg, jpeg, png o webp y no superar 2 MB.");
        // Se valida y guarda la misma copia, limitada incluso si la longitud declarada es incorrecta.
        await using var origen = foto.OpenReadStream();
        using var contenido = new MemoryStream();
        var buffer = new byte[81920];
        int leidos;
        while ((leidos = await origen.ReadAsync(buffer.AsMemory(), cancellationToken)) != 0)
        {
            if (contenido.Length + leidos > MaximoBytes) throw new ExcepcionNegocio("La foto no debe superar 2 MB.");
            contenido.Write(buffer, 0, leidos);
        }
        if (!FirmaValida(contenido.ToArray(), extension)) throw new ExcepcionNegocio("El contenido de la foto no corresponde a una imagen válida.");
        Directory.CreateDirectory(Carpeta);
        var nombre = Guid.NewGuid().ToString("N") + extension;
        var destino = Path.Combine(Carpeta, nombre);
        try
        {
            await using var archivo = new FileStream(destino, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            contenido.Position = 0;
            await contenido.CopyToAsync(archivo, cancellationToken);
        }
        catch
        {
            Eliminar(Directorio + "/" + nombre);
            throw;
        }
        return Directorio + "/" + nombre;
    }

    public string? Resolver(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta) || !ruta.StartsWith(Directorio + "/", StringComparison.OrdinalIgnoreCase)) return null;
        var nombre = ruta[(Directorio.Length + 1)..];
        if (nombre.IndexOfAny(['/', '\\']) >= 0 || nombre is "." or ".." || TipoContenido(Path.GetExtension(nombre)) == null) return null;
        var destino = Path.GetFullPath(Path.Combine(Carpeta, nombre));
        return destino.StartsWith(Carpeta + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? destino : null;
    }

    public void Eliminar(string? ruta, string? excepto = null)
    {
        if (ruta == excepto) return;
        var archivo = Resolver(ruta);
        if (archivo == null) return;
        try { File.Delete(archivo); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "No se pudo limpiar una fotografía de empleado.");
        }
    }
}
