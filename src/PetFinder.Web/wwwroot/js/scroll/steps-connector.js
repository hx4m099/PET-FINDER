const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

export function initStepsConnector() {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const section = document.querySelector('.steps-grid');
    const path = section?.querySelector('.steps-connector path');
    if (!section || !path) return;

    path.style.strokeDasharray = '1';
    path.style.strokeDashoffset = '1';
    let frame = 0;
    const update = () => {
        frame = 0;
        const rect = section.getBoundingClientRect();
        const progress = clamp((window.innerHeight * 0.83 - rect.top) / (rect.height + window.innerHeight * 0.35), 0, 1);
        path.style.strokeDashoffset = String(1 - progress);
    };
    const requestUpdate = () => {
        if (!frame) frame = window.requestAnimationFrame(update);
    };

    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', requestUpdate, { passive: true });
    requestUpdate();
}
