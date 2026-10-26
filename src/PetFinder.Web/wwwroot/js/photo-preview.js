(() => {
    'use strict';

    const urlInput = document.querySelector('[name="FotoUrl"]');
    const imageInput = document.querySelector('[name="Foto"]');
    const preview = document.getElementById('photo-preview-image');
    const placeholder = document.getElementById('photo-preview-placeholder');
    if (!preview || !placeholder) return;

    let objectUrl;
    const show = (url) => {
        preview.onload = () => { preview.hidden = false; placeholder.hidden = true; };
        preview.onerror = () => { preview.hidden = true; placeholder.hidden = false; placeholder.textContent = 'No se pudo cargar la vista previa. Revisa la imagen.'; };
        preview.src = url;
    };

    if (imageInput) {
        imageInput.addEventListener('change', () => {
            const file = imageInput.files?.[0];
            if (!file) return;
            if (objectUrl) URL.revokeObjectURL(objectUrl);
            objectUrl = URL.createObjectURL(file);
            show(objectUrl);
        });
    }

    if (urlInput) {
        urlInput.addEventListener('input', () => {
            if (imageInput?.files?.length) return;
            const value = urlInput.value.trim();
            if (!value) {
                preview.hidden = true;
                placeholder.hidden = false;
                placeholder.textContent = 'La vista previa aparecerá aquí';
                return;
            }
            show(value);
        });
    }
})();
