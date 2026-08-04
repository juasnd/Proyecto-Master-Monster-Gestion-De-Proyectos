document.addEventListener("DOMContentLoaded", function () {
    const selector = document.getElementById("perfilCodigo");
    const filtro = document.getElementById("formSeleccionPerfil");
    selector?.addEventListener("change", () => filtro.submit());

    const form = document.getElementById("formAsignacionUsuarios");
    const disponibles = document.getElementById("usuariosDisponibles");
    const asignados = document.getElementById("usuariosAsignados");
    const aviso = document.getElementById("avisoSeleccion");
    if (!form || !disponibles || !asignados) return;
    let arrastre = null;

    [disponibles, asignados].forEach(lista => {
        lista.addEventListener("click", evento => {
            const item = evento.target.closest(".asignacion-usuario-item");
            if (!item) return;
            if (!evento.ctrlKey && !evento.metaKey) limpiar(lista);
            seleccionar(item, !item.classList.contains("seleccionado"));
        });
        lista.addEventListener("dblclick", evento => {
            const item = evento.target.closest(".asignacion-usuario-item");
            if (!item) return;
            limpiar(lista); seleccionar(item, true); enviarSeleccionados(lista);
        });
        lista.addEventListener("keydown", evento => {
            if ((evento.ctrlKey || evento.metaKey) && evento.key.toLowerCase() === "a") {
                evento.preventDefault(); items(lista).forEach(x => seleccionar(x, true)); return;
            }
            const item = evento.target.closest(".asignacion-usuario-item");
            if (!item) return;
            if (evento.key === " " || evento.key === "Spacebar") { evento.preventDefault(); seleccionar(item, !item.classList.contains("seleccionado")); }
            if (evento.key === "Enter") { evento.preventDefault(); if (!item.classList.contains("seleccionado")) { limpiar(lista); seleccionar(item, true); } enviarSeleccionados(lista); }
        });
        lista.querySelectorAll(".asignacion-usuario-item").forEach(item => {
            item.addEventListener("dragstart", evento => {
                if (!item.classList.contains("seleccionado")) { limpiar(lista); seleccionar(item, true); }
                arrastre = { origenId: lista.id, logins: seleccionados(lista).map(x => x.dataset.login) };
                evento.dataTransfer.effectAllowed = "move";
                evento.dataTransfer.setData("text/plain", JSON.stringify(arrastre));
                seleccionados(lista).forEach(x => x.classList.add("arrastrando"));
            });
            item.addEventListener("dragend", () => {
                document.querySelectorAll(".arrastrando").forEach(x => x.classList.remove("arrastrando"));
                disponibles.classList.remove("recibiendo"); asignados.classList.remove("recibiendo"); arrastre = null;
            });
        });
        lista.addEventListener("dragover", evento => {
            if (!arrastre || arrastre.origenId === lista.id) return;
            evento.preventDefault(); lista.classList.add("recibiendo");
        });
        lista.addEventListener("dragleave", evento => { if (!lista.contains(evento.relatedTarget)) lista.classList.remove("recibiendo"); });
        lista.addEventListener("drop", evento => {
            evento.preventDefault(); lista.classList.remove("recibiendo");
            let datos = arrastre;
            if (!datos) { try { datos = JSON.parse(evento.dataTransfer.getData("text/plain")); } catch { return; } }
            if (!datos?.logins?.length || datos.origenId === lista.id) return;
            const origen = document.getElementById(datos.origenId);
            enviar(datos.origenId === "usuariosDisponibles" ? "asignarSeleccionados" : "retirarSeleccionados", origen.dataset.inputName, datos.logins);
        });
    });

    form.querySelectorAll("button[data-accion]").forEach(boton => boton.addEventListener("click", () => {
        const origenId = boton.dataset.origen;
        if (!origenId) return enviar(boton.dataset.accion, "", []);
        const origen = document.getElementById(origenId);
        const logins = seleccionados(origen).map(x => x.dataset.login);
        if (!logins.length) return mostrarAviso();
        enviar(boton.dataset.accion, origen.dataset.inputName, logins);
    }));
    buscar("buscarDisponibles", disponibles); buscar("buscarAsignados", asignados);

    function items(lista) { return Array.from(lista.querySelectorAll(".asignacion-usuario-item:not(.oculto)")); }
    function seleccionados(lista) { return Array.from(lista.querySelectorAll(".asignacion-usuario-item.seleccionado")); }
    function seleccionar(item, valor) { item.classList.toggle("seleccionado", valor); item.setAttribute("aria-selected", valor ? "true" : "false"); }
    function limpiar(lista) { seleccionados(lista).forEach(x => seleccionar(x, false)); }
    function enviarSeleccionados(lista) { const valores = seleccionados(lista).map(x => x.dataset.login); if (!valores.length) return mostrarAviso(); enviar(lista.id === "usuariosDisponibles" ? "asignarSeleccionados" : "retirarSeleccionados", lista.dataset.inputName, valores); }
    function enviar(accion, nombre, valores) {
        if (form.dataset.enviando === "true") return;
        form.dataset.enviando = "true";
        form.querySelectorAll(".campo-transferencia").forEach(x => x.remove());
        oculto("accion", accion); valores.forEach(x => oculto(nombre, x));
        form.querySelectorAll("button").forEach(x => x.disabled = true); form.submit();
    }
    function oculto(nombre, valor) { if (!nombre) return; const x = document.createElement("input"); x.type = "hidden"; x.name = nombre; x.value = valor; x.className = "campo-transferencia"; form.appendChild(x); }
    function buscar(id, lista) { document.getElementById(id)?.addEventListener("input", evento => { const q = normalizar(evento.target.value); lista.querySelectorAll(".asignacion-usuario-item").forEach(x => x.classList.toggle("oculto", !normalizar(x.dataset.search || x.textContent).includes(q))); }); }
    function normalizar(x) { return (x || "").toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "").trim(); }
    function mostrarAviso() { if (!aviso) return; aviso.hidden = false; clearTimeout(mostrarAviso.t); mostrarAviso.t = setTimeout(() => aviso.hidden = true, 3000); }
});
