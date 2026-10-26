import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { SceneManager } from './SceneManager.js';

const loader = new GLTFLoader();
const MODEL_HEIGHT = 2.35;
const FLOOR_Y = -1.2;
const ENTER_DURATION = 0.82;
const JUMP_DURATION = 0.54;
const JUMP_HEIGHT = 0.18;

export class PetScene extends SceneManager {
    constructor(manager, options) {
        super(manager);
        this.manager = manager;
        this.options = options;
        this.root = new THREE.Group();
        this.root.name = `${options.kind}-scene-root`;
        this.model = null;
        this.mixer = null;
        this.headBone = null;
        this.spineBone = null;
        this.tailBone = null;
        this.earBones = [];
        this.baseEarRotations = [];
        this.blinkTargets = [];
        this.hovered = false;
        this.ready = false;
        this.entranceStart = 0;
        this.jumpStartedAt = null;
        this.baseHeadQuaternion = null;
        this.baseSpineScale = null;
        this.baseTailQuaternion = null;
        this.root.position.set(options.x, FLOOR_Y, 0);
        this.manager.addActor(this);
    }

    async init() {
        let gltf;
        try {
            gltf = await loader.loadAsync(this.options.modelUrl);
        } catch (primaryError) {
            console.warn(`PetFinder: no se pudo cargar ${this.options.kind}; se intentará el GLB de respaldo.`, primaryError);
            if (!this.options.fallbackUrl) return false;
            try {
                gltf = await loader.loadAsync(this.options.fallbackUrl);
            } catch (fallbackError) {
                console.warn(`PetFinder: tampoco se pudo cargar el respaldo de ${this.options.kind}.`, fallbackError);
                return false;
            }
        }

        this.gltf = gltf;
        this.model = gltf.scene;
        this.model.name = `${this.options.kind}-model`;
        this.polishMaterials();
        this.normalizeAndPlaceModel();
        this.findRigTargets();
        this.manager.scene.add(this.root);
        this.ready = true;
        return true;
    }

    polishMaterials() {
        this.model.traverse((object) => {
            if (!object.isMesh) return;
            object.castShadow = true;
            object.receiveShadow = true;
            const polish = (material) => {
                if (!material) return material;
                const physical = new THREE.MeshPhysicalMaterial({
                    color: material.color?.clone() ?? 0xffffff,
                    roughness: Math.max(0.42, Math.min(0.72, material.roughness ?? 0.58)),
                    metalness: Math.min(material.metalness ?? 0, 0.12),
                    emissive: material.emissive?.clone() ?? 0x000000,
                    emissiveIntensity: material.emissiveIntensity ?? 1,
                    opacity: material.opacity ?? 1,
                    transparent: material.transparent ?? false,
                    alphaTest: material.alphaTest ?? 0,
                    side: material.side ?? THREE.FrontSide,
                    vertexColors: material.vertexColors ?? false,
                    flatShading: material.flatShading ?? false
                });
                for (const key of ['map', 'alphaMap', 'aoMap', 'bumpMap', 'displacementMap', 'emissiveMap', 'envMap', 'lightMap', 'metalnessMap', 'normalMap', 'roughnessMap']) {
                    if (key in physical && key in material) physical[key] = material[key];
                }
                if (material.normalScale) physical.normalScale.copy(material.normalScale);
                if (material.emissiveMap) physical.emissiveMap = material.emissiveMap;
                physical.name = material.name;
                physical.depthTest = material.depthTest;
                physical.depthWrite = material.depthWrite;
                physical.toneMapped = material.toneMapped;
                physical.wireframe = material.wireframe;
                physical.clearcoat = 0.16;
                physical.clearcoatRoughness = 0.52;
                physical.sheen = 0.16;
                physical.sheenColor = new THREE.Color('#fff1df');
                physical.needsUpdate = true;
                return physical;
            };
            object.material = Array.isArray(object.material)
                ? object.material.map(polish)
                : polish(object.material);
        });
    }

    normalizeAndPlaceModel() {
        const before = new THREE.Box3().setFromObject(this.model);
        const height = Math.max(before.getSize(new THREE.Vector3()).y, 0.1);
        this.model.scale.setScalar(MODEL_HEIGHT / height);
        this.model.rotation.y = this.options.modelYaw ?? -Math.PI / 2;
        this.model.updateMatrixWorld(true);

        const bounds = new THREE.Box3().setFromObject(this.model);
        const center = bounds.getCenter(new THREE.Vector3());
        this.model.position.x -= center.x;
        this.model.position.y -= bounds.min.y;
        this.model.position.z -= center.z;
        this.root.add(this.model);
        this.root.updateMatrixWorld(true);
    }

    findRigTargets() {
        const bones = [];
        this.model.traverse((object) => {
            if (object.isBone) bones.push(object);
            if (!object.isMesh || !object.morphTargetDictionary || !object.morphTargetInfluences) return;
            for (const [name, index] of Object.entries(object.morphTargetDictionary)) {
                if (/blink|eye.?close|lid.?close/i.test(name)) this.blinkTargets.push({ mesh: object, index });
            }
        });

        const pick = (pattern) => bones.find((bone) => pattern.test(bone.name));
        this.headBone = pick(/head/i) ?? null;
        this.spineBone = pick(/spine|chest|body/i) ?? null;
        this.tailBone = pick(/tail/i) ?? null;
        this.earBones = bones.filter((bone) => /ear/i.test(bone.name));
        this.baseEarRotations = this.earBones.map((bone) => bone.rotation.z);
        this.baseHeadQuaternion = this.headBone?.quaternion.clone() ?? null;
        this.baseSpineScale = this.spineBone?.scale.clone() ?? null;
        this.baseTailQuaternion = this.tailBone?.quaternion.clone() ?? null;
    }

    startEntrance(elapsed = 0) {
        this.entranceStart = elapsed + this.options.delay;
        if (this.manager.reducedMotion.matches) {
            this.root.scale.setScalar(1);
            return;
        }
        this.root.scale.setScalar(0.001);
        this.root.position.y = FLOOR_Y - 0.18;
    }

    update(delta, elapsed, pointer) {
        if (!this.ready) return;
        this.mixer?.update(delta);
        this.animateEntrance(elapsed);

        const pointerActive = pointer.x >= -1 && pointer.x <= 1 && pointer.y >= -1 && pointer.y <= 1;
        const cursorX = pointerActive ? THREE.MathUtils.clamp(pointer.x, -1, 1) : 0;
        const cursorY = pointerActive ? THREE.MathUtils.clamp(pointer.y, -1, 1) : 0;
        const targetYaw = cursorX * 0.055 + (this.hovered ? 0.045 : 0);
        const targetTilt = -cursorY * 0.025 + (this.hovered ? -0.035 : 0);
        this.root.rotation.y = THREE.MathUtils.damp(this.root.rotation.y, targetYaw, 4.2, delta);
        this.root.rotation.z = THREE.MathUtils.damp(this.root.rotation.z, targetTilt, 4.2, delta);

        if (this.headBone && this.baseHeadQuaternion) {
            const gaze = new THREE.Quaternion().setFromEuler(new THREE.Euler(-cursorY * 0.035, cursorX * 0.075, 0));
            const pose = this.mixer ? this.headBone.quaternion.clone() : this.baseHeadQuaternion;
            this.headBone.quaternion.copy(pose).multiply(gaze);
        }
        if (this.spineBone && this.baseSpineScale && !this.mixer) {
            const breath = 1 + Math.sin(elapsed * 1.55 + this.options.breathOffset) * 0.006;
            this.spineBone.scale.copy(this.baseSpineScale);
            this.spineBone.scale.y *= breath;
        }
        this.animateTail(elapsed);
        this.animateEars(elapsed);
        this.animateBlink(elapsed);
    }

    animateEntrance(elapsed) {
        if (this.manager.reducedMotion.matches) return;
        const progress = THREE.MathUtils.clamp((elapsed - this.entranceStart) / ENTER_DURATION, 0, 1);
        const shifted = progress - 1;
        const overshoot = 1 + 1.35 * shifted * shifted * shifted + 0.35 * shifted * shifted;
        this.root.scale.setScalar(Math.max(0.001, overshoot));
        let jump = 0;
        if (this.jumpStartedAt !== null) {
            const age = elapsed - this.jumpStartedAt;
            if (age >= JUMP_DURATION) this.jumpStartedAt = null;
            else if (age >= 0) jump = Math.sin(Math.PI * age / JUMP_DURATION) * JUMP_HEIGHT;
        }
        this.root.position.y = FLOOR_Y - 0.18 * (1 - progress) + 0.025 * Math.sin(progress * Math.PI) + jump;
    }

    jump(elapsed = this.manager.elapsed) {
        if (!this.manager.reducedMotion.matches) this.jumpStartedAt = elapsed;
    }

    animateTail(elapsed) {
        if (!this.tailBone || !this.baseTailQuaternion) return;
        const sway = Math.sin(elapsed * this.options.tailSpeed + this.options.breathOffset) * this.options.tailAmplitude;
        const wiggle = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 1, 0), sway);
        this.tailBone.quaternion.copy(this.baseTailQuaternion).multiply(wiggle);
    }

    animateEars(elapsed) {
        if (!this.earBones.length) return;
        const phase = Math.sin(elapsed * 1.1 + this.options.breathOffset) * 0.035;
        this.earBones.forEach((bone, index) => {
            bone.rotation.z = (this.baseEarRotations[index] ?? 0) + Math.sin(elapsed * 0.7 + index * 0.8) * phase;
        });
    }

    animateBlink(elapsed) {
        if (!this.blinkTargets.length || this.manager.reducedMotion.matches) return;
        if (elapsed >= (this.nextBlinkAt ?? 4.2)) {
            this.blinkStartedAt = elapsed;
            this.nextBlinkAt = elapsed + 4.2 + Math.random() * 3.8;
        }
        if (this.blinkStartedAt === undefined) return;
        const age = elapsed - this.blinkStartedAt;
        const close = age < 0.065 ? age / 0.065 : age < 0.13 ? 1 - (age - 0.065) / 0.065 : 0;
        this.blinkTargets.forEach(({ mesh, index }) => { mesh.morphTargetInfluences[index] = Math.max(0, close); });
        if (age > 0.14) this.blinkStartedAt = undefined;
    }

    resize() {
        if (this.ready) this.root.updateMatrixWorld(true);
    }

    dispose() {
        this.mixer?.stopAllAction();
        this.manager.scene.remove(this.root);
        this.root.traverse((object) => {
            object.geometry?.dispose?.();
            const materials = Array.isArray(object.material) ? object.material : [object.material];
            materials.filter(Boolean).forEach((material) => material.dispose?.());
        });
    }
}
