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
