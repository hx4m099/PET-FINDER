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
import { initSearchingScenes } from './three/searching-scene.js';

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
    initSearchingScenes,
    initRefugiosMap,
    initCustomCursor,
    initMagneticButtons
];

let enhanced = true;
initializers.forEach((initialize) => {
    try {
        initialize();
    } catch (error) {
        enhanced = false;
        console.warn(`PetFinder: no se pudo iniciar ${initialize.name}; se conserva el contenido estático.`, error);
    }
});

if (enhanced) document.documentElement.classList.add('js-enabled');
