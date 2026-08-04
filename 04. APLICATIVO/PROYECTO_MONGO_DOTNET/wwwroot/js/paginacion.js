(function () {
    const TAMANIO_PAGINA = 3;

    document.addEventListener("DOMContentLoaded", function () {
        configurarPaginacionTablas();
    });

    window.reiniciarPaginacionTablas = configurarPaginacionTablas;

    function configurarPaginacionTablas() {
        document.querySelectorAll("table[data-paginacion='true']").forEach(function (tabla) {
            paginarTabla(tabla);
        });
    }

    function paginarTabla(tabla) {
        const cuerpo = tabla.tBodies[0];

        if (!cuerpo) {
            return;
        }

        const filas = obtenerFilasValidas(cuerpo);
        const tamanio = obtenerTamanioPagina(tabla);
        const totalPaginas = Math.ceil(filas.length / tamanio);
        const contenedor = tabla.closest(".tabla-contenedor") || tabla;

        quitarControles(tabla, contenedor);

        if (filas.length <= tamanio) {
            mostrarTodasLasFilas(filas);
            return;
        }

        const paginaActual = normalizarPagina(tabla, totalPaginas);
        mostrarPagina(filas, paginaActual, tamanio);
        contenedor.insertAdjacentElement(
            "afterend",
            crearControles(tabla, paginaActual, totalPaginas, filas.length)
        );
    }

    function obtenerFilasValidas(cuerpo) {
        return Array.from(cuerpo.rows).filter(function (fila) {
            return !fila.querySelector(".tabla-vacia");
        });
    }

    function obtenerTamanioPagina(tabla) {
        const valor = Number(tabla.dataset.paginacionTamanio);
        return Number.isInteger(valor) && valor > 0 ? valor : TAMANIO_PAGINA;
    }

    function normalizarPagina(tabla, totalPaginas) {
        const pagina = Number(tabla.dataset.paginacionPagina || "1");
        const paginaActual = Number.isInteger(pagina) ? pagina : 1;
        return Math.min(Math.max(paginaActual, 1), totalPaginas);
    }

    function mostrarTodasLasFilas(filas) {
        filas.forEach(function (fila) {
            fila.style.display = "";
            fila.dataset.paginacionOculta = "false";
        });
    }

    function mostrarPagina(filas, paginaActual, tamanio) {
        const inicio = (paginaActual - 1) * tamanio;
        const fin = inicio + tamanio;

        filas.forEach(function (fila, indice) {
            const visible = indice >= inicio && indice < fin;
            fila.style.display = visible ? "" : "none";
            fila.dataset.paginacionOculta = visible ? "false" : "true";
        });
    }

    function crearControles(tabla, paginaActual, totalPaginas, totalRegistros) {
        const controles = document.createElement("div");
        controles.className = "tabla-paginacion";
        controles.dataset.paginacionControles = tabla.id;

        controles.appendChild(crearBoton(tabla, "Anterior", paginaActual - 1, paginaActual === 1));

        for (let pagina = 1; pagina <= totalPaginas; pagina++) {
            const boton = crearBoton(tabla, String(pagina), pagina, false);

            if (pagina === paginaActual) {
                boton.classList.add("activo");
                boton.setAttribute("aria-current", "page");
            }

            controles.appendChild(boton);
        }

        controles.appendChild(crearBoton(
            tabla,
            "Siguiente",
            paginaActual + 1,
            paginaActual === totalPaginas
        ));

        const resumen = document.createElement("span");
        resumen.className = "tabla-paginacion-resumen";
        resumen.textContent = "Pagina " + paginaActual + " de " + totalPaginas
                + " | " + totalRegistros + " registros";
        controles.appendChild(resumen);

        return controles;
    }

    function crearBoton(tabla, texto, pagina, deshabilitado) {
        const boton = document.createElement("button");
        boton.type = "button";
        boton.className = "paginacion-btn";
        boton.textContent = texto;
        boton.disabled = deshabilitado;

        boton.addEventListener("click", function () {
            tabla.dataset.paginacionPagina = String(pagina);
            paginarTabla(tabla);
        });

        return boton;
    }

    function quitarControles(tabla, contenedor) {
        const siguiente = contenedor.nextElementSibling;

        if (siguiente && siguiente.dataset.paginacionControles === tabla.id) {
            siguiente.remove();
        }
    }
}());
