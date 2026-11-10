document.querySelectorAll('[data-contar]').forEach(el => {
    const meta = parseInt(el.dataset.contar, 10) || 0;
    if (meta === 0 || window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        el.textContent = meta;
        return;
    }
    let actual = 0;
    const paso = Math.max(Math.floor(900 / meta), 40);
    const timer = setInterval(() => {
        el.textContent = ++actual;
        if (actual >= meta) clearInterval(timer);
    }, paso);
});