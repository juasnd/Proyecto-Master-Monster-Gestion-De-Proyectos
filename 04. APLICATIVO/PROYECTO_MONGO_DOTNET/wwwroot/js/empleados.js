document.addEventListener("DOMContentLoaded", function () {
    configurarTabs();
    configurarPanelFormulario();
    configurarFiltroCargos();
    configurarFotoEmpleado();
    configurarBusqueda();
    configurarFamiliares();
    configurarValidacionTiempoReal();
});

const FOTO_EXT_VALIDAS = ["jpg", "jpeg", "png", "webp"];
const FOTO_MIME_VALIDOS = ["image/jpeg", "image/png", "image/webp"];
const FOTO_MAX_BYTES = 2 * 1024 * 1024;
const CAMPOS_FORMACION = [
    "formacionNivel",
    "formacionTitulo",
    "formacionInstitucion",
    "formacionInicio",
    "formacionFin",
    "formacionObservacion"
];
const CAMPOS_VALIDABLES = [
    "cedula",
    "nombres",
    "apellidos",
    "fechaNacimiento",
    "sexoCodigo",
    "estadoCivilCodigo",
    "departamentoCodigo",
    "cargoCodigo",
    "direccion",
    "celular",
    "telefonoDomicilio",
    "email"
].concat(CAMPOS_FORMACION);
let familiares = [];
let familiarEditando = -1;

function configurarTabs() {
    const contenedores = document.querySelectorAll("[data-tabs-scope]");

    contenedores.forEach(function (contenedor) {
        const botones = contenedor.querySelectorAll(".tab-empleado");
        const paneles = contenedor.querySelectorAll(".tab-panel");

        botones.forEach(function (boton) {
            boton.addEventListener("click", function () {
                const tab = boton.dataset.tab;

                botones.forEach(function (item) {
                    item.classList.remove("activo");
                });

                paneles.forEach(function (panel) {
                    panel.classList.remove("activo");
                });

                boton.classList.add("activo");

                const panelActivo = contenedor.querySelector("#" + tab);

                if (panelActivo) {
                    panelActivo.classList.add("activo");
                }
            });
        });
    });
}

function configurarPanelFormulario() {
    const panel = document.getElementById("panelFormularioEmpleado");
    const form = document.getElementById("formEmpleado");
    const btnMostrar = document.getElementById("btnMostrarFormularioEmpleado");
    const btnCancelar = document.getElementById("btnCancelarFormulario");
    const btnCerrar = document.getElementById("btnCerrarFormularioEmpleado");
    const accion = document.getElementById("accionEmpleado");
    const titulo = document.getElementById("tituloFormularioEmpleado");

    if (!panel || !form) {
        return;
    }

    if (btnMostrar) {
        btnMostrar.addEventListener("click", function () {
            prepararFormularioNuevo(panel, form, accion, titulo);
            mostrarPanel(panel);
            actualizarEstadoSubmit();
        });
    }

    if (btnCancelar) {
        btnCancelar.addEventListener("click", function () {
            ocultarPanel(panel);
        });
    }

    if (btnCerrar) {
        btnCerrar.addEventListener("click", function () {
            ocultarPanel(panel);
        });
    }

    panel.addEventListener("click", function (event) {
        if (event.target === panel) {
            ocultarPanel(panel);
        }
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && !panel.hidden) {
            ocultarPanel(panel);
        }
    });

    if (!panel.hidden) {
        mostrarPanel(panel);
    }
}

function prepararFormularioNuevo(panel, form, accion, titulo) {
    panel.dataset.modo = "nuevo";

    if (accion) {
        accion.value = "guardar";
    }

    if (titulo) {
        titulo.textContent = "Añadir empleado";
    }

    form.reset();
    limpiarEstados(form);

    form.querySelectorAll("input, select").forEach(function (campo) {
        if (campo.id === "accionEmpleado" || campo.name === "__RequestVerificationToken") {
            return;
        }

        if (campo.tagName === "SELECT") {
            campo.selectedIndex = 0;
        } else {
            campo.value = "";
        }
    });

    form.querySelectorAll("input, select, button").forEach(function (campo) {
        if (campo.id === "accionEmpleado" || campo.id === "btnCancelarFormulario") {
            return;
        }

        campo.disabled = false;
        campo.readOnly = false;
    });

    const codigoPersona = document.getElementById("codigoPersona");
    const codigoEmpleado = document.getElementById("codigoEmpleado");
    const cargo = document.getElementById("cargoCodigo");
    limpiarPreviewFoto();
    limpiarFamiliares();

    if (codigoPersona) {
        codigoPersona.readOnly = true;
        codigoPersona.placeholder = "Se genera automaticamente";
    }

    if (codigoEmpleado) {
        codigoEmpleado.readOnly = true;
        codigoEmpleado.placeholder = "Se genera automaticamente";
    }

    if (cargo) {
        cargo.dataset.selected = "";
        cargo.innerHTML = "";
        cargo.appendChild(new Option("Seleccione...", ""));
        cargo.disabled = true;
    }

    activarPrimerTab();
}

function configurarFotoEmpleado() {
    const inputFoto = document.getElementById("fotoEmpleado");

    if (!inputFoto) {
        return;
    }

    inputFoto.addEventListener("change", function () {
        const archivo = inputFoto.files && inputFoto.files.length > 0 ? inputFoto.files[0] : null;

        if (!archivo) {
            limpiarCampo("fotoEmpleado");
            return;
        }

        const mensaje = validarArchivoFoto(archivo);

        if (mensaje) {
            inputFoto.value = "";
            mostrarErrorCampo("fotoEmpleado", mensaje);
            return;
        }

        mostrarPreviewFoto(archivo);
        mostrarCampoValido("fotoEmpleado");
    });
}

function validarArchivoFoto(archivo) {
    const partes = archivo.name.split(".");
    const extension = partes.length > 1 ? partes.pop().toLowerCase() : "";

    if (!FOTO_EXT_VALIDAS.includes(extension)) {
        return "La foto debe ser jpg, jpeg, png o webp.";
    }

    if (!FOTO_MIME_VALIDOS.includes(archivo.type)) {
        return "El tipo de archivo no es una imagen válida.";
    }

    if (archivo.size > FOTO_MAX_BYTES) {
        return "La foto no debe superar 2 MB.";
    }

    return "";
}

function mostrarPreviewFoto(archivo) {
    const preview = document.getElementById("previewFotoEmpleado");
    const texto = document.getElementById("textoSinFoto");
    const marco = document.querySelector(".foto-marco-preview");

    if (!preview) {
        return;
    }

    const lector = new FileReader();

    lector.onload = function (event) {
        preview.src = event.target.result;
        preview.hidden = false;

        if (texto) {
            texto.hidden = true;
        }

        if (marco) {
            marco.classList.add("con-foto");
        }
    };

    lector.readAsDataURL(archivo);
}

function limpiarPreviewFoto() {
    const inputFoto = document.getElementById("fotoEmpleado");
    const fotoActual = document.getElementById("fotoActual");
    const preview = document.getElementById("previewFotoEmpleado");
    const texto = document.getElementById("textoSinFoto");
    const marco = document.querySelector(".foto-marco-preview");

    if (inputFoto) {
        inputFoto.value = "";
    }

    if (fotoActual) {
        fotoActual.value = "";
    }

    if (preview) {
        preview.src = "";
        preview.hidden = true;
    }

    if (texto) {
        texto.hidden = false;
    }

    if (marco) {
        marco.classList.remove("con-foto");
    }

    limpiarCampo("fotoEmpleado");
}

function mostrarPanel(panel) {
    panel.hidden = false;
    panel.classList.add("abierto");
    document.body.classList.add("empleado-form-modal-abierto");

    const primerCampo = panel.querySelector(
            "input:not([type='hidden']):not([disabled]), select:not([disabled])"
    );

    if (primerCampo) {
        window.setTimeout(function () {
            primerCampo.focus();
        }, 80);
    }
}

function ocultarPanel(panel) {
    panel.classList.remove("abierto");
    panel.hidden = true;
    document.body.classList.remove("empleado-form-modal-abierto");
}

function activarPrimerTab() {
    const primerTab = document.querySelector(".tab-empleado[data-tab='datosGenerales']");

    if (primerTab) {
        primerTab.click();
    }
}

function activarTabPorId(id) {
    const tab = document.querySelector(".tab-empleado[data-tab='" + id + "']");

    if (tab) {
        tab.click();
    }
}

function configurarFiltroCargos() {
    const form = document.getElementById("formEmpleado");
    const panel = document.getElementById("panelFormularioEmpleado");
    const departamento = document.getElementById("departamentoCodigo");
    const cargo = document.getElementById("cargoCodigo");

    if (!form || !departamento || !cargo) {
        return;
    }

    const opcionesOriginales = Array.from(cargo.options).map(function (opcion) {
        return {
            value: opcion.value,
            text: opcion.textContent.trim(),
            departamento: opcion.dataset.departamento || "",
            selected: opcion.selected
        };
    });

    async function cargarCargos() {
        const codigoDepartamento = departamento.value;
        const cargoSeleccionado = cargo.dataset.selected || cargo.value;

        cargo.innerHTML = "";
        cargo.appendChild(new Option("Seleccione...", ""));

        if (!codigoDepartamento) {
            cargo.disabled = true;
            validarCampoPorId("cargoCodigo");
            actualizarEstadoSubmit();
            return;
        }

        let cargos = await obtenerCargosDesdeServidor(form, codigoDepartamento);

        if (cargos.length === 0) {
            cargos = opcionesOriginales
                    .filter(function (opcion) {
                        return opcion.value && opcion.departamento === codigoDepartamento;
                    })
                    .map(function (opcion) {
                        return {
                            codigo: opcion.value,
                            descripcion: opcion.text,
                            departamento: opcion.departamento
                        };
                    });
        }

        cargos.forEach(function (item) {
            const opcion = new Option(item.descripcion, item.codigo);
            opcion.dataset.departamento = item.departamento || codigoDepartamento;

            if (item.codigo === cargoSeleccionado) {
                opcion.selected = true;
            }

            cargo.appendChild(opcion);
        });

        cargo.disabled = panel && panel.dataset.modo === "ver";
        validarCampoPorId("cargoCodigo");
        actualizarEstadoSubmit();
    }

    departamento.addEventListener("change", function () {
        cargo.dataset.selected = "";
        cargarCargos();
        validarCampoPorId("departamentoCodigo");
    });

    cargarCargos();
}

async function obtenerCargosDesdeServidor(form, codigoDepartamento) {
    try {
        const url = new URL(form.action);
        url.searchParams.set("accion", "cargosPorDepartamento");
        url.searchParams.set("departamento", codigoDepartamento);

        const respuesta = await fetch(url.toString(), {
            headers: {
                "Accept": "application/json"
            }
        });

        if (!respuesta.ok) {
            return [];
        }

        return await respuesta.json();
    } catch (error) {
        return [];
    }
}

function configurarBusqueda() {
    const criterio = document.getElementById("criterioBusqueda");
    const inputBuscar = document.getElementById("terminoBusqueda");
    const tabla = document.getElementById("tablaEmpleados");

    if (!criterio || !inputBuscar || !tabla) {
        return;
    }

    function filtrarTabla() {
        const texto = normalizar(inputBuscar.value);
        const campo = criterio.value || "codigo";
        const filas = tabla.querySelectorAll("tbody tr");

        filas.forEach(function (fila) {
            const celda = fila.querySelector("[data-campo='" + campo + "']");
            const datoOculto = fila.dataset ? fila.dataset[campo] : "";

            if (!celda && !datoOculto) {
                fila.style.display = "";
                return;
            }

            const contenido = normalizar(datoOculto || celda.textContent);
            fila.style.display = contenido.includes(texto) ? "" : "none";
        });
    }

    inputBuscar.addEventListener("input", filtrarTabla);
    criterio.addEventListener("change", filtrarTabla);
}

function configurarValidacionTiempoReal() {
    const form = document.getElementById("formEmpleado");

    if (!form) {
        return;
    }

    camposValidables().forEach(function (id) {
        const campo = document.getElementById(id);

        if (!campo) {
            return;
        }

        ["input", "blur", "change"].forEach(function (evento) {
            campo.addEventListener(evento, function () {
                validarCampoPorId(id);

                if (esCampoFormacion(id)) {
                    validarCamposFormacion();
                }

                actualizarEstadoSubmit();
            });
        });
    });

    form.addEventListener("submit", function (event) {
        const fotoValida = validarFotoAntesDeEnviar();
        const formValido = validarFormularioCompleto();
        const familiaValida = validarFamiliaAntesDeEnviar();
        const valido = formValido && fotoValida && familiaValida;

        if (!valido) {
            event.preventDefault();
            enfocarPrimerError();
        }
    });

    actualizarEstadoSubmit();
}

function validarFotoAntesDeEnviar() {
    const inputFoto = document.getElementById("fotoEmpleado");

    if (!inputFoto || !inputFoto.files || inputFoto.files.length === 0) {
        return true;
    }

    const mensaje = validarArchivoFoto(inputFoto.files[0]);

    if (mensaje) {
        inputFoto.value = "";
        mostrarErrorCampo("fotoEmpleado", mensaje);
        return false;
    }

    return true;
}

function camposValidables() {
    return CAMPOS_VALIDABLES;
}

function camposFormacion() {
    return CAMPOS_FORMACION;
}

function esCampoFormacion(id) {
    return camposFormacion().includes(id);
}

function formacionTieneDatos() {
    return camposFormacion().some(function (id) {
        const campo = document.getElementById(id);
        return campo && campo.value.trim();
    });
}

function validarCamposFormacion() {
    camposFormacion().forEach(function (id) {
        validarCampoPorId(id);
    });
}

function validarFormularioCompleto() {
    let valido = true;

    camposValidables().forEach(function (id) {
        if (!validarCampoPorId(id)) {
            valido = false;
        }
    });

    actualizarEstadoSubmit();
    return valido;
}

function validarCampoPorId(id) {
    const campo = document.getElementById(id);

    if (!campo || campo.disabled) {
        limpiarCampo(id);
        return true;
    }

    const valor = campo.value.trim();
    const mensaje = obtenerMensajeValidacion(id, valor);

    if (mensaje) {
        mostrarErrorCampo(id, mensaje);
        return false;
    }

    mostrarCampoValido(id);
    return true;
}

function obtenerMensajeValidacion(id, valor) {
    switch (id) {
        case "codigoEmpleado":
            return validarCodigo(valor, "El código de empleado");
        case "cedula":
            return validarCedula(valor);
        case "nombres":
            return validarSoloLetras(valor, "Los nombres");
        case "apellidos":
            return validarSoloLetras(valor, "Los apellidos");
        case "fechaNacimiento":
            return validarFechaNacimiento(valor);
        case "sexoCodigo":
            return valor ? "" : "Debe seleccionar el sexo.";
        case "estadoCivilCodigo":
            return "";
        case "departamentoCodigo":
            return valor ? "" : "Debe seleccionar el departamento.";
        case "cargoCodigo":
            return validarCargo(valor);
        case "direccion":
            return validarDireccion(valor);
        case "celular":
            return validarCelular(valor);
        case "telefonoDomicilio":
            return validarTelefonoDomicilio(valor);
        case "email":
            return validarEmail(valor);
        case "formacionNivel":
        case "formacionTitulo":
        case "formacionInstitucion":
        case "formacionInicio":
        case "formacionFin":
        case "formacionObservacion":
            return validarCampoFormacion(id, valor);
        default:
            return "";
    }
}

function validarCodigo(valor, etiqueta) {
    if (!valor) {
        return etiqueta + " es obligatorio.";
    }

    if (valor.length > 10) {
        return etiqueta + " no debe superar 10 caracteres.";
    }

    if (!/^[A-Za-z0-9]+$/.test(valor)) {
        return etiqueta + " solo debe contener letras y números, sin espacios.";
    }

    return "";
}

function validarCedula(valor) {
    if (!valor) {
        return "La cédula es obligatoria.";
    }

    if (!/^\d{10}$/.test(valor)) {
        return "La cédula debe tener exactamente 10 dígitos.";
    }

    if (!cedulaEcuatorianaValida(valor)) {
        return "La cédula ecuatoriana no es válida.";
    }

    return "";
}

function cedulaEcuatorianaValida(cedula) {
    const provincia = Number(cedula.substring(0, 2));
    const tercerDigito = Number(cedula.charAt(2));

    if (provincia < 1 || provincia > 24 || tercerDigito >= 6) {
        return false;
    }

    const coeficientes = [2, 1, 2, 1, 2, 1, 2, 1, 2];
    let suma = 0;

    for (let i = 0; i < coeficientes.length; i++) {
        let valor = Number(cedula.charAt(i)) * coeficientes[i];

        if (valor > 9) {
            valor -= 9;
        }

        suma += valor;
    }

    const decenaSuperior = Math.ceil(suma / 10) * 10;
    let digitoVerificador = decenaSuperior - suma;

    if (digitoVerificador === 10) {
        digitoVerificador = 0;
    }

    return digitoVerificador === Number(cedula.charAt(9));
}

function validarSoloLetras(valor, etiqueta) {
    if (!valor) {
        return etiqueta + " son obligatorios.";
    }

    if (valor.length < 2) {
        return etiqueta + " deben tener mínimo 2 caracteres.";
    }

    if (valor.length > 15) {
        return etiqueta + " no deben superar 15 caracteres.";
    }

    if (!/^[A-Za-zÁÉÍÓÚÜáéíóúüÑñ ]+$/.test(valor)) {
        return etiqueta + " solo deben contener letras y espacios.";
    }

    return "";
}

function validarFechaNacimiento(valor) {
    if (!valor) {
        return "La fecha de nacimiento es obligatoria.";
    }

    const fecha = new Date(valor + "T00:00:00");
    const hoy = new Date();

    if (Number.isNaN(fecha.getTime())) {
        return "Ingrese una fecha válida.";
    }

    if (fecha > hoy) {
        return "La fecha de nacimiento no puede ser futura.";
    }

    let edad = hoy.getFullYear() - fecha.getFullYear();
    const mes = hoy.getMonth() - fecha.getMonth();

    if (mes < 0 || (mes === 0 && hoy.getDate() < fecha.getDate())) {
        edad--;
    }

    if (edad < 18) {
        return "El empleado debe tener al menos 18 años.";
    }

    return "";
}

function validarCargo(valor) {
    const departamento = document.getElementById("departamentoCodigo");
    const cargo = document.getElementById("cargoCodigo");

    if (!valor) {
        return "Debe seleccionar el cargo.";
    }

    if (departamento && cargo) {
        const opcion = cargo.options[cargo.selectedIndex];

        if (opcion && opcion.dataset.departamento && opcion.dataset.departamento !== departamento.value) {
            return "El cargo seleccionado no pertenece al departamento.";
        }
    }

    return "";
}

function validarDireccion(valor) {
    if (!valor) {
        return "La dirección es obligatoria.";
    }

    if (valor.length > 100) {
        return "La dirección no debe superar 100 caracteres.";
    }

    return "";
}

function validarCelular(valor) {
    if (!valor) {
        return "";
    }

    if (!/^09\d{8}$/.test(valor)) {
        return "El celular debe tener 10 dígitos e iniciar con 09.";
    }

    return "";
}

function validarTelefonoDomicilio(valor) {
    if (!valor) {
        return "";
    }

    if (!/^0[2-7]\d{8}$/.test(valor)) {
        return "El teléfono debe tener 10 dígitos e iniciar con 02, 03, 04, 05, 06 o 07.";
    }

    return "";
}

function validarEmail(valor) {
    if (!valor) {
        return "El email es obligatorio.";
    }

    if (valor.length > 100) {
        return "El email no debe superar 100 caracteres.";
    }

    if (/\s/.test(valor)) {
        return "El email no debe contener espacios.";
    }

    if (!/^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$/.test(valor)) {
        return "Ingrese un email válido.";
    }

    return "";
}

function validarTextoObligatorio(valor, etiqueta, maximo, permiteNumeros) {
    if (!valor) {
        return etiqueta + " es obligatorio.";
    }

    if (valor.length < 2) {
        return etiqueta + " debe tener minimo 2 caracteres.";
    }

    if (valor.length > maximo) {
        return etiqueta + " no debe superar " + maximo + " caracteres.";
    }

    const patron = permiteNumeros
            ? /^[A-Za-z0-9\u00C0-\u017F .,'-]+$/
            : /^[A-Za-z\u00C0-\u017F .,'-]+$/;

    if (!patron.test(valor)) {
        return permiteNumeros
                ? etiqueta + " solo debe contener letras, numeros y signos basicos."
                : etiqueta + " solo debe contener letras y signos basicos.";
    }

    return "";
}

function validarFechaRequerida(valor, etiqueta) {
    if (!valor) {
        return etiqueta + " es obligatoria.";
    }

    const fecha = new Date(valor + "T00:00:00");

    if (Number.isNaN(fecha.getTime())) {
        return "Ingrese una fecha valida.";
    }

    return "";
}

function validarCampoFormacion(id, valor) {
    const requerida = formacionTieneDatos();

    switch (id) {
        case "formacionNivel":
            return requerida || valor
                    ? validarTextoObligatorio(valor, "El nivel de formacion", 40, true)
                    : "";
        case "formacionTitulo":
            return requerida || valor
                    ? validarTextoObligatorio(valor, "El titulo obtenido", 80, true)
                    : "";
        case "formacionInstitucion":
            return requerida || valor
                    ? validarTextoObligatorio(valor, "La institucion", 80, true)
                    : "";
        case "formacionInicio":
            return requerida || valor
                    ? validarFechaRequerida(valor, "La fecha de inicio")
                    : "";
        case "formacionFin":
            return validarFechaFinFormacion(valor);
        case "formacionObservacion":
            return validarObservacion(valor, "La observacion de formacion");
        default:
            return "";
    }
}

function validarFechaFinFormacion(valor) {
    if (!valor) {
        return "";
    }

    const mensajeBase = validarFechaRequerida(valor, "La fecha de finalizacion");

    if (mensajeBase) {
        return mensajeBase;
    }

    const inicio = document.getElementById("formacionInicio");

    if (inicio && inicio.value) {
        const fechaInicio = new Date(inicio.value + "T00:00:00");
        const fechaFin = new Date(valor + "T00:00:00");

        if (!Number.isNaN(fechaInicio.getTime()) && fechaFin < fechaInicio) {
            return "La fecha de finalizacion no puede ser anterior a la fecha de inicio.";
        }
    }

    return "";
}

function validarFechaNoFutura(valor, etiqueta) {
    const mensajeBase = validarFechaRequerida(valor, etiqueta);

    if (mensajeBase) {
        return mensajeBase;
    }

    const fecha = new Date(valor + "T00:00:00");
    const hoy = new Date();

    if (fecha > hoy) {
        return etiqueta + " no puede ser futura.";
    }

    return "";
}

function validarObservacion(valor, etiqueta) {
    if (valor.length > 200) {
        return etiqueta + " no debe superar 200 caracteres.";
    }

    if (valor && !/^[A-Za-z0-9\u00C0-\u017F .,';:()/-]+$/.test(valor)) {
        return etiqueta + " contiene caracteres no permitidos.";
    }

    return "";
}

function configurarFamiliares() {
    inicializarFamiliaresDesdeJson();

    const botonAgregar = document.getElementById("btnAgregarFamiliar");
    const botonCancelar = document.getElementById("btnCancelarEdicionFamiliar");
    const cuerpo = document.getElementById("tablaFamiliaresBody");

    camposFamiliares().forEach(function (id) {
        const campo = document.getElementById(id);

        if (!campo) {
            return;
        }

        ["input", "blur", "change"].forEach(function (evento) {
            campo.addEventListener(evento, function () {
                validarCampoFamiliar(id);
                limpiarCampo("familiares");
                actualizarEstadoSubmit();
            });
        });
    });

    if (botonAgregar) {
        botonAgregar.addEventListener("click", function () {
            guardarFamiliarDesdeFormulario();
        });
    }

    if (botonCancelar) {
        botonCancelar.addEventListener("click", function () {
            cancelarEdicionFamiliar();
        });
    }

    if (cuerpo) {
        cuerpo.addEventListener("click", function (event) {
            const boton = event.target.closest("[data-familiar-accion]");

            if (!boton) {
                return;
            }

            const indice = Number(boton.dataset.indice);

            if (Number.isNaN(indice)) {
                return;
            }

            if (boton.dataset.familiarAccion === "editar") {
                editarFamiliar(indice);
            } else if (boton.dataset.familiarAccion === "eliminar") {
                eliminarFamiliar(indice);
            }
        });
    }

    renderizarFamiliares();
    actualizarFamiliaresJson();
}

function inicializarFamiliaresDesdeJson() {
    const hidden = document.getElementById("familiaresJson");

    familiares = [];
    familiarEditando = -1;

    if (!hidden || !hidden.value.trim()) {
        return;
    }

    try {
        const datos = JSON.parse(hidden.value);

        if (Array.isArray(datos)) {
            familiares = datos.map(normalizarFamiliar).filter(function (item) {
                return item.nombre || item.apellido || item.codigoParentesco;
            });
        }
    } catch (error) {
        familiares = [];
    }
}

function camposFamiliares() {
    return [
        "familiarNombres",
        "familiarApellidos",
        "familiarParentesco",
        "familiarFechaNacimiento",
        "familiarTelefono",
        "familiarObservacion"
    ];
}

function guardarFamiliarDesdeFormulario() {
    if (!validarFamiliarActual()) {
        activarTabPorId("familia");
        enfocarPrimerError();
        return;
    }

    const familiar = leerFamiliarFormulario();

    if (familiarEditando >= 0) {
        familiar.codigo = familiares[familiarEditando] ? familiares[familiarEditando].codigo : "";
        familiares[familiarEditando] = familiar;
    } else {
        familiares.push(familiar);
    }

    limpiarCamposFamiliar();
    cancelarEdicionFamiliar(false);
    limpiarCampo("familiares");
    renderizarFamiliares();
    actualizarFamiliaresJson();
    actualizarEstadoSubmit();
}

function validarFamiliarActual() {
    let valido = true;

    camposFamiliares().forEach(function (id) {
        if (!validarCampoFamiliar(id)) {
            valido = false;
        }
    });

    return valido;
}

function validarCampoFamiliar(id) {
    const campo = document.getElementById(id);

    if (!campo || campo.disabled) {
        limpiarCampo(id);
        return true;
    }

    const valor = campo.value.trim();
    let mensaje = "";

    switch (id) {
        case "familiarNombres":
            mensaje = validarNombreFamiliar(valor, "Los nombres del familiar");
            break;
        case "familiarApellidos":
            mensaje = validarNombreFamiliar(valor, "Los apellidos del familiar");
            break;
        case "familiarParentesco":
            mensaje = validarParentescoFamiliar(valor);
            break;
        case "familiarFechaNacimiento":
            mensaje = validarFechaNoFutura(valor, "La fecha de nacimiento del familiar");
            break;
        case "familiarTelefono":
            mensaje = validarTelefonoFamiliar(valor);
            break;
        case "familiarObservacion":
            mensaje = valor.length > 200 ? "La observacion familiar no debe superar 200 caracteres." : "";
            break;
        default:
            mensaje = "";
    }

    if (mensaje) {
        mostrarErrorCampo(id, mensaje);
        return false;
    }

    mostrarCampoValido(id);
    return true;
}

function validarNombreFamiliar(valor, etiqueta) {
    if (!valor) {
        return etiqueta + " es obligatorio.";
    }

    if (valor.length > 30) {
        return etiqueta + " no debe superar 30 caracteres.";
    }

    if (!/^[A-Za-z\u00C0-\u017F ]+$/.test(valor)) {
        return etiqueta + " solo debe contener letras y espacios.";
    }

    return "";
}

function validarParentescoFamiliar(valor) {
    const select = document.getElementById("familiarParentesco");

    if (!valor) {
        return "Debe seleccionar el parentesco.";
    }

    if (!select || !Array.from(select.options).some(function (opcion) {
        return opcion.value === valor;
    })) {
        return "El parentesco seleccionado no existe.";
    }

    return "";
}

function validarTelefonoFamiliar(valor) {
    if (!valor) {
        return "El telefono del familiar es obligatorio.";
    }

    if (!/^\d{10}$/.test(valor)) {
        return "El telefono debe tener exactamente 10 digitos.";
    }

    if (!/^(09|0[2-7])\d{8}$/.test(valor)) {
        return "El telefono debe iniciar con 09, 02, 03, 04, 05, 06 o 07.";
    }

    return "";
}

function leerFamiliarFormulario() {
    const parentesco = document.getElementById("familiarParentesco");
    const opcionParentesco = parentesco && parentesco.selectedIndex >= 0
            ? parentesco.options[parentesco.selectedIndex]
            : null;

    return normalizarFamiliar({
        codigo: "",
        codigoParentesco: parentesco ? parentesco.value.trim() : "",
        descripcionParentesco: opcionParentesco ? opcionParentesco.textContent.trim() : "",
        nombre: valorCampo("familiarNombres"),
        apellido: valorCampo("familiarApellidos"),
        fechaNacimiento: valorCampo("familiarFechaNacimiento"),
        telefono: valorCampo("familiarTelefono"),
        cargaFamiliar: "S",
        observacion: valorCampo("familiarObservacion")
    });
}

function normalizarFamiliar(familiar) {
    return {
        codigo: familiar && familiar.codigo ? String(familiar.codigo).trim() : "",
        codigoParentesco: familiar && familiar.codigoParentesco ? String(familiar.codigoParentesco).trim() : "",
        descripcionParentesco: familiar && familiar.descripcionParentesco
            ? String(familiar.descripcionParentesco).trim()
            : "",
        nombre: familiar && familiar.nombre ? String(familiar.nombre).trim() : "",
        apellido: familiar && familiar.apellido ? String(familiar.apellido).trim() : "",
        fechaNacimiento: familiar && familiar.fechaNacimiento ? String(familiar.fechaNacimiento).trim() : "",
        telefono: familiar && familiar.telefono ? String(familiar.telefono).trim() : "",
        cargaFamiliar: familiar && familiar.cargaFamiliar ? String(familiar.cargaFamiliar).trim().toUpperCase() : "S",
        observacion: familiar && familiar.observacion ? String(familiar.observacion).trim() : ""
    };
}

function valorCampo(id) {
    const campo = document.getElementById(id);
    return campo ? campo.value.trim() : "";
}

function renderizarFamiliares() {
    const cuerpo = document.getElementById("tablaFamiliaresBody");

    if (!cuerpo) {
        return;
    }

    cuerpo.innerHTML = "";

    if (familiares.length === 0) {
        const fila = document.createElement("tr");
        const celda = document.createElement("td");
        celda.colSpan = 7;
        celda.className = "tabla-vacia";
        celda.textContent = "Sin familiares agregados.";
        fila.appendChild(celda);
        cuerpo.appendChild(fila);
        return;
    }

    familiares.forEach(function (familiar, indice) {
        const fila = document.createElement("tr");
        fila.dataset.familiar = "true";

        [
            familiar.nombre,
            familiar.apellido,
            familiar.descripcionParentesco,
            familiar.fechaNacimiento,
            familiar.telefono,
            familiar.observacion
        ].forEach(function (valor) {
            const celda = document.createElement("td");
            celda.textContent = valor || "";
            fila.appendChild(celda);
        });

        const acciones = document.createElement("td");
        acciones.appendChild(crearBotonFamiliar("Editar", "editar", indice));
        acciones.appendChild(crearBotonFamiliar("Eliminar", "eliminar", indice));
        fila.appendChild(acciones);
        cuerpo.appendChild(fila);
    });
}

function crearBotonFamiliar(texto, accion, indice) {
    const boton = document.createElement("button");
    boton.type = "button";
    boton.className = accion === "eliminar"
            ? "btn-familiar-tabla peligro"
            : "btn-familiar-tabla";
    boton.dataset.familiarAccion = accion;
    boton.dataset.indice = String(indice);
    boton.textContent = texto;
    return boton;
}

function editarFamiliar(indice) {
    const familiar = familiares[indice];

    if (!familiar) {
        return;
    }

    familiarEditando = indice;
    asignarValorCampo("familiarNombres", familiar.nombre);
    asignarValorCampo("familiarApellidos", familiar.apellido);
    asignarValorCampo("familiarParentesco", familiar.codigoParentesco);
    asignarValorCampo("familiarFechaNacimiento", familiar.fechaNacimiento);
    asignarValorCampo("familiarTelefono", familiar.telefono);
    asignarValorCampo("familiarObservacion", familiar.observacion);
    actualizarBotonFamiliar();
    activarTabPorId("familia");

    const campoNombre = document.getElementById("familiarNombres");

    if (campoNombre) {
        campoNombre.focus();
    }
}

function eliminarFamiliar(indice) {
    if (!familiares[indice]) {
        return;
    }

    if (!confirm("¿Seguro que desea eliminar este familiar?")) {
        return;
    }

    familiares.splice(indice, 1);

    if (familiarEditando === indice) {
        limpiarCamposFamiliar();
        cancelarEdicionFamiliar(false);
    } else if (familiarEditando > indice) {
        familiarEditando--;
    }

    renderizarFamiliares();
    actualizarFamiliaresJson();
    actualizarEstadoSubmit();
}

function cancelarEdicionFamiliar(limpiar) {
    familiarEditando = -1;

    if (limpiar !== false) {
        limpiarCamposFamiliar();
    }

    actualizarBotonFamiliar();
}

function actualizarBotonFamiliar() {
    const botonAgregar = document.getElementById("btnAgregarFamiliar");
    const botonCancelar = document.getElementById("btnCancelarEdicionFamiliar");

    if (botonAgregar) {
        botonAgregar.textContent = familiarEditando >= 0 ? "Actualizar familiar" : "+ Añadir familiar";
    }

    if (botonCancelar) {
        botonCancelar.hidden = familiarEditando < 0;
    }
}

function asignarValorCampo(id, valor) {
    const campo = document.getElementById(id);

    if (campo) {
        campo.value = valor || "";
        limpiarCampo(id);
    }
}

function limpiarCamposFamiliar() {
    camposFamiliares().forEach(function (id) {
        const campo = document.getElementById(id);

        if (!campo) {
            return;
        }

        if (campo.tagName === "SELECT") {
            campo.selectedIndex = 0;
        } else {
            campo.value = "";
        }

        limpiarCampo(id);
    });
}

function limpiarFamiliares() {
    familiares = [];
    familiarEditando = -1;
    limpiarCamposFamiliar();
    limpiarCampo("familiares");
    actualizarBotonFamiliar();
    renderizarFamiliares();
    actualizarFamiliaresJson();
}

function actualizarFamiliaresJson() {
    const hidden = document.getElementById("familiaresJson");

    if (hidden) {
        hidden.value = JSON.stringify(familiares);
    }

    actualizarCargasFamiliares();
}

function actualizarCargasFamiliares() {
    const campo = document.getElementById("cargasFamiliares");

    if (!campo) {
        return;
    }

    campo.value = String(contarCargasFamiliares());
    limpiarCampo("cargasFamiliares");
}

function contarCargasFamiliares() {
    return familiares.length;
}

function familiaTieneDatosParciales() {
    return camposFamiliares().some(function (id) {
        const campo = document.getElementById(id);
        return campo && campo.value.trim();
    });
}

function validarFamiliaAntesDeEnviar() {
    actualizarFamiliaresJson();

    if (familiaTieneDatosParciales()) {
        const valido = validarFamiliarActual();

        if (valido) {
            mostrarErrorCampo("familiares", "Presione + Añadir familiar para pasar los datos a la tabla.");
        }

        activarTabPorId("familia");
        return false;
    }

    limpiarCampo("familiares");
    return true;
}

function mostrarErrorCampo(id, mensaje) {
    const campo = document.getElementById(id);
    const error = document.querySelector("[data-error-for='" + id + "']");

    if (campo) {
        campo.classList.add("input-error");
        campo.classList.remove("input-success", "input-correcto");
    }

    if (error) {
        error.textContent = mensaje;
    }
}

function mostrarCampoValido(id) {
    const campo = document.getElementById(id);
    const error = document.querySelector("[data-error-for='" + id + "']");

    if (campo && !campo.disabled) {
        campo.classList.remove("input-error");
        campo.classList.add("input-success");
    }

    if (error) {
        error.textContent = "";
    }
}

function limpiarCampo(id) {
    const campo = document.getElementById(id);
    const error = document.querySelector("[data-error-for='" + id + "']");

    if (campo) {
        campo.classList.remove("input-error", "input-success", "input-correcto");
    }

    if (error) {
        error.textContent = "";
    }
}

function limpiarEstados(form) {
    form.querySelectorAll(".input-error, .input-success, .input-correcto").forEach(function (campo) {
        campo.classList.remove("input-error", "input-success", "input-correcto");
    });

    form.querySelectorAll(".field-error-message").forEach(function (mensaje) {
        mensaje.textContent = "";
    });
}

function actualizarEstadoSubmit() {
    const panel = document.getElementById("panelFormularioEmpleado");
    const boton = document.getElementById("btnSubmitEmpleado");

    if (!boton || !panel || panel.hidden || panel.dataset.modo === "ver") {
        return;
    }

    const hayErrores = camposValidables().some(function (id) {
        const campo = document.getElementById(id);

        if (!campo || campo.disabled) {
            return false;
        }

        return Boolean(validarMensajeSilencioso(id));
    });

    boton.disabled = hayErrores;
}

function validarMensajeSilencioso(id) {
    const campo = document.getElementById(id);

    if (!campo || campo.disabled) {
        return "";
    }

    const valor = campo.value.trim();

    return obtenerMensajeValidacion(id, valor);
}

function enfocarPrimerError() {
    const primerError = document.querySelector(".input-error");

    if (primerError) {
        primerError.focus();
        primerError.scrollIntoView({ behavior: "smooth", block: "center" });
    }
}

function normalizar(texto) {
    return (texto || "")
            .toLowerCase()
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .trim();
}
