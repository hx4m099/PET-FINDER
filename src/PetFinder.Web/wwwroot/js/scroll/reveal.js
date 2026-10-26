const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

export function initReveal() {
    const revealSelector = '[data-reveal], [data-story-step], .section-heading, .site-footer .footer-grid, .site-footer .footer-bottom';
    const elements = [...document.querySelectorAll(revealSelector)]
        .filter((element) => !element.closest('#hero'));
    if (!elements.length) return;

    if (reducedMotion.matches || !('IntersectionObserver' in window)) {
        elements.forEach((element) => element.classList.add('reveal-visible'));
        return;
    }

    const parents = new Map();
    elements.forEach((element) => {
        const parent = element.parentElement;
        const siblings = parents.get(parent) ?? [...parent.children].filter((child) => child.matches(revealSelector) && !child.closest('#hero'));
        parents.set(parent, siblings);
        const index = siblings.indexOf(element);
        if (!element.dataset.reveal && element.matches('.section-heading')) element.dataset.reveal = 'left';
        if (!element.dataset.reveal && siblings.length >= 3) {
            element.dataset.reveal = index === 0 ? 'left' : index === siblings.length - 1 ? 'right' : 'scale';
        }
        element.style.setProperty('--reveal-delay', `${Math.min(index, 4) * 95}ms`);
        element.classList.add('reveal-pending');
    });

    const observer = new IntersectionObserver((entries, currentObserver) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;
            entry.target.classList.add('reveal-visible');
            entry.target.classList.remove('reveal-pending');
            entry.target.style.removeProperty('--reveal-delay');
            currentObserver.unobserve(entry.target);
        });
    }, { threshold: 0.12, rootMargin: '0px 0px -6% 0px' });

    elements.forEach((element) => observer.observe(element));
    reducedMotion.addEventListener?.('change', (event) => {
        if (!event.matches) return;
        observer.disconnect();
        elements.forEach((element) => {
            element.classList.add('reveal-visible');
            element.classList.remove('reveal-pending');
            element.style.removeProperty('--reveal-delay');
        });
    });
}
