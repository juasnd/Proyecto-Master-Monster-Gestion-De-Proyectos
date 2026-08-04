document.addEventListener("DOMContentLoaded", function () {
    configurarBusquedaUsuarios();
    configurarLoginDesdePersona();
    configurarLoginExterno();
    configurarModalesUsuarios();
    configurarAccionesTabla();
});

function configurarBusquedaUsuarios() {
    const input = document.getElementById("buscarUsuario");
    const tabla = document.getElementById("tablaUsuarios");

    if (!input || !tabla) {
        return;
    }

    input.addEventListener("input", function () {
        const filtro = normalizar(input.value);
        const filas = tabla.querySelectorAll("tbody tr[data-usuario-row]");
        const controles = document.querySelector("[data-paginacion-controles='" + tabla.id + "']");

        if (!filtro) {
            filas.forEach(function (fila) {
                fila.hidden = false;
                fila.style.display = "";
                fila.dataset.busquedaOculta = "false";
            });

            if (typeof window.reiniciarPaginacionTablas === "function") {
                tabla.dataset.paginacionPagina = "1";
                window.reiniciarPaginacionTablas();
            }
            return;
        }

        if (controles) {
            controles.remove();
        }

        filas.forEach(function (fila) {
            const contenido = normalizar([
                fila.dataset.login,
                fila.dataset.persona,
                fila.dataset.tipo,
                fila.dataset.perfil,
                fila.dataset.estado
            ].join(" "));

            const visible = contenido.includes(filtro);
            fila.hidden = !visible;
            fila.style.display = visible ? "" : "none";
            fila.dataset.busquedaOculta = visible ? "false" : "true";
        });
    });
}

function configurarLoginDesdePersona() {
    const persona = document.getElementById("personaCodigo");
    const login = document.getElementById("loginExistente");

    if (!persona || !login) {
        return;
    }

    persona.addEventListener("change", function () {
        const opcion = persona.options[persona.selectedIndex];

        if (!opcion) {
            login.value = "";
            return;
        }

        login.value = opcion.dataset.cedula || opcion.dataset.email || "";
    });
}

function configurarLoginExterno() {
    const cedula = document.getElementById("cedula");
    const email = document.getElementById("email");
    const login = document.getElementById("loginExterno");

    if (!login) {
        return;
    }

    function completarLogin() {
        if (login.value.trim()) {
            return;
        }

        if (cedula && cedula.value.trim()) {
            login.value = cedula.value.trim();
            return;
        }

        if (email && email.value.trim()) {
            login.value = email.value.trim();
        }
    }

    if (cedula) {
        cedula.addEventListener("blur", completarLogin);
    }

    if (email) {
        email.addEventListener("blur", completarLogin);
    }
}

function configurarModalesUsuarios() {
    document.querySelectorAll("[data-modal-open]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            abrirModal(boton.dataset.modalOpen);
        });
    });

    document.querySelectorAll("[data-modal-switch]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            cerrarModales();
            abrirModal(boton.dataset.modalSwitch);
        });
    });

    document.querySelectorAll("[data-modal-close]").forEach(function (boton) {
        boton.addEventListener("click", cerrarModales);
    });

    document.querySelectorAll(".usuario-modal-backdrop").forEach(function (backdrop) {
        backdrop.addEventListener("click", function (evento) {
            if (evento.target === backdrop) {
                cerrarModales();
            }
        });
    });

    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape") {
            cerrarModales();
        }
    });
}

function configurarAccionesTabla() {
    document.querySelectorAll("[data-usuario-detalle]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            const fila = boton.closest("tr[data-usuario-row]");
            cargarDetalleUsuario(fila);
            abrirModal("modalDetalleUsuario");
        });
    });

    document.querySelectorAll("[data-usuario-perfil]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            const fila = boton.closest("tr[data-usuario-row]");
            cargarPerfilUsuario(fila);
            abrirModal("modalCambiarPerfil");
        });
    });

    document.querySelectorAll("[data-usuario-seguridad]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            const fila = boton.closest("tr[data-usuario-row]");
            cargarSeguridadUsuario(fila);
            abrirModal("modalSeguridadUsuario");
        });
    });
}

function cargarDetalleUsuario(fila) {
    if (!fila) {
        return;
    }

    setTexto("detalleLogin", fila.dataset.login);
    setTexto("detallePersona", fila.dataset.persona || "Sin nombre");
    setTexto("detalleCedula", fila.dataset.cedula || "Sin cédula");
    setTexto("detalleTipo", fila.dataset.tipo);
    setTexto("detalleCodigoEmpleado", fila.dataset.codigoEmpleado || "No aplica");
    setTexto("detallePerfil", fila.dataset.perfil);
    setTexto("detalleEstado", fila.dataset.estado);
    setTexto("detalleCambio", fila.dataset.cambio);
    setTexto("detalleIntentos", fila.dataset.intentos);
    setTexto("detalleUltimo", fila.dataset.ultimo || "Sin acceso registrado");
}

function cargarPerfilUsuario(fila) {
    if (!fila) {
        return;
    }

    const inputLogin = document.getElementById("perfilLogin");
    const resumen = document.getElementById("perfilUsuarioResumen");
    const select = document.getElementById("perfilCodigoModal");

    if (inputLogin) {
        inputLogin.value = fila.dataset.login || "";
    }

    if (resumen) {
        resumen.textContent = "Usuario: " + (fila.dataset.login || "")
                + " | Nombre: " + (fila.dataset.persona || "Sin nombre");
    }

    if (select && fila.dataset.perfilCodigo) {
        select.value = fila.dataset.perfilCodigo;
    }
}

function cargarSeguridadUsuario(fila) {
    if (!fila) {
        return;
    }

    const resumen = document.getElementById("seguridadUsuarioResumen");
    const resetLink = document.getElementById("resetUsuarioLink");
    const estadoForm = document.getElementById("estadoUsuarioForm");
    const estadoLogin = document.getElementById("estadoUsuarioLogin");
    const estadoDestino = document.getElementById("estadoUsuarioEstado");
    const estadoAccion = document.getElementById("estadoUsuarioAccion");
    const estadoDescripcion = document.getElementById("estadoUsuarioDescripcion");

    const login = fila.dataset.login || "";
    const persona = fila.dataset.persona || "Sin nombre";
    const accionEstado = fila.dataset.estadoAccion || "Cambiar estado";

    if (resumen) {
        resumen.textContent = "Usuario: " + login + " | Nombre: " + persona;
    }

    if (resetLink) {
        resetLink.href = fila.dataset.resetUrl || ("UsuarioController?accion=resetear&login=" + encodeURIComponent(login));
    }

    if (estadoLogin) {
        estadoLogin.value = login;
    }

    if (estadoDestino) {
        estadoDestino.value = fila.dataset.estadoDestino || "I";
    }

    if (estadoForm) {
        estadoForm.onsubmit = function () {
            return window.confirm("¿Confirma la operación sobre la cuenta " + login + "?");
        };
    }

    if (estadoAccion) {
        estadoAccion.textContent = accionEstado;
    }

    if (estadoDescripcion) {
        estadoDescripcion.textContent = accionEstado.indexOf("Activar") >= 0
                ? "Permite nuevamente el acceso al sistema."
                : "Bloquea el acceso de esta cuenta al sistema.";
    }
}

function abrirModal(id) {
    const modal = document.getElementById(id);

    if (!modal) {
        return;
    }

    modal.hidden = false;
    document.body.classList.add("usuario-modal-abierto");

    const primerCampo = modal.querySelector("input, select, button:not([data-modal-close]), a");
    if (primerCampo) {
        setTimeout(function () {
            primerCampo.focus();
        }, 60);
    }
}

function cerrarModales() {
    document.querySelectorAll(".usuario-modal-backdrop").forEach(function (modal) {
        modal.hidden = true;
    });
    document.body.classList.remove("usuario-modal-abierto");
}

function setTexto(id, valor) {
    const elemento = document.getElementById(id);

    if (elemento) {
        elemento.textContent = valor || "";
    }
}

function normalizar(texto) {
    return (texto || "")
            .toLowerCase()
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .trim();
}
