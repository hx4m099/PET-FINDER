export function initCustomCursor() {
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const finePointer = window.matchMedia('(pointer: fine)');
    if (reducedMotion.matches || !finePointer.matches) return;

    const cursor = document.createElement('div');
    cursor.className = 'custom-cursor';
    cursor.setAttribute('aria-hidden', 'true');
    cursor.innerHTML = '<span class="custom-cursor__dot"></span><span class="custom-cursor__ring"></span>';
    document.body.append(cursor);
    document.documentElement.classList.add('has-custom-cursor');

    const dot = cursor.querySelector('.custom-cursor__dot');
    const ring = cursor.querySelector('.custom-cursor__ring');
    const point = { x: window.innerWidth / 2, y: window.innerHeight / 2 };
    const current = { ...point };
    let frame = 0;
    let visible = false;

    const render = () => {
        current.x += (point.x - current.x) * 0.27;
        current.y += (point.y - current.y) * 0.27;
        const transform = `translate3d(${current.x.toFixed(1)}px, ${current.y.toFixed(1)}px, 0)`;
        dot.style.transform = transform;
        ring.style.transform = transform;
        if (Math.abs(point.x - current.x) > 0.2 || Math.abs(point.y - current.y) > 0.2) frame = window.requestAnimationFrame(render);
        else frame = 0;
    };

    const onPointerMove = (event) => {
        if (event.pointerType && event.pointerType !== 'mouse') return;
        point.x = event.clientX;
        point.y = event.clientY;
        if (!visible) {
            visible = true;
            cursor.classList.add('is-visible');
        }
        if (!frame) frame = window.requestAnimationFrame(render);
    };

    const onPointerOut = (event) => {
        if (event.relatedTarget === null) {
            visible = false;
            cursor.classList.remove('is-visible', 'is-interactive', 'is-text');
        }
        const target = event.target instanceof Element ? event.target : null;
        if (!target) return;
        if (target.closest('a, button, [role="button"], input[type="submit"], summary, label')) cursor.classList.remove('is-interactive');
        if (target.closest('input:not([type="submit"]), textarea, select, [contenteditable="true"]')) cursor.classList.remove('is-text');
    };

    const onPointerOver = (event) => {
        const target = event.target instanceof Element ? event.target : null;
        if (!target) return;
        if (target.closest('a, button, [role="button"], input[type="submit"], summary, label')) cursor.classList.add('is-interactive');
        if (target.closest('input:not([type="submit"]), textarea, select, [contenteditable="true"]')) cursor.classList.add('is-text');
    };

    document.addEventListener('pointermove', onPointerMove, { passive: true });
    document.addEventListener('pointerover', onPointerOver, { passive: true });
    document.addEventListener('pointerout', onPointerOut, { passive: true });

    reducedMotion.addEventListener?.('change', (event) => {
        if (!event.matches) return;
        if (frame) window.cancelAnimationFrame(frame);
        document.removeEventListener('pointermove', onPointerMove);
        document.removeEventListener('pointerover', onPointerOver);
        document.removeEventListener('pointerout', onPointerOut);
        cursor.remove();
        document.documentElement.classList.remove('has-custom-cursor');
    });
}
