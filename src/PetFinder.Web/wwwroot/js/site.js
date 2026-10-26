(() => {
    'use strict';

    const mainNav = document.getElementById('mainNav');
    if (!mainNav || !window.bootstrap) return;

    mainNav.querySelectorAll('a.nav-link').forEach((link) => {
        link.addEventListener('click', () => {
            if (window.matchMedia('(max-width: 991.98px)').matches) {
                bootstrap.Collapse.getOrCreateInstance(mainNav).hide();
            }
        });
    });
})();


(() => {
    'use strict';

    const cards = document.querySelectorAll('main .pet-card');
    cards.forEach((card) => {
        const image = card.querySelector('.pet-card-image img');
        if (!image) return;

        const species = card.querySelector('.pet-card-body .eyebrow')?.textContent?.toLowerCase() ?? '';
        const isCat = species.includes('gato');
        const isDog = species.includes('perro');
        const localPhoto = isCat ? '/img/demo/gato-3.jpg' : isDog ? '/img/demo/perro-1.jpg' : null;
        const speciesPlaceholder = isCat ? '/img/especies/gato.svg' : isDog ? '/img/especies/perro.svg' : null;
        const frame = image.closest('.pet-card-image');
        const wrapper = image.closest('.img-reveal');

        if (frame) {
            frame.style.setProperty('position', 'relative', 'important');
            frame.style.setProperty('display', 'block', 'important');
            frame.style.setProperty('height', 'clamp(230px, 22vw, 290px)', 'important');
            frame.style.setProperty('overflow', 'hidden', 'important');
        }
        if (wrapper) {
            wrapper.style.setProperty('position', 'absolute', 'important');
            wrapper.style.setProperty('inset', '0', 'important');
            wrapper.style.setProperty('display', 'block', 'important');
            wrapper.style.setProperty('width', '100%', 'important');
            wrapper.style.setProperty('height', '100%', 'important');
        }
        const ensureVisible = () => {
            wrapper?.classList.remove('is-loading', 'is-error');
            wrapper?.classList.add('is-revealed');
            image.style.setProperty('position', 'relative', 'important');
            image.style.removeProperty('inset');
            image.style.setProperty('display', 'block', 'important');
            image.style.setProperty('width', '100%', 'important');
            image.style.setProperty('height', '100%', 'important');
            image.style.setProperty('object-fit', 'cover', 'important');
            image.style.setProperty('opacity', '1', 'important');
            image.style.setProperty('visibility', 'visible', 'important');
        };
        const revealPhoto = () => {
            ensureVisible();
            if (!wrapper || window.matchMedia('(prefers-reduced-motion: reduce)').matches || typeof image.animate !== 'function') return;
            const animation = image.animate([
                { clipPath: 'inset(0 100% 0 0)', transform: 'scale(1.035)' },
                { clipPath: 'inset(0 0 0 0)', transform: 'scale(1)' }
            ], { duration: 680, easing: 'cubic-bezier(.22,.72,.28,1)' });
            animation.addEventListener('finish', () => animation.cancel(), { once: true });
        };
        let fallbackTimer = 0;
        image.addEventListener('load', () => {
            window.clearTimeout(fallbackTimer);
            revealPhoto();
        }, { capture: true });
        image.addEventListener('error', (event) => {
            if (!localPhoto) return;
            event.stopImmediatePropagation();
            if (image.dataset.petFinderLocalFallback !== 'true') {
                image.dataset.petFinderLocalFallback = 'true';
                image.removeAttribute('srcset');
                image.src = localPhoto;
            } else if (speciesPlaceholder && image.dataset.petFinderSpeciesPlaceholder !== 'true') {
                image.dataset.petFinderSpeciesPlaceholder = 'true';
                image.src = speciesPlaceholder;
            }
        }, { capture: true });

        ensureVisible();
        if (image.complete) {
            if (image.naturalWidth > 0) revealPhoto();
            else if (localPhoto) image.src = localPhoto;
        } else if (localPhoto) {
            fallbackTimer = window.setTimeout(() => {
                if (!image.naturalWidth && image.dataset.petFinderLocalFallback !== 'true') {
                    image.dataset.petFinderLocalFallback = 'true';
                    image.removeAttribute('srcset');
                    image.src = localPhoto;
                }
            }, 6500);
        }
    });
})();
