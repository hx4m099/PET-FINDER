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
                    end: () => `+=${Math.round(window.innerHeight * 2)}`,
                    pin: story,
                    pinSpacing: true,
                    scrub: 0.75,
                    anticipatePin: 0.5,
                    invalidateOnRefresh: true
                }
            });
            timeline.fromTo(progress, { scaleX: 0 }, { scaleX: 1, ease: 'none', duration: 1 }, 0);
            steps.forEach((step, index) => {
                const position = index * 0.31;
                timeline.fromTo(step,
                    { y: 12, autoAlpha: 0.76, scale: 0.99 },
                    { y: 0, autoAlpha: 1, scale: 1, duration: 0.34, ease: 'power2.out' },
                    position);
            });
        });
    });

    return media;
}
