// ---------- Tema claro / oscuro: se aplica apenas carga (el script va en el <head>) ----------
(function () {
    var guardado = null;
    try { guardado = localStorage.getItem("tema"); } catch (e) { }

    var prefiereClaro = window.matchMedia("(prefers-color-scheme: light)").matches;
    var tema = guardado || (prefiereClaro ? "light" : "dark");

    document.documentElement.setAttribute("data-theme", tema);
})();

// ---------- Botones: cambiar tema y abrir/cerrar el menú en celular ----------
document.addEventListener("DOMContentLoaded", function () {
    var botonTema = document.getElementById("tema-toggle");
    var botonMenu = document.getElementById("menu-toggle");
    var menu = document.getElementById("menu-lateral");
    var fondo = document.getElementById("menu-fondo");

    botonTema.addEventListener("click", function () {
        var actual = document.documentElement.getAttribute("data-theme");
        var nuevo = actual === "dark" ? "light" : "dark";

        document.documentElement.setAttribute("data-theme", nuevo);
        try { localStorage.setItem("tema", nuevo); } catch (e) { }
    });

    function abrirMenu() {
        menu.classList.add("is-open");
        fondo.classList.add("is-open");
        document.body.classList.add("menu-abierto");
        botonMenu.setAttribute("aria-expanded", "true");
    }

    function cerrarMenu() {
        menu.classList.remove("is-open");
        fondo.classList.remove("is-open");
        document.body.classList.remove("menu-abierto");
        botonMenu.setAttribute("aria-expanded", "false");
    }

    botonMenu.addEventListener("click", function () {
        if (menu.classList.contains("is-open")) {
            cerrarMenu();
        } else {
            abrirMenu();
        }
    });

    fondo.addEventListener("click", cerrarMenu);

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            cerrarMenu();
        }
    });

    window.addEventListener("resize", function () {
        if (window.innerWidth > 800) {
            cerrarMenu();
        }
    });
});