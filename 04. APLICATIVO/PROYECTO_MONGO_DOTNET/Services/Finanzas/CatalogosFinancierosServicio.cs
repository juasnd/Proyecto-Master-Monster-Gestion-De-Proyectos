using PROYECTO_MONGO_DOTNET.Models.Dtos.Finanzas;
using PROYECTO_MONGO_DOTNET.Models.Entidades;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Finanzas;

public sealed class CatalogosFinancierosServicio
{
    private readonly CategoriaGastoRepositorio _categorias;
    private readonly ParametroFinancieroRepositorio _parametros;
    private readonly CodigoDAO _codigos;
    public CatalogosFinancierosServicio(CategoriaGastoRepositorio categorias, ParametroFinancieroRepositorio parametros, CodigoDAO codigos) { _categorias = categorias; _parametros = parametros; _codigos = codigos; }
    public Task<List<CategoriaGasto>> ListarCategoriasAsync() => _categorias.ListarAsync();
    public Task<List<ParametroFinanciero>> ListarParametrosAsync() => _parametros.ListarAsync();

    public async Task<CategoriaGasto> GuardarCategoriaAsync(GuardarCategoriaGastoDto dto, string usuario)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ExcepcionNegocio("El nombre de la categoría es obligatorio.");
        var existente = string.IsNullOrWhiteSpace(dto.CodigoCategoria) ? null : await _categorias.ObtenerAsync(dto.CodigoCategoria);
        var x = existente ?? new CategoriaGasto { CodigoCategoria = _codigos.GenerarCodigo("fin_categoria_gasto", "CODIGOCATEGORIA"), CreadoPor = usuario, FechaCreacion = DateTime.UtcNow };
        x.Nombre = N(dto.Nombre); x.Descripcion = dto.Descripcion.Trim(); x.Activo = dto.Activo;
        if (existente == null) await _categorias.CrearAsync(x); else { x.ModificadoPor = usuario; x.FechaModificacion = DateTime.UtcNow; await _categorias.ActualizarAsync(x); }
        return x;
    }

    public async Task<ParametroFinanciero> GuardarParametroAsync(GuardarParametroFinancieroDto dto, string usuario)
    {
        var tipo = N(dto.TipoDato);
        if (tipo is not ("TEXTO" or "DECIMAL" or "ENTERO")) throw new ExcepcionNegocio("El tipo de dato debe ser TEXTO, DECIMAL o ENTERO.");
        if (tipo == "TEXTO" && string.IsNullOrWhiteSpace(dto.ValorTexto)) throw new ExcepcionNegocio("El valor de texto es obligatorio.");
        if (tipo == "DECIMAL" && !dto.ValorDecimal.HasValue) throw new ExcepcionNegocio("El valor decimal es obligatorio.");
        if (tipo == "ENTERO" && !dto.ValorEntero.HasValue) throw new ExcepcionNegocio("El valor entero es obligatorio.");
        var x = await _parametros.ObtenerAsync(dto.CodigoParametro) ?? new ParametroFinanciero { CodigoParametro = N(dto.CodigoParametro) };
        x.Descripcion = dto.Descripcion.Trim(); x.TipoDato = tipo; x.ValorTexto = tipo == "TEXTO" ? dto.ValorTexto?.Trim() : null;
        x.ValorDecimal = tipo == "DECIMAL" ? dto.ValorDecimal : null; x.ValorEntero = tipo == "ENTERO" ? dto.ValorEntero : null;
        x.Activo = dto.Activo; x.ModificadoPor = usuario; x.FechaModificacion = DateTime.UtcNow;
        await _parametros.UpsertAsync(x);
        return x;
    }
    private static string N(string? x) => (x ?? "").Trim().ToUpperInvariant();
}
