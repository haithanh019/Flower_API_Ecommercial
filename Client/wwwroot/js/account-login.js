document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('togglePassword');
    const input = document.getElementById('Input_Password');
    if (btn && input) {
        btn.addEventListener('click', () => {
            const isPwd = input.type === 'password';
            input.type = isPwd ? 'text' : 'password';
            btn.setAttribute('aria-pressed', (!isPwd).toString());
            const i = btn.querySelector('i');
            if (i) {
                i.classList.toggle('fa-eye');
                i.classList.toggle('fa-eye-slash');
            }
        });
    }
});
