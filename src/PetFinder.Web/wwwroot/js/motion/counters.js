import { initCounters as initCountersFallback } from '../scroll/counters.js';

const formatter = new Intl.NumberFormat('es-CO');

export function initCounters() {
    const elements = [...document.querySelectorAll('[data-counter]')];
    const gsap = window.gsap;
    const scrollTrigger = window.ScrollTrigger;
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    if (!elements.length) return;
    if (!gsap || !scrollTrigger || reducedMotion.matches) {
        initCountersFallback();
        return;
    }

    gsap.registerPlugin(scrollTrigger);
    const animations = [];
    elements.forEach((element) => {
        const target = Number.parseInt(element.dataset.counter, 10);
        if (!Number.isFinite(target)) return;
        const value = { count: 0 };
        const animation = gsap.to(value, {
            count: target,
            duration: 1.15,
            ease: 'power3.out',
            scrollTrigger: { trigger: element, start: 'top 90%', once: true },
            onUpdate: () => { element.textContent = formatter.format(Math.round(value.count)); },
            onComplete: () => { element.textContent = formatter.format(target); }
        });
        animations.push(animation);
    });

    reducedMotion.addEventListener?.('change', (event) => {
        if (!event.matches) return;
        animations.forEach((animation) => {
            animation.scrollTrigger?.kill();
            animation.progress(1).kill();
        });
        elements.forEach((element) => {
            const target = Number.parseInt(element.dataset.counter, 10);
            if (Number.isFinite(target)) element.textContent = formatter.format(target);
        });
    });
}
