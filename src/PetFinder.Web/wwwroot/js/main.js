import { initImageReveal } from './motion/image-reveal.js';

try {
    initImageReveal();
} catch (error) {
    document.querySelectorAll('.img-reveal').forEach((wrapper) => {
        wrapper.classList.add('is-revealed');
        wrapper.classList.remove('is-loading');
    });
    console.warn('PetFinder: el sistema de revelado de imágenes usará su contenido estático.', error);
}
