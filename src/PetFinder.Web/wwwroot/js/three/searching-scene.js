import * as THREE from 'three';
import { SceneManager } from './SceneManager.js';

function createPawTexture() {
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = 64;
    const context = canvas.getContext('2d');
    context.fillStyle = '#d76c43';
    context.beginPath();
    context.ellipse(32, 43, 13, 10, 0, 0, Math.PI * 2);
    [[18, 23], [28, 16], [39, 16], [49, 24]].forEach(([x, y]) => {
        context.moveTo(x + 5, y + 5);
        context.ellipse(x, y, 6, 8, -0.45, 0, Math.PI * 2);
    });
    context.fill();
    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    return texture;
}

export class SearchingScene extends SceneManager {
    init() {
        if (!super.init()) return false;
        this.camera.position.set(0, 0, 4.2);
        this.camera.lookAt(0, 0, 0);
        this.camera.updateProjectionMatrix();
        this.root = new THREE.Group();
        this.scene.add(this.root);
        const texture = createPawTexture();
        this.particleTextures.set('searching-paw', texture);
        this.sprites = [];

        for (let index = 0; index < 3; index += 1) {
            const material = new THREE.SpriteMaterial({ map: texture, transparent: true, opacity: 0.86, depthWrite: false });
            const sprite = new THREE.Sprite(material);
            const scale = 0.32 - index * 0.045;
            const phase = index * (Math.PI * 2 / 3);
            sprite.scale.set(scale, scale, 1);
            sprite.position.set(Math.cos(phase) * 0.34, Math.sin(phase) * 0.34, 0.1 * index);
            this.root.add(sprite);
            this.sprites.push(sprite);
        }

        this.searchActor = {
            root: this.root,
            update: (_delta, elapsed) => this.animateSearch(elapsed)
        };
        this.addActor(this.searchActor);
        this.render();
        return true;
    }

    animateSearch(elapsed) {
        if (!this.root || this.reducedMotion.matches) return;
        this.root.rotation.z = Math.sin(elapsed * 0.55) * 0.08;
        this.sprites.forEach((sprite, index) => {
            const phase = elapsed * 0.72 + index * (Math.PI * 2 / 3);
            sprite.position.set(Math.cos(phase) * 0.34, Math.sin(phase) * 0.34, 0.1 * index);
            const breathe = 1 + Math.sin(phase * 1.4) * 0.08;
            sprite.scale.setScalar((0.32 - index * 0.045) * breathe);
        });
    }
}

export function initSearchingScenes() {
    document.querySelectorAll('[data-searching-scene]').forEach((host) => {
        const scene = new SearchingScene(host);
        if (scene.init()) window.addEventListener('pagehide', () => scene.dispose(), { once: true });
    });
}
