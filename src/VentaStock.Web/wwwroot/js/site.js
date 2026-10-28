document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-password-toggle]');
    if (!button) return;

    const input = document.getElementById(button.getAttribute('aria-controls'));
    if (!input) return;

    const showPassword = input.type === 'password';
    const showIcon = button.querySelector('[data-password-icon="show"]');
    const hideIcon = button.querySelector('[data-password-icon="hide"]');
    if (!showIcon || !hideIcon) return;

    input.type = showPassword ? 'text' : 'password';
    showIcon.hidden = showPassword;
    hideIcon.hidden = !showPassword;
    button.setAttribute('aria-pressed', String(showPassword));
    const label = `${showPassword ? 'Ocultar' : 'Mostrar'} contraseña`;
    button.setAttribute('aria-label', label);
    button.setAttribute('title', label);
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
