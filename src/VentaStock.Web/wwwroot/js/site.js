document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-password-toggle]');
    if (!button) return;

    const input = document.getElementById(button.getAttribute('aria-controls'));
    if (!input) return;

    const showPassword = input.type === 'password';
    input.type = showPassword ? 'text' : 'password';
    button.textContent = showPassword ? 'Ocultar' : 'Mostrar';
    button.setAttribute('aria-pressed', String(showPassword));
    button.setAttribute('aria-label', `${showPassword ? 'Ocultar' : 'Mostrar'} contraseña`);
});

document.addEventListener('keydown', (event) => {
    if (event.target.matches('[data-no-exponent]') && ['e', 'E'].includes(event.key)) {
        event.preventDefault();
    }
});

document.addEventListener('paste', (event) => {
    if (!event.target.matches('[data-no-exponent]')) return;

    const pastedText = event.clipboardData?.getData('text') ?? '';
    if (/[eE]/.test(pastedText)) {
        event.preventDefault();
    }
});
