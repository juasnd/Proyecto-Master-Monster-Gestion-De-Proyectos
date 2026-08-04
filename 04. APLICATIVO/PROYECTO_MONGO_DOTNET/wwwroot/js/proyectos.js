document.addEventListener('DOMContentLoaded', () => {
    'use strict';
    const pagina = document.querySelector('[data-proyectos-gestion]');
    if (!pagina) return;
    const UI = window.ProyectosUI;
    const tabla = pagina.querySelector('[data-tabla-proyectos] tbody');
    const permisos = { crear: pagina.dataset.puedeCrear === 'true', editar: pagina.dataset.puedeEditar === 'true', inactivar: pagina.dataset.puedeInactivar === 'true', equipo: pagina.dataset.puedeEquipo === 'true', planificar: pagina.dataset.puedePlanificar === 'true', estado: pagina.dataset.puedeEstado === 'true', finalizar: pagina.dataset.puedeFinalizar === 'true' };
    let proyectos = [];
    let paginaActual = 1;
    const tamanio = 10;

    async function cargar() {
        tabla.innerHTML = '<tr><td colspan="9" class="tabla-vacia">Cargando proyectos...</td></tr>';
        try {
            const incluir = pagina.querySelector('[data-incluir-inactivos]').checked;
            proyectos = await UI.api(`/api/v1/proyectos?incluirInactivos=${incluir}`);
            paginaActual = 1;
            pintar();
        } catch (error) { tabla.innerHTML = `<tr><td colspan="9" class="tabla-vacia">${UI.escapar(error.message)}</td></tr>`; UI.toast(error.message, 'error'); }
    }

    function filtrados() {
        const texto = pagina.querySelector('[data-filtro-texto]').value.trim().toLocaleLowerCase('es');
        const estado = pagina.querySelector('[data-filtro-estado]').value;
        const prioridad = pagina.querySelector('[data-filtro-prioridad]').value;
        const departamento = pagina.querySelector('[data-filtro-departamento]').value;
        return proyectos.filter(x => {
            const p = x.proyecto;
            return (!texto || `${p.nombre} ${x.nombreJefeProyecto} ${x.nombreDepartamento}`.toLocaleLowerCase('es').includes(texto)) && (!estado || p.estado === estado) && (!prioridad || p.prioridad === prioridad) && (!departamento || p.codigoDepartamento === departamento);
        });
    }

    function pintar() {
        const lista = filtrados();
        const totalPaginas = Math.max(1, Math.ceil(lista.length / tamanio));
        paginaActual = Math.min(paginaActual, totalPaginas);
        const visibles = lista.slice((paginaActual - 1) * tamanio, paginaActual * tamanio);
        tabla.innerHTML = visibles.length ? visibles.map(fila).join('') : '<tr><td colspan="9" class="tabla-vacia">No hay proyectos que coincidan con los filtros.</td></tr>';
        pagina.querySelector('[data-resumen-paginacion]').textContent = lista.length ? `Mostrando ${(paginaActual - 1) * tamanio + 1}–${Math.min(paginaActual * tamanio, lista.length)} de ${lista.length}` : '0 proyectos';
        pagina.querySelector('[data-pagina-anterior]').disabled = paginaActual <= 1;
        pagina.querySelector('[data-pagina-siguiente]').disabled = paginaActual >= totalPaginas;
    }

    function fila(x) {
        const p = x.proyecto;
        const acciones = [`<button type="button" data-ver-proyecto="${UI.escapar(p.codigo)}">Ver</button>`];
        if (permisos.editar && p.activo && !['FINALIZADO', 'CANCELADO'].includes(p.estado)) acciones.push(`<button type="button" data-editar-proyecto="${UI.escapar(p.codigo)}">Editar</button>`);
        if (permisos.equipo && p.activo && !['FINALIZADO', 'CANCELADO'].includes(p.estado)) acciones.push(`<button type="button" data-equipo-proyecto="${UI.escapar(p.codigo)}">Equipo</button>`);
        if (permisos.planificar) acciones.push(`<a href="/Proyectos/${encodeURIComponent(p.codigo)}/Planificacion">Planificar</a>`);
        if (permisos.estado && transicion(p)) acciones.push(`<button type="button" data-estado-proyecto="${UI.escapar(p.codigo)}">${transicion(p).accion}</button>`);
        if (permisos.finalizar && p.activo && ['EN_CURSO', 'PAUSADO'].includes(p.estado)) acciones.push(`<button type="button" data-finalizar-proyecto="${UI.escapar(p.codigo)}">Finalizar</button>`);
        if (permisos.finalizar && p.activo && !['FINALIZADO', 'CANCELADO'].includes(p.estado)) acciones.push(`<button type="button" class="peligro" data-cancelar-proyecto="${UI.escapar(p.codigo)}">Cancelar</button>`);
        if (permisos.inactivar && !['EN_CURSO'].includes(p.estado)) acciones.push(`<button type="button" data-activo-proyecto="${UI.escapar(p.codigo)}" data-activo-destino="${!p.activo}">${p.activo ? 'Inactivar' : 'Reactivar'}</button>`);
        return `<tr class="${p.activo ? '' : 'fila-inactiva'}"><td><strong>${UI.escapar(p.nombre)}</strong>${p.activo ? '' : '<small>Inactivo</small>'}</td><td>${UI.escapar(x.nombreDepartamento)}</td><td>${UI.escapar(x.nombreJefeProyecto)}</td><td>${UI.fecha(p.fechaInicio)}</td><td>${UI.fecha(p.fechaFinPlanificada)}</td><td><span class="estado-proyecto estado-${p.estado.toLowerCase()}">${UI.textoEstado(p.estado)}</span></td><td><span class="prioridad-proyecto prioridad-${p.prioridad.toLowerCase()}">${UI.textoPrioridad(p.prioridad)}</span></td><td><div class="avance-tabla"><span style="width:${Math.min(100, Number(p.avance))}%"></span></div><small>${UI.numero(p.avance)} %</small></td><td><div class="acciones-compactas acciones-proyecto">${acciones.join('')}</div></td></tr>`;
    }

    const buscar = codigo => proyectos.find(x => x.proyecto.codigo === codigo);
    const transicion = p => p.estado === 'PLANIFICADO' ? { destino: 'EN_CURSO', accion: 'Iniciar', explicacion: 'Al iniciar, el plan debe cubrir el 100 % del proyecto.' } : p.estado === 'EN_CURSO' ? { destino: 'PAUSADO', accion: 'Pausar', explicacion: 'El proyecto quedará en pausa hasta que se reanude.' } : p.estado === 'PAUSADO' ? { destino: 'EN_CURSO', accion: 'Reanudar', explicacion: 'El proyecto volverá a estar en curso.' } : null;

    function abrirFormulario(item) {
        const modal = pagina.querySelector('[data-modal-proyecto]'); const form = modal.querySelector('[data-form-proyecto]'); const p = item?.proyecto;
        form.reset(); UI.limpiarErrores(form); form.codigo.value = p?.codigo || ''; form.nombre.value = p?.nombre || ''; form.codigoDepartamento.value = p?.codigoDepartamento || ''; form.codigoJefeProyecto.value = p?.codigoJefeProyecto || ''; form.fechaInicio.value = UI.fechaInput(p?.fechaInicio || new Date().toISOString()); form.fechaFinPlanificada.value = UI.fechaInput(p?.fechaFinPlanificada); form.prioridad.value = p?.prioridad || 'MEDIA'; form.presupuesto.value = p?.presupuesto ?? 0; form.descripcion.value = p?.descripcion || ''; form.observacion.value = p?.observacion || '';
        modal.querySelector('[data-titulo-modal-proyecto]').textContent = p ? `Editar ${p.nombre}` : 'Nuevo proyecto'; UI.abrir(modal);
    }

    async function guardar(evento) {
        evento.preventDefault(); const form = evento.currentTarget; const codigo = form.codigo.value; const boton = form.querySelector('[data-guardar-proyecto]'); UI.limpiarErrores(form);
        if (!UI.validarFormulario(form)) { UI.toast('Revise los campos señalados.', 'error'); return; }
        const dto = { nombre: form.nombre.value, codigoDepartamento: form.codigoDepartamento.value, codigoJefeProyecto: form.codigoJefeProyecto.value, fechaInicio: form.fechaInicio.value, fechaFinPlanificada: form.fechaFinPlanificada.value, prioridad: form.prioridad.value, presupuesto: Number(form.presupuesto.value || 0), descripcion: form.descripcion.value, observacion: form.observacion.value };
        UI.bloquear(boton, true);
        try { await UI.api(`/api/v1/proyectos${codigo ? `/${encodeURIComponent(codigo)}` : ''}`, { method: codigo ? 'PUT' : 'POST', body: JSON.stringify(dto) }); UI.cerrar(form.closest('dialog')); UI.toast(codigo ? 'Proyecto actualizado.' : 'Proyecto creado en estado Planificado.'); await cargar(); }
        catch (error) { UI.mostrarErrores(form, error.errores); UI.toast(error.message, 'error'); }
        finally { UI.bloquear(boton, false); }
    }

    function ver(item) {
        const p = item.proyecto; const modal = pagina.querySelector('[data-modal-detalle-proyecto]');
        modal.querySelector('[data-detalle-titulo]').textContent = p.nombre;
        const campos = [['Departamento', item.nombreDepartamento], ['Jefe del proyecto', item.nombreJefeProyecto], ['Estado', UI.textoEstado(p.estado)], ['Prioridad', UI.textoPrioridad(p.prioridad)], ['Fecha inicial', UI.fecha(p.fechaInicio)], ['Fin planificado', UI.fecha(p.fechaFinPlanificada)], ['Fin real', UI.fecha(p.fechaFinReal)], ['Avance', `${UI.numero(p.avance)} %`], ['Presupuesto', UI.moneda(p.presupuesto)], ['Saldo disponible', UI.moneda(item.saldoDisponible)], ['Descripción', p.descripcion || 'Sin descripción'], ['Observación', p.observacion || 'Sin observación']];
        modal.querySelector('[data-detalle-proyecto]').innerHTML = campos.map(([k, v]) => `<div><span>${UI.escapar(k)}</span><strong>${UI.escapar(v)}</strong></div>`).join(''); UI.abrir(modal);
    }

    function abrirEstado(item, cancelacion = false) {
        const p = item.proyecto; const modal = pagina.querySelector('[data-modal-estado]'); const form = modal.querySelector('[data-form-estado]'); const cambio = cancelacion ? { destino: 'CANCELADO', accion: 'Cancelar', explicacion: 'La cancelación cerrará el proyecto y sus asignaciones activas. Indique el motivo.' } : transicion(p);
        form.reset(); form.dataset.codigo = p.codigo; form.dataset.destino = cambio.destino; form.dataset.cancelacion = cancelacion; modal.querySelector('[data-estado-titulo]').textContent = `${cambio.accion} proyecto`; modal.querySelector('[data-estado-explicacion]').textContent = cambio.explicacion; form.observacion.required = cancelacion; UI.abrir(modal);
    }

    async function guardarEstado(evento) {
        evento.preventDefault(); const form = evento.currentTarget; const boton = form.querySelector('[type="submit"]'); UI.bloquear(boton, true, 'Procesando...');
        try { const url = form.dataset.cancelacion === 'true' ? `/api/v1/proyectos/${form.dataset.codigo}/cancelar` : `/api/v1/proyectos/${form.dataset.codigo}/estado`; const cuerpo = form.dataset.cancelacion === 'true' ? { motivo: form.observacion.value } : { estado: form.dataset.destino, observacion: form.observacion.value }; await UI.api(url, { method: form.dataset.cancelacion === 'true' ? 'POST' : 'PATCH', body: JSON.stringify(cuerpo) }); UI.cerrar(form.closest('dialog')); UI.toast('Estado actualizado.'); await cargar(); }
        catch (error) { UI.toast(error.message, 'error'); }
        finally { UI.bloquear(boton, false); }
    }

    async function abrirCierre(item) {
        const modal = pagina.querySelector('[data-modal-cierre]'); const form = modal.querySelector('[data-form-cierre]'); form.reset(); form.dataset.codigo = item.proyecto.codigo; form.fechaFinReal.value = UI.fechaInput(new Date().toISOString()); const zona = modal.querySelector('[data-validacion-cierre]'); zona.innerHTML = '<p>Validando requisitos...</p>'; UI.abrir(modal);
        try { const v = await UI.api(`/api/v1/proyectos/${item.proyecto.codigo}/finalizacion/validar`); zona.innerHTML = v.puedeFinalizar ? '<p class="validacion-correcta">El proyecto cumple los requisitos del plan. Complete la observación final.</p>' : `<p><strong>No se puede finalizar todavía:</strong></p><ul>${v.pendientes.map(x => `<li>${UI.escapar(x)}</li>`).join('')}</ul>`; form.querySelector('[data-confirmar-finalizacion]').disabled = !v.puedeFinalizar; }
        catch (error) { zona.innerHTML = `<p class="texto-peligro">${UI.escapar(error.message)}</p>`; form.querySelector('[data-confirmar-finalizacion]').disabled = true; }
    }

    async function finalizar(evento) { evento.preventDefault(); const form = evento.currentTarget; const boton = form.querySelector('[data-confirmar-finalizacion]'); UI.bloquear(boton, true, 'Finalizando...'); try { await UI.api(`/api/v1/proyectos/${form.dataset.codigo}/finalizar`, { method: 'POST', body: JSON.stringify({ fechaFinReal: form.fechaFinReal.value, observacionFinal: form.observacionFinal.value }) }); UI.cerrar(form.closest('dialog')); UI.toast('Proyecto finalizado correctamente.'); await cargar(); } catch (error) { UI.toast(error.message, 'error'); } finally { UI.bloquear(boton, false); } }

    pagina.querySelector('[data-form-proyecto]')?.addEventListener('submit', guardar);
    pagina.querySelector('[data-form-estado]')?.addEventListener('submit', guardarEstado);
    pagina.querySelector('[data-form-cierre]')?.addEventListener('submit', finalizar);
    pagina.querySelector('[data-nuevo-proyecto]')?.addEventListener('click', () => abrirFormulario());
    pagina.querySelector('[data-recargar-proyectos]').addEventListener('click', cargar);
    pagina.querySelector('[data-incluir-inactivos]').addEventListener('change', cargar);
    pagina.querySelectorAll('[data-filtro-texto],[data-filtro-estado],[data-filtro-prioridad],[data-filtro-departamento]').forEach(x => x.addEventListener(x.tagName === 'INPUT' ? 'input' : 'change', () => { paginaActual = 1; pintar(); }));
    pagina.querySelector('[data-pagina-anterior]').addEventListener('click', () => { paginaActual--; pintar(); }); pagina.querySelector('[data-pagina-siguiente]').addEventListener('click', () => { paginaActual++; pintar(); });
    tabla.addEventListener('click', evento => {
        const boton = evento.target.closest('button,[data-equipo-proyecto]'); if (!boton) return; const codigo = boton.dataset.verProyecto || boton.dataset.editarProyecto || boton.dataset.equipoProyecto || boton.dataset.estadoProyecto || boton.dataset.finalizarProyecto || boton.dataset.cancelarProyecto || boton.dataset.activoProyecto; const item = buscar(codigo); if (!item) return;
        if (boton.dataset.verProyecto) ver(item); else if (boton.dataset.editarProyecto) abrirFormulario(item); else if (boton.dataset.equipoProyecto) window.EquipoProyecto.abrir(codigo, item.proyecto); else if (boton.dataset.estadoProyecto) abrirEstado(item); else if (boton.dataset.cancelarProyecto) abrirEstado(item, true); else if (boton.dataset.finalizarProyecto) abrirCierre(item); else if (boton.dataset.activoProyecto) UI.confirmar(boton.dataset.activoDestino === 'true' ? 'Reactivar proyecto' : 'Inactivar proyecto', `¿Desea ${boton.dataset.activoDestino === 'true' ? 'reactivar' : 'inactivar'} el proyecto ${item.proyecto.nombre}?`, async () => { await UI.api(`/api/v1/proyectos/${codigo}/activo`, { method: 'PATCH', body: JSON.stringify({ activo: boton.dataset.activoDestino === 'true', observacion: '' }) }); UI.toast(boton.dataset.activoDestino === 'true' ? 'Proyecto reactivado.' : 'Proyecto inactivado.'); await cargar(); });
    });
    cargar();
});
