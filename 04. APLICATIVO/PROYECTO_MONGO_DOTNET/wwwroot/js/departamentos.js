document.addEventListener("DOMContentLoaded", function () {
    configurarFiltro("buscarDepartamento", "tablaDepartamentos");
    configurarFiltro("buscarCargo", "tablaCargos");
});

function configurarFiltro(inputId, tablaId) {
    const inputBuscar = document.getElementById(inputId);
    const tabla = document.getElementById(tablaId);

    if (!inputBuscar || !tabla) {
        return;
    }

    inputBuscar.addEventListener("keyup", function () {
        const texto = this.value.toLowerCase().trim();
        const filas = tabla.querySelectorAll("tbody tr");

        filas.forEach(function (fila) {
            const contenidoFila = fila.textContent.toLowerCase();
            fila.style.display = contenidoFila.includes(texto) ? "" : "none";
        });
    });
}
