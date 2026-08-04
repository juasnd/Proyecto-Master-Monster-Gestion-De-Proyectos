document.addEventListener("DOMContentLoaded", function () {
    configurarReportesGestion();
});

function configurarReportesGestion() {
    const paneles = document.querySelectorAll("[data-reporte-panel]");

    if (!paneles.length) {
        return;
    }

    paneles.forEach(function (panel) {
        prepararPanelModal(panel);

        const botonToggle = document.querySelector("[data-reporte-toggle='" + panel.id + "']");

        if (botonToggle) {
            botonToggle.addEventListener("click", function () {
                if (panel.hidden) {
                    abrirReporte(panel, paneles);
                } else {
                    cerrarReporte(panel);
                }
            });
        }

        panel.querySelectorAll("[data-reporte-accion]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                ejecutarAccionReporte(panel, boton.dataset.reporteAccion);
            });
        });

        actualizarVistaPrevia(panel);
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
            cerrarTodosReportes(paneles);
        }
    });
}

function prepararPanelModal(panel) {
    const header = panel.querySelector(".reporte-panel-header");

    if (!header || panel.querySelector("[data-reporte-cerrar]")) {
        return;
    }

    const botonCerrar = document.createElement("button");
    botonCerrar.type = "button";
    botonCerrar.className = "reporte-cerrar";
    botonCerrar.dataset.reporteCerrar = "true";
    botonCerrar.innerHTML = "&times;";
    botonCerrar.title = "Cerrar";
    botonCerrar.setAttribute("aria-label", "Cerrar reportes");

    botonCerrar.addEventListener("click", function () {
        cerrarReporte(panel);
    });

    header.appendChild(botonCerrar);
}

function abrirReporte(panel, paneles) {
    paneles.forEach(function (item) {
        if (item !== panel) {
            item.hidden = true;
        }
    });

    panel.hidden = false;
    document.body.classList.add("reporte-modal-abierto");
    actualizarVistaPrevia(panel);

    const primerBoton = panel.querySelector("[data-reporte-accion], [data-reporte-cerrar]");

    if (primerBoton) {
        primerBoton.focus();
    }
}

function cerrarReporte(panel) {
    panel.hidden = true;

    if (!document.querySelector("[data-reporte-panel]:not([hidden])")) {
        document.body.classList.remove("reporte-modal-abierto");
    }
}

function cerrarTodosReportes(paneles) {
    paneles.forEach(function (panel) {
        panel.hidden = true;
    });

    document.body.classList.remove("reporte-modal-abierto");
}

function ejecutarAccionReporte(panel, accion) {
    const datos = leerDatosReporte(panel);

    if (!datos.filas.length) {
        alert("No hay datos para generar el reporte.");
        return;
    }

    registrarHistorialReporte(datos, accion);

    if (accion === "csv") {
        descargarCsv(datos);
    } else if (accion === "excel") {
        descargarExcel(datos);
    } else if (accion === "pdf") {
        generarPdf(datos);
    }
}

function leerDatosReporte(panel) {
    const tabla = document.getElementById(panel.dataset.reporteTabla);
    const indices = (panel.dataset.reporteColumnas || "")
        .split(",")
        .map(function (valor) {
            return Number(valor.trim());
        })
        .filter(function (valor) {
            return !Number.isNaN(valor);
        });

    const encabezados = (panel.dataset.reporteEncabezados || "")
        .split("|")
        .map(function (valor) {
            return valor.trim();
        })
        .filter(Boolean);

    const titulo = panel.dataset.reporteTitulo || "Reporte";
    const datos = {
        titulo: titulo,
        archivo: panel.dataset.reporteArchivo || "reporte",
        codigoReporte: panel.dataset.reporteCodigo || "",
        modulo: panel.dataset.reporteModulo || titulo.replace(/^Reporte de\s+/i, ""),
        registroUrl: panel.dataset.reporteRegistroUrl || "",
        filtros: obtenerFiltrosReporte(panel),
        encabezados: encabezados,
        filas: []
    };

    if (!tabla || !tabla.tHead || !tabla.tBodies.length) {
        return datos;
    }

    if (!datos.encabezados.length) {
        const celdasEncabezado = Array.from(tabla.tHead.rows[0].cells);
        datos.encabezados = indices.map(function (indice) {
            return obtenerTexto(celdasEncabezado[indice]);
        });
    }

    datos.filas = Array.from(tabla.tBodies[0].rows)
        .filter(function (fila) {
            return !fila.querySelector(".tabla-vacia")
                    && (fila.dataset.paginacionOculta === "true"
                    || getComputedStyle(fila).display !== "none");
        })
        .map(function (fila) {
            const celdas = Array.from(fila.cells);

            return indices.map(function (indice) {
                return obtenerTexto(celdas[indice]);
            });
        })
        .filter(function (fila) {
            return fila.some(Boolean);
        });

    return datos;
}

function actualizarVistaPrevia(panel) {
    const datos = leerDatosReporte(panel);
    const tablaPreview = panel.querySelector("[data-reporte-preview]");
    const total = panel.querySelector("[data-reporte-total]");
    const fecha = panel.querySelector("[data-reporte-fecha]");

    if (total) {
        total.textContent = String(datos.filas.length);
    }

    if (fecha) {
        fecha.textContent = new Date().toLocaleString("es-EC");
    }

    if (!tablaPreview) {
        return;
    }

    const thead = tablaPreview.querySelector("thead");
    const tbody = tablaPreview.querySelector("tbody");

    thead.innerHTML = "";
    tbody.innerHTML = "";

    const filaEncabezado = document.createElement("tr");
    datos.encabezados.forEach(function (encabezado) {
        const th = document.createElement("th");
        th.textContent = encabezado;
        filaEncabezado.appendChild(th);
    });
    thead.appendChild(filaEncabezado);

    if (!datos.filas.length) {
        const fila = document.createElement("tr");
        const celda = document.createElement("td");
        celda.colSpan = Math.max(datos.encabezados.length, 1);
        celda.className = "tabla-vacia";
        celda.textContent = "No hay datos para previsualizar.";
        fila.appendChild(celda);
        tbody.appendChild(fila);
        return;
    }

    datos.filas.forEach(function (filaDatos) {
        const fila = document.createElement("tr");

        filaDatos.forEach(function (valor) {
            const td = document.createElement("td");
            td.textContent = valor;
            fila.appendChild(td);
        });

        tbody.appendChild(fila);
    });
}

function registrarHistorialReporte(datos, accion) {
    if (!datos || !datos.registroUrl || !datos.codigoReporte) {
        return;
    }

    const cuerpo = new URLSearchParams();
    cuerpo.append("accion", "registrar");
    cuerpo.append("codigoReporte", datos.codigoReporte);
    cuerpo.append("modulo", datos.modulo || "");
    cuerpo.append("tipoReporte", datos.titulo || "Reporte");
    cuerpo.append("formato", normalizarFormatoReporte(accion));
    cuerpo.append("totalRegistros", String(datos.filas ? datos.filas.length : 0));
    cuerpo.append("filtros", datos.filtros || "");

    fetch(datos.registroUrl, {
        method: "POST",
        credentials: "same-origin",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8"
        },
        body: cuerpo.toString()
    }).then(function (respuesta) {
        if (!respuesta.ok && window.console) {
            console.warn("No se pudo registrar el historial del reporte.");
        }
    }).catch(function (error) {
        if (window.console) {
            console.warn("No se pudo registrar el historial del reporte.", error);
        }
    });
}

function normalizarFormatoReporte(accion) {
    if (accion === "pdf") {
        return "PDF";
    }

    if (accion === "excel") {
        return "Excel";
    }

    if (accion === "csv") {
        return "CSV";
    }

    return accion || "";
}

function obtenerFiltrosReporte(panel) {
    const contenedor = panel.closest(".crud-contenedor") || document;
    const campos = Array.from(contenedor.querySelectorAll(
        ".buscar-registros input, .buscar-registros select, .busqueda-acciones input, .busqueda-acciones select"
    ));
    const filtros = [];

    campos.forEach(function (campo) {
        if (!campo || campo.type === "hidden" || campo.disabled) {
            return;
        }

        const valor = obtenerValorFiltro(campo);

        if (!valor) {
            return;
        }

        filtros.push(obtenerEtiquetaFiltro(campo) + ": " + valor);
    });

    return filtros.join("; ");
}

function obtenerValorFiltro(campo) {
    if (!campo) {
        return "";
    }

    if (campo.tagName === "SELECT") {
        const opcion = campo.options[campo.selectedIndex];
        return opcion ? obtenerTexto(opcion) : "";
    }

    return campo.value ? campo.value.trim() : "";
}

function obtenerEtiquetaFiltro(campo) {
    if (campo.id) {
        const labels = Array.from(document.querySelectorAll("label"));
        const etiqueta = labels.find(function (label) {
            return label.getAttribute("for") === campo.id;
        });

        if (etiqueta) {
            return obtenerTexto(etiqueta).replace(/:$/, "") || campo.id;
        }
    }

    return campo.name || campo.id || "Filtro";
}
function descargarCsv(datos) {
    const lineas = [datos.encabezados].concat(datos.filas).map(function (fila) {
        return fila.map(escaparCsv).join(";");
    });

    descargarArchivo(datos.archivo + ".csv", "\uFEFF" + lineas.join("\r\n"), "text/csv;charset=utf-8;");
}

function descargarExcel(datos) {
    const html = [
        "<html><head><meta charset='utf-8'></head><body>",
        "<table border='1'>",
        "<thead><tr>",
        datos.encabezados.map(function (valor) {
            return "<th>" + escaparHtml(valor) + "</th>";
        }).join(""),
        "</tr></thead><tbody>",
        datos.filas.map(function (fila) {
            return "<tr>" + fila.map(function (valor) {
                return "<td>" + escaparHtml(valor) + "</td>";
            }).join("") + "</tr>";
        }).join(""),
        "</tbody></table>",
        "</body></html>"
    ].join("");

    descargarArchivo(
        datos.archivo + ".xls",
        "\uFEFF" + html,
        "application/vnd.ms-excel;charset=utf-8;"
    );
}

function generarPdf(datos) {
    const ventana = window.open("", "_blank", "width=1000,height=720");

    if (!ventana) {
        alert("El navegador bloqueo la ventana del PDF.");
        return;
    }

    const filas = datos.filas.map(function (fila) {
        return "<tr>" + fila.map(function (valor) {
            return "<td>" + escaparHtml(valor) + "</td>";
        }).join("") + "</tr>";
    }).join("");

    const encabezados = datos.encabezados.map(function (valor) {
        return "<th>" + escaparHtml(valor) + "</th>";
    }).join("");

    const ayudaPdf = [
        "<p class='ayuda'>",
        "Seleccione la opcion Guardar como PDF en la ventana del navegador.",
        "</p>"
    ].join("");
    const fecha = new Date().toLocaleString("es-EC");
    const modulo = datos.titulo.replace(/^Reporte de\s+/i, "");
    const marcaAguaUrl = new URL("img/inge.monster.png", window.location.href).href;
    const estilosPdf = [
        "@page{size:A4 landscape;margin:12mm;}",
        "*{box-sizing:border-box;}",
        "body{font-family:Segoe UI,Arial,sans-serif;color:#263238;margin:0;",
        "background:#eef3f6;padding:24px;}",
        ".documento{position:relative;background:#fff;border:1px solid #d5dde3;",
        "border-radius:8px;box-shadow:0 18px 40px rgba(16,24,32,.14);overflow:hidden;}",
        ".documento>*:not(.marca-agua){position:relative;z-index:1;}",
        ".marca-agua{position:fixed;top:52%;left:50%;width:58%;max-width:620px;",
        "opacity:.08;transform:translate(-50%,-50%);z-index:0;pointer-events:none;}",
        ".cabecera{display:flex;justify-content:space-between;gap:24px;",
        "align-items:flex-start;padding:22px 24px;border-bottom:4px solid #0073bc;",
        "background:linear-gradient(90deg,#f8fbfd,#eef6fb);}",
        ".marca{display:inline-block;color:#0073bc;font-weight:900;font-size:13px;",
        "letter-spacing:.6px;text-transform:uppercase;margin-bottom:8px;}",
        "h1{font-size:26px;line-height:1.15;margin:0;color:#101820;}",
        ".subtitulo{margin:7px 0 0;color:#607d8b;font-size:13px;font-weight:700;}",
        ".resumen{display:grid;grid-template-columns:repeat(3,minmax(150px,1fr));",
        "gap:10px;min-width:430px;}",
        ".resumen div{background:#fff;border:1px solid #d5dde3;border-radius:6px;",
        "padding:10px 12px;}",
        ".resumen span{display:block;color:#607d8b;font-size:11px;font-weight:800;",
        "text-transform:uppercase;margin-bottom:4px;}",
        ".resumen strong{display:block;color:#263238;font-size:14px;line-height:1.25;}",
        ".contenido{padding:20px 24px 24px;}",
        ".ayuda{margin:0 0 14px;padding:10px 12px;border:1px solid #b8d8e5;",
        "border-radius:6px;background:#f2f9fc;color:#37474f;font-size:12px;font-weight:700;}",
        "table{width:100%;border-collapse:collapse;font-size:12.5px;table-layout:auto;}",
        "th,td{border:1px solid #cfd8dc;padding:9px 10px;text-align:left;",
        "vertical-align:top;overflow-wrap:anywhere;}",
        "th{background:#f4f6f8;color:#263238;font-weight:900;}",
        "tbody tr:nth-child(even){background:#f9fbfc;}",
        "tbody tr:hover{background:#eef6fb;}",
        ".pie{padding:12px 24px;border-top:1px solid #d5dde3;color:#607d8b;",
        "font-size:11px;font-weight:700;display:flex;justify-content:space-between;gap:12px;}",
        "@media print{body{background:#fff;padding:0}",
        ".documento{box-shadow:none;border:none;border-radius:0}",
        ".cabecera{background:#fff}.marca-agua{opacity:.09}.ayuda{display:none}",
        ".pie{position:fixed;bottom:0;left:0;right:0;background:#fff}",
        ".contenido{padding-bottom:34px}}"
    ].join("");
    const cabeceraPdf = [
        "<header class='cabecera'>",
        "<div>",
        "<span class='marca'>Master Monster</span>",
        "<h1>", escaparHtml(datos.titulo), "</h1>",
        "<p class='subtitulo'>Vista preparada para exportar PDF</p>",
        "</div>",
        "<section class='resumen'>",
        "<div><span>Modulo</span><strong>", escaparHtml(modulo), "</strong></div>",
        "<div><span>Registros</span><strong>", datos.filas.length, "</strong></div>",
        "<div><span>Generado</span><strong>", escaparHtml(fecha), "</strong></div>",
        "</section>",
        "</header>"
    ].join("");
    const cuerpoPdf = [
        "<section class='contenido'>",
        ayudaPdf,
        "<table><thead><tr>", encabezados, "</tr></thead><tbody>",
        filas,
        "</tbody></table>",
        "</section>"
    ].join("");
    const piePdf = [
        "<footer class='pie'>",
        "<span>Master Monster</span>",
        "<span>", escaparHtml(datos.archivo), "</span>",
        "</footer>"
    ].join("");
    const documentoPdf = [
        "<!doctype html><html lang='es'><head><meta charset='utf-8'>",
        "<title>", escaparHtml(datos.titulo), "</title>",
        "<style>", estilosPdf, "</style>",
        "</head><body>",
        "<main class='documento'>",
        "<img class='marca-agua' src='",
        escaparHtml(marcaAguaUrl),
        "' alt='Marca de agua'>",
        cabeceraPdf,
        cuerpoPdf,
        piePdf,
        "</main>",
        "<script>window.onload=function(){window.focus();window.print();};<\/script>",
        "</body></html>"
    ].join("");

    ventana.document.open();
    ventana.document.write(documentoPdf);
    ventana.document.close();
}

function descargarArchivo(nombre, contenido, tipo) {
    const blob = new Blob([contenido], { type: tipo });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement("a");

    enlace.href = url;
    enlace.download = nombre;
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);
    URL.revokeObjectURL(url);
}

function obtenerTexto(celda) {
    if (!celda) {
        return "";
    }

    return celda.textContent.replace(/\s+/g, " ").trim();
}

function escaparCsv(valor) {
    const texto = String(valor || "");
    return "\"" + texto.replace(/"/g, "\"\"") + "\"";
}

function escaparHtml(valor) {
    return String(valor || "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}
