(function () {
    'use strict';
    let codigoProyecto = '';
    let proyectoActual = null;
    let equipo = [];
    let destinoPlan = null;
    const UI = () => window.ProyectosUI;

    function establecerProyecto(codigo, proyecto) { codigoProyecto = codigo; proyectoActual = proyecto; }

    async function cargar() {
        if (!codigoProyecto) return [];
        equipo = await UI().api(`/api/v1/proyectos/${encodeURIComponent(codigoProyecto)}/empleados/capacidad`);
        return equipo;
    }

    async function abrir(codigo, proyecto) {
        establecerProyecto(codigo, proyecto);
        const modal = document.querySelector('[data-modal-equipo]');
        if (!modal) return;
        modal.querySelector('[data-equipo-titulo]').textContent = `Equipo — ${proyecto.nombre}`;
        modal.querySelector('[data-equipo-subtitulo]').textContent = `Estado: ${UI().textoEstado(proyecto.estado)}`;
        UI().abrir(modal);
        await refrescarModal();
    }

    async function refrescarModal() {
        const cuerpo = document.querySelector('[data-equipo-tabla]');
        if (!cuerpo) return;
        cuerpo.innerHTML = '<tr><td colspan="7" class="tabla-vacia">Cargando equipo...</td></tr>';
        try { await cargar(); pintarTabla(cuerpo, false); }
        catch (error) { cuerpo.innerHTML = `<tr><td colspan="7" class="tabla-vacia">${UI().escapar(error.message)}</td></tr>`; }
    }

    async function pintarEnPlan(codigo, proyecto, cuerpo) {
        establecerProyecto(codigo, proyecto);
        destinoPlan = cuerpo;
        if (!cuerpo) return;
        try { await cargar(); pintarTabla(cuerpo, true); }
        catch (error) { cuerpo.innerHTML = `<tr><td colspan="8" class="tabla-vacia">${UI().escapar(error.message)}</td></tr>`; }
    }

    function pintarTabla(cuerpo, capacidadCompleta) {
        if (!equipo.length) { cuerpo.innerHTML = `<tr><td colspan="${capacidadCompleta ? 8 : 7}" class="tabla-vacia">Aún no hay integrantes. Use “Asignar integrante” para formar el equipo.</td></tr>`; return; }
        cuerpo.innerHTML = equipo.map(x => {
            const a = x.asignacion;
            const estado = x.estadoCapacidad === 'SOBRECARGADO' ? 'Sobrecargado' : x.estadoCapacidad === 'CERCA_DEL_LIMITE' ? 'Cerca del límite' : 'Disponible';
            const acciones = a.activo ? `<div class="acciones-compactas"><button type="button" data-editar-integrante="${UI().escapar(a.codigoEmpleado)}">Editar</button><button type="button" class="peligro" data-retirar-integrante="${UI().escapar(a.codigoEmpleado)}">Retirar</button></div>` : '';
            if (capacidadCompleta) return `<tr><td><strong>${UI().escapar(x.nombreEmpleado)}</strong></td><td>${UI().escapar(a.rol)}</td><td>${UI().numero(a.disponibilidadAsignada)} %<small>${UI().numero(x.disponibilidadRestante)} % restante</small></td><td>${UI().numero(x.horasDisponiblesEstimadas)} h</td><td>${UI().numero(x.horasPlanificadasTareas)} h</td><td class="${x.diferenciaHoras < 0 ? 'texto-peligro' : ''}">${UI().numero(x.diferenciaHoras)} h</td><td><span class="estado-capacidad ${x.estadoCapacidad.toLowerCase()}">${estado}</span></td><td>${acciones}</td></tr>`;
            return `<tr><td><strong>${UI().escapar(x.nombreEmpleado)}</strong></td><td>${UI().escapar(a.rol)}</td><td>${UI().numero(a.disponibilidadAsignada)} %<small>${UI().numero(x.disponibilidadUsada)} % usado · ${UI().numero(x.disponibilidadRestante)} % restante</small></td><td>${UI().fecha(a.fechaInicio)} – ${UI().fecha(a.fechaFin)}</td><td>${UI().numero(x.horasDisponiblesEstimadas)} h / ${UI().numero(x.horasPlanificadasTareas)} h</td><td><span class="estado-capacidad ${x.estadoCapacidad.toLowerCase()}">${estado}</span></td><td>${acciones}</td></tr>`;
        }).join('');
        cuerpo.querySelectorAll('small').forEach(x => x.classList.add('tabla-secundario'));
    }

    function abrirAsignacion(codigoEmpleado = '') {
        if (!codigoProyecto) return;
        const modal = document.querySelector('[data-modal-asignacion]');
        const form = modal?.querySelector('[data-form-asignacion]');
        if (!modal || !form) return;
        form.reset(); UI().limpiarErrores(form);
        const actual = equipo.find(x => x.asignacion.codigoEmpleado === codigoEmpleado)?.asignacion;
        form.dataset.editarEmpleado = actual?.codigoEmpleado || '';
        form.codigoEmpleado.disabled = Boolean(actual);
        form.codigoEmpleado.value = actual?.codigoEmpleado || '';
        form.rol.value = actual?.rol || '';
        form.disponibilidadAsignada.value = String(actual?.disponibilidadAsignada || 25);
        form.fechaInicio.value = UI().fechaInput(actual?.fechaInicio || proyectoActual?.fechaInicio);
        form.fechaFin.value = UI().fechaInput(actual?.fechaFin || proyectoActual?.fechaFinPlanificada);
        form.observacion.value = actual?.observacion || '';
        modal.querySelector('[data-asignacion-titulo]').textContent = actual ? 'Editar integrante' : 'Asignar integrante';
        const resumen = modal.querySelector('[data-disponibilidad-ayuda]');
        resumen.textContent = actual ? `Disponibilidad usada en fechas coincidentes: ${UI().numero(equipo.find(x => x.asignacion.codigoEmpleado === codigoEmpleado)?.disponibilidadUsada)} %. Restante: ${UI().numero(equipo.find(x => x.asignacion.codigoEmpleado === codigoEmpleado)?.disponibilidadRestante)} %.` : 'La disponibilidad final se comprobará al guardar.';
        UI().abrir(modal);
    }

    async function guardar(evento) {
        evento.preventDefault();
        const form = evento.currentTarget;
        UI().limpiarErrores(form);
        if (!UI().validarFormulario(form)) { UI().toast('Revise los campos señalados.', 'error'); return; }
        const boton = form.querySelector('[data-guardar-asignacion]');
        const dto = { codigoEmpleado: form.codigoEmpleado.value, rol: form.rol.value, disponibilidadAsignada: Number(form.disponibilidadAsignada.value), fechaInicio: form.fechaInicio.value, fechaFin: form.fechaFin.value, observacion: form.observacion.value };
        const empleadoEditar = form.dataset.editarEmpleado;
        UI().bloquear(boton, true);
        try {
            await UI().api(`/api/v1/proyectos/${encodeURIComponent(codigoProyecto)}/empleados${empleadoEditar ? `/${encodeURIComponent(empleadoEditar)}` : ''}`, { method: empleadoEditar ? 'PUT' : 'POST', body: JSON.stringify(dto) });
            UI().cerrar(form.closest('dialog')); UI().toast(empleadoEditar ? 'Integrante actualizado.' : 'Integrante asignado.');
            if (document.querySelector('[data-equipo-tabla]')) await refrescarModal();
            if (destinoPlan) await pintarEnPlan(codigoProyecto, proyectoActual, destinoPlan);
            document.dispatchEvent(new CustomEvent('equipo-proyecto-actualizado'));
        } catch (error) { UI().mostrarErrores(form, error.errores); UI().toast(error.message, 'error'); }
        finally { UI().bloquear(boton, false); }
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelector('[data-form-asignacion]')?.addEventListener('submit', guardar);
        document.addEventListener('click', evento => {
            if (evento.target.closest('[data-asignar-integrante]')) abrirAsignacion();
            const editar = evento.target.closest('[data-editar-integrante]');
            if (editar) abrirAsignacion(editar.dataset.editarIntegrante);
            const retirar = evento.target.closest('[data-retirar-integrante]');
            if (retirar) UI().confirmar('Retirar integrante', 'Solo podrá retirarse si no tiene tareas activas. Si las tiene, el sistema indicará cuáles debe reasignar.', async () => {
                await UI().api(`/api/v1/proyectos/${encodeURIComponent(codigoProyecto)}/empleados/${encodeURIComponent(retirar.dataset.retirarIntegrante)}`, { method: 'DELETE' });
                UI().toast('Integrante retirado.');
                if (document.querySelector('[data-equipo-tabla]')) await refrescarModal();
                if (destinoPlan) await pintarEnPlan(codigoProyecto, proyectoActual, destinoPlan);
            }, 'Retirar');
        });
    });

    window.EquipoProyecto = { abrir, establecerProyecto, pintarEnPlan, cargar };
})();
