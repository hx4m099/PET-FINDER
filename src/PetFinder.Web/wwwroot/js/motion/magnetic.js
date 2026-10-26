const TARGETS = 'a.btn-primary, a.btn-soft, a.btn-light-outline, button.btn-primary, [data-magnetic]';

export function initMagneticButtons() {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const finePointer = window.matchMedia('(pointer: fine)').matches;
    const buttons = [...document.querySelectorAll(TARGETS)];
    if (!buttons.length) return;

    buttons.forEach((button) => {
        button.classList.add('magnetic-target');

        if (finePointer) {
            button.addEventListener('pointermove', (event) => {
                if (event.pointerType !== 'mouse') return;
                const rect = button.getBoundingClientRect();
                const offsetX = ((event.clientX - rect.left) / rect.width - 0.5) * 10;
                const offsetY = ((event.clientY - rect.top) / rect.height - 0.5) * 8;
                button.style.setProperty('--magnetic-x', `${offsetX.toFixed(1)}px`);
                button.style.setProperty('--magnetic-y', `${offsetY.toFixed(1)}px`);
                button.classList.add('is-magnetic');
            }, { passive: true });
            button.addEventListener('pointerleave', () => {
                button.classList.remove('is-magnetic');
                button.style.removeProperty('--magnetic-x');
                button.style.removeProperty('--magnetic-y');
            }, { passive: true });
        }

        button.addEventListener('pointerdown', (event) => {
            if (event.button !== 0 && event.pointerType === 'mouse') return;
            const rect = button.getBoundingClientRect();
            const diameter = Math.max(rect.width, rect.height) * 1.7;
            const ripple = document.createElement('span');
            ripple.className = 'button-ripple';
            ripple.setAttribute('aria-hidden', 'true');
            ripple.style.width = `${diameter}px`;
            ripple.style.height = `${diameter}px`;
            ripple.style.left = `${event.clientX - rect.left - diameter / 2}px`;
            ripple.style.top = `${event.clientY - rect.top - diameter / 2}px`;
            button.append(ripple);
            ripple.addEventListener('animationend', () => ripple.remove(), { once: true });
        });
    });
}
