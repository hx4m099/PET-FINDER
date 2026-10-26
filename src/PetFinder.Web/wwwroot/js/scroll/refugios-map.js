const earthRadiusKm = 6371.0088;
const clamp = (value, min, max) => Math.min(max, Math.max(min, value));
const normalize = (value) => String(value ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es');

function distanceKm(latA, lonA, latB, lonB) {
    const radians = Math.PI / 180;
    const phiA = latA * radians;
    const phiB = latB * radians;
    const deltaPhi = (latB - latA) * radians;
    const deltaLambda = (lonB - lonA) * radians;
    const a = (Math.sin(deltaPhi / 2) ** 2)
        + (Math.cos(phiA) * Math.cos(phiB) * (Math.sin(deltaLambda / 2) ** 2));
    return earthRadiusKm * 2 * Math.asin(Math.min(1, Math.sqrt(a)));
}

function coordinatesAreValid(item) {
    const lat = Number(item.latitud);
    const lon = Number(item.longitud);
    return Number.isFinite(lat) && Number.isFinite(lon) && lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180;
}

function readInitialRefugios(list) {
    return [...list.querySelectorAll('.refuge-list-item')].map((element) => ({
        id: element.dataset.refugioId,
        nombre: element.dataset.nombre,
        direccion: element.dataset.direccion,
        ciudad: element.dataset.ciudad,
        horario: element.dataset.horario,
        telefono: element.dataset.telefono,
        fotoUrl: element.dataset.fotoUrl,
        latitud: Number(element.dataset.latitud),
        longitud: Number(element.dataset.longitud),
        mascotasDisponibles: Number(element.dataset.mascotas),
        especiesDisponibles: (element.dataset.especies || '').split(',').filter(Boolean)
    })).filter((item) => item.id && coordinatesAreValid(item));
}

function makeElement(tag, className, text) {
    const element = document.createElement(tag);
    if (className) element.className = className;
    if (text !== undefined) element.textContent = text;
    return element;
}

function makePopup(item) {
    const card = makeElement('article', 'map-popup-card');
    const image = document.createElement('img');
    image.src = item.fotoUrl || '';
    image.alt = '';
    image.loading = 'lazy';
    image.addEventListener('error', () => image.classList.add('is-fallback'), { once: true });
    card.append(image);

    const title = makeElement('h3', '', item.nombre || 'Refugio aliado');
    const address = makeElement('p', 'map-popup-address', `${item.direccion || ''} · ${item.ciudad || ''}`);
    const hours = makeElement('p', 'map-popup-hours', item.horario || 'Horario por confirmar');
    const count = makeElement('p', 'map-popup-count', `${Number(item.mascotasDisponibles) || 0} mascotas disponibles · ubicación de ejemplo`);
    const action = makeElement('a', 'btn btn-primary btn-sm', 'Ver mascotas y agendar cita');
    action.href = `/Adopcion/Index?refugioId=${encodeURIComponent(item.id)}`;
    card.append(title, address, hours, count, action);
    return card;
}

function renderList(list, items, selectRefugio) {
    const fragment = document.createDocumentFragment();
    items.forEach((item) => {
        const li = makeElement('li', 'refuge-list-item');
        li.dataset.refugioId = item.id;
        const button = makeElement('button', 'refuge-list-card');
        button.type = 'button';
        button.dataset.refugioSelect = item.id;
        button.setAttribute('aria-label', `Enfocar ${item.nombre} en el mapa`);
        const pin = makeElement('span', 'refuge-pin', '⌖');
        pin.setAttribute('aria-hidden', 'true');
        const copy = makeElement('span', 'refuge-card-copy');
        copy.append(
            makeElement('strong', '', item.nombre),
            makeElement('small', '', `${item.direccion || ''} · ${item.ciudad || ''}`),
            makeElement('small', '', `${Number(item.mascotasDisponibles) || 0} mascotas · ${item.horario || 'Horario por confirmar'}`)
        );
        const distance = makeElement('small', 'refuge-distance');
        distance.dataset.distanceFor = item.id;
        copy.append(distance);
        const arrow = makeElement('span', 'refuge-chevron', '↗');
        arrow.setAttribute('aria-hidden', 'true');
        button.append(pin, copy, arrow);
        button.addEventListener('click', () => selectRefugio(item, true));
        button.addEventListener('pointerenter', () => selectRefugio(item, false), { passive: true });
        button.addEventListener('focus', () => selectRefugio(item, false));

        const link = makeElement('a', 'refuge-catalog-link', 'Ver mascotas y agendar cita →');
        link.href = `/Adopcion/Index?refugioId=${encodeURIComponent(item.id)}`;
        li.append(button, link);
        fragment.append(li);
    });
    list.replaceChildren(fragment);
}

export function initRefugiosMap() {
    const container = document.getElementById('refugios-map');
    if (!container) return;

    const list = document.getElementById('refugio-list');
    const status = document.getElementById('map-status');
    const locationStatus = document.getElementById('location-status');
    const speciesFilter = document.getElementById('refugio-species');
    const searchInput = document.getElementById('refugio-search');
    const count = document.getElementById('refuge-result-count');
    const emptyState = document.getElementById('refuge-empty-state');
    const locationButton = document.getElementById('use-my-location');
    const wheelButton = document.getElementById('map-wheel-toggle');
    const instructions = document.getElementById('map-instructions');

    if (!window.L || !list || !status) {
        status.textContent = 'El mapa interactivo no se pudo cargar. La lista de refugios y sus enlaces siguen disponibles.';
        container.classList.add('map-unavailable');
        return;
    }

    const latitude = Number(container.dataset.centerLat);
    const longitude = Number(container.dataset.centerLon);
    const zoom = clamp(Number.parseInt(container.dataset.zoom, 10) || 12, 3, 18);
    const center = Number.isFinite(latitude) && Number.isFinite(longitude) ? [latitude, longitude] : [10.463, -73.253];
    let map;
    let tileLayer;
    let refugios = [];
    const markers = new Map();
    let origin = null;

    try {
        map = window.L.map(container, {
            scrollWheelZoom: false,
            keyboard: true,
            tap: true,
            maxBounds: [[10.39, -73.32], [10.53, -73.18]],
            maxBoundsViscosity: 0.85
        }).setView(center, zoom);
        tileLayer = window.L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener noreferrer">OpenStreetMap</a> contributors'
        }).addTo(map);
        tileLayer.on('tileerror', () => {
            status.textContent = 'No se pudieron cargar algunos mosaicos del mapa. Puedes seguir usando la lista de refugios.';
            status.classList.add('is-warning');
        });
        tileLayer.on('load', () => {
            if (!status.classList.contains('is-warning')) status.textContent = 'Mapa interactivo listo. Selecciona un refugio para ver su ubicación.';
        });
        map.on('click', () => {
            map.scrollWheelZoom.enable();
            wheelButton?.setAttribute('aria-pressed', 'true');
            if (wheelButton) wheelButton.textContent = 'Desactivar zoom con rueda';
            if (instructions) instructions.textContent = 'Zoom con rueda activado. Puedes desactivarlo para volver al desplazamiento normal.';
        });
        map.getContainer().addEventListener('wheel', (event) => {
            if (event.ctrlKey) map.scrollWheelZoom.enable();
            else if (wheelButton?.getAttribute('aria-pressed') !== 'true') map.scrollWheelZoom.disable();
        }, { capture: true, passive: true });
    } catch {
        container.classList.add('map-unavailable');
        status.textContent = 'El mapa interactivo no se pudo iniciar. La lista de refugios y sus enlaces siguen disponibles.';
        return;
    }

    wheelButton?.addEventListener('click', () => {
        const enabled = wheelButton.getAttribute('aria-pressed') !== 'true';
        map.scrollWheelZoom[enabled ? 'enable' : 'disable']();
        wheelButton.setAttribute('aria-pressed', String(enabled));
        wheelButton.textContent = enabled ? 'Desactivar zoom con rueda' : 'Activar zoom con rueda';
        if (instructions) instructions.textContent = enabled
            ? 'Zoom con rueda activado. Desactívalo para volver al desplazamiento normal.'
            : 'El zoom de rueda está desactivado para no interrumpir el desplazamiento de la página.';
    });

    function updateActive(item, openPopup) {
        list.querySelectorAll('.refuge-list-item').forEach((element) => {
            const active = element.dataset.refugioId === item.id;
            element.classList.toggle('is-active', active);
            if (active) element.setAttribute('aria-current', 'true');
            else element.removeAttribute('aria-current');
        });
        const marker = markers.get(item.id);
        if (!marker) return;
        map.flyTo([Number(item.latitud), Number(item.longitud)], Math.max(map.getZoom(), 14), { duration: 0.6 });
        if (openPopup) marker.openPopup();
    }

    function setMarkers(items) {
        markers.forEach((marker) => map.removeLayer(marker));
        markers.clear();
        items.forEach((item) => {
            if (!coordinatesAreValid(item)) return;
            const marker = window.L.marker([Number(item.latitud), Number(item.longitud)], {
                icon: window.L.divIcon({
                    className: 'refuge-marker-shell',
                    html: '<span class="refuge-map-marker" aria-hidden="true">⌖</span>',
                    iconSize: [42, 48],
                    iconAnchor: [21, 44]
                }),
                title: item.nombre,
                alt: `Ubicación de ${item.nombre}`,
                keyboard: true
            }).addTo(map);
            marker.bindPopup(makePopup(item), { maxWidth: 280, minWidth: 210, closeButton: true });
            marker.on('click', () => {
                const card = list.querySelector(`[data-refugio-id="${CSS.escape(item.id)}"]`);
                card?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
                updateActive(item, false);
            });
            markers.set(item.id, marker);
        });

        if (markers.size > 1) {
            const group = window.L.featureGroup([...markers.values()]);
            map.fitBounds(group.getBounds().pad(0.15), { maxZoom: 14, animate: false });
        } else if (items[0] && coordinatesAreValid(items[0])) {
            map.setView([Number(items[0].latitud), Number(items[0].longitud)], Math.max(zoom, 14));
        }
    }

    function applyFilters() {
        const query = normalize(searchInput?.value);
        const species = speciesFilter?.value ?? 'all';
        const filtered = refugios.filter((item) => {
            const searchable = normalize(`${item.nombre} ${item.direccion} ${item.ciudad}`);
            const matchesQuery = !query || searchable.includes(query);
            const speciesNames = (item.especiesDisponibles ?? []).map(normalize);
            const matchesSpecies = species === 'all' || speciesNames.includes(species);
            return matchesQuery && matchesSpecies;
        });

        list.querySelectorAll('.refuge-list-item').forEach((element) => {
            element.hidden = !filtered.some((item) => item.id === element.dataset.refugioId);
        });
        markers.forEach((marker, id) => {
            const visible = filtered.some((item) => item.id === id);
            if (visible && !map.hasLayer(marker)) marker.addTo(map);
            else if (!visible && map.hasLayer(marker)) map.removeLayer(marker);
        });
        if (count) count.textContent = String(filtered.length);
        if (emptyState) emptyState.hidden = filtered.length > 0;
    }

    function setOrigin(userLatitude, userLongitude) {
        origin = { lat: userLatitude, lon: userLongitude };
        refugios = [...refugios].sort((a, b) => distanceKm(userLatitude, userLongitude, Number(a.latitud), Number(a.longitud))
            - distanceKm(userLatitude, userLongitude, Number(b.latitud), Number(b.longitud)));
        renderList(list, refugios, updateActive);
        refugios.forEach((item) => {
            const distance = distanceKm(userLatitude, userLongitude, Number(item.latitud), Number(item.longitud));
            const element = list.querySelector(`[data-distance-for="${CSS.escape(item.id)}"]`);
            if (element) element.textContent = `${distance.toFixed(1)} km de ti`;
        });
        applyFilters();
        locationStatus.textContent = 'Refugios ordenados por distancia aproximada en línea recta.';
    }

    refugios = readInitialRefugios(list);
    setMarkers(refugios);
    applyFilters();

    async function loadRefugios(coords = null) {
        status.textContent = 'Cargando ubicaciones…';
        const url = new URL(container.dataset.apiUrl, window.location.origin);
        if (coords) {
            url.searchParams.set('latitud', String(coords.latitude));
            url.searchParams.set('longitud', String(coords.longitude));
        }
        const response = await fetch(url, { headers: { Accept: 'application/json' }, credentials: 'same-origin' });
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const data = await response.json();
        const desdeApi = Array.isArray(data) ? data.filter(coordinatesAreValid) : [];
        if (coords) origin = { lat: coords.latitude, lon: coords.longitude };
        if (desdeApi.length === 0) {
            applyFilters();
            status.textContent = refugios.length > 0
                ? 'El directorio usa los datos disponibles localmente; el mapa y los enlaces siguen activos.'
                : 'No hay ubicaciones disponibles; el directorio y los enlaces siguen visibles.';
            return;
        }
        refugios = desdeApi;
        setMarkers(refugios);
        renderList(list, refugios, updateActive);
        if (origin) {
            refugios.forEach((item) => {
                const distance = distanceKm(origin.lat, origin.lon, Number(item.latitud), Number(item.longitud));
                const element = list.querySelector(`[data-distance-for="${CSS.escape(item.id)}"]`);
                if (element) element.textContent = `${distance.toFixed(1)} km de ti`;
            });
        }
        applyFilters();
        if (refugios.length === 0) status.textContent = 'No hay ubicaciones disponibles; el directorio y los enlaces siguen visibles.';
    }

    searchInput?.addEventListener('input', applyFilters, { passive: true });
    speciesFilter?.addEventListener('change', applyFilters);
    locationButton?.addEventListener('click', () => {
        if (!('geolocation' in navigator)) {
            locationStatus.textContent = 'Este navegador no ofrece geolocalización. Puedes usar el mapa y los filtros.';
            return;
        }
        locationButton.disabled = true;
        locationStatus.textContent = 'Solicitando ubicación…';
        navigator.geolocation.getCurrentPosition(async (position) => {
            const coords = { latitude: position.coords.latitude, longitude: position.coords.longitude };
            try {
                await loadRefugios(coords);
                setOrigin(coords.latitude, coords.longitude);
            } catch {
                setOrigin(coords.latitude, coords.longitude);
                status.textContent = 'No se pudo actualizar el mapa. La lista quedó ordenada por cercanía con los datos disponibles.';
                status.classList.add('is-warning');
            } finally {
                locationButton.disabled = false;
            }
        }, (error) => {
            locationButton.disabled = false;
            const message = error.code === error.PERMISSION_DENIED
                ? 'No se concedió permiso de ubicación. Puedes seguir usando el mapa sin compartirla.'
                : 'No pudimos obtener tu ubicación. Puedes seguir usando el mapa y los filtros.';
            locationStatus.textContent = message;
        }, { enableHighAccuracy: false, timeout: 10000, maximumAge: 300000 });
    });

    loadRefugios().catch(() => {
        status.textContent = 'El mapa o el directorio dinámico no se pudo cargar. La lista inicial y sus enlaces siguen disponibles.';
        status.classList.add('is-warning');
        container.classList.add('map-unavailable');
    });
}
