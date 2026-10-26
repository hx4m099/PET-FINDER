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

        const adoptionRoute = window.location.pathname.toLowerCase().startsWith('/adopcion');
        const adoptionCard = adoptionRoute
            ? wrapper.closest('.pet-card')
            : wrapper.closest('.adoption-section .pet-card');
        if (adoptionCard) {
            wrapper.style.setProperty('position', 'absolute', 'important');
            wrapper.style.setProperty('inset', '0', 'important');
            wrapper.style.setProperty('width', '100%', 'important');
            wrapper.style.setProperty('height', '100%', 'important');
            image.style.setProperty('position', 'absolute', 'important');
            image.style.setProperty('inset', '0', 'important');
            image.style.setProperty('width', '100%', 'important');
            image.style.setProperty('height', '100%', 'important');
            image.style.setProperty('object-fit', 'cover', 'important');
            image.style.setProperty('opacity', '1', 'important');
            image.style.setProperty('visibility', 'visible', 'important');
        }
        const declaredFallback = image.dataset.imageFallback;
        const speciesLabel = adoptionCard?.querySelector('.pet-card-body .eyebrow')?.textContent?.toLowerCase() ?? '';
        const inferredPhotoFallback = speciesLabel.includes('gato')
            ? '/img/demo/gato-3.jpg'
            : speciesLabel.includes('perro') ? '/img/demo/perro-1.jpg' : null;
        const fallback = adoptionCard ? inferredPhotoFallback ?? declaredFallback : declaredFallback;
        const inferredSpeciesPlaceholder = speciesLabel.includes('gato')
            ? '/img/especies/gato.svg'
            : speciesLabel.includes('perro') ? '/img/especies/perro.svg' : null;
        const placeholder = image.dataset.imagePlaceholder
            ?? (adoptionCard ? inferredSpeciesPlaceholder : null);
        let fallbackStage = 0;
        let slowLoadTimer = 0;
        let loadWatchObserver = null;
        const reveal = () => {
            if (adoptionCard) {
                revealWithCss(wrapper);
                if (!reducedMotion.matches && typeof image.animate === 'function') {
                    const animation = image.animate([
                        { clipPath: 'inset(0 100% 0 0)', transform: 'scale(1.035)' },
                        { clipPath: 'inset(0 0 0 0)', transform: 'scale(1)' }
                    ], { duration: 680, easing: 'cubic-bezier(.22,.72,.28,1)' });
                    animation.addEventListener('finish', () => animation.cancel(), { once: true });
                }
                return;
            }
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
            window.clearTimeout(slowLoadTimer);
            loadWatchObserver?.disconnect();
            if (fallbackStage === 0 && fallback) {
                fallbackStage = 1;
                image.dataset.fallbackAttempted = 'true';
                image.removeAttribute('srcset');
                image.src = fallback;
                return;
            }
            if (fallbackStage === 1 && placeholder) {
                fallbackStage = 2;
                image.removeAttribute('srcset');
                image.src = placeholder;
                return;
            }
            wrapper.classList.remove('is-loading');
            wrapper.classList.add('is-error', 'is-revealed');
        };

        const handleLoad = () => {
            window.clearTimeout(slowLoadTimer);
            loadWatchObserver?.disconnect();
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
        const timeoutMs = Number(image.dataset.imageTimeout) || (adoptionCard ? 6500 : 0);
        if (timeoutMs > 0 && fallback) {
            const armSlowLoadTimer = () => {
                slowLoadTimer = window.setTimeout(() => {
                    if (!image.naturalWidth && fallbackStage === 0) handleError();
                }, timeoutMs);
            };
            if ('IntersectionObserver' in window) {
                loadWatchObserver = new IntersectionObserver((entries, observer) => {
                    if (!entries.some((entry) => entry.isIntersecting)) return;
                    observer.disconnect();
                    if (!image.complete && !image.naturalWidth) armSlowLoadTimer();
                }, { rootMargin: '160px' });
                loadWatchObserver.observe(image);
            } else {
                armSlowLoadTimer();
            }
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
