const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');
const script = fs.readFileSync(path.join(__dirname, '../wwwroot/js/http-compartido.js'), 'utf8');
function cliente(fetch, token = '') {
    const contexto = { window: {}, document: { querySelector: () => token ? { value: token } : null }, fetch, Error, Object };
    vm.runInNewContext(script, contexto);
    return contexto.window.MonsterHttp;
}
test('Envía el token del formulario y conserva el cuerpo URL encoded', async () => {
    let peticion;
    const api = cliente(async (url, opciones) => { peticion = { url, opciones }; return { ok: true }; }, 'csrf-form');
    await api.enviarFormulario('/ReporteController', new URLSearchParams({ accion: 'registrar', filtros: 'A&B' }));
    assert.equal(peticion.url, '/ReporteController');
    assert.equal(peticion.opciones.headers['X-CSRF-TOKEN'], 'csrf-form');
    assert.equal(peticion.opciones.credentials, 'same-origin');
    assert.equal(peticion.opciones.body, 'accion=registrar&filtros=A%26B');
});
test('Solicitudes concurrentes comparten una sola obtención del token', async () => {
    let tokens = 0, envios = 0;
    const api = cliente(async (url, opciones) => {
        if (url.includes('token-antifalsificacion')) { tokens++; return { ok: true, json: async () => ({ datos: { token: 'csrf-api' } }) }; }
        envios++; assert.equal(opciones.headers['X-CSRF-TOKEN'], 'csrf-api'); return { ok: true };
    });
    await Promise.all([api.enviarFormulario('/ReporteController', new URLSearchParams()), api.enviarFormulario('/ReporteController', new URLSearchParams())]);
    assert.equal(tokens, 1); assert.equal(envios, 2);
});
test('Un fallo de token no envía datos y permite reintentar', async () => {
    let tokens = 0, envios = 0;
    const api = cliente(async url => {
        if (url.includes('token-antifalsificacion')) { tokens++; return { ok: tokens > 1, json: async () => tokens > 1 ? { datos: { token: 'nuevo' } } : {} }; }
        envios++; return { ok: true };
    });
    await assert.rejects(api.enviarFormulario('/ReporteController', new URLSearchParams()), /validar la sesión/);
    assert.equal(envios, 0);
    await api.enviarFormulario('/ReporteController', new URLSearchParams());
    assert.equal(tokens, 2); assert.equal(envios, 1);
});
test('Una respuesta 403 se propaga como fallo', async () => {
    const api = cliente(async () => ({ ok: false, status: 403 }), 'csrf');
    await assert.rejects(api.enviarFormulario('/ReporteController', new URLSearchParams()), /403/);
});
