// Pide confirmación antes de aceptar un envío (aceptar escribe en la base de datos)
document.querySelectorAll("form[data-confirmar]").forEach(function (form) {
    form.addEventListener("submit", function (evento) {
        if (!window.confirm(form.dataset.confirmar)) {
            evento.preventDefault();
        }
    });
});
// Al cambiar de viaje en el select, recarga la lista con ese viaje
document.querySelectorAll("select[data-autoenviar]").forEach(function (select) {
    select.addEventListener("change", function () {
        select.form.submit();
    });
});