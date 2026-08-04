(function () {
    'use strict';
    const token = () => document.querySelector('.csrf-token input[name="__RequestVerificationToken"]')?.value || '';
    const escapar = valor => String(valor ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const fecha = valor => valor ? new Intl.DateTimeFormat('es-EC', { dateStyle: 'medium', timeZone: 'UTC' }).format(new Date(valor)) : 'No registrada';
    const fechaInput = valor => valor ? String(valor).slice(0, 10) : '';
    const moneda = valor => new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(Number(valor || 0));
    const numero = valor => new Intl.NumberFormat('es-EC', { maximumFractionDigits: 2 }).format(Number(valor || 0));
    const textoEstado = valor => ({ PLANIFICADO: 'Planificado', EN_CURSO: 'En curso', PAUSADO: 'Pausado', FINALIZADO: 'Finalizado', CANCELADO: 'Cancelado', PENDIENTE: 'Pendiente', EN_PROCESO: 'En progreso', COMPLETADA: 'Completada', ENTREGADO: 'Entregado', JUSTIFICADO: 'Justificado', ACTIVA: 'Activa', RETIRADA: 'Retirada', CERRADA: 'Cerrada' }[valor] || valor || 'Sin estado');
    const textoPrioridad = valor => ({ BAJA: 'Baja', MEDIA: 'Media', ALTA: 'Alta', CRITICA: 'Crítica' }[valor] || valor || '');

    async function api(url, opciones = {}) {
        const config = { credentials: 'same-origin', ...opciones, headers: { Accept: 'application/json', ...(opciones.headers || {}) } };
        if (config.body && !(config.body instanceof FormData)) config.headers['Content-Type'] = 'application/json';
        if (!['GET', 'HEAD'].includes((config.method || 'GET').toUpperCase())) config.headers['X-CSRF-TOKEN'] = token();
        const respuesta = await fetch(url, config);
        const tipo = respuesta.headers.get('content-type') || '';
        const json = tipo.includes('json') ? await respuesta.json() : null;
        if (!respuesta.ok || json?.exito === false) {
            const error = new Error(json?.mensaje || `No fue posible completar la operación (${respuesta.status}).`);
            error.errores = json?.errores || {};
            error.estadoHttp = respuesta.status;
            throw error;
        }
        return json?.datos ?? json;
    }

    function abrir(modal) {
        if (!modal) return;
        modal.showModal();
        requestAnimationFrame(() => modal.querySelector('[autofocus], input:not([type="hidden"]), select, button')?.focus());
    }
    function cerrar(modal) { if (modal?.open) modal.close(); }
    function limpiarErrores(form) { form?.querySelectorAll('[data-error]').forEach(x => x.textContent = ''); }
    function mostrarErrores(form, errores = {}) {
        limpiarErrores(form);
        Object.entries(errores).forEach(([campo, mensajes]) => {
            const destino = form?.querySelector(`[data-error="${CSS.escape(campo)}"]`);
            if (destino) destino.textContent = Array.isArray(mensajes) ? mensajes.join(' ') : mensajes;
        });
    }
    function validarFormulario(form) {
        limpiarErrores(form);
        let primero = null;
        form?.querySelectorAll('[required]').forEach(campo => {
            if (campo.disabled || campo.checkValidity()) return;
            const destino = form.querySelector(`[data-error="${CSS.escape(campo.name)}"]`);
            const mensaje = campo.validity.valueMissing ? 'Este campo es obligatorio.'
                : campo.validity.rangeUnderflow || campo.validity.rangeOverflow ? 'El valor está fuera del rango permitido.'
                    : 'Ingrese un valor válido para este campo.';
            if (destino) destino.textContent = mensaje;
            primero ||= campo;
        });
        primero?.focus();
        return !primero;
    }
    function bloquear(boton, bloqueado, texto = 'Guardando...') {
        if (!boton) return;
        if (bloqueado) { boton.dataset.textoOriginal = boton.textContent; boton.textContent = texto; boton.disabled = true; }
        else { boton.textContent = boton.dataset.textoOriginal || boton.textContent; boton.disabled = false; }
    }
    function toast(mensaje, tipo = 'exito') {
        const contenedor = document.querySelector('[data-toast-contenedor]') || document.body;
        const item = document.createElement('div');
        item.className = `toast-proyecto ${tipo}`;
        item.setAttribute('role', tipo === 'error' ? 'alert' : 'status');
        item.textContent = mensaje;
        contenedor.appendChild(item);
        requestAnimationFrame(() => item.classList.add('visible'));
        setTimeout(() => { item.classList.remove('visible'); setTimeout(() => item.remove(), 250); }, 4500);
    }
    function confirmar(titulo, mensaje, accion, textoBoton = 'Confirmar') {
        const modal = document.querySelector('[data-modal-confirmacion]');
        if (!modal) return;
        modal.querySelector('[data-confirmacion-titulo]').textContent = titulo;
        modal.querySelector('[data-confirmacion-mensaje]').textContent = mensaje;
        const boton = modal.querySelector('[data-confirmar-operacion]');
        boton.textContent = textoBoton;
        boton.onclick = async () => {
            bloquear(boton, true, 'Procesando...');
            try { await accion(); cerrar(modal); }
            catch (error) { toast(error.message, 'error'); }
            finally { bloquear(boton, false); }
        };
        abrir(modal);
    }

    document.addEventListener('click', evento => {
        const cerrarBoton = evento.target.closest('[data-cerrar-modal]');
        if (cerrarBoton) cerrar(cerrarBoton.closest('dialog'));
    });

    window.ProyectosUI = { api, escapar, fecha, fechaInput, moneda, numero, textoEstado, textoPrioridad, abrir, cerrar, limpiarErrores, mostrarErrores, validarFormulario, bloquear, toast, confirmar };
})();
