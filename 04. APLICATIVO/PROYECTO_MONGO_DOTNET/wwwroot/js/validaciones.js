/* 
 * Validaciones Login - Master Monster
 */

document.addEventListener("DOMContentLoaded", function () {
    const formLogin = document.getElementById("formLogin");
    const usuario = document.getElementById("usuario");
    const password = document.getElementById("password");
    const btnMostrar = document.getElementById("btnMostrar");

    const errorUsuario = document.getElementById("errorUsuario");
    const errorPassword = document.getElementById("errorPassword");

    mostrarErrorLoginDesdeURL();

    if (btnMostrar) {
        btnMostrar.addEventListener("click", function () {
            if (password.type === "password") {
                password.type = "text";
                btnMostrar.textContent = "Ocultar";
            } else {
                password.type = "password";
                btnMostrar.textContent = "Ver";
            }
        });
    }

    usuario.addEventListener("input", validarUsuario);
    password.addEventListener("input", validarPassword);

    formLogin.addEventListener("submit", function (event) {
        const usuarioValido = validarUsuario();
        const passwordValido = validarPassword();

        if (!usuarioValido || !passwordValido) {
            event.preventDefault();
        }
    });

    function validarUsuario() {
        const valorUsuario = usuario.value.trim();

        limpiarEstado(usuario, errorUsuario);

        if (valorUsuario === "") {
            mostrarError(usuario, errorUsuario, "El usuario es obligatorio.");
            return false;
        }

        if (valorUsuario.length < 4) {
            mostrarError(usuario, errorUsuario, "El usuario debe tener mínimo 4 caracteres.");
            return false;
        }

        if (valorUsuario.length > 30) {
            mostrarError(usuario, errorUsuario, "El usuario no debe superar los 30 caracteres.");
            return false;
        }

        if (!/^[a-zA-Z0-9._-]+$/.test(valorUsuario)) {
            mostrarError(usuario, errorUsuario, "Use solo letras, números, punto, guion o guion bajo.");
            return false;
        }

        mostrarCorrecto(usuario);
        return true;
    }

    function validarPassword() {
        const valorPassword = password.value.trim();

        limpiarEstado(password, errorPassword);

        if (valorPassword === "") {
            mostrarError(password, errorPassword, "La contraseña es obligatoria.");
            return false;
        }

        if (valorPassword.length < 3) {
            mostrarError(password, errorPassword, "La contraseña debe tener mínimo 3 caracteres.");
            return false;
        }

        if (valorPassword.length > 20) {
            mostrarError(password, errorPassword, "La contraseña no debe superar los 20 caracteres.");
            return false;
        }

        if (/\s/.test(valorPassword)) {
            mostrarError(password, errorPassword, "La contraseña no debe contener espacios.");
            return false;
        }

        mostrarCorrecto(password);
        return true;
    }

    function mostrarError(input, contenedorError, mensaje) {
        input.classList.add("input-error");
        input.classList.remove("input-correcto");
        contenedorError.textContent = mensaje;
    }

    function mostrarCorrecto(input) {
        input.classList.remove("input-error");
        input.classList.add("input-correcto");
    }

    function limpiarEstado(input, contenedorError) {
        input.classList.remove("input-error");
        input.classList.remove("input-correcto");
        contenedorError.textContent = "";
    }

    function mostrarErrorLoginDesdeURL() {
        const parametros = new URLSearchParams(window.location.search);
        const error = parametros.get("error");

        if (error === "1") {
            const mensaje = document.createElement("p");
            mensaje.className = "mensaje-login-error";
            mensaje.textContent = "Usuario o contraseña incorrectos.";

            formLogin.parentNode.insertBefore(mensaje, formLogin);
        }
    }
});
