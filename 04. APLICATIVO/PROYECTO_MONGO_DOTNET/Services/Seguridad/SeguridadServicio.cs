using System.Text.RegularExpressions;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Seguridad;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Seguridad;

public sealed class SeguridadServicio
{
    private readonly UsuarioDAO _usuarios;
    private readonly AuditoriaDAO _auditoria;
    public SeguridadServicio(UsuarioDAO usuarios, AuditoriaDAO auditoria) { _usuarios = usuarios; _auditoria = auditoria; }
    public IReadOnlyCollection<Usuario> ListarUsuarios() => _usuarios.Listar();

    public void Restablecer(RestablecerClaveDto dto, string administrador)
    {
        ValidarPolitica(dto.ClaveTemporal);
        if (dto.ClaveTemporal != dto.Confirmacion) throw new ExcepcionNegocio("La contraseña temporal y su confirmación no coinciden.");
        if (!_usuarios.RestablecerClave(dto.Login, dto.ClaveTemporal, administrador)) throw ExcepcionNegocio.NoEncontrado("El usuario no existe.");
        _auditoria.Registrar(administrador, "xeusu_usuari", "RESTABLECER_CLAVE", $"Clave restablecida para {dto.Login.Trim()}.");
    }

    public void Cambiar(CambiarClaveDto dto, string login)
    {
        var usuario = _usuarios.BuscarPorLogin(login) ?? throw ExcepcionNegocio.NoEncontrado("El usuario no existe.");
        if (!_usuarios.ValidarPassword(usuario, dto.ClaveActual)) throw new ExcepcionNegocio("La contraseña actual no es correcta.");
        if (dto.NuevaClave != dto.Confirmacion) throw new ExcepcionNegocio("La nueva contraseña y su confirmación no coinciden.");
        if (dto.NuevaClave == dto.ClaveActual) throw new ExcepcionNegocio("La nueva contraseña debe ser diferente de la actual.");
        ValidarPolitica(dto.NuevaClave);
        if (!_usuarios.CambiarClave(login, dto.NuevaClave, login)) throw new ExcepcionNegocio("No fue posible cambiar la contraseña.");
        _auditoria.Registrar(login, "xeusu_usuari", "CAMBIAR_CLAVE", "El usuario cambió su propia contraseña.");
    }

    public static void ValidarPolitica(string? clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 128
            || !Regex.IsMatch(clave, "[A-Z]") || !Regex.IsMatch(clave, "[a-z]")
            || !Regex.IsMatch(clave, "[0-9]") || !Regex.IsMatch(clave, "[^A-Za-z0-9]"))
            throw new ExcepcionNegocio("La contraseña debe tener entre 8 y 128 caracteres e incluir mayúscula, minúscula, número y símbolo.");
    }
}
