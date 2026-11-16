(function () {
    'use strict';

    const MIN_CARACTERES = 3;
    const ESPERA_MS = 180;
    const SELECTOR = 'input[data-lugares-url]';

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

        input.setAttribute('role', 'combobox');
        input.setAttribute('aria-autocomplete', 'list');
        input.setAttribute('aria-expanded', 'false');
        input.setAttribute('aria-controls', lista.id);

        estado = {
            lista,
            temporizador: null,
            controlador: null,
            activo: -1,
            version: 0
        };
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

        e.version++;
        clearTimeout(e.temporizador);

        if (e.controlador) {
            e.controlador.abort();
            e.controlador = null;
        }

        e.lista.hidden = true;
        e.lista.replaceChildren();
        e.activo = -1;

        input.setAttribute('aria-expanded', 'false');
        input.removeAttribute('aria-activedescendant');
    }

    function elegir(input, texto) {
        input.value = texto;
        cerrar(input);
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
            item.textContent = lugar.nombre;

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

    async function buscar(input, version) {
        const e = obtenerEstado(input);
        const texto = input.value.trim();

        if (version !== e.version) return;

        if (texto.length < MIN_CARACTERES) {
            cerrar(input);
            return;
        }

        const controlador = new AbortController();
        e.controlador = controlador;

        try {
            const url = input.dataset.lugaresUrl + '?q=' + encodeURIComponent(texto);

            const respuesta = await fetch(url, {
                signal: controlador.signal,
                headers: { 'Accept': 'application/json' }
            });

            if (version !== e.version) return;

            if (!respuesta.ok) {
                cerrar(input);
                return;
            }

            const lugares = await respuesta.json();

            if (version !== e.version) return;

            mostrar(input, lugares);

        } catch (error) {
            if (error.name !== 'AbortError' && version === e.version) {
                console.error('Error al buscar lugares:', error);
                cerrar(input);
            }
        } finally {
            if (e.controlador === controlador) {
                e.controlador = null;
            }
        }
    }

    document.addEventListener('input', function (ev) {
        if (!esInputDeLugares(ev.target)) return;

        const input = ev.target;
        const e = obtenerEstado(input);

        clearTimeout(e.temporizador);

        e.version++;

        if (e.controlador) {
            e.controlador.abort();
            e.controlador = null;
        }

        const version = e.version;

        e.temporizador = setTimeout(function () {
            buscar(input, version);
        }, ESPERA_MS);
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
            ev.preventDefault();
            elegir(input, items[e.activo].textContent);
        } else if (ev.key === 'Escape') {
            cerrar(input);
        }
    });

    document.addEventListener('focusout', function (ev) {
        if (esInputDeLugares(ev.target)) cerrar(ev.target);
    });
})();