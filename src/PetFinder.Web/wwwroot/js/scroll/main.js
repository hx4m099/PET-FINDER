import { initReveal } from './reveal.js';
import { initCounters } from './counters.js';
import { initScrollProgress } from './scroll-progress.js';
import { initNavbar } from './navbar.js';
import { initParallax } from './parallax.js';
import { initHeroScroll } from './hero-scroll.js';
import { initStepsConnector } from './steps-connector.js';
import { initRefugiosMap } from './refugios-map.js';
import { initFeaturedPets } from './featured-pets.js';
import { initCommunityBoard } from './community-board.js';
import { initFaq } from './faq.js';
import { initBackToTop } from './back-to-top.js';

const initializers = [
    initReveal,
    initCounters,
    initScrollProgress,
    initNavbar,
    initParallax,
    initHeroScroll,
    initStepsConnector,
    initFeaturedPets,
    initCommunityBoard,
    initFaq,
    initBackToTop,
    initRefugiosMap
];

let enhanced = true;
initializers.forEach((initialize) => {
    try { initialize(); }
    catch { enhanced = false; }
});

// La clase solo activa estados visuales iniciales ocultos después de preparar los observers.
if (enhanced) document.documentElement.classList.add('js-enabled');
