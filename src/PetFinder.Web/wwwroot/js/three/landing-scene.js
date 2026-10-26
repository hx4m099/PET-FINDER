import { SceneManager } from './SceneManager.js?v=20261026125900';
import { DogScene } from './DogScene.js?v=20261026125900';
import { CatScene } from './CatScene.js?v=20261026125900';

const hosts = document.querySelectorAll('[data-scene="landing"]');

for (const host of hosts) {
    const manager = new SceneManager(host);
    if (!manager.init()) continue;

    const onPageShow = (event) => {
        if (!event.persisted) return;
        manager.resize();
        manager.syncLoop();
    };
    const onPageHide = (event) => {
        if (event.persisted) return;
        manager.dispose();
        window.removeEventListener('pageshow', onPageShow);
    };
    window.addEventListener('pageshow', onPageShow);
    window.addEventListener('pagehide', onPageHide);

    new DogScene(manager, {
        modelUrl: host.dataset.dogModel,
        fallbackUrl: host.dataset.dogFallback
    });
    new CatScene(manager, { modelUrl: host.dataset.catModel });
    await manager.loadActors();
}
