const DESKTOP_MOTION = '(min-width: 768px) and (prefers-reduced-motion: no-preference)';

export function initScrollStory() {
    const gsap = window.gsap;
    const scrollTrigger = window.ScrollTrigger;
    if (!gsap || !scrollTrigger) return;
    gsap.registerPlugin(scrollTrigger);

    const media = gsap.matchMedia();
    media.add(DESKTOP_MOTION, () => {
        const hero = document.getElementById('hero');
        const canvas = hero?.querySelector('.hero-3d-canvas');
        if (hero && canvas) {
            gsap.fromTo(canvas,
                { y: 0, rotationZ: 0, scale: 1, opacity: 1 },
                {
                    y: 42,
                    rotationZ: -3,
                    scale: 0.95,
                    opacity: 0.12,
                    ease: 'none',
                    scrollTrigger: { trigger: hero, start: 'top top', end: 'bottom top', scrub: 0.75 }
                });
        }

        const stepGrid = document.querySelector('.steps-grid');
        const progressPath = stepGrid?.querySelector('.steps-connector-progress');
        if (stepGrid && progressPath) {
            gsap.fromTo(progressPath,
                { strokeDasharray: 1, strokeDashoffset: 1 },
                {
                    strokeDashoffset: 0,
                    ease: 'none',
                    scrollTrigger: { trigger: stepGrid, start: 'top 76%', end: 'bottom 44%', scrub: 0.55 }
                });
        }

        document.querySelectorAll('[data-scroll-story]').forEach((story) => {
            const progress = story.querySelector('[data-story-progress]');
            const steps = [...story.querySelectorAll('[data-story-step]')];
            if (!progress || !steps.length) return;

            const timeline = gsap.timeline({
                scrollTrigger: {
                    trigger: story,
                    start: 'top top+=82',
                    end: () => `+=${Math.round(window.innerHeight * 1.55)}`,
                    pin: story,
                    pinSpacing: true,
                    scrub: 0.65,
                    anticipatePin: 1,
                    invalidateOnRefresh: true
                }
            });
            timeline.fromTo(progress, { scaleX: 0 }, { scaleX: 1, ease: 'none', duration: 1 }, 0);
            steps.forEach((step, index) => {
                const position = index * 0.32;
                timeline.fromTo(step,
                    { y: 18, autoAlpha: 0.58 },
                    { y: 0, autoAlpha: 1, duration: 0.24, ease: 'power3.out' },
                    position);
                timeline.to(step, { scale: 1.018, duration: 0.12, ease: 'power2.out' }, position + 0.22);
                timeline.to(step, { scale: 1, duration: 0.14, ease: 'power2.out' }, position + 0.34);
            });
        });
    });

    return media;
}
