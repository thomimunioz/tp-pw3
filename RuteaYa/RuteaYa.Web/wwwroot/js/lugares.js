/**
 * lugares.js — Sugerencias de lugares para los inputs de origen, destino y paradas.
 *
 * Uso: agregar al input los atributos  autocomplete="off"  y
 *      data-lugares-url="@Url.Action("Sugerir", "Lugares")"
 *
 * Funciona con delegación de eventos en document, así que también sirve para
 * inputs que se agregan después (por ejemplo, las paradas intermedias).
 * El JS solo dibuja: la búsqueda y sus reglas viven en el servidor.
 */
(function () {
    'use strict';

    const MIN_CARACTERES = 3;   // mismo valor que LugaresReglas.MinCaracteres
    const ESPERA_MS = 300;      // debounce: esperar a que el usuario deje de escribir
    const SELECTOR = 'input[data-lugares-url]';

    // Estado propio de cada input: lista, temporizador, consulta en curso e ítem marcado
    const estados = new WeakMap();
    let contador = 0;

    function esInputDeLugares(elemento) {
        return elemento instanceof HTMLInputElement && elemento.matches(SELECTOR);
    }

    function obtenerEstado(input) {
        let estado = estados.get(input);
        if (estado) return estado;

        const lista = document.createElement('ul');
        lista.id = 'lugares-lista-' + (++contador);
        lista.className = 'lugares-lista';
        lista.setAttribute('role', 'listbox');
        lista.hidden = true;
        input.insertAdjacentElement('afterend', lista);

        // Accesibilidad: el input se anuncia como un combobox con lista de opciones
        input.setAttribute('role', 'combobox');
        input.setAttribute('aria-autocomplete', 'list');
        input.setAttribute('aria-expanded', 'false');
        input.setAttribute('aria-controls', lista.id);

        estado = { lista, temporizador: null, controlador: null, activo: -1 };
        estados.set(input, estado);
        return estado;
    }

    function posicionar(input, lista) {
        lista.style.top = (input.offsetTop + input.offsetHeight) + 'px';
        lista.style.left = input.offsetLeft + 'px';
        lista.style.width = input.offsetWidth + 'px';
    }

    function cerrar(input) {
        const e = obtenerEstado(input);
        clearTimeout(e.temporizador);
        if (e.controlador) e.controlador.abort();   // descarta consultas pendientes
        e.lista.hidden = true;
        e.lista.replaceChildren();
        e.activo = -1;
        input.setAttribute('aria-expanded', 'false');
        input.removeAttribute('aria-activedescendant');
    }

    function elegir(input, texto) {
        input.value = texto;
        cerrar(input);
        // Avisa a la validación del formulario que el valor cambió
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function marcar(input, indice) {
        const e = obtenerEstado(input);
        Array.from(e.lista.children).forEach((item, i) => {
            const activo = i === indice;
            item.classList.toggle('lugares-item--activo', activo);
            item.setAttribute('aria-selected', String(activo));
        });
        e.activo = indice;
        const item = e.lista.children[indice];
        input.setAttribute('aria-activedescendant', item.id);
        item.scrollIntoView({ block: 'nearest' });
    }

    function mostrar(input, lugares) {
        const e = obtenerEstado(input);
        e.lista.replaceChildren();
        e.activo = -1;

        if (!Array.isArray(lugares) || lugares.length === 0) {
            cerrar(input);
            return;
        }

        lugares.forEach((lugar, i) => {
            const item = document.createElement('li');
            item.id = e.lista.id + '-' + i;
            item.className = 'lugares-item';
            item.setAttribute('role', 'option');
            item.setAttribute('aria-selected', 'false');
            item.textContent = lugar.nombre;   // textContent y no innerHTML: el dato viene de afuera

            // mousedown (y no click) para que ocurra antes de que el input pierda el foco
            item.addEventListener('mousedown', function (ev) {
                ev.preventDefault();
                elegir(input, lugar.nombre);
            });
            e.lista.appendChild(item);
        });

        posicionar(input, e.lista);
        e.lista.hidden = false;
        input.setAttribute('aria-expanded', 'true');
    }

    async function buscar(input) {
        const e = obtenerEstado(input);
        const texto = input.value.trim();

        if (texto.length < MIN_CARACTERES) {
            cerrar(input);
            return;
        }

        if (e.controlador) e.controlador.abort();   // solo importa la última consulta
        e.controlador = new AbortController();

        try {
            const url = input.dataset.lugaresUrl + '?q=' + encodeURIComponent(texto);
            const respuesta = await fetch(url, {
                signal: e.controlador.signal,
                headers: { 'Accept': 'application/json' }
            });

            if (!respuesta.ok) {
                cerrar(input);
                return;
            }
            mostrar(input, await respuesta.json());
        } catch (error) {
            // AbortError es normal (el usuario siguió escribiendo). Con cualquier otro error
            // el input sigue funcionando como texto libre.
            if (error.name !== 'AbortError') cerrar(input);
        }
    }

    document.addEventListener('input', function (ev) {
        if (!esInputDeLugares(ev.target)) return;
        const input = ev.target;
        const e = obtenerEstado(input);
        clearTimeout(e.temporizador);
        e.temporizador = setTimeout(function () { buscar(input); }, ESPERA_MS);
    });

    document.addEventListener('keydown', function (ev) {
        if (!esInputDeLugares(ev.target)) return;
        const input = ev.target;
        const e = obtenerEstado(input);
        const items = e.lista.children;
        if (e.lista.hidden || items.length === 0) return;

        if (ev.key === 'ArrowDown' || ev.key === 'ArrowUp') {
            ev.preventDefault();
            let nuevo = e.activo + (ev.key === 'ArrowDown' ? 1 : -1);
            if (nuevo < 0) nuevo = items.length - 1;
            if (nuevo >= items.length) nuevo = 0;
            marcar(input, nuevo);
        } else if (ev.key === 'Enter' && e.activo >= 0) {
            ev.preventDefault();   // que Enter elija la sugerencia y no envíe el formulario
            elegir(input, items[e.activo].textContent);
        } else if (ev.key === 'Escape') {
            cerrar(input);
        }
    });

    // Al salir del input se cierra la lista (el click en una sugerencia usa mousedown, ya se resolvió antes)
    document.addEventListener('focusout', function (ev) {
        if (esInputDeLugares(ev.target)) cerrar(ev.target);
    });
})();