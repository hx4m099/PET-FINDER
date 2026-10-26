export function initCommunityBoard() {
    const board = document.getElementById('community-board');
    if (!board) return;
    const buttons = [...document.querySelectorAll('[data-report-filter]')];
    const cards = [...board.querySelectorAll('[data-report-card]')];
    const status = document.getElementById('board-filter-status');

    buttons.forEach((button) => {
        button.addEventListener('click', () => {
            const filter = button.dataset.reportFilter;
            let visible = 0;
            cards.forEach((card) => {
                const show = filter === 'all' || card.dataset.species === filter;
                card.hidden = !show;
                if (show) visible += 1;
            });
            buttons.forEach((item) => {
                const active = item === button;
                item.classList.toggle('is-active', active);
                item.setAttribute('aria-pressed', String(active));
            });
            if (status) status.textContent = `${visible} ${visible === 1 ? 'aviso visible' : 'avisos visibles'}.`;
        });
    });
}
