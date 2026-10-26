import { initPageLoader } from './motion/page-loader.js';
import { initImageReveal } from './motion/image-reveal.js';
import { initReveal } from './motion/reveal.js';
import { initCounters } from './motion/counters.js';
import { initParallax } from './motion/parallax.js';
import { initScrollStory } from './motion/scroll-story.js';
import { initCustomCursor } from './motion/cursor.js';
import { initMagneticButtons } from './motion/magnetic.js';
import { initScrollProgress } from './scroll/scroll-progress.js';
import { initNavbar } from './scroll/navbar.js';
import { initRefugiosMap } from './scroll/refugios-map.js';
import { initFeaturedPets } from './scroll/featured-pets.js';
import { initCommunityBoard } from './scroll/community-board.js';
import { initFaq } from './scroll/faq.js';
import { initBackToTop } from './scroll/back-to-top.js';

const initializers = [
    initPageLoader,
    initImageReveal,
    initReveal,
    initCounters,
    initScrollProgress,
    initNavbar,
    initParallax,
    initScrollStory,
    initFeaturedPets,
    initCommunityBoard,
    initFaq,
    initBackToTop,
    initRefugiosMap,
    initCustomCursor,
    initMagneticButtons
];

document.documentElement.classList.add('js-enabled');
initializers.forEach((initialize) => {
    try {
        initialize();
    } catch (error) {
        console.warn(`PetFinder: no se pudo iniciar ${initialize.name}; se conserva el contenido estático.`, error);
    }
});

// La escena 3D de resultados es una mejora opcional; un fallo de WebGL/CDN no debe
// impedir que se inicialicen imágenes, reveals, mapa ni el resto de interacciones.
if (document.querySelector('[data-searching-scene]')) {
    import('./three/searching-scene.js')
        .then(({ initSearchingScenes }) => initSearchingScenes())
        .catch((error) => console.warn('PetFinder: la escena 3D de resultados no está disponible.', error));
}
