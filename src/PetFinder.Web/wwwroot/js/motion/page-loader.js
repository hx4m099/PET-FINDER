const MAX_VISIBLE_MS = 900;
const MIN_VISIBLE_MS = 180;

export function initPageLoader() {
    if (document.readyState === 'complete') return;
    const loader = document.createElement('div');
    loader.className = 'page-loader';
    loader.dataset.pageLoader = 'true';
    loader.setAttribute('aria-hidden', 'true');
    loader.innerHTML = '<div class="page-loader__paw"><span class="paw-pad paw-toe-1"></span><span class="paw-pad paw-toe-2"></span><span class="paw-pad paw-toe-3"></span><span class="paw-pad paw-toe-4"></span><span class="paw-pad paw-center"></span></div><span class="page-loader__label">Preparando PetFinder</span>';
    document.body.prepend(loader);

    const startedAt = performance.now();
    let finished = false;
    let maxTimer = 0;
    let hideTimer = 0;
    const remove = () => {
        if (finished) return;
        finished = true;
        window.clearTimeout(maxTimer);
        window.clearTimeout(hideTimer);
        const wait = Math.max(0, MIN_VISIBLE_MS - (performance.now() - startedAt));
        hideTimer = window.setTimeout(() => {
            loader.classList.add('is-leaving');
            window.setTimeout(() => loader.remove(), 300);
        }, wait);
    };

    window.requestAnimationFrame(() => loader.classList.add('is-visible'));
    window.addEventListener('load', remove, { once: true });
    maxTimer = window.setTimeout(remove, MAX_VISIBLE_MS);
}
