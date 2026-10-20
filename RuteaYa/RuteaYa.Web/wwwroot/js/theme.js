document.getElementById("tema-toggle").addEventListener("click", function () {
    var actual = document.documentElement.getAttribute("data-theme");
    var nuevo = actual === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", nuevo);
    localStorage.setItem("tema", nuevo);
});
