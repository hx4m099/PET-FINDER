import * as THREE from 'three';

const MAX_PIXEL_RATIO = 2;
const CAMERA_MARGIN = 1.18;
const REDUCED_MOTION = '(prefers-reduced-motion: reduce)';

export class SceneManager {
    constructor(hostOrManager) {
        if (hostOrManager instanceof SceneManager) {
            this.parentManager = hostOrManager;
            this.host = hostOrManager.host;
            this.scene = hostOrManager.scene;
            this.camera = hostOrManager.camera;
            this.renderer = hostOrManager.renderer;
            this.actors = hostOrManager.actors;
            this.reducedMotion = hostOrManager.reducedMotion;
            this.isActor = true;
            return;
        }

        this.parentManager = null;
        this.isActor = false;
        this.host = hostOrManager;
        this.scene = new THREE.Scene();
        this.camera = new THREE.PerspectiveCamera(34, 1, 0.1, 60);
        this.renderer = null;
        this.actors = [];
        this.particles = [];
        this.raycaster = new THREE.Raycaster();
        this.pointer = new THREE.Vector2(2, 2);
        this.clock = new THREE.Clock();
        this.elapsed = 0;
        this.frameId = 0;
        this.visible = false;
        this.ready = false;
        this.reducedMotion = window.matchMedia(REDUCED_MOTION);
        this.contextLost = false;
        this.listeners = [];
        this.particleTextures = new Map();
        this.interactionSurface = null;
    }

    init() {
        if (!this.host || !('WebGLRenderingContext' in window)) return false;
        try {
            this.renderer = new THREE.WebGLRenderer({
                alpha: true,
                antialias: true,
                powerPreference: 'low-power',
                stencil: false
            });
        } catch {
            return false;
        }

        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, MAX_PIXEL_RATIO));
        this.renderer.setClearColor(0x000000, 0);
        this.renderer.outputColorSpace = THREE.SRGBColorSpace;
        this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
        this.renderer.toneMappingExposure = 1.18;
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        this.renderer.domElement.setAttribute('aria-hidden', 'true');
        this.renderer.domElement.setAttribute('role', 'presentation');
        this.host.appendChild(this.renderer.domElement);
        this.createLighting();
        this.createContactShadow();
        this.bindLifecycle();
        this.resize();
        this.render();
        return true;
    }

    createLighting() {
        this.scene.add(new THREE.HemisphereLight(0xfff4e8, 0x6c594b, 1.75));

        const key = new THREE.DirectionalLight(0xfff7ef, 3.1);
        key.position.set(-3.5, 5.5, 6);
        key.castShadow = true;
        key.shadow.mapSize.set(1024, 1024);
        key.shadow.camera.near = 0.5;
        key.shadow.camera.far = 18;
        key.shadow.bias = -0.00018;
        key.shadow.radius = 5;
        this.scene.add(key);

        const fill = new THREE.DirectionalLight(0xffe1c8, 1.05);
        fill.position.set(4, 2.4, 4.2);
        this.scene.add(fill);

        const rim = new THREE.PointLight(0xff995c, 16, 16, 2);
        rim.position.set(2.8, 2.2, -2.5);
        this.scene.add(rim);

        const front = new THREE.PointLight(0xffffff, 4.5, 12, 2);
        front.position.set(-0.5, 1.2, 5.5);
        this.scene.add(front);
    }

    createContactShadow() {
        const ground = new THREE.Mesh(
            new THREE.PlaneGeometry(12, 8),
            new THREE.ShadowMaterial({ color: 0x4a3022, opacity: 0.19 })
        );
        ground.rotation.x = -Math.PI / 2;
        ground.position.y = -1.22;
        ground.receiveShadow = true;
        ground.renderOrder = 2;
        this.ground = ground;
        this.scene.add(ground);
    }

    bindLifecycle() {
        this.resizeObserver = new ResizeObserver(() => this.resize());
        this.resizeObserver.observe(this.host);
        this.intersectionObserver = new IntersectionObserver((entries) => {
            this.visible = entries[0]?.isIntersecting ?? false;
            this.syncLoop();
        }, { threshold: 0.04 });
        this.intersectionObserver.observe(this.host);

        const visibility = () => this.syncLoop();
        const motion = () => this.syncLoop();
        document.addEventListener('visibilitychange', visibility, { passive: true });
        this.reducedMotion.addEventListener?.('change', motion);
        this.listeners.push(() => document.removeEventListener('visibilitychange', visibility));
        this.listeners.push(() => this.reducedMotion.removeEventListener?.('change', motion));

        this.renderer.domElement.addEventListener('webglcontextlost', (event) => {
            event.preventDefault();
            this.contextLost = true;
            this.stopLoop();
        }, { passive: false });
        this.renderer.domElement.addEventListener('webglcontextrestored', () => {
            this.contextLost = false;
            this.resize();
            this.syncLoop();
        }, { passive: true });

        this.interactionSurface = this.host.closest('.hero-art') ?? this.host;
        if (!this.reducedMotion.matches) {
            this.onPointerMove = (event) => {
                const rect = this.host.getBoundingClientRect();
                this.pointer.set(
                    ((event.clientX - rect.left) / Math.max(rect.width, 1)) * 2 - 1,
                    -((event.clientY - rect.top) / Math.max(rect.height, 1)) * 2 + 1
                );
                this.raycaster.setFromCamera(this.pointer, this.camera);
                const roots = this.actors.map((actor) => actor.root).filter(Boolean);
                const hits = roots.length ? this.raycaster.intersectObjects(roots, true) : [];
                const hitActor = hits.length
                    ? this.actors.find((actor) => actor.root && this.isDescendant(actor.root, hits[0].object))
                    : null;
                this.actors.forEach((actor) => { actor.hovered = actor === hitActor; });
            };
            this.onPointerLeave = () => {
                this.pointer.set(2, 2);
                this.actors.forEach((actor) => { actor.hovered = false; });
            };
            this.onClick = (event) => {
                if (event.target.closest?.('a, button, input, select, textarea')) return;
                const actor = this.actors.find((item) => item.hovered);
                if (actor) {
                    actor.jump?.(this.elapsed);
                    this.spawnParticles(actor);
                }
            };
            this.interactionSurface.addEventListener('pointermove', this.onPointerMove, { passive: true });
            this.interactionSurface.addEventListener('pointerleave', this.onPointerLeave, { passive: true });
            this.interactionSurface.addEventListener('click', this.onClick, { passive: true });
            this.listeners.push(() => this.interactionSurface.removeEventListener('pointermove', this.onPointerMove));
            this.listeners.push(() => this.interactionSurface.removeEventListener('pointerleave', this.onPointerLeave));
            this.listeners.push(() => this.interactionSurface.removeEventListener('click', this.onClick));
        }
    }

    addActor(actor) {
        actor.manager = this;
        this.actors.push(actor);
        return actor;
    }

    isDescendant(root, object) {
        for (let current = object; current; current = current.parent) {
            if (current === root) return true;
        }
        return false;
    }

    async loadActors() {
        const results = await Promise.allSettled(this.actors.map((actor) => actor.init()));
        const loaded = results.filter((result) => result.status === 'fulfilled' && result.value !== false).length;
        results.filter((result) => result.status === 'rejected').forEach((result) => console.warn('PetFinder: falló la inicialización de una mascota.', result.reason?.stack ?? String(result.reason)));
        if (loaded === this.actors.length && loaded > 0) {
            this.ready = true;
            this.host.closest('.hero-art')?.classList.add('scene-ready');
            this.fitCamera();
            this.actors.forEach((actor) => actor.startEntrance?.(this.elapsed));
            this.syncLoop();
        } else if (loaded > 0) {
            this.fitCamera();
            this.syncLoop();
        }
        return loaded;
    }

    fitCamera() {
        const bounds = new THREE.Box3();
        for (const actor of this.actors) {
            if (!actor.root) continue;
            const animatedScale = actor.root.scale.clone();
            try {
                actor.root.scale.setScalar(1);
                actor.root.updateWorldMatrix(true, true);
                bounds.expandByObject(actor.root);
            } finally {
                actor.root.scale.copy(animatedScale);
                actor.root.updateWorldMatrix(true, true);
            }
        }
        if (bounds.isEmpty()) return;

        const size = bounds.getSize(new THREE.Vector3());
        const center = bounds.getCenter(new THREE.Vector3());
        const aspect = Math.max(this.host.clientWidth / Math.max(this.host.clientHeight, 1), 0.5);
        const verticalFov = THREE.MathUtils.degToRad(this.camera.fov);
        const horizontalFov = 2 * Math.atan(Math.tan(verticalFov / 2) * aspect);
        const heightDistance = (size.y * 0.5 * CAMERA_MARGIN) / Math.tan(verticalFov / 2);
        const widthDistance = (size.x * 0.5 * CAMERA_MARGIN) / Math.tan(horizontalFov / 2);
        const distance = Math.max(heightDistance, widthDistance) + size.z * 0.52;

        this.camera.near = Math.max(0.05, distance / 120);
        this.camera.far = Math.max(40, distance * 8);
        this.camera.position.set(center.x, center.y + size.y * 0.02, center.z + distance);
        this.camera.lookAt(center);
        this.camera.updateProjectionMatrix();
        this.ground.position.y = bounds.min.y + 0.012;
    }

    resize() {
        if (!this.renderer) return;
        const width = Math.max(this.host.clientWidth, 1);
        const height = Math.max(this.host.clientHeight, 1);
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, MAX_PIXEL_RATIO));
        this.renderer.setSize(width, height, false);
        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();
        this.actors.forEach((actor) => actor.resize?.(width, height));
        if (this.ready) this.fitCamera();
        this.render();
    }

    syncLoop() {
        if (!this.renderer || this.contextLost) return;
        const shouldAnimate = this.visible && !document.hidden && !this.reducedMotion.matches;
        if (shouldAnimate) {
            if (!this.frameId) {
                this.clock.start();
                this.frameId = requestAnimationFrame((time) => this.tick(time));
            }
        } else {
            this.stopLoop();
            this.render();
        }
    }

    tick() {
        this.frameId = 0;
        if (!this.visible || document.hidden || this.reducedMotion.matches || this.contextLost) {
            this.render();
            return;
        }
        const delta = Math.min(this.clock.getDelta() || 0, 0.05);
        this.elapsed += delta;
        this.actors.forEach((actor) => actor.update?.(delta, this.elapsed, this.pointer));
        this.updateParticles(delta);
        this.render();
        this.frameId = requestAnimationFrame(() => this.tick());
    }

    render() {
        if (this.renderer && !this.contextLost) this.renderer.render(this.scene, this.camera);
    }

    createParticleTexture(kind) {
        if (this.particleTextures.has(kind)) return this.particleTextures.get(kind);
        const canvas = document.createElement('canvas');
        canvas.width = canvas.height = 64;
        const context = canvas.getContext('2d');
        context.fillStyle = kind === 'heart' ? '#ed7954' : '#ef8255';
        if (kind === 'heart') {
            context.beginPath();
            context.moveTo(32, 53);
            context.bezierCurveTo(23, 45, 7, 33, 10, 22);
            context.bezierCurveTo(12, 10, 27, 8, 32, 20);
            context.bezierCurveTo(37, 8, 52, 10, 54, 22);
            context.bezierCurveTo(57, 33, 41, 45, 32, 53);
            context.fill();
        } else {
            context.beginPath();
            context.ellipse(32, 42, 13, 10, 0, 0, Math.PI * 2);
            [[18, 23], [28, 16], [39, 16], [49, 24]].forEach(([x, y]) => {
                context.moveTo(x + 5, y + 5);
                context.ellipse(x, y, 6, 8, -0.45, 0, Math.PI * 2);
            });
            context.fill();
        }
        const texture = new THREE.CanvasTexture(canvas);
        texture.colorSpace = THREE.SRGBColorSpace;
        this.particleTextures.set(kind, texture);
        return texture;
    }

    spawnParticles(actor) {
        if (this.reducedMotion.matches) return;
        for (let index = 0; index < 6; index += 1) {
            const kind = index % 2 ? 'heart' : 'paw';
            const material = new THREE.SpriteMaterial({
                map: this.createParticleTexture(kind),
                transparent: true,
                depthWrite: false,
                opacity: 0.94
            });
            const sprite = new THREE.Sprite(material);
            const origin = actor.root.getWorldPosition(new THREE.Vector3());
            sprite.position.set(origin.x + (Math.random() - 0.5) * 0.48, origin.y + 0.6, origin.z + 0.55);
            const size = 0.13 + Math.random() * 0.09;
            sprite.scale.set(size, size, 1);
            this.scene.add(sprite);
            this.particles.push({ sprite, age: 0, duration: 0.9 + Math.random() * 0.35, drift: (Math.random() - 0.5) * 0.55 });
        }
        this.syncLoop();
    }

    updateParticles(delta) {
        for (let index = this.particles.length - 1; index >= 0; index -= 1) {
            const particle = this.particles[index];
            particle.age += delta;
            const progress = Math.min(particle.age / particle.duration, 1);
            particle.sprite.position.y += delta * (0.56 + (1 - progress) * 0.22);
            particle.sprite.position.x += particle.drift * delta;
            particle.sprite.material.opacity = 1 - progress;
            particle.sprite.scale.multiplyScalar(1 + delta * 0.28);
            if (progress >= 1) {
                this.scene.remove(particle.sprite);
                particle.sprite.material.dispose();
                this.particles.splice(index, 1);
            }
        }
    }

    stopLoop() {
        if (this.frameId) cancelAnimationFrame(this.frameId);
        this.frameId = 0;
    }

    dispose() {
        this.stopLoop();
        this.resizeObserver?.disconnect();
        this.intersectionObserver?.disconnect();
        this.listeners.forEach((disposeListener) => disposeListener());
        this.actors.forEach((actor) => actor.dispose?.());
        this.particles.forEach(({ sprite }) => {
            this.scene.remove(sprite);
            sprite.material.dispose();
        });
        this.particleTextures.forEach((texture) => texture.dispose());
        this.scene.traverse((object) => {
            object.geometry?.dispose?.();
            const materials = Array.isArray(object.material) ? object.material : [object.material];
            materials.filter(Boolean).forEach((material) => material.dispose?.());
        });
        this.renderer?.dispose();
        this.renderer?.domElement.remove();
        this.host.closest('.hero-art')?.classList.remove('scene-ready');
    }
}
