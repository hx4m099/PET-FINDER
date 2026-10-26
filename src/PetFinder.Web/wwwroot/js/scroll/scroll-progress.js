export function initScrollProgress() {
    const progress = document.querySelector('[data-scroll-progress]');
    const fill = progress?.querySelector('span');
    if (!progress || !fill) return;

    let frame = 0;
    let settleTimer = 0;
    const update = () => {
        frame = 0;
        const scroller = document.scrollingElement;
        if (!scroller) return;
        const available = Math.max(1, scroller.scrollHeight - window.innerHeight);
        const ratio = Math.max(0, Math.min(1, window.scrollY / available));
        fill.style.transform = `scaleX(${ratio})`;
        progress.setAttribute('aria-valuenow', String(Math.round(ratio * 100)));
        progress.classList.add('is-updating');
        window.clearTimeout(settleTimer);
        settleTimer = window.setTimeout(() => progress.classList.remove('is-updating'), 140);
    };
    const requestUpdate = () => {
        if (!frame) frame = window.requestAnimationFrame(update);
    };

    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', requestUpdate, { passive: true });
    requestUpdate();
}
