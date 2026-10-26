export function initBackToTop() {
    const button = document.querySelector('[data-back-to-top]');
    const hero = document.getElementById('hero');
    if (!button || !hero) return;

    const update = () => {
        const passedHero = hero.getBoundingClientRect().bottom < 0;
        button.classList.toggle('is-visible', passedHero);
        button.setAttribute('tabindex', passedHero ? '0' : '-1');
        button.setAttribute('aria-hidden', String(!passedHero));
    };
    let frame = 0;
    const requestUpdate = () => {
        if (frame) return;
        frame = window.requestAnimationFrame(() => {
            frame = 0;
            update();
        });
    };

    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', requestUpdate, { passive: true });
    update();
}
