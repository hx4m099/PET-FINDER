const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

export function initHeroScroll() {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const hero = document.getElementById('hero');
    const canvas = hero?.querySelector('.hero-3d-canvas');
    if (!hero || !canvas) return;

    let visible = true;
    let frame = 0;
    const observer = new IntersectionObserver(([entry]) => {
        visible = entry.isIntersecting;
        if (visible) requestUpdate();
    }, { threshold: 0.01 });
    observer.observe(hero);

    const update = () => {
        frame = 0;
        if (!visible) return;
        const heroHeight = Math.max(1, hero.offsetHeight);
        const progress = clamp(window.scrollY / (heroHeight * 0.82), 0, 1);
        canvas.style.setProperty('--hero-scroll-opacity', String(1 - progress * 0.92));
        canvas.style.setProperty('--hero-scroll-scale', String(1 - progress * 0.12));
    };
    function requestUpdate() {
        if (!frame) frame = window.requestAnimationFrame(update);
    }

    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', requestUpdate, { passive: true });
    requestUpdate();
}
