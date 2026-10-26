export function initFaq() {
    const items = [...document.querySelectorAll('.faq-item')];
    items.forEach((item) => {
        const summary = item.querySelector('summary');
        if (!summary) return;
        item.open = false;
        summary.setAttribute('aria-expanded', 'false');
        item.addEventListener('toggle', () => {
            summary.setAttribute('aria-expanded', String(item.open));
            if (!item.open) return;
            items.forEach((other) => {
                if (other === item || !other.open) return;
                other.open = false;
                other.querySelector('summary')?.setAttribute('aria-expanded', 'false');
            });
        });
    });
}
