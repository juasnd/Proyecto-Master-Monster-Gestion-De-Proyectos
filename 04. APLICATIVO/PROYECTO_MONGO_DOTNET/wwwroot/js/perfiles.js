document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("buscarPerfil");
    const tabla = document.getElementById("tablaPerfiles");

    if (!input || !tabla) {
        return;
    }

    input.addEventListener("input", function () {
        const filtro = normalizar(input.value);

        tabla.querySelectorAll("tbody tr").forEach(function (fila) {
            fila.style.display = normalizar(fila.textContent).includes(filtro) ? "" : "none";
        });
    });
});

function normalizar(texto) {
    return (texto || "")
            .toLowerCase()
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .trim();
}
