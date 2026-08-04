document.addEventListener("DOMContentLoaded", function () {
    const todos = Array.from(document.querySelectorAll("input[name='opciones']:not(:disabled)"));
    document.getElementById("btnMarcarPermisos")?.addEventListener("click", () => todos.forEach(x => x.checked = true));
    document.getElementById("btnLimpiarPermisos")?.addEventListener("click", () => todos.forEach(x => x.checked = false));

    document.querySelectorAll(".matriz-toggle").forEach(boton => boton.addEventListener("click", function () {
        boton.closest(".matriz-sistema-card, .matriz-grupo-card")?.classList.toggle("abierto");
    }));

    document.querySelectorAll(".matriz-sistema-card").forEach(sistema => {
        const principal = sistema.querySelector(".matriz-sistema-check");
        principal?.addEventListener("change", () => sistema.querySelectorAll("input[name='opciones']:not(:disabled)").forEach(x => x.checked = principal.checked));
    });
    document.querySelectorAll(".matriz-grupo-card").forEach(grupo => {
        const principal = grupo.querySelector(".matriz-grupo-check");
        principal?.addEventListener("change", () => grupo.querySelectorAll("input[name='opciones']:not(:disabled)").forEach(x => x.checked = principal.checked));
    });
});
