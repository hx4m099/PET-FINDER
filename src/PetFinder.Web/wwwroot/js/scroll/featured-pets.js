const storageKey = 'petfinder-favoritos-v1';

function readFavorites() {
    try {
        const parsed = JSON.parse(window.localStorage.getItem(storageKey) || '[]');
        return new Set(Array.isArray(parsed) ? parsed.filter((id) => typeof id === 'string') : []);
    } catch {
        return new Set();
    }
}

export function initFeaturedPets() {
    const favorites = readFavorites();
    document.querySelectorAll('[data-favorite-button]').forEach((button) => {
        const id = button.dataset.favoriteId;
        const active = favorites.has(id);
        button.setAttribute('aria-pressed', String(active));
        button.querySelector('span')?.replaceChildren(document.createTextNode(active ? '♥' : '♡'));
        button.addEventListener('click', () => {
            const nextActive = button.getAttribute('aria-pressed') !== 'true';
            button.setAttribute('aria-pressed', String(nextActive));
            button.querySelector('span')?.replaceChildren(document.createTextNode(nextActive ? '♥' : '♡'));
            button.classList.toggle('is-favorite', nextActive);
            if (nextActive) favorites.add(id);
            else favorites.delete(id);
            try { window.localStorage.setItem(storageKey, JSON.stringify([...favorites])); } catch { /* El favorito sigue funcionando en esta sesión. */ }
        });
    });

    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches || !window.matchMedia('(pointer: fine)').matches) return;
    document.querySelectorAll('[data-tilt-card]').forEach((card) => {
        card.addEventListener('pointermove', (event) => {
            if (event.pointerType !== 'mouse') return;
            const rect = card.getBoundingClientRect();
            const x = (event.clientX - rect.left) / rect.width;
            const y = (event.clientY - rect.top) / rect.height;
            card.style.setProperty('--tilt-x', `${((0.5 - y) * 5).toFixed(2)}deg`);
            card.style.setProperty('--tilt-y', `${((x - 0.5) * 5).toFixed(2)}deg`);
            card.classList.add('is-tilting');
        }, { passive: true });
        card.addEventListener('pointerleave', () => {
            card.classList.remove('is-tilting');
            card.style.removeProperty('--tilt-x');
            card.style.removeProperty('--tilt-y');
        }, { passive: true });
    });

    document.querySelectorAll('[data-image-fallback]').forEach((image) => {
        image.addEventListener('error', () => image.classList.add('is-fallback'), { once: true });
    });
}
