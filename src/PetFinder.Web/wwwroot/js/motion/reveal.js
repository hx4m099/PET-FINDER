import { initReveal as initRevealFallback } from '../scroll/reveal.js';

const REDUCED_MOTION = '(prefers-reduced-motion: reduce)';
const REVEAL_START = 'top 86%';
const REVEAL_DURATION = 0.72;
const STAGGER_SECONDS = 0.1;

export function initReveal() {
    const elements = [...document.querySelectorAll('[data-reveal]')];
    const gsap = window.gsap;
    const scrollTrigger = window.ScrollTrigger;
    const reducedMotion = window.matchMedia(REDUCED_MOTION);
    if (!elements.length) return;
    if (!gsap || !scrollTrigger || reducedMotion.matches) {
        document.documentElement.classList.add('motion-fallback');
        initRevealFallback();
        document.querySelectorAll('#hero .hero-title-accent').forEach((accent) => accent.classList.add('is-highlighted'));
        return;
    }

    gsap.registerPlugin(scrollTrigger);
    const animations = [];
    const groups = new Map();

    elements.forEach((element) => {
        const parent = element.parentElement;
        const siblings = groups.get(parent) ?? [...parent.children].filter((child) => child.matches('[data-reveal]'));
        groups.set(parent, siblings);
        const index = siblings.indexOf(element);
        if (!element.dataset.reveal && siblings.length >= 3) {
            element.dataset.reveal = index === 0 ? 'left' : index === siblings.length - 1 ? 'right' : 'scale';
        }
        element.style.setProperty('--reveal-delay', `${Math.min(index, 4) * STAGGER_SECONDS}s`);
        element.classList.add('reveal-pending');

        const direction = element.dataset.reveal;
        const from = { autoAlpha: 0, y: 26 };
        if (direction === 'left') { from.x = -28; from.y = 0; }
        else if (direction === 'right') { from.x = 28; from.y = 0; }
        else if (direction === 'scale') { from.scale = 0.94; from.y = 0; }

        const tween = gsap.fromTo(element, from, {
            autoAlpha: 1,
            x: 0,
            y: 0,
            scale: 1,
            duration: REVEAL_DURATION,
            delay: Math.min(index, 4) * STAGGER_SECONDS,
            ease: 'power3.out',
            clearProps: 'opacity,visibility,transform',
            scrollTrigger: { trigger: element, start: REVEAL_START, once: true },
            onComplete: () => {
                element.classList.add('reveal-visible');
                element.classList.remove('reveal-pending');
                element.style.removeProperty('--reveal-delay');
            }
        });
        animations.push(tween);
    });

    const hero = document.getElementById('hero');
    if (hero) {
        const eyebrow = hero.querySelector('.hero-copy > .eyebrow');
        const titleLines = [...hero.querySelectorAll('.hero-title-line')];
        const lead = hero.querySelector('.hero-lead');
        const actions = hero.querySelector('.hero-actions');
        const note = hero.querySelector('.hero-note');
        const floatingNotes = [...hero.querySelectorAll('.floating-note')];
        const orbits = [...hero.querySelectorAll('.hero-orbit-path')];
        const timeline = gsap.timeline({ defaults: { ease: 'power3.out' } });
        const drifts = [];

        if (eyebrow) timeline.fromTo(eyebrow, { autoAlpha: 0, y: 12 }, { autoAlpha: 1, y: 0, duration: 0.42 }, 0);
        if (titleLines.length) timeline.fromTo(titleLines,
            { autoAlpha: 0, y: 28, clipPath: 'inset(0 0 100% 0)' },
            { autoAlpha: 1, y: 0, clipPath: 'inset(0 0 0 0)', duration: 0.66, stagger: 0.12, clearProps: 'clipPath,transform,opacity' },
            0.1);
        if (lead) timeline.fromTo(lead, { autoAlpha: 0, y: 16 }, { autoAlpha: 1, y: 0, duration: 0.48 }, '-=0.32');
        if (actions) timeline.fromTo(actions, { autoAlpha: 0, y: 14 }, { autoAlpha: 1, y: 0, duration: 0.42 }, '-=0.25');
        if (note) timeline.fromTo(note, { autoAlpha: 0, y: 10 }, { autoAlpha: 1, y: 0, duration: 0.38 }, '-=0.22');
        if (floatingNotes.length) timeline.fromTo(floatingNotes, { autoAlpha: 0, y: 18, scale: 0.96 }, { autoAlpha: 1, y: 0, scale: 1, duration: 0.52, stagger: 0.12 }, '-=0.25');
        if (orbits.length) timeline.fromTo(orbits, { strokeDasharray: 1, strokeDashoffset: 1 }, { strokeDashoffset: 0, duration: 0.82, stagger: 0.12, clearProps: 'strokeDasharray,strokeDashoffset' }, '-=0.48');
        timeline.eventCallback('onComplete', () => {
            hero.querySelector('.hero-title-accent')?.classList.add('is-highlighted');
            drifts.forEach((drift) => drift.play());
        });
        animations.push(timeline);

        floatingNotes.forEach((floatingNote, index) => {
            const drift = gsap.to(floatingNote, { y: index ? 7 : -7, duration: 3.4 + index * 0.45, repeat: -1, yoyo: true, ease: 'sine.inOut', paused: true });
            drifts.push(drift);
            animations.push(drift);
        });
    }

    reducedMotion.addEventListener?.('change', (event) => {
        if (!event.matches) return;
        animations.forEach((animation) => {
            animation.scrollTrigger?.kill();
            animation.progress(1).kill();
        });
        elements.forEach((element) => {
            element.classList.add('reveal-visible');
            element.classList.remove('reveal-pending');
            element.style.removeProperty('--reveal-delay');
        });
    });
}
