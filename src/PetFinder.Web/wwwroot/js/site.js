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
