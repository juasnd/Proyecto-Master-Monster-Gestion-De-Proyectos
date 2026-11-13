(function () {
    'use strict';
    let tokenPendiente;
    async function token() {
        const existente = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (existente) return existente;
        tokenPendiente ||= fetch('/api/v1/sesion/token-antifalsificacion', {
            credentials: 'same-origin', headers: { Accept: 'application/json' }
        }).then(async respuesta => {
            const json = await respuesta.json();
            if (!respuesta.ok || !json.datos?.token) throw new Error('No se pudo validar la sesión.');
            return json.datos.token;
        }).catch(error => { tokenPendiente = null; throw error; });
        return tokenPendiente;
    }

    async function enviarFormulario(url, cuerpo) {
        const respuesta = await fetch(url, {
            method: 'POST', credentials: 'same-origin',
            headers: { Accept: 'application/json', 'X-CSRF-TOKEN': await token(),
                'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' },
            body: cuerpo.toString()
        });
        if (!respuesta.ok) throw new Error(`No se pudo completar la operación (${respuesta.status}).`);
        return respuesta;
    }
    window.MonsterHttp = Object.freeze({ token, enviarFormulario });
})();
