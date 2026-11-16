document.addEventListener('submit', function (evento) {
    const formulario = evento.target;

    if (formulario.matches('.form-cancelar')) {
        const mensaje = formulario.dataset.confirmar || '¿Seguro que querés cancelar este envío?';
        const confirmo = confirm(mensaje);
        if (!confirmo) {
            evento.preventDefault();
        }
    }
});