import { PetScene } from './PetScene.js?v=20261026125900';

export class CatScene extends PetScene {
    constructor(manager, options = {}) {
        super(manager, {
            kind: 'cat',
            modelUrl: options.modelUrl,
            x: 1.18,
            delay: 0.24,
            breathOffset: 1.2,
            tailSpeed: 0.52,
            tailAmplitude: 0.11,
            modelYaw: -Math.PI / 2
        });
    }
}
