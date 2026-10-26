const REDUCED_MOTION = '(prefers-reduced-motion: reduce)';
const REVEAL_DURATION = 0.48;
const STAGGER_SECONDS = 0.07;

export function initImageReveal() {
    const reducedMotion = window.matchMedia(REDUCED_MOTION);
    const preparedImages = new WeakSet();
    const revealObserver = 'IntersectionObserver' in window
        ? new IntersectionObserver((entries, observer) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                revealWithCss(entry.target);
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.16, rootMargin: '0px 0px -5% 0px' })
        : null;

    function revealWithCss(wrapper) {
        wrapper.classList.add('is-revealed');
        wrapper.classList.remove('is-loading');
        wrapper.style.removeProperty('--image-stagger-delay');
    }

    function prepareWrapper(wrapper, image) {
        if (preparedImages.has(image)) return;
        preparedImages.add(image);
        wrapper.classList.add('img-reveal');
        wrapper.dataset.imagePrepared = 'true';
        wrapper.classList.add('is-loading');

        const fallback = image.dataset.imageFallback;
        const reveal = () => {
            if (reducedMotion.matches) {
                revealWithCss(wrapper);
                return;
            }

            const gsap = window.gsap;
            if (gsap) {
                const siblings = [...(wrapper.parentElement?.querySelectorAll(':scope > .img-reveal') ?? [])];
                const delay = Math.min(Math.max(siblings.indexOf(wrapper), 0), 4) * STAGGER_SECONDS;
                gsap.fromTo(image,
                    { clipPath: 'inset(0 100% 0 0)', scale: 1.035, opacity: 0.98 },
                    {
                        clipPath: 'inset(0 0 0 0)',
                        scale: 1,
                        opacity: 1,
                        duration: REVEAL_DURATION,
                        delay,
                        ease: 'power2.out',
                        clearProps: 'clipPath,transform,opacity',
                        onStart: () => wrapper.classList.remove('is-loading'),
                        onComplete: () => revealWithCss(wrapper)
                    });
                return;
            }

            const siblings = [...(wrapper.parentElement?.querySelectorAll(':scope > .img-reveal') ?? [])];
            const delay = Math.min(Math.max(siblings.indexOf(wrapper), 0), 4) * STAGGER_SECONDS;
            wrapper.style.setProperty('--image-stagger-delay', `${delay}s`);
            if (revealObserver) revealObserver.observe(wrapper);
            else revealWithCss(wrapper);
        };

        const handleError = () => {
            if (fallback && image.dataset.fallbackAttempted !== 'true') {
                image.dataset.fallbackAttempted = 'true';
                image.removeAttribute('srcset');
                image.src = fallback;
                return;
            }
            wrapper.classList.remove('is-loading');
            wrapper.classList.add('is-error', 'is-revealed');
        };

        const handleLoad = () => {
            if (!image.naturalWidth) {
                handleError();
                return;
            }
            wrapper.classList.remove('is-error');
            reveal();
        };

        image.addEventListener('load', handleLoad);
        image.addEventListener('error', handleError);
        if (!image.getAttribute('src')?.trim() && !image.currentSrc) {
            handleError();
            return;
        }
        if (image.complete) {
            if (image.naturalWidth) handleLoad();
            else handleError();
        }
    }

    function initialize(target) {
        if (!(target instanceof Element)) return;
        let wrapper = target.matches('.img-reveal') ? target : target.closest('.img-reveal');
        let image = target.matches('img') ? target : wrapper?.querySelector('img');
        if (!image && target.matches('[data-image-reveal]')) image = target.querySelector('img');
        if (!image) return;

        if (!wrapper) {
            wrapper = document.createElement('span');
            wrapper.className = 'img-reveal';
            image.parentNode?.insertBefore(wrapper, image);
            wrapper.append(image);
        }
        wrapper.dataset.imageReveal = 'true';
        prepareWrapper(wrapper, image);
    }

    document.querySelectorAll('[data-image-reveal]').forEach(initialize);

    const mutationObserver = 'MutationObserver' in window ? new MutationObserver((records) => {
        records.forEach((record) => record.addedNodes.forEach((node) => {
            if (!(node instanceof Element)) return;
            if (node.matches('[data-image-reveal]')) initialize(node);
            node.querySelectorAll?.('[data-image-reveal]').forEach(initialize);
        }));
    }) : null;
    mutationObserver?.observe(document.body, { childList: true, subtree: true });

    reducedMotion.addEventListener?.('change', (event) => {
        if (!event.matches) return;
        revealObserver?.disconnect();
        document.querySelectorAll('.img-reveal[data-image-prepared="true"]').forEach(revealWithCss);
        window.gsap?.globalTimeline?.getChildren?.(true, true, false).forEach((animation) => {
            if (animation.scrollTrigger) animation.scrollTrigger.kill();
            animation.progress(1).pause();
        });
    });
}
