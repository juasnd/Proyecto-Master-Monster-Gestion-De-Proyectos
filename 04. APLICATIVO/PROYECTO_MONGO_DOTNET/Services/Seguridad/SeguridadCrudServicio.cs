using System.Text.RegularExpressions;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Models.Dtos.Seguridad;
using PROYECTO_MONGO_DOTNET.Models.ViewModels;
using PROYECTO_MONGO_DOTNET.Services.Compartidos;

namespace PROYECTO_MONGO_DOTNET.Services.Seguridad;

public sealed class SeguridadCrudServicio
{
    private readonly UsuarioDAO _usuarios;
    private readonly PerfilDAO _perfiles;
    private readonly PermisoDAO _permisos;
    private readonly PersonaDAO _personas;
    private readonly EmpleadoDAO _empleados;
    private readonly AuditoriaDAO _auditoria;
    private readonly AsignacionUsuarioPerfilDAO _asignacionesUsuarioPerfil;

    public SeguridadCrudServicio(UsuarioDAO usuarios, PerfilDAO perfiles, PermisoDAO permisos,
        PersonaDAO personas, EmpleadoDAO empleados, AuditoriaDAO auditoria,
        AsignacionUsuarioPerfilDAO asignacionesUsuarioPerfil)
    {
        _usuarios = usuarios;
        _perfiles = perfiles;
        _permisos = permisos;
        _personas = personas;
        _empleados = empleados;
        _auditoria = auditoria;
        _asignacionesUsuarioPerfil = asignacionesUsuarioPerfil;
    }

    public UsuariosPaginaVm PaginaUsuarios(string? mensaje = null, string? error = null)
    {
        var usuarios = _usuarios.Listar();
        var personasOcupadas = usuarios.Select(x => x.CodigoPersona).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var empleados = _empleados.Listar().Where(x => x.Persona != null)
            .GroupBy(x => x.Persona.PeperCodigo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().PeempCodigo, StringComparer.OrdinalIgnoreCase);
        var personas = _personas.Listar().Where(x => !personasOcupadas.Contains(x.PeperCodigo)).Select(x => new PersonaUsuarioVm
        {
            Codigo = x.PeperCodigo,
            NombreCompleto = (x.Nombres + " " + x.Apellidos).Trim(),
            Cedula = x.Cedula,
            Email = x.Email,
            Tipo = x.Tipo,
            CodigoEmpleado = empleados.GetValueOrDefault(x.PeperCodigo, "")
        }).ToList();

        return new UsuariosPaginaVm
        {
            Usuarios = usuarios,
            Perfiles = _perfiles.Listar(false),
            PersonasDisponibles = personas,
            Mensaje = mensaje,
            Error = error
        };
    }

    public void CrearUsuario(CrearUsuarioDto dto, string administrador)
    {
        dto.Login = dto.Login.Trim();
        if (!Regex.IsMatch(dto.Login, "^[A-Za-z0-9._@-]{3,60}$"))
            throw new ExcepcionNegocio("El usuario solo puede contener letras, números, punto, guion, guion bajo o arroba.");
        if (_usuarios.ExisteLogin(dto.Login)) throw new ExcepcionNegocio("Ya existe un usuario con ese login.");
        SeguridadServicio.ValidarPolitica(dto.ClaveTemporal);
        if (dto.ClaveTemporal != dto.Confirmacion) throw new ExcepcionNegocio("La contraseña temporal y su confirmación no coinciden.");

        var perfil = _perfiles.Buscar(dto.PerfilCodigo);
        if (perfil == null || !perfil.EstaActivo) throw new ExcepcionNegocio("El perfil seleccionado no existe o está inactivo.");

        Persona persona;
        var personaCreada = false;
        if (string.Equals(dto.TipoRegistro, "externo", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(dto.Nombres) || string.IsNullOrWhiteSpace(dto.Apellidos))
                throw new ExcepcionNegocio("Ingrese los nombres y apellidos de la persona externa.");
            if (string.IsNullOrWhiteSpace(dto.Cedula) && string.IsNullOrWhiteSpace(dto.Email))
                throw new ExcepcionNegocio("Ingrese al menos la cédula o el correo de la persona externa.");
            if (!string.IsNullOrWhiteSpace(dto.Cedula) && _personas.ExisteCedula(dto.Cedula))
                throw new ExcepcionNegocio("Ya existe una persona con la cédula indicada.");
            persona = new Persona
            {
                Tipo = "INV",
                Nombres = dto.Nombres.Trim(),
                Apellidos = dto.Apellidos.Trim(),
                Cedula = dto.Cedula.Trim(),
                Email = dto.Email.Trim(),
                Direccion = "Sin dirección"
            };
            _personas.Insertar(persona);
            personaCreada = true;
        }
        else
        {
            persona = _personas.Buscar(dto.CodigoPersona)
                ?? throw ExcepcionNegocio.NoEncontrado("La persona seleccionada no existe.");
        }

        if (_usuarios.ExistePersona(persona.PeperCodigo))
        {
            if (personaCreada) _personas.Eliminar(persona.PeperCodigo);
            throw new ExcepcionNegocio("La persona seleccionada ya tiene una cuenta de usuario.");
        }

        var usuarioInsertado = false;
        try
        {
            var usuario = new Usuario { Login = dto.Login, CodigoPersona = persona.PeperCodigo };
            _usuarios.Insertar(usuario, dto.ClaveTemporal, administrador);
            usuarioInsertado = true;
            if (!_usuarios.AsignarPerfil(dto.Login, perfil.Codigo, administrador))
                throw new ExcepcionNegocio("No fue posible asignar el perfil al nuevo usuario.");
            _auditoria.Registrar(administrador, "xeusu_usuari", "CREAR", $"Usuario {dto.Login} creado con perfil {perfil.Codigo}.");
        }
        catch
        {
            if (usuarioInsertado) _usuarios.EliminarCreado(dto.Login);
            if (personaCreada) _personas.Eliminar(persona.PeperCodigo);
            throw;
        }
    }

    public void AsignarPerfil(AsignarPerfilUsuarioDto dto, string administrador)
    {
        var perfil = _perfiles.Buscar(dto.PerfilCodigo);
        if (perfil == null || !perfil.EstaActivo) throw new ExcepcionNegocio("El perfil seleccionado no existe o está inactivo.");
        if (!_usuarios.AsignarPerfil(dto.Login, perfil.Codigo, administrador)) throw ExcepcionNegocio.NoEncontrado("El usuario no existe.");
        _auditoria.Registrar(administrador, "xeuxp_usuper", "ASIGNAR_PERFIL", $"Perfil {perfil.Codigo} asignado a {dto.Login}.");
    }

    public void CambiarEstadoUsuario(CambiarEstadoUsuarioDto dto, string administrador)
    {
        var estado = dto.Estado.Trim().ToUpperInvariant();
        if (estado is not ("A" or "I")) throw new ExcepcionNegocio("El estado solicitado no es válido.");
        if (string.Equals(dto.Login, administrador, StringComparison.OrdinalIgnoreCase) && estado != "A")
            throw new ExcepcionNegocio("No puede inactivar su propia cuenta mientras mantiene la sesión iniciada.");
        if (!_usuarios.CambiarEstado(dto.Login, estado, administrador)) throw ExcepcionNegocio.NoEncontrado("El usuario no existe.");
        _auditoria.Registrar(administrador, "xeusu_usuari", estado == "A" ? "ACTIVAR" : "INACTIVAR", $"Estado de {dto.Login} cambiado a {estado}.");
    }

    public PerfilesPaginaVm PaginaPerfiles(string modo = "listar", string? codigo = null, string? mensaje = null, string? error = null) => new()
    {
        Perfiles = _perfiles.Listar(),
        Modo = modo,
        PerfilEditar = string.IsNullOrWhiteSpace(codigo) ? null : _perfiles.Buscar(codigo),
        Mensaje = mensaje,
        Error = error
    };

    public void GuardarPerfil(GuardarPerfilDto dto, bool editar, string administrador)
    {
        dto.Codigo = dto.Codigo.Trim().ToUpperInvariant();
        dto.Descripcion = dto.Descripcion.Trim();
        if (!Regex.IsMatch(dto.Codigo, "^[A-Z0-9_]{3,20}$"))
            throw new ExcepcionNegocio("El código del perfil debe tener entre 3 y 20 caracteres: letras, números o guion bajo.");
        if (string.IsNullOrWhiteSpace(dto.Descripcion)) throw new ExcepcionNegocio("La descripción del perfil es obligatoria.");
        var perfil = new Perfil { Codigo = dto.Codigo, Descripcion = dto.Descripcion, Observacion = dto.Observacion.Trim() };
        if (editar)
        {
            if (!_perfiles.Actualizar(perfil, administrador)) throw ExcepcionNegocio.NoEncontrado("El perfil no existe.");
            _auditoria.Registrar(administrador, "xeper_perfil", "ACTUALIZAR", $"Perfil {perfil.Codigo} actualizado.");
        }
        else
        {
            if (_perfiles.Existe(perfil.Codigo)) throw new ExcepcionNegocio("Ya existe un perfil con ese código.");
            _perfiles.Insertar(perfil, administrador);
            _auditoria.Registrar(administrador, "xeper_perfil", "CREAR", $"Perfil {perfil.Codigo} creado.");
        }
    }

    public void CambiarEstadoPerfil(CambiarEstadoPerfilDto dto, string administrador)
    {
        var codigo = dto.Codigo.Trim().ToUpperInvariant();
        var estado = dto.Estado.Trim().ToUpperInvariant();
        if (estado is not ("A" or "I")) throw new ExcepcionNegocio("El estado solicitado no es válido.");
        if (codigo == "ADMIN" && estado != "A") throw new ExcepcionNegocio("El perfil ADMIN no puede inactivarse.");
        if (estado == "I" && _perfiles.EnUso(codigo)) throw new ExcepcionNegocio("No se puede inactivar un perfil que tiene usuarios asignados.");
        if (!_perfiles.CambiarEstado(codigo, estado, administrador)) throw ExcepcionNegocio.NoEncontrado("El perfil no existe.");
        _auditoria.Registrar(administrador, "xeper_perfil", estado == "A" ? "ACTIVAR" : "INACTIVAR", $"Estado del perfil {codigo} cambiado a {estado}.");
    }

    public PermisosPaginaVm PaginaPermisos(string? perfilCodigo = null, string? mensaje = null, string? error = null)
    {
        var perfiles = _perfiles.Listar();
        var seleccionado = perfiles.FirstOrDefault(x => string.Equals(x.Codigo, perfilCodigo, StringComparison.OrdinalIgnoreCase))
                           ?? perfiles.FirstOrDefault(x => x.EstaActivo);
        return new PermisosPaginaVm
        {
            Perfiles = perfiles,
            PerfilSeleccionado = seleccionado,
            Permisos = seleccionado == null ? [] : _permisos.ListarPorPerfil(seleccionado.Codigo),
            Mensaje = mensaje,
            Error = error
        };
    }

    public void GuardarPermisos(GuardarPermisosPerfilDto dto, string administrador)
    {
        var perfil = _perfiles.Buscar(dto.PerfilCodigo) ?? throw ExcepcionNegocio.NoEncontrado("El perfil no existe.");
        if (!perfil.EstaActivo) throw new ExcepcionNegocio("No se pueden editar permisos de un perfil inactivo.");
        if (perfil.Codigo == "ADMIN") throw new ExcepcionNegocio("El perfil ADMIN conserva acceso total y no requiere edición de permisos.");

        var filas = _permisos.ListarPorPerfil(perfil.Codigo);
        var enviados = dto.Permisos.GroupBy(x => x.CodigoOpcion, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        foreach (var fila in filas)
        {
            if (!enviados.TryGetValue(fila.CodigoOpcion, out var enviado))
            {
                fila.Ver = fila.Crear = fila.Editar = fila.Eliminar = false;
                continue;
            }
            fila.Crear = enviado.Crear;
            fila.Editar = enviado.Editar;
            fila.Eliminar = enviado.Eliminar;
            fila.Ver = enviado.Ver || fila.Crear || fila.Editar || fila.Eliminar;
        }

        var mapa = filas.ToDictionary(x => x.CodigoOpcion, StringComparer.OrdinalIgnoreCase);
        foreach (var fila in filas.Where(x => x.Ver).ToList())
        {
            var padre = fila.CodigoPadre;
            while (!string.IsNullOrWhiteSpace(padre) && mapa.TryGetValue(padre, out var filaPadre))
            {
                filaPadre.Ver = true;
                padre = filaPadre.CodigoPadre;
            }
        }

        _permisos.GuardarPerfil(perfil.Codigo, filas, administrador);
        _auditoria.Registrar(administrador, "xeoxp_opcper", "ACTUALIZAR_PERMISOS", $"Permisos actualizados para el perfil {perfil.Codigo}.");
    }

    public void GuardarOpciones(string perfilCodigo, IEnumerable<string>? opciones, string administrador)
    {
        var perfil = _perfiles.Buscar(perfilCodigo) ?? throw ExcepcionNegocio.NoEncontrado("El perfil no existe.");
        if (!perfil.EstaActivo) throw new ExcepcionNegocio("No se pueden editar opciones de un perfil inactivo.");
        if (perfil.Codigo == "ADMIN") throw new ExcepcionNegocio("El perfil ADMIN conserva acceso total y no requiere edición.");
        var seleccionadas = (opciones ?? []).Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var filas = _permisos.ListarPorPerfil(perfil.Codigo);
        foreach (var fila in filas)
            fila.Ver = fila.Crear = fila.Editar = fila.Eliminar = seleccionadas.Contains(fila.CodigoOpcion);
        _permisos.GuardarPerfil(perfil.Codigo, filas, administrador);
        _auditoria.Registrar(administrador, "xeoxp_opcper", "ACTUALIZAR_OPCIONES", $"Opciones actualizadas para el perfil {perfil.Codigo}.");
    }

    public UsuariosPerfilPaginaVm PaginaUsuariosPerfil(string? perfilCodigo, string? mensaje = null, string? error = null)
    {
        var perfiles = _perfiles.Listar(false);
        var seleccionado = string.IsNullOrWhiteSpace(perfilCodigo)
            ? null
            : perfiles.FirstOrDefault(x => string.Equals(x.Codigo, perfilCodigo, StringComparison.OrdinalIgnoreCase));
        return new UsuariosPerfilPaginaVm
        {
            Perfiles = perfiles,
            PerfilSeleccionado = seleccionado,
            UsuariosDisponibles = seleccionado == null ? [] : _asignacionesUsuarioPerfil.ListarDisponibles(seleccionado.Codigo),
            UsuariosAsignados = seleccionado == null ? [] : _asignacionesUsuarioPerfil.ListarAsignados(seleccionado.Codigo),
            Mensaje = mensaje,
            Error = error
        };
    }

    public string ProcesarUsuariosPerfil(string perfilCodigo, string accion, IEnumerable<string>? disponibles,
        IEnumerable<string>? asignados, string administrador)
    {
        var perfil = _perfiles.Buscar(perfilCodigo) ?? throw ExcepcionNegocio.NoEncontrado("Seleccione un perfil válido.");
        if (!perfil.EstaActivo) throw new ExcepcionNegocio("El perfil seleccionado está inactivo.");
        var resultado = accion switch
        {
            "asignarSeleccionados" => _asignacionesUsuarioPerfil.Asignar(perfil.Codigo, disponibles ?? [], administrador),
            "asignarTodos" => _asignacionesUsuarioPerfil.AsignarTodos(perfil.Codigo, administrador),
            "retirarSeleccionados" => _asignacionesUsuarioPerfil.Retirar(perfil.Codigo, asignados ?? [], administrador),
            "retirarTodos" => _asignacionesUsuarioPerfil.RetirarTodos(perfil.Codigo, administrador),
            _ => throw new ExcepcionNegocio("La acción solicitada no es válida.")
        };
        if (accion is "asignarSeleccionados" or "retirarSeleccionados" && resultado == 0)
            throw new ExcepcionNegocio("Seleccione al menos un usuario.");
        _auditoria.Registrar(administrador, "xeuxp_usuper", accion.ToUpperInvariant(), $"Proceso {accion} para el perfil {perfil.Codigo}: {resultado} usuario(s).");
        return accion switch
        {
            "asignarSeleccionados" => "asignados",
            "asignarTodos" => "asignados_todos",
            "retirarSeleccionados" => "retirados",
            _ => "retirados_todos"
        };
    }
}
