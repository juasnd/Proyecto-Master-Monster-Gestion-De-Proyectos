document.addEventListener('DOMContentLoaded', () => {
    'use strict';
    const modal = document.querySelector('[data-reporte-modal]');
    const abrir = document.querySelector('[data-reporte-abrir]');
    if (!modal || !abrir) return;
    const form = modal.querySelector('[data-reporte-form]');
    const tabla = modal.querySelector('[data-reporte-tabla]');
    const errorZona = modal.querySelector('[data-reporte-error]');
    let filas = [], columnas = [];
    const escapar = v => String(v ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const etiquetas = { codigo: 'Código', nombre: 'Nombre', departamento: 'Departamento', jefe: 'Jefe', fechaInicio: 'Fecha inicial', fechaFinPlanificada: 'Fecha final planificada', estado: 'Estado', prioridad: 'Prioridad', avance: 'Avance (%)', presupuesto: 'Presupuesto (USD)', saldoDisponible: 'Saldo disponible (USD)', activo: 'Activo', codigoProyecto: 'Código del proyecto', nombreProyecto: 'Proyecto', costoLaboralPlanificado: 'Costo laboral planificado', gastosAprobados: 'Gastos aprobados', costoComprometidoEstimado: 'Costo comprometido estimado', porcentajeConsumo: 'Consumo (%)', excedido: 'Excedido', enAlerta: 'En alerta' };
    const estados = { PLANIFICADO: 'Planificado', EN_CURSO: 'En curso', PAUSADO: 'Pausado', FINALIZADO: 'Finalizado', CANCELADO: 'Cancelado', PENDIENTE: 'Pendiente', EN_PROCESO: 'En progreso', COMPLETADA: 'Completada', ENTREGADO: 'Entregado', JUSTIFICADO: 'Justificado' };
    const prioridades = { BAJA: 'Baja', MEDIA: 'Media', ALTA: 'Alta', CRITICA: 'Crítica' };
    const etiqueta = c => etiquetas[c] || c.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, x => x.toUpperCase());
    const formato = (v, c) => {
        if (typeof v === 'boolean') return v ? 'Sí' : 'No';
        if (c === 'estado') return estados[v] || v || '';
        if (c === 'prioridad') return prioridades[v] || v || '';
        if (v && /fecha/i.test(c)) { const d = new Date(v); if (!Number.isNaN(d.valueOf())) return new Intl.DateTimeFormat('es-EC', { dateStyle: 'medium', timeZone: 'UTC' }).format(d); }
        if (typeof v === 'number' && /presupuesto|saldo|costo|gastos|valor/i.test(c)) return new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v);
        if (v && typeof v === 'object') return JSON.stringify(v);
        return v ?? '';
    };

    abrir.addEventListener('click', () => { modal.showModal(); setTimeout(() => form.querySelector('input,select')?.focus(), 0); });
    modal.querySelector('[data-reporte-cerrar]').addEventListener('click', () => modal.close());
    modal.querySelector('[data-reporte-limpiar]').addEventListener('click', () => { form.reset(); filas = []; pintar([]); errorZona.hidden = true; });
    form.addEventListener('submit', async e => {
        e.preventDefault(); errorZona.hidden = true;
        const submit = form.querySelector('[type="submit"]'); const original = submit.textContent; submit.disabled = true; submit.textContent = 'Generando...';
        try {
            const r = await fetch(`${modal.dataset.endpoint}?${new URLSearchParams(new FormData(form))}`, { credentials: 'same-origin', headers: { Accept: 'application/json' } });
            const j = await r.json(); if (!r.ok || j.exito === false) throw new Error(j.mensaje || 'No se pudo generar el reporte.');
            const d = j.datos; filas = Array.isArray(d) ? d : (d.elementos || []); pintar(filas);
        } catch (ex) { errorZona.textContent = ex.message; errorZona.hidden = false; }
        finally { submit.disabled = false; submit.textContent = original; }
    });

    function pintar(datos) {
        modal.querySelector('[data-reporte-total]').textContent = datos.length;
        modal.querySelector('[data-reporte-fecha]').textContent = new Date().toLocaleString('es-EC');
        if (!datos.length) { columnas = []; tabla.querySelector('thead').innerHTML = ''; tabla.querySelector('tbody').innerHTML = '<tr><td class="tabla-vacia">No existen registros para los filtros seleccionados.</td></tr>'; return; }
        columnas = Object.keys(datos[0]).filter(x => !['id', 'hijos'].includes(x));
        tabla.querySelector('thead').innerHTML = `<tr>${columnas.map(x => `<th>${escapar(etiqueta(x))}</th>`).join('')}</tr>`;
        tabla.querySelector('tbody').innerHTML = datos.map(x => `<tr>${columnas.map(c => `<td>${escapar(formato(x[c], c))}</td>`).join('')}</tr>`).join('');
    }

    function validarDatos() { if (filas.length) return true; errorZona.textContent = 'Genere una vista previa antes de exportar.'; errorZona.hidden = false; return false; }
    function descargar(nombre, contenido, tipo) { const url = URL.createObjectURL(new Blob([contenido], { type: tipo })); const a = document.createElement('a'); a.href = url; a.download = nombre; document.body.appendChild(a); a.click(); a.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000); }
    function registrar(formatoReporte) {
        if (!modal.dataset.codigoReporte) return;
        const cuerpo = new URLSearchParams({ accion: 'registrar', codigoReporte: modal.dataset.codigoReporte, modulo: modal.dataset.modulo || '', tipoReporte: modal.dataset.titulo || 'Reporte', formato: formatoReporte, totalRegistros: String(filas.length), filtros: [...new FormData(form)].filter(([, v]) => v).map(([k, v]) => `${k}: ${v}`).join('; ') });
        fetch('/ReporteController', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' }, body: cuerpo }).catch(() => {});
    }
    const csvCelda = v => `"${String(v ?? '').replaceAll('"', '""')}"`;
    modal.querySelector('[data-reporte-csv]').addEventListener('click', () => { if (!validarDatos()) return; const csv = '\ufeff' + [columnas.map(c => csvCelda(etiqueta(c))).join(';'), ...filas.map(x => columnas.map(c => csvCelda(formato(x[c], c))).join(';'))].join('\r\n'); descargar(`${modal.dataset.archivo}.csv`, csv, 'text/csv;charset=utf-8'); registrar('CSV'); });
    modal.querySelector('[data-reporte-excel]').addEventListener('click', () => { if (!validarDatos()) return; const contenido = `<!doctype html><html><head><meta charset="utf-8"></head><body><h1>${escapar(modal.dataset.titulo)}</h1><p>Master Monster · ${escapar(modal.dataset.modulo)} · ${escapar(new Date().toLocaleString('es-EC'))}</p><table border="1"><thead><tr>${columnas.map(c => `<th>${escapar(etiqueta(c))}</th>`).join('')}</tr></thead><tbody>${filas.map(x => `<tr>${columnas.map(c => `<td>${escapar(formato(x[c], c))}</td>`).join('')}</tr>`).join('')}</tbody></table></body></html>`; descargar(`${modal.dataset.archivo}.xls`, '\ufeff' + contenido, 'application/vnd.ms-excel;charset=utf-8'); registrar('Excel'); });
    modal.querySelector('[data-reporte-imprimir]').addEventListener('click', () => {
        if (!validarDatos()) return;
        const ventana = window.open('', '_blank', 'width=1100,height=760');
        if (!ventana) { errorZona.textContent = 'El navegador bloqueó la vista del PDF. Permita ventanas emergentes para este sitio.'; errorZona.hidden = false; return; }
        const fecha = new Date().toLocaleString('es-EC');
        ventana.document.write(`<!doctype html><html lang="es"><head><meta charset="utf-8"><title>${escapar(modal.dataset.titulo)}</title><style>@page{size:A4 landscape;margin:12mm}*{box-sizing:border-box}body{font:11px "Segoe UI",Arial;color:#172433;margin:0}.cabecera{display:flex;justify-content:space-between;border-bottom:4px solid #0877b9;padding:0 0 12px;margin-bottom:14px}.marca{color:#0877b9;font-weight:900;text-transform:uppercase}.cabecera h1{margin:4px 0;font-size:24px}.meta{display:grid;grid-template-columns:repeat(2,auto);gap:5px 18px}.meta span{color:#5e7080}.meta strong{display:block;color:#172433}table{width:100%;border-collapse:collapse}th,td{border:1px solid #bdcbd5;padding:6px;text-align:left;vertical-align:top}th{background:#eaf1f5;color:#173e5e}tr:nth-child(even){background:#f8fafb}.pie{position:fixed;bottom:0;left:0;right:0;border-top:1px solid #bdcbd5;padding-top:6px;display:flex;justify-content:space-between;color:#607384}.pagina:after{content:"Página " counter(page)}@media print{button{display:none}}</style></head><body><header class="cabecera"><div><span class="marca">Master Monster</span><h1>${escapar(modal.dataset.titulo)}</h1><p>${escapar(modal.dataset.modulo)}</p></div><div class="meta"><div><span>Generado</span><strong>${escapar(fecha)}</strong></div><div><span>Usuario</span><strong>${escapar(modal.dataset.usuario)}</strong></div><div><span>Registros</span><strong>${filas.length}</strong></div><div><span>Moneda</span><strong>USD · es-EC</strong></div></div></header><table><thead><tr>${columnas.map(c => `<th>${escapar(etiqueta(c))}</th>`).join('')}</tr></thead><tbody>${filas.map(x => `<tr>${columnas.map(c => `<td>${escapar(formato(x[c], c))}</td>`).join('')}</tr>`).join('')}</tbody></table><footer class="pie"><span>Master Monster · ${escapar(modal.dataset.archivo)}</span><span class="pagina"></span></footer><script>window.onload=()=>{window.focus();window.print()}<\/script></body></html>`);
        ventana.document.close(); registrar('PDF');
    });
});
