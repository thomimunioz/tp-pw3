document.addEventListener('submit', function (evento) {
    const formulario = evento.target;

    if (formulario.matches('.form-cancelar')) {
        const confirmo = confirm('¿Seguro que querés cancelar este envío?');
        if (!confirmo) {
            evento.preventDefault();
        }
    }
});