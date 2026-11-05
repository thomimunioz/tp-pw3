(() => {
    const contenedor = document.getElementById('mapa-recorrido');
    const estado = document.getElementById('mapa-recorrido-estado');

    if (!contenedor || !estado) return;

    const mostrarError = (mensaje) => {
        estado.textContent = mensaje;
        contenedor.innerHTML = '<p class="viaje-mapa__estado">No se pudo cargar el mapa.</p>';
    };

    const escaparHtml = (valor) => String(valor ?? '').replace(/[&<>"']/g, caracter => ({
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#39;'
    })[caracter]);

    const crearIcono = (tipo, texto) => L.divIcon({
        className: '',
        html: `<div class="viaje-mapa__marcador viaje-mapa__marcador--${tipo}"><span>${escaparHtml(texto)}</span></div>`,
        iconSize: [32, 32],
        iconAnchor: [16, 30],
        popupAnchor: [0, -28]
    });

    const puntoValido = (punto) => punto
        && Number.isFinite(Number(punto.latitud))
        && Number.isFinite(Number(punto.longitud))
        && Number(punto.latitud) >= -90
        && Number(punto.latitud) <= 90
        && Number(punto.longitud) >= -180
        && Number(punto.longitud) <= 180;

    const inicializarMapa = async () => {
        if (typeof L === 'undefined') {
            mostrarError('No se pudo cargar la biblioteca del mapa. Revisá tu conexión e intentá nuevamente.');
            return;
        }

        try {
            const respuesta = await fetch(contenedor.dataset.url, {
                method: 'GET',
                headers: { 'Accept': 'application/json' },
                credentials: 'same-origin'
            });

            if (respuesta.status === 404) {
                mostrarError('No se encontró el recorrido o no tenés permiso para consultarlo.');
                return;
            }

            if (respuesta.status === 401) {
                mostrarError('Tu sesión no está disponible. Volvé a iniciar sesión.');
                return;
            }

            if (!respuesta.ok) {
                throw new Error('La API no pudo devolver los datos del recorrido.');
            }

            const recorrido = await respuesta.json();
            const puntos = [
                { ...recorrido.origen, tipo: 'origen', etiqueta: 'O', titulo: 'Origen' },
                ...(recorrido.paradas ?? []).map((parada, indice) => ({
                    ...parada,
                    tipo: 'parada',
                    etiqueta: String(indice + 1),
                    titulo: `Parada ${indice + 1}`
                })),
                { ...recorrido.destino, tipo: 'destino', etiqueta: 'D', titulo: 'Destino' }
            ];

            if (puntos.some(punto => !puntoValido(punto))) {
                mostrarError('El recorrido contiene ubicaciones sin coordenadas válidas.');
                return;
            }

            contenedor.innerHTML = '';
            const coordenadas = puntos.map(punto => [Number(punto.latitud), Number(punto.longitud)]);
            const mapa = L.map(contenedor, { scrollWheelZoom: false });

            L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(mapa);

            puntos.forEach(punto => {
                const latLng = [Number(punto.latitud), Number(punto.longitud)];
                const marcador = L.marker(latLng, {
                    icon: crearIcono(punto.tipo, punto.etiqueta)
                }).addTo(mapa);

                marcador.bindPopup(
                    `<strong>${escaparHtml(punto.titulo)}</strong><br>${escaparHtml(punto.direccion)}`
                );
            });

            if (coordenadas.length > 1) {
                L.polyline(coordenadas, {
                    color: '#4f46e5',
                    weight: 4,
                    opacity: .8,
                    lineJoin: 'round',
                    dashArray: '8 7'
                }).addTo(mapa);
            }

            mapa.fitBounds(coordenadas, { padding: [35, 35], maxZoom: 14 });
            estado.textContent = 'Seleccioná los marcadores para consultar cada ubicación.';
        } catch (error) {
            console.error('Error al cargar el recorrido:', error);
            mostrarError('Ocurrió un problema al cargar el recorrido. Intentá actualizar la página.');
        }
    };

    inicializarMapa();
})();
