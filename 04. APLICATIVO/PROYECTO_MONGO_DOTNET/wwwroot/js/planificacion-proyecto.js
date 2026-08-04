document.addEventListener('DOMContentLoaded', () => {
    'use strict';
    const UI = window.ProyectosUI;
    const listaPagina = document.querySelector('[data-planificacion-lista]');
    if (listaPagina) { iniciarLista(listaPagina); return; }
    const pagina = document.querySelector('[data-planificacion-proyecto]');
    if (!pagina) return;
    const codigo = pagina.dataset.codigoProyecto;
    let plan = null;

    async function cargar() {
        try {
            plan = await UI.api(`/api/v1/proyectos/${encodeURIComponent(codigo)}/planificacion`);
            pintarContexto(); pintarResumen(); pintarPlanTrabajo();
            window.CronogramaProyecto?.render(plan, abrirDetalle);
            window.CalendarioProyecto?.render(plan, abrirDetalle);
            window.EquipoProyecto?.pintarEnPlan(codigo, plan.proyecto, pagina.querySelector('[data-equipo-plan]'));
        } catch (error) { UI.toast(error.message, 'error'); pagina.querySelector('[data-contexto-nombre]').textContent = 'No fue posible cargar el proyecto'; }
    }

    function pintarContexto() {
        const p = plan.proyecto;
        pagina.querySelector('[data-contexto-nombre]').textContent = p.nombre;
        pagina.querySelector('[data-contexto-fechas]').textContent = `${UI.fecha(p.fechaInicio)} – ${UI.fecha(p.fechaFinPlanificada)}`;
        const jefe = plan.equipo.find(x => x.asignacion.codigoEmpleado === p.codigoJefeProyecto)?.nombreEmpleado || pagina.dataset.nombreJefe || 'Sin jefe asignado';
        pagina.querySelector('[data-contexto-datos]').innerHTML = `<span class="estado-proyecto estado-${p.estado.toLowerCase()}">${UI.textoEstado(p.estado)}</span><span><small>Jefe</small><strong>${UI.escapar(jefe)}</strong></span><span><small>Prioridad</small><strong>${UI.textoPrioridad(p.prioridad)}</strong></span><span><small>Presupuesto</small><strong>${UI.moneda(p.presupuesto)}</strong></span>`;
        const acciones = [];
        const trans = p.estado === 'PLANIFICADO' ? ['EN_CURSO', 'Iniciar proyecto'] : p.estado === 'EN_CURSO' ? ['PAUSADO', 'Pausar proyecto'] : p.estado === 'PAUSADO' ? ['EN_CURSO', 'Reanudar proyecto'] : null;
        if (pagina.dataset.puedeEstado === 'true' && trans) acciones.push(`<button type="button" class="btn-crud celeste" data-contexto-estado="${trans[0]}">${trans[1]}</button>`);
        if (pagina.dataset.puedeFinalizar === 'true' && ['EN_CURSO', 'PAUSADO'].includes(p.estado)) acciones.push('<button type="button" class="btn-crud verde" data-contexto-finalizar>Finalizar</button>');
        if (pagina.dataset.puedeFinalizar === 'true' && !['FINALIZADO', 'CANCELADO'].includes(p.estado)) acciones.push('<button type="button" class="btn-crud" data-contexto-cancelar>Cancelar</button>');
        pagina.querySelector('[data-contexto-acciones]').innerHTML = acciones.join('');
    }

    function pintarResumen() {
        const i = plan.indicadores;
        const diferencia = i.avanceReal - i.avancePlanificado;
        const cronograma = diferencia < -5 ? ['Atrasado', `${UI.numero(Math.abs(diferencia))} % por debajo de lo planificado`, 'alerta'] : diferencia > 5 ? ['Adelantado', `${UI.numero(diferencia)} % por encima de lo planificado`, 'bien'] : ['En fecha', 'El avance está dentro del margen esperado', 'normal'];
        const pendientes = plan.entregables.filter(x => !['ENTREGADO', 'JUSTIFICADO'].includes(x.estado)).sort((a, b) => String(a.fechaCompromiso).localeCompare(String(b.fechaCompromiso)));
        pagina.querySelector('[data-resumen-proyecto]').innerHTML = `<article class="resumen-indicador principal"><span>Avance general</span><strong>${UI.numero(i.avanceReal)} %</strong><p>${i.tareasCompletadas} de ${i.tareasTotales} tareas completadas</p><div class="barra-resumen"><i style="width:${Math.min(100, i.avanceReal)}%"></i></div></article><article class="resumen-indicador ${cronograma[2]}"><span>Estado del cronograma</span><strong>${cronograma[0]}</strong><p>${cronograma[1]}</p></article><article class="resumen-indicador"><span>Cobertura del plan</span><strong>${UI.numero(i.pesoPlanificado)} %</strong><p>${i.planCompleto ? 'Plan completo' : `Falta distribuir ${UI.numero(100 - i.pesoPlanificado)} %`}</p></article><article class="resumen-indicador resumen-presupuesto"><span>Presupuesto</span><strong>${UI.moneda(i.presupuesto)}</strong><dl><div><dt>Costo laboral planificado</dt><dd>${UI.moneda(i.costoPlanificadoLaboral)}</dd></div><div><dt>Gastos aprobados</dt><dd>${UI.moneda(i.gastosAprobados)}</dd></div><div title="Estimación basada en horas planificadas, avance y gastos aprobados."><dt>Costo comprometido estimado</dt><dd>${UI.moneda(i.costoComprometidoEstimado)}</dd></div><div><dt>Saldo disponible</dt><dd>${UI.moneda(i.saldoDisponible)}</dd></div></dl></article><article class="resumen-indicador"><span>Entregables</span><strong>${i.entregablesEntregados} de ${i.entregablesTotales}</strong><p>${pendientes.length ? `Próximo: ${UI.escapar(pendientes[0].nombre)} · ${UI.fecha(pendientes[0].fechaCompromiso)}` : 'No hay entregables pendientes'}</p></article>`;
    }

    function pintarPlanTrabajo() {
        const cont = pagina.querySelector('[data-plan-trabajo]');
        if (!plan.fases.length) { cont.innerHTML = '<p class="tabla-vacia">El proyecto todavía no tiene fases. Cree la primera fase para comenzar.</p>'; return; }
        cont.innerHTML = plan.fases.map(f => `<details class="plan-fase-acordeon" open><summary><span><small>Fase ${f.fase.orden}</small><strong>${UI.escapar(f.fase.nombre)}</strong><em>${UI.fecha(f.fase.fechaInicio)} – ${UI.fecha(f.fase.fechaFin)} · ${UI.numero(f.avance)} %</em></span><span class="acciones-compactas"><button type="button" data-editar-plan="fase:${f.fase.codigoFase}">Editar</button><button type="button" class="peligro" data-eliminar-plan="fase:${f.fase.codigoFase}">Eliminar</button></span></summary><div class="plan-fase-contenido">${(f.entregables || []).map(entregable).join('')}${f.actividades.length ? f.actividades.map(a => actividad(f, a)).join('') : '<p class="plan-vacio-interno">No hay actividades en esta fase.</p>'}</div></details>`).join('');
    }

    function actividad(fase, a) {
        return `<section class="plan-actividad-tarjeta"><header><div><small>Actividad ${a.actividad.orden}</small><h4>${UI.escapar(a.actividad.nombre)}</h4><p>${UI.fecha(a.actividad.fechaInicio)} – ${UI.fecha(a.actividad.fechaFin)} · ${UI.textoEstado(a.actividad.estado)} · ${UI.numero(a.avance)} %</p></div><div class="acciones-compactas"><button type="button" data-editar-plan="actividad:${a.actividad.codigoActividad}">Editar</button><button type="button" class="peligro" data-eliminar-plan="actividad:${a.actividad.codigoActividad}">Eliminar</button></div></header>${(a.entregables || []).map(entregable).join('')}<div class="plan-tareas">${a.tareas.length ? a.tareas.map(t => tarea(t)).join('') : '<p class="plan-vacio-interno">No hay tareas en esta actividad.</p>'}</div></section>`;
    }
    function tarea(t) {
        const x = t.tarea; const siguiente = x.estado === 'PENDIENTE' ? 'EN_PROCESO' : x.estado === 'EN_PROCESO' ? 'COMPLETADA' : '';
        return `<article class="plan-tarea-tarjeta ${t.bloqueada ? 'bloqueada' : ''}"><div><small>Tarea${t.bloqueada ? ' · Bloqueada por predecesora' : ''}</small><strong>${UI.escapar(x.nombre)}</strong><span>${UI.fecha(x.fechaInicio)} – ${UI.fecha(x.fechaFin)} · ${UI.numero(x.horasPlanificadas)} h · Peso ${UI.numero(x.peso)} %</span>${t.dependencias.length ? `<em>Depende de: ${t.dependencias.map(d => UI.escapar(d.nombrePredecesora)).join(', ')}</em>` : ''}</div><span class="estado-proyecto">${UI.textoEstado(x.estado)}</span><div class="acciones-compactas"><button type="button" data-detalle-plan="tarea:${x.codigoTarea}">Ver</button><button type="button" data-editar-plan="tarea:${x.codigoTarea}">Editar</button>${siguiente ? `<button type="button" data-estado-tarea="${x.codigoTarea}" data-destino-tarea="${siguiente}">${siguiente === 'COMPLETADA' ? 'Completar' : 'Iniciar'}</button>` : ''}<button type="button" class="peligro" data-eliminar-plan="tarea:${x.codigoTarea}">Eliminar</button></div>${t.entregables.map(entregable).join('')}</article>`;
    }
    function entregable(e) {
        return `<div class="plan-entregable"><span><small>Entregable</small><strong>${UI.escapar(e.nombre)}</strong><em>${UI.fecha(e.fechaCompromiso)} · ${UI.textoEstado(e.estado)}</em></span><div class="acciones-compactas"><button type="button" data-editar-plan="entregable:${e.codigoEntregable}">Editar</button>${e.estado === 'PENDIENTE' ? `<button type="button" data-estado-entregable="${e.codigoEntregable}" data-destino-entregable="ENTREGADO">Entregar</button><button type="button" data-estado-entregable="${e.codigoEntregable}" data-destino-entregable="JUSTIFICADO">Justificar</button>` : ''}<button type="button" class="peligro" data-eliminar-plan="entregable:${e.codigoEntregable}">Eliminar</button></div></div>`;
    }

    function responsables() { return plan.equipo.filter(x => x.asignacion.activo).map(x => `<option value="${UI.escapar(x.asignacion.codigoEmpleado)}">${UI.escapar(x.nombreEmpleado)} · ${UI.numero(x.disponibilidadRestante)} % restante</option>`).join(''); }
    function fasesOpciones() { return plan.fases.map(x => `<option value="${UI.escapar(x.fase.codigoFase)}">${UI.escapar(x.fase.nombre)}</option>`).join(''); }
    function actividadesOpciones() { return plan.fases.flatMap(f => f.actividades).map(x => `<option value="${UI.escapar(x.actividad.codigoActividad)}">${UI.escapar(x.actividad.nombre)}</option>`).join(''); }
    function tareasOpciones(excluir = '') { return plan.fases.flatMap(f => f.actividades).flatMap(a => a.tareas).filter(x => x.tarea.codigoTarea !== excluir).map(x => `<option value="${UI.escapar(x.tarea.codigoTarea)}">${UI.escapar(x.tarea.nombre)}</option>`).join(''); }

    function localizar(tipo, codigoElemento) {
        if (tipo === 'fase') return plan.fases.find(x => x.fase.codigoFase === codigoElemento)?.fase;
        if (tipo === 'actividad') return plan.fases.flatMap(x => x.actividades).find(x => x.actividad.codigoActividad === codigoElemento)?.actividad;
        if (tipo === 'tarea') return plan.fases.flatMap(x => x.actividades).flatMap(x => x.tareas).find(x => x.tarea.codigoTarea === codigoElemento);
        return plan.entregables.find(x => x.codigoEntregable === codigoElemento);
    }

    function abrirFormulario(tipo, codigoElemento = '') {
        const modal = pagina.querySelector('[data-modal-plan]'); const form = modal.querySelector('[data-form-plan]'); const campos = modal.querySelector('[data-campos-plan]'); const dato = localizar(tipo, codigoElemento); const valor = (obj, propiedad) => obj?.[propiedad] ?? '';
        form.reset(); form.tipo.value = tipo; form.codigo.value = codigoElemento; UI.limpiarErrores(form);
        const genero = ['fase', 'actividad', 'tarea'].includes(tipo) ? 'Nueva' : 'Nuevo';
        modal.querySelector('[data-plan-modal-titulo]').textContent = `${codigoElemento ? 'Editar' : genero} ${tipo}`;
        if (tipo === 'fase') { const x = dato; campos.innerHTML = campo('Nombre', 'nombre', 'text', valor(x, 'nombre'), true) + campo('Orden', 'orden', 'number', valor(x, 'orden') || 1, true) + campo('Fecha inicial', 'fechaInicio', 'date', UI.fechaInput(valor(x, 'fechaInicio')), true) + campo('Fecha final', 'fechaFin', 'date', UI.fechaInput(valor(x, 'fechaFin')), true) + area('Descripción', 'descripcion', valor(x, 'descripcion')); }
        if (tipo === 'actividad') { const x = dato; campos.innerHTML = seleccion('Fase', 'codigoFase', fasesOpciones(), valor(x, 'codigoFase')) + seleccion('Responsable', 'codigoResponsable', responsables(), valor(x, 'codigoResponsable')) + campo('Nombre', 'nombre', 'text', valor(x, 'nombre'), true) + campo('Orden', 'orden', 'number', valor(x, 'orden') || 1, true) + campo('Fecha inicial', 'fechaInicio', 'date', UI.fechaInput(valor(x, 'fechaInicio')), true) + campo('Fecha final', 'fechaFin', 'date', UI.fechaInput(valor(x, 'fechaFin')), true) + area('Descripción', 'descripcion', valor(x, 'descripcion')); }
        if (tipo === 'tarea') { const x = dato?.tarea; const deps = dato?.dependencias?.map(d => d.codigoPredecesora) || []; campos.innerHTML = seleccion('Actividad', 'codigoActividad', actividadesOpciones(), valor(x, 'codigoActividad')) + seleccion('Responsable', 'codigoResponsable', responsables(), valor(x, 'codigoResponsable')) + campo('Nombre', 'nombre', 'text', valor(x, 'nombre'), true) + campo('Peso del proyecto (%)', 'peso', 'number', valor(x, 'peso'), true, '0.01') + campo('Horas estimadas', 'horasPlanificadas', 'number', valor(x, 'horasPlanificadas') || 0, true, '0.25') + campo('Fecha inicial', 'fechaInicio', 'date', UI.fechaInput(valor(x, 'fechaInicio')), true) + campo('Fecha final', 'fechaFin', 'date', UI.fechaInput(valor(x, 'fechaFin')), true) + area('Descripción', 'descripcion', valor(x, 'descripcion')) + `<div class="grupo-campo campo-ancho"><label for="plan-dependencias">Predecesoras válidas</label><select id="plan-dependencias" name="dependencias" multiple size="5">${tareasOpciones(codigoElemento)}</select><small>Use Ctrl o Cmd para seleccionar varias. Se aplicará relación Fin–Inicio.</small></div>`; setTimeout(() => [...form.dependencias.options].forEach(o => o.selected = deps.includes(o.value)), 0); }
        if (tipo === 'entregable') { const x = dato; campos.innerHTML = seleccion('Relacionar con', 'tipoRelacion', '<option value="FASE">Fase</option><option value="ACTIVIDAD">Actividad</option><option value="TAREA">Tarea</option>', valor(x, 'tipoRelacion') || 'TAREA') + `<div class="grupo-campo"><label for="plan-codigoRelacion">Elemento relacionado</label><select id="plan-codigoRelacion" name="codigoRelacion" required></select></div>` + campo('Nombre', 'nombre', 'text', valor(x, 'nombre'), true) + campo('Fecha compromiso', 'fechaCompromiso', 'date', UI.fechaInput(valor(x, 'fechaCompromiso')), true) + area('Descripción', 'descripcion', valor(x, 'descripcion')) + campo('Evidencia o ubicación', 'evidencia', 'text', valor(x, 'evidencia')) + area('Observación o justificación', 'observacion', valor(x, 'observacion')); const actualizar = () => { form.codigoRelacion.innerHTML = form.tipoRelacion.value === 'FASE' ? fasesOpciones() : form.tipoRelacion.value === 'ACTIVIDAD' ? actividadesOpciones() : tareasOpciones(); const seleccionActual = valor(x, 'codigoRelacion') || valor(x, 'codigoTarea'); if (seleccionActual) form.codigoRelacion.value = seleccionActual; else form.codigoRelacion.selectedIndex = form.codigoRelacion.options.length ? 0 : -1; }; form.tipoRelacion.addEventListener('change', actualizar); actualizar(); }
        UI.abrir(modal);
    }
    const campo = (etiqueta, nombre, tipo, valor, requerido = false, paso = '') => `<div class="grupo-campo"><label for="plan-${nombre}">${etiqueta}</label><input id="plan-${nombre}" name="${nombre}" type="${tipo}" value="${UI.escapar(valor)}" ${requerido ? 'required' : ''} ${paso ? `step="${paso}" min="0"` : ''}><span class="campo-error" data-error="${nombre}"></span></div>`;
    const area = (etiqueta, nombre, valor) => `<div class="grupo-campo campo-ancho"><label for="plan-${nombre}">${etiqueta}</label><textarea id="plan-${nombre}" name="${nombre}" rows="3">${UI.escapar(valor)}</textarea><span class="campo-error" data-error="${nombre}"></span></div>`;
    const seleccion = (etiqueta, nombre, opciones, valor) => `<div class="grupo-campo"><label for="plan-${nombre}">${etiqueta}</label><select id="plan-${nombre}" name="${nombre}" required><option value="">Seleccione...</option>${opciones}</select><span class="campo-error" data-error="${nombre}"></span></div>`.replace(`value="${UI.escapar(valor)}"`, `value="${UI.escapar(valor)}" selected`);

    async function guardarPlan(e) {
        e.preventDefault(); const form = e.currentTarget; const tipo = form.tipo.value; const codigoElemento = form.codigo.value; const boton = form.querySelector('[type="submit"]'); let dto, url, method = codigoElemento ? 'PUT' : 'POST'; UI.limpiarErrores(form);
        if (!UI.validarFormulario(form)) { UI.toast('Revise los campos señalados.', 'error'); return; }
        if (tipo === 'fase') { dto = { nombre: form.nombre.value, descripcion: form.descripcion.value, orden: Number(form.orden.value), fechaInicio: form.fechaInicio.value, fechaFin: form.fechaFin.value }; url = `fases${codigoElemento ? `/${codigoElemento}` : ''}`; }
        if (tipo === 'actividad') { dto = { nombre: form.nombre.value, descripcion: form.descripcion.value, codigoResponsable: form.codigoResponsable.value, orden: Number(form.orden.value), fechaInicio: form.fechaInicio.value, fechaFin: form.fechaFin.value }; url = codigoElemento ? `actividades/${codigoElemento}` : `fases/${form.codigoFase.value}/actividades`; }
        if (tipo === 'tarea') { dto = { codigoActividad: form.codigoActividad.value, nombre: form.nombre.value, descripcion: form.descripcion.value, codigoResponsable: form.codigoResponsable.value, fechaInicio: form.fechaInicio.value, fechaFin: form.fechaFin.value, peso: Number(form.peso.value), horasPlanificadas: Number(form.horasPlanificadas.value), dependencias: [...form.dependencias.selectedOptions].map(o => ({ codigoPredecesora: o.value, tipoDependencia: 'FS', desfaseDias: 0 })) }; url = codigoElemento ? `tareas/${codigoElemento}` : `actividades/${form.codigoActividad.value}/tareas`; }
        if (tipo === 'entregable') { dto = { tipoRelacion: form.tipoRelacion.value, codigoRelacion: form.codigoRelacion.value, nombre: form.nombre.value, descripcion: form.descripcion.value, fechaCompromiso: form.fechaCompromiso.value, evidencia: form.evidencia.value, observacion: form.observacion.value }; url = `entregables${codigoElemento ? `/${codigoElemento}` : ''}`; }
        UI.bloquear(boton, true);
        try { await UI.api(`/api/v1/proyectos/${encodeURIComponent(codigo)}/planificacion/${url}`, { method, body: JSON.stringify(dto) }); UI.cerrar(form.closest('dialog')); UI.toast('Plan de trabajo actualizado.'); await cargar(); }
        catch (error) { UI.mostrarErrores(form, error.errores); UI.toast(error.message, 'error'); }
        finally { UI.bloquear(boton, false); }
    }

    async function eliminar(tipo, cod) { const plural = { fase: 'fases', actividad: 'actividades', tarea: 'tareas', entregable: 'entregables' }[tipo]; await UI.api(`/api/v1/proyectos/${codigo}/planificacion/${plural}/${cod}`, { method: 'DELETE' }); UI.toast(`${tipo[0].toUpperCase() + tipo.slice(1)} eliminado.`); await cargar(); }
    async function cambiarEstadoTarea(cod, estado) { await UI.api(`/api/v1/proyectos/${codigo}/planificacion/tareas/${cod}/estado`, { method: 'PATCH', body: JSON.stringify({ estado }) }); UI.toast('Estado de la tarea actualizado.'); await cargar(); }
    async function cambiarEstadoEntregable(cod, estado) { const e = plan.entregables.find(x => x.codigoEntregable === cod); if (estado === 'JUSTIFICADO' && !e.observacion) throw new Error('Edite el entregable e ingrese una justificación antes de marcarlo como Justificado.'); await UI.api(`/api/v1/proyectos/${codigo}/planificacion/entregables/${cod}/estado`, { method: 'PATCH', body: JSON.stringify({ estado, evidencia: e.evidencia || '', observacion: e.observacion || '' }) }); UI.toast('Estado del entregable actualizado.'); await cargar(); }

    function abrirDetalle(item) {
        const modal = pagina.querySelector('[data-modal-elemento]');
        const etiquetas = { tipo: 'Tipo', nivel: 'Nivel', codigoResponsable: 'Responsable', responsable: 'Responsable', fase: 'Fase', inicio: 'Fecha inicial', fin: 'Fecha final', fechaInicio: 'Fecha inicial', fechaFin: 'Fecha final', fechaCompromiso: 'Fecha compromiso', estado: 'Estado', avance: 'Avance (%)', peso: 'Peso (%)', horasPlanificadas: 'Horas estimadas', bloqueada: 'Bloqueada' };
        const valorDetalle = (clave, valor) => {
            if (typeof valor === 'boolean') return valor ? 'Sí' : 'No';
            if (/fecha|inicio|fin/i.test(clave)) return UI.fecha(valor);
            if (clave === 'estado' || clave === 'tipo') return UI.textoEstado(valor);
            if (clave === 'responsable' || clave === 'codigoResponsable') return plan.equipo.find(x => x.asignacion.codigoEmpleado === valor)?.nombreEmpleado || 'Sin responsable';
            if (clave === 'fase') return plan.fases.find(x => x.fase.codigoFase === valor)?.fase.nombre || 'Sin fase';
            return valor ?? '';
        };
        modal.querySelector('[data-elemento-titulo]').textContent = item.nombre;
        const campos = Object.entries(item).filter(([k, v]) => !['nombre', 'dependencias'].includes(k) && (!/^codigo/i.test(k) || k === 'codigoResponsable') && typeof v !== 'object');
        modal.querySelector('[data-elemento-detalle]').innerHTML = campos.map(([k, v]) => `<div><span>${UI.escapar(etiquetas[k] || k.replace(/([A-Z])/g, ' $1').replace(/^./, x => x.toUpperCase()))}</span><strong>${UI.escapar(valorDetalle(k, v))}</strong></div>`).join('');
        UI.abrir(modal);
    }

    pagina.querySelector('[data-form-plan]').addEventListener('submit', guardarPlan);
    pagina.querySelectorAll('[data-tab-plan]').forEach(b => b.addEventListener('click', () => { pagina.querySelectorAll('[data-tab-plan]').forEach(x => x.classList.toggle('activo', x === b)); pagina.querySelectorAll('[data-panel-plan]').forEach(x => x.hidden = x.dataset.panelPlan !== b.dataset.tabPlan); if (b.dataset.tabPlan === 'cronograma') window.CronogramaProyecto?.render(plan, abrirDetalle); if (b.dataset.tabPlan === 'calendario') window.CalendarioProyecto?.render(plan, abrirDetalle); }));
    pagina.addEventListener('click', e => {
        const nuevo = e.target.closest('[data-nuevo-plan]'); if (nuevo) abrirFormulario(nuevo.dataset.nuevoPlan);
        const editar = e.target.closest('[data-editar-plan]'); if (editar) { const [t, c] = editar.dataset.editarPlan.split(':'); e.preventDefault(); abrirFormulario(t, c); }
        const borrar = e.target.closest('[data-eliminar-plan]'); if (borrar) { const [t, c] = borrar.dataset.eliminarPlan.split(':'); e.preventDefault(); UI.confirmar(`Eliminar ${t}`, 'La eliminación solo se permitirá si no existen elementos dependientes activos.', () => eliminar(t, c), 'Eliminar'); }
        const estadoT = e.target.closest('[data-estado-tarea]'); if (estadoT) UI.confirmar('Actualizar tarea', `¿Desea cambiar la tarea a ${UI.textoEstado(estadoT.dataset.destinoTarea)}?`, () => cambiarEstadoTarea(estadoT.dataset.estadoTarea, estadoT.dataset.destinoTarea));
        const estadoE = e.target.closest('[data-estado-entregable]'); if (estadoE) UI.confirmar('Actualizar entregable', `¿Desea marcar el entregable como ${UI.textoEstado(estadoE.dataset.destinoEntregable)}?`, () => cambiarEstadoEntregable(estadoE.dataset.estadoEntregable, estadoE.dataset.destinoEntregable));
        const ver = e.target.closest('[data-detalle-plan]'); if (ver) { const [t, c] = ver.dataset.detallePlan.split(':'); const dato = localizar(t, c); abrirDetalle(t === 'tarea' ? { tipo: 'Tarea', ...dato.tarea, bloqueada: dato.bloqueada } : dato); }
        const cambio = e.target.closest('[data-contexto-estado]'); if (cambio) abrirEstadoContexto(cambio.dataset.contextoEstado, false);
        if (e.target.closest('[data-contexto-cancelar]')) abrirEstadoContexto('CANCELADO', true);
        if (e.target.closest('[data-contexto-finalizar]')) abrirCierreContexto();
    });

    function abrirEstadoContexto(destino, cancelar) { const modal = pagina.querySelector('[data-modal-estado]'); const form = modal.querySelector('[data-form-estado]'); form.reset(); form.dataset.destino = destino; form.dataset.cancelacion = cancelar; form.observacion.required = cancelar; modal.querySelector('[data-estado-titulo]').textContent = cancelar ? 'Cancelar proyecto' : destino === 'PAUSADO' ? 'Pausar proyecto' : plan.proyecto.estado === 'PAUSADO' ? 'Reanudar proyecto' : 'Iniciar proyecto'; modal.querySelector('[data-estado-explicacion]').textContent = cancelar ? 'Indique el motivo. La cancelación cerrará las asignaciones activas.' : 'El cambio quedará registrado en el seguimiento del proyecto.'; UI.abrir(modal); }
    pagina.querySelector('[data-form-estado]').addEventListener('submit', async e => { e.preventDefault(); const f = e.currentTarget; try { await UI.api(`/api/v1/proyectos/${codigo}/${f.dataset.cancelacion === 'true' ? 'cancelar' : 'estado'}`, { method: f.dataset.cancelacion === 'true' ? 'POST' : 'PATCH', body: JSON.stringify(f.dataset.cancelacion === 'true' ? { motivo: f.observacion.value } : { estado: f.dataset.destino, observacion: f.observacion.value }) }); UI.cerrar(f.closest('dialog')); UI.toast('Estado actualizado.'); await cargar(); } catch (error) { UI.toast(error.message, 'error'); } });
    async function abrirCierreContexto() { const modal = pagina.querySelector('[data-modal-cierre]'); const form = modal.querySelector('[data-form-cierre]'); form.reset(); form.fechaFinReal.value = UI.fechaInput(new Date().toISOString()); const z = modal.querySelector('[data-validacion-cierre]'); z.textContent = 'Validando requisitos...'; UI.abrir(modal); try { const v = await UI.api(`/api/v1/proyectos/${codigo}/finalizacion/validar`); z.innerHTML = v.puedeFinalizar ? '<p class="validacion-correcta">Todos los requisitos se cumplen.</p>' : `<p><strong>Requisitos pendientes:</strong></p><ul>${v.pendientes.map(x => `<li>${UI.escapar(x)}</li>`).join('')}</ul>`; form.querySelector('[data-confirmar-finalizacion]').disabled = !v.puedeFinalizar; } catch (error) { UI.toast(error.message, 'error'); } }
    pagina.querySelector('[data-form-cierre]').addEventListener('submit', async e => { e.preventDefault(); const f = e.currentTarget; try { await UI.api(`/api/v1/proyectos/${codigo}/finalizar`, { method: 'POST', body: JSON.stringify({ fechaFinReal: f.fechaFinReal.value, observacionFinal: f.observacionFinal.value }) }); UI.cerrar(f.closest('dialog')); UI.toast('Proyecto finalizado.'); await cargar(); } catch (error) { UI.toast(error.message, 'error'); } });
    document.addEventListener('equipo-proyecto-actualizado', cargar);
    window.PlanificacionApp = { abrirDetalle, obtenerPlan: () => plan };
    cargar();

    async function iniciarLista(root) {
        const cuerpo = root.querySelector('[data-lista-plan]'); let datos = [];
        async function cargarLista() { try { datos = await UI.api('/api/v1/proyectos'); pintarLista(); } catch (error) { cuerpo.innerHTML = `<tr><td colspan="6" class="tabla-vacia">${UI.escapar(error.message)}</td></tr>`; } }
        function pintarLista() { const texto = root.querySelector('[data-buscar-plan]').value.toLowerCase(); const xs = datos.filter(x => !['FINALIZADO', 'CANCELADO'].includes(x.proyecto.estado) && `${x.proyecto.nombre} ${x.nombreJefeProyecto} ${x.nombreDepartamento}`.toLowerCase().includes(texto)); cuerpo.innerHTML = xs.length ? xs.map(x => `<tr><td><strong>${UI.escapar(x.proyecto.nombre)}</strong><small>${UI.escapar(x.nombreDepartamento)}</small></td><td>${UI.escapar(x.nombreJefeProyecto)}</td><td>${UI.fecha(x.proyecto.fechaInicio)} – ${UI.fecha(x.proyecto.fechaFinPlanificada)}</td><td>${UI.textoEstado(x.proyecto.estado)}</td><td>${UI.numero(x.proyecto.avance)} %</td><td><a class="btn-crud celeste" href="/Proyectos/${encodeURIComponent(x.proyecto.codigo)}/Planificacion">Abrir planificación</a></td></tr>`).join('') : '<tr><td colspan="6" class="tabla-vacia">No hay proyectos activos que coincidan.</td></tr>'; }
        root.querySelector('[data-buscar-plan]').addEventListener('input', pintarLista); root.querySelector('[data-recargar-lista-plan]').addEventListener('click', cargarLista); cargarLista();
    }
});
