(function () {
    'use strict';
    let plan = null, detalle = null, vista = 'mes', cursor = new Date(), filtros = {};
    const UI = () => window.ProyectosUI;
    const diaMs = 86400000;
    const utc = v => new Date(`${String(v).slice(0, 10)}T00:00:00Z`);
    const iso = d => d.toISOString().slice(0, 10);
    const inicioSemana = d => { const x = new Date(d); x.setUTCHours(0, 0, 0, 0); x.setUTCDate(x.getUTCDate() - ((x.getUTCDay() + 6) % 7)); return x; };

    function items() {
        const xs = [];
        (plan?.fases || []).forEach(f => (f.actividades || []).forEach(a => {
            xs.push({ tipo: 'ACTIVIDAD', codigo: a.actividad.codigoActividad, nombre: a.actividad.nombre, inicio: a.actividad.fechaInicio, fin: a.actividad.fechaFin, estado: a.actividad.estado, responsable: a.actividad.codigoResponsable, fase: f.fase.codigoFase, progreso: a.avance });
            (a.tareas || []).forEach(t => xs.push({ tipo: 'TAREA', codigo: t.tarea.codigoTarea, nombre: t.tarea.nombre, inicio: t.tarea.fechaInicio, fin: t.tarea.fechaFin, estado: t.tarea.estado, responsable: t.tarea.codigoResponsable, fase: f.fase.codigoFase, progreso: t.avance }));
        }));
        (plan?.entregables || []).forEach(e => xs.push({ tipo: 'ENTREGABLE', codigo: e.codigoEntregable, nombre: e.nombre, inicio: e.fechaCompromiso, fin: e.fechaCompromiso, estado: e.estado, responsable: '', fase: '', progreso: e.estado === 'ENTREGADO' ? 100 : 0 }));
        const hoy = new Date(); hoy.setUTCHours(0, 0, 0, 0);
        return xs.filter(x => (!filtros.responsable || x.responsable === filtros.responsable) && (!filtros.fase || x.fase === filtros.fase) && (!filtros.estado || x.estado === filtros.estado) && (!filtros.tipo || x.tipo === filtros.tipo) && (!filtros.atrasadas || (utc(x.fin) < hoy && !['COMPLETADA', 'ENTREGADO', 'JUSTIFICADO'].includes(x.estado))) && (!filtros.proximas || (utc(x.fin) >= hoy && utc(x.fin) - hoy <= 7 * diaMs && !['COMPLETADA', 'ENTREGADO', 'JUSTIFICADO'].includes(x.estado))));
    }
    const clase = x => x.tipo === 'ENTREGABLE' ? 'entregable' : x.estado === 'COMPLETADA' ? 'completada' : x.estado === 'EN_PROCESO' ? 'proceso' : 'pendiente';

    function render(nuevoPlan, alDetalle) {
        if (nuevoPlan) { plan = nuevoPlan; configurarFiltros(); }
        if (alDetalle) detalle = alDetalle;
        const cont = document.querySelector('[data-calendario-grafico]'); if (!cont || !plan) return;
        const xs = items();
        if (vista === 'mes') pintarMes(cont, xs); else if (vista === 'semana') pintarSemana(cont, xs); else pintarLista(cont, xs);
    }

    function pintarMes(cont, xs) {
        const primero = new Date(Date.UTC(cursor.getUTCFullYear(), cursor.getUTCMonth(), 1)); const inicio = inicioSemana(primero); const dias = Array.from({ length: 42 }, (_, i) => new Date(inicio.getTime() + i * diaMs));
        document.querySelector('[data-cal-periodo]').textContent = new Intl.DateTimeFormat('es-EC', { month: 'long', year: 'numeric', timeZone: 'UTC' }).format(primero);
        cont.innerHTML = `<div class="cal-mes"><div class="cal-dias-semana">${['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'].map(x => `<span>${x}</span>`).join('')}</div><div class="cal-celdas">${dias.map(d => { const clave = iso(d); const delDia = xs.filter(x => utc(x.inicio) <= d && utc(x.fin) >= d).slice(0, 4); return `<div class="cal-celda ${d.getUTCMonth() === primero.getUTCMonth() ? '' : 'otro-mes'} ${clave === iso(new Date()) ? 'hoy' : ''}"><time>${d.getUTCDate()}</time>${delDia.map(x => `<button type="button" class="cal-evento ${clase(x)} ${clave !== String(x.inicio).slice(0, 10) ? 'continua' : ''}" data-cal-item="${UI().escapar(x.codigo)}" title="${UI().escapar(x.nombre)}">${UI().escapar(x.nombre)}</button>`).join('')}${xs.filter(x => utc(x.inicio) <= d && utc(x.fin) >= d).length > 4 ? '<small>Más elementos…</small>' : ''}</div>`; }).join('')}</div></div>`;
        activar(cont, xs);
    }

    function pintarSemana(cont, xs) {
        const inicio = inicioSemana(cursor); const dias = Array.from({ length: 7 }, (_, i) => new Date(inicio.getTime() + i * diaMs)); const fin = dias[6];
        document.querySelector('[data-cal-periodo]').textContent = `${UI().fecha(iso(inicio))} – ${UI().fecha(iso(fin))}`;
        cont.innerHTML = `<div class="cal-semana">${dias.map(d => { const delDia = xs.filter(x => utc(x.inicio) <= d && utc(x.fin) >= d); return `<section><header><strong>${new Intl.DateTimeFormat('es-EC', { weekday: 'long', day: 'numeric', month: 'short', timeZone: 'UTC' }).format(d)}</strong></header>${delDia.length ? delDia.map(x => `<button type="button" class="cal-tarjeta ${clase(x)}" data-cal-item="${UI().escapar(x.codigo)}"><strong>${UI().escapar(x.nombre)}</strong><span>${UI().escapar(x.tipo)} · ${UI().textoEstado(x.estado)} · ${UI().numero(x.progreso)} %</span></button>`).join('') : '<p>Sin trabajo programado.</p>'}</section>`; }).join('')}</div>`; activar(cont, xs);
    }

    function pintarLista(cont, xs) {
        const hoy = new Date(); hoy.setUTCHours(0, 0, 0, 0); const lista = xs.filter(x => utc(x.fin) >= hoy).sort((a, b) => utc(a.inicio) - utc(b.inicio)); document.querySelector('[data-cal-periodo]').textContent = 'Próximos elementos';
        cont.innerHTML = lista.length ? `<div class="cal-lista">${lista.map(x => `<button type="button" data-cal-item="${UI().escapar(x.codigo)}"><time>${UI().fecha(x.inicio)}</time><span><strong>${UI().escapar(x.nombre)}</strong><small>${UI().escapar(x.tipo)} · ${UI().textoEstado(x.estado)} · hasta ${UI().fecha(x.fin)}</small></span></button>`).join('')}</div>` : '<p class="tabla-vacia">No hay próximos elementos para los filtros seleccionados.</p>'; activar(cont, xs);
    }
    function activar(cont, xs) { cont.onclick = e => { const b = e.target.closest('[data-cal-item]'); if (b) detalle?.(xs.find(x => x.codigo === b.dataset.calItem)); }; }

    function configurarFiltros() {
        const form = document.querySelector('[data-form-filtros-calendario]'); if (!form || form.dataset.configurado) return; form.dataset.configurado = 'true';
        form.responsable.innerHTML = '<option value="">Todos</option>' + (plan.equipo || []).map(x => `<option value="${UI().escapar(x.asignacion.codigoEmpleado)}">${UI().escapar(x.nombreEmpleado)}</option>`).join('');
        form.fase.innerHTML = '<option value="">Todas</option>' + (plan.fases || []).map(x => `<option value="${UI().escapar(x.fase.codigoFase)}">${UI().escapar(x.fase.nombre)}</option>`).join('');
    }
    function mover(n) { if (vista === 'mes') cursor.setUTCMonth(cursor.getUTCMonth() + n); else cursor.setUTCDate(cursor.getUTCDate() + n * (vista === 'semana' ? 7 : 30)); render(); }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-cal-vista]').forEach(b => b.addEventListener('click', () => { vista = b.dataset.calVista; document.querySelectorAll('[data-cal-vista]').forEach(x => x.classList.toggle('activo', x === b)); render(); }));
        document.querySelector('[data-cal-anterior]')?.addEventListener('click', () => mover(-1)); document.querySelector('[data-cal-siguiente]')?.addEventListener('click', () => mover(1)); document.querySelector('[data-cal-hoy]')?.addEventListener('click', () => { cursor = new Date(); render(); });
        document.querySelector('[data-cal-filtros]')?.addEventListener('click', () => UI().abrir(document.querySelector('[data-modal-filtros-calendario]')));
        document.querySelector('[data-form-filtros-calendario]')?.addEventListener('submit', e => { e.preventDefault(); const f = e.currentTarget; filtros = { responsable: f.responsable.value, fase: f.fase.value, estado: f.estado.value, tipo: f.tipo.value, atrasadas: f.atrasadas.checked, proximas: f.proximas.checked }; UI().cerrar(f.closest('dialog')); render(); });
        document.querySelector('[data-form-filtros-calendario]')?.addEventListener('reset', () => setTimeout(() => { filtros = {}; render(); }, 0));
    });
    window.CalendarioProyecto = { render };
})();
