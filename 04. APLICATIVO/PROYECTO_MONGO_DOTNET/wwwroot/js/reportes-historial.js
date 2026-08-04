document.addEventListener("DOMContentLoaded", function () {
    const buscador = document.getElementById("buscarReporteHistorial");
    const tabla = document.getElementById("tablaHistorialReportes");

    if (!buscador || !tabla || !tabla.tBodies.length) {
        return;
    }

    buscador.addEventListener("input", function () {
        const termino = buscador.value.trim().toLowerCase();
        const filas = Array.from(tabla.tBodies[0].rows);

        filas.forEach(function (fila) {
            if (fila.querySelector(".tabla-vacia")) {
                return;
            }

            const texto = fila.textContent.replace(/\s+/g, " ").toLowerCase();
            fila.style.display = !termino || texto.includes(termino) ? "" : "none";
        });
    });
});