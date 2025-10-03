// Nút +/- và auto submit
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('tr[data-id]').forEach(function (row) {
        const form = row.querySelector('form[method="post"]');
        if (!form) return;

        const input = form.querySelector('input[name="qty"]');
        const dec = form.querySelector('.js-dec');
        const inc = form.querySelector('.js-inc');

        function submitNow() { form.submit(); }

        dec?.addEventListener('click', function () {
            const v = Math.max(1, (parseInt(input.value || '1', 10) || 1) - 1);
            input.value = v; submitNow();
        });
        inc?.addEventListener('click', function () {
            const v = Math.max(1, (parseInt(input.value || '1', 10) || 1) + 1);
            input.value = v; submitNow();
        });
        input?.addEventListener('change', submitNow);
    });

    // Cập nhật badge giỏ (nếu có #cart-count)
    fetch('/Cart?handler=Count', { credentials: 'same-origin' })
        .then(r => r.json()).then(d => {
            const b = document.getElementById('cart-count');
            if (b && d && typeof d.count === 'number') {
                b.textContent = d.count;
                b.classList.toggle('d-none', d.count <= 0);
            }
        }).catch(() => { });
});
