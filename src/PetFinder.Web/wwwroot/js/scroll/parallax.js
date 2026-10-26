const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

export function initParallax() {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const elements = [...document.querySelectorAll('[data-parallax]')];
    if (!elements.length) return;

    const items = elements.map((element) => ({
        element,
        speed: Number.parseFloat(element.dataset.parallax) || 0.2,
        baseTransform: getComputedStyle(element).transform
    }));
    let frame = 0;
    let settleTimer = 0;

    const update = () => {
        frame = 0;
        const center = window.innerHeight / 2;
        items.forEach(({ element, speed, baseTransform }) => {
            const rect = element.getBoundingClientRect();
            const offset = clamp((rect.top + rect.height / 2 - center) * speed, -40, 40);
            const base = baseTransform === 'none' ? '' : ` ${baseTransform}`;
            element.style.transform = `translate3d(0, ${offset.toFixed(1)}px, 0)${base}`;
        });
        elements.forEach((element) => element.classList.add('parallax-moving'));
        window.clearTimeout(settleTimer);
        settleTimer = window.setTimeout(() => elements.forEach((element) => element.classList.remove('parallax-moving')), 140);
    };
    const requestUpdate = () => {
        if (!frame) frame = window.requestAnimationFrame(update);
    };

    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', requestUpdate, { passive: true });
    requestUpdate();
}
