import { SceneManager } from './SceneManager.js';
import { DogScene } from './DogScene.js';
import { CatScene } from './CatScene.js';

const hosts = document.querySelectorAll('[data-scene="landing"]');

for (const host of hosts) {
    const manager = new SceneManager(host);
    if (!manager.init()) continue;

    new DogScene(manager, {
        modelUrl: host.dataset.dogModel,
        fallbackUrl: host.dataset.dogFallback
    });
    new CatScene(manager, { modelUrl: host.dataset.catModel });
    await manager.loadActors();

    window.addEventListener('pagehide', () => manager.dispose(), { once: true });
}
