import * as THREE from 'three';
import { PetScene } from './PetScene.js?v=20261026125900';

export class DogScene extends PetScene {
    constructor(manager, options = {}) {
        super(manager, {
            kind: 'dog',
            modelUrl: options.modelUrl,
            fallbackUrl: options.fallbackUrl,
            x: -1.18,
            delay: 0.02,
            breathOffset: 0,
            tailSpeed: 1.8,
            tailAmplitude: 0.16,
            modelYaw: -Math.PI / 2
        });
    }

    async init() {
        const loaded = await super.init();
        if (!loaded) return false;
        const idle = this.gltf.animations.find((clip) => /idle/i.test(clip.name)) ?? this.gltf.animations[0];
        if (idle && !this.manager.reducedMotion.matches) {
            const stableTracks = idle.tracks.filter((track) => {
                const target = track.name.split('.')[0].replace(/\[.*\]$/, '').toLowerCase();
                const stableBone = /spine|chest|neck|head|tail|ear/.test(target);
                const limbBone = /leg|foot|paw|knee|toe|ankle|thigh|arm|hand|shoulder|elbow/.test(target);
                return stableBone && !limbBone;
            });
            if (stableTracks.length) {
                const sittingIdle = new THREE.AnimationClip(idle.name, idle.duration, stableTracks);
                this.mixer = new THREE.AnimationMixer(this.model);
                const action = this.mixer.clipAction(sittingIdle);
                action.setLoop(THREE.LoopRepeat, Infinity);
                action.clampWhenFinished = false;
                action.play();
            }
        }
        return true;
    }
}
