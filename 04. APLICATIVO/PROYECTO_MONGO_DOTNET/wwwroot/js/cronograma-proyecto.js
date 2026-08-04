(function () {
    'use strict';
    let plan = null;
    let detalle = null;
    let escala = 'semana';
    const diaMs = 86400000;
    const UI = () => window.ProyectosUI;
    const utc = valor => new Date(`${String(valor).slice(0, 10)}T00:00:00Z`);

    function elementos() {
        const lista = [];
        (plan?.fases || []).forEach(f => {
            lista.push({ tipo: 'FASE', nivel: 0, codigo: f.fase.codigoFase, nombre: f.fase.nombre, inicio: f.fase.fechaInicio, fin: f.fase.fechaFin, estado: f.fase.estado, avance: f.avance });
            (f.actividades || []).forEach(a => {
                lista.push({ tipo: 'ACTIVIDAD', nivel: 1, codigo: a.actividad.codigoActividad, nombre: a.actividad.nombre, inicio: a.actividad.fechaInicio, fin: a.actividad.fechaFin, estado: a.actividad.estado, avance: a.avance });
                (a.tareas || []).forEach(t => lista.push({ tipo: 'TAREA', nivel: 2, codigo: t.tarea.codigoTarea, nombre: t.tarea.nombre, inicio: t.tarea.fechaInicio, fin: t.tarea.fechaFin, estado: t.tarea.estado, avance: t.avance, bloqueada: t.bloqueada, dependencias: t.dependencias }));
                (a.entregables || []).forEach(e => lista.push({ tipo: 'ENTREGABLE', nivel: 2, codigo: e.codigoEntregable, nombre: e.nombre, inicio: e.fechaCompromiso, fin: e.fechaCompromiso, estado: e.estado }));
            });
            (f.entregables || []).forEach(e => lista.push({ tipo: 'ENTREGABLE', nivel: 1, codigo: e.codigoEntregable, nombre: e.nombre, inicio: e.fechaCompromiso, fin: e.fechaCompromiso, estado: e.estado }));
        });
        (plan?.entregables || []).filter(e => e.tipoRelacion === 'TAREA').forEach(e => {
            const tareaExiste = lista.some(x => x.tipo === 'TAREA' && x.codigo === e.codigoRelacion);
            if (tareaExiste) lista.push({ tipo: 'ENTREGABLE', nivel: 3, codigo: e.codigoEntregable, nombre: e.nombre, inicio: e.fechaCompromiso, fin: e.fechaCompromiso, estado: e.estado });
        });
        return lista;
    }

    function clase(item) {
        if (item.tipo === 'ENTREGABLE') return 'hito';
        const hoy = new Date(); hoy.setUTCHours(0, 0, 0, 0); const fin = utc(item.fin);
        if (item.estado !== 'COMPLETADA' && fin < hoy) return 'atrasada';
        if (item.estado !== 'COMPLETADA' && (fin - hoy) / diaMs <= 7 && fin >= hoy) return 'proxima';
        return item.estado === 'COMPLETADA' ? 'completada' : item.estado === 'EN_PROCESO' ? 'proceso' : 'pendiente';
    }

    function render(nuevoPlan, alDetalle) {
        if (nuevoPlan) plan = nuevoPlan;
        if (alDetalle) detalle = alDetalle;
        const contenedor = document.querySelector('[data-cronograma-grafico]');
        if (!contenedor || !plan) return;
        escala = document.querySelector('[data-crono-escala]')?.value || escala;
        const inicio = utc(plan.proyecto.fechaInicio); const fin = utc(plan.proyecto.fechaFinPlanificada); const totalDias = Math.max(1, Math.round((fin - inicio) / diaMs) + 1);
        const anchoDia = escala === 'dia' ? 34 : escala === 'semana' ? 15 : 5;
        const ancho = Math.max(760, totalDias * anchoDia);
        const pasos = [];
        if (escala === 'dia') for (let i = 0; i < totalDias; i++) pasos.push({ i, dias: 1, texto: new Intl.DateTimeFormat('es-EC', { day: '2-digit', month: 'short', timeZone: 'UTC' }).format(new Date(inicio.getTime() + i * diaMs)) });
        else if (escala === 'semana') for (let i = 0; i < totalDias; i += 7) pasos.push({ i, dias: Math.min(7, totalDias - i), texto: `Sem. ${new Intl.DateTimeFormat('es-EC', { day: '2-digit', month: 'short', timeZone: 'UTC' }).format(new Date(inicio.getTime() + i * diaMs))}` });
        else for (let fecha = new Date(inicio), i = 0; fecha <= fin;) { const siguiente = new Date(Date.UTC(fecha.getUTCFullYear(), fecha.getUTCMonth() + 1, 1)); const dias = Math.min(totalDias - i, Math.max(1, Math.round((siguiente - fecha) / diaMs))); pasos.push({ i, dias, texto: new Intl.DateTimeFormat('es-EC', { month: 'short', year: 'numeric', timeZone: 'UTC' }).format(fecha) }); i += dias; fecha = new Date(fecha.getTime() + dias * diaMs); }
        const hoy = new Date(); hoy.setUTCHours(0, 0, 0, 0); const hoyPos = (hoy - inicio) / diaMs * anchoDia;
        const items = elementos();
        contenedor.innerHTML = `<div class="gantt"><div class="gantt-nombres"><div class="gantt-esquina">Elemento</div>${items.map((x, i) => `<button type="button" class="gantt-nombre nivel-${x.nivel}" data-crono-item="${i}"><small>${UI().escapar(x.tipo)}</small><span>${UI().escapar(x.nombre)}</span>${x.bloqueada ? '<em>Bloqueada</em>' : ''}</button>`).join('')}</div><div class="gantt-tiempo" data-gantt-tiempo><div class="gantt-encabezado" style="width:${ancho}px">${pasos.map(p => `<span style="left:${p.i * anchoDia}px;width:${p.dias * anchoDia}px">${UI().escapar(p.texto)}</span>`).join('')}</div><div class="gantt-filas" style="width:${ancho}px">${hoyPos >= 0 && hoyPos <= ancho ? `<i class="gantt-hoy" style="left:${hoyPos}px" title="Hoy"></i>` : ''}${items.map((x, i) => { const a = Math.max(0, Math.round((utc(x.inicio) - inicio) / diaMs)); const b = Math.max(a, Math.round((utc(x.fin) - inicio) / diaMs)); const izquierda = a * anchoDia; const barra = Math.max(x.tipo === 'ENTREGABLE' ? 12 : anchoDia, (b - a + 1) * anchoDia); const deps = (x.dependencias || []).map(d => d.nombrePredecesora).join(', '); return `<div class="gantt-fila"><button type="button" class="gantt-barra ${clase(x)} ${x.tipo === 'ENTREGABLE' ? 'es-hito' : ''}" data-crono-item="${i}" style="left:${izquierda}px;width:${barra}px" title="${UI().escapar(`${x.nombre} · ${UI().fecha(x.inicio)} a ${UI().fecha(x.fin)}${deps ? ` · Depende de: ${deps}` : ''}`)}"><span>${x.tipo === 'ENTREGABLE' ? 'Hito' : `${UI().numero(x.avance || 0)} %`}</span></button></div>`; }).join('')}</div></div></div>`;
        contenedor.onclick = e => { const b = e.target.closest('[data-crono-item]'); if (b) detalle?.(items[Number(b.dataset.cronoItem)]); };
        requestAnimationFrame(() => irHoy(false));
    }

    function area() { return document.querySelector('[data-gantt-tiempo]'); }
    function mover(direccion) { area()?.scrollBy({ left: direccion * Math.max(350, area().clientWidth * .75), behavior: 'smooth' }); }
    function irHoy(animar = true) { const linea = document.querySelector('.gantt-hoy'); const zona = area(); if (linea && zona) zona.scrollTo({ left: Math.max(0, parseFloat(linea.style.left) - zona.clientWidth / 2), behavior: animar ? 'smooth' : 'auto' }); }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelector('[data-crono-escala]')?.addEventListener('change', () => render());
        document.querySelector('[data-crono-anterior]')?.addEventListener('click', () => mover(-1));
        document.querySelector('[data-crono-siguiente]')?.addEventListener('click', () => mover(1));
        document.querySelector('[data-crono-hoy]')?.addEventListener('click', () => irHoy());
    });
    window.CronogramaProyecto = { render };
})();
