document.addEventListener("DOMContentLoaded", function () {
    var toggles = document.querySelectorAll(".tree-toggle");

    toggles.forEach(function (btn) {
        btn.addEventListener("click", function () {
            var item = btn.closest(".tree-item");

            if (item) {
                item.classList.toggle("open");
            }
        });
    });

    var links = document.querySelectorAll(".tree-link");
    var panelInicio = document.getElementById("panelInicio");
    var panelModulo = document.getElementById("panelModulo");
    var frameModulo = document.getElementById("frameModulo");
    var tituloModuloActual = document.getElementById("tituloModuloActual");
    var btnVolverInicio = document.getElementById("btnVolverInicio");

    links.forEach(function (link) {
        link.addEventListener("click", function (event) {
            event.preventDefault();

            var url = link.getAttribute("data-tab-url") || link.getAttribute("href");
            var titulo = link.getAttribute("data-tab-title") || link.textContent.trim();

            if (!url || !frameModulo) {
                return;
            }

            links.forEach(function (otro) {
                otro.classList.remove("activo");
            });

            link.classList.add("activo");
            abrirPadres(link);

            if (tituloModuloActual) {
                tituloModuloActual.textContent = titulo;
            }

            if (panelInicio) {
                panelInicio.hidden = true;
            }

            if (panelModulo) {
                panelModulo.hidden = false;
            }

            frameModulo.src = url;
        });
    });

    if (btnVolverInicio) {
        btnVolverInicio.addEventListener("click", function () {
            if (frameModulo) {
                frameModulo.removeAttribute("src");
            }

            links.forEach(function (otro) {
                otro.classList.remove("activo");
            });

            if (panelModulo) {
                panelModulo.hidden = true;
            }

            if (panelInicio) {
                panelInicio.hidden = false;
            }
        });
    }

    function abrirPadres(elemento) {
        var padre = elemento.closest(".tree-parent");

        while (padre) {
            padre.classList.add("open");
            padre = padre.parentElement.closest(".tree-parent");
        }
    }
});
const btnSidebarInicio = document.getElementById("btnSidebarInicio");

if (btnSidebarInicio) {
    btnSidebarInicio.addEventListener("click", function () {
        const panelInicio = document.getElementById("panelInicio");
        const panelModulo = document.getElementById("panelModulo");
        const frameModulo = document.getElementById("frameModulo");
        const links = document.querySelectorAll(".tree-link");

        if (panelInicio) {
            panelInicio.hidden = false;
        }

        if (panelModulo) {
            panelModulo.hidden = true;
        }

        if (frameModulo) {
            frameModulo.src = "about:blank";
        }

        links.forEach(function (link) {
            link.classList.remove("activo");
        });
    });
}