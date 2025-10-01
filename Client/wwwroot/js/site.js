// Update cart badge
(function () {
    const el = document.getElementById('cart-count');
    if (!el) return;
    fetch('/Cart?handler=Count', { credentials: 'same-origin' })
        .then(r => r.json()).then(d => {
            if (!d || !d.ok) return;
            const n = parseInt(d.count || 0, 10);
            el.textContent = n;
            el.classList.toggle('d-none', n <= 0);
        }).catch(() => { });
})();
