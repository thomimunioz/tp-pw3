(() => {
    'use strict';

    document.querySelectorAll('[data-paradas-editor]').forEach(editor => {
        const botonAgregar = editor.closest('.form-campo').querySelector('[data-agregar-parada]');

        const actualizar = () => {
            const items = [...editor.querySelectorAll('.parada-item')];
            items.forEach((item, indice) => {
                item.querySelector('.parada-item__numero').textContent = String(indice + 1);
                const input = item.querySelector('input');
                input.name = `ParadasIntermedias[${indice}]`;
                item.querySelector('[data-subir-parada]').disabled = indice === 0;
                item.querySelector('[data-bajar-parada]').disabled = indice === items.length - 1;
            });
        };

        const crearParada = () => {
            const item = document.createElement('div');
            item.className = 'parada-item';
            item.innerHTML = `
                <span class="parada-item__numero"></span>
                <input placeholder="Dirección de la parada" autocomplete="off"
                       data-lugares-url="${editor.dataset.lugaresUrl}" />
                <div class="parada-item__acciones">
                    <button type="button" class="btn-parada btn-parada--icono" data-subir-parada aria-label="Subir parada" title="Subir">↑</button>
                    <button type="button" class="btn-parada btn-parada--icono" data-bajar-parada aria-label="Bajar parada" title="Bajar">↓</button>
                    <button type="button" class="btn-parada btn-parada--eliminar" data-eliminar-parada aria-label="Eliminar parada" title="Eliminar">×</button>
                </div>`;
            return item;
        };

        botonAgregar.addEventListener('click', () => {
            editor.appendChild(crearParada());
            actualizar();
            editor.lastElementChild.querySelector('input').focus();
        });

        editor.addEventListener('click', event => {
            const boton = event.target.closest('button');
            if (!boton) return;
            const item = boton.closest('.parada-item');
            if (boton.matches('[data-eliminar-parada]')) {
                item.remove();
            } else if (boton.matches('[data-subir-parada]') && item.previousElementSibling) {
                editor.insertBefore(item, item.previousElementSibling);
            } else if (boton.matches('[data-bajar-parada]') && item.nextElementSibling) {
                editor.insertBefore(item.nextElementSibling, item);
            }
            actualizar();
        });

        actualizar();
    });

    document.querySelectorAll('[data-form-viaje]').forEach(form => {
        form.addEventListener('submit', event => {
            const salida = form.querySelector('[name="HoraSalida"]');
            const llegada = form.querySelector('[name="HoraLlegada"]');
            if (!salida || !llegada) return;

            llegada.setCustomValidity('');
            if (salida.value && llegada.value && llegada.value <= salida.value) {
                llegada.setCustomValidity('La hora de llegada debe ser posterior a la hora de salida.');
                llegada.reportValidity();
                event.preventDefault();
            }
        });
    });
})();
