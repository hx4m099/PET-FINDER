export function initNavbar() {
    const header = document.querySelector('[data-smart-header]');
    if (!header) return;

    const links = [...document.querySelectorAll('[data-scrollspy]')];
    const sections = links
        .map((link) => {
            const id = new URL(link.href, window.location.href).hash.slice(1);
            return id ? { id, link, section: document.getElementById(id) } : null;
        })
        .filter((item) => item?.section);

    let lastY = window.scrollY;
    let frame = 0;
    const update = () => {
        frame = 0;
        const y = Math.max(0, window.scrollY);
        header.classList.toggle('is-scrolled', y > 12);
        const hero = document.getElementById('hero');
        const hideAfter = hero ? hero.getBoundingClientRect().bottom + y : 180;

        if (y < 90 || header.contains(document.activeElement)) {
            header.classList.remove('is-hidden');
        } else if (y > lastY + 5 && y > hideAfter) {
            header.classList.add('is-hidden');
        } else if (y < lastY - 5) {
            header.classList.remove('is-hidden');
        }
        lastY = y;

        let current = null;
        sections.forEach((item) => {
            if (item.section.getBoundingClientRect().top <= header.offsetHeight + 120) current = item;
        });
        links.forEach((link) => {
            const active = current?.link === link;
            if (active) link.setAttribute('aria-current', 'location');
            else link.removeAttribute('aria-current');
        });
    };
    const requestUpdate = () => {
        if (!frame) frame = window.requestAnimationFrame(update);
    };

    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', requestUpdate, { passive: true });
    header.addEventListener('focusin', () => header.classList.remove('is-hidden'));
    requestUpdate();
}
