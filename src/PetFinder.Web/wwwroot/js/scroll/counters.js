const formatter = new Intl.NumberFormat('es-CO');

export function initCounters() {
    const counters = [...document.querySelectorAll('[data-counter]')];
    if (!counters.length) return;

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const setFinal = (element) => {
        const target = Number.parseInt(element.dataset.counter, 10);
        if (Number.isFinite(target)) element.textContent = formatter.format(target);
    };

    if (reducedMotion || !('IntersectionObserver' in window)) {
        counters.forEach(setFinal);
        return;
    }

    const observer = new IntersectionObserver((entries, currentObserver) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;
            const element = entry.target;
            const target = Number.parseInt(element.dataset.counter, 10);
            currentObserver.unobserve(element);
            if (!Number.isFinite(target) || target <= 0) {
                setFinal(element);
                return;
            }

            const startedAt = performance.now();
            const duration = 1150;
            const tick = (now) => {
                const progress = Math.min(1, (now - startedAt) / duration);
                const eased = 1 - ((1 - progress) ** 3);
                element.textContent = formatter.format(Math.round(target * eased));
                if (progress < 1) requestAnimationFrame(tick);
                else element.textContent = formatter.format(target);
            };
            requestAnimationFrame(tick);
        });
    }, { threshold: 0.5 });

    counters.forEach((element) => {
        const target = Number.parseInt(element.dataset.counter, 10);
        if (Number.isFinite(target)) element.textContent = '0';
        observer.observe(element);
    });
}
