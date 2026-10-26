import * as THREE from 'three';

const hosts = [...document.querySelectorAll('[data-scene="landing"]')];
const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

for (const host of hosts) {
    if (!('WebGLRenderingContext' in window)) continue;

    let renderer;
    try {
        renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true, powerPreference: 'low-power' });
    } catch {
        continue;
    }

    const width = Math.max(host.clientWidth, 1);
    const height = Math.max(host.clientHeight, 1);
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(34, width / height, 0.1, 40);
    camera.position.set(0, 0, 10.5);

    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.5));
    renderer.setSize(width, height, false);
    renderer.outputColorSpace = THREE.SRGBColorSpace;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.25;
    renderer.domElement.setAttribute('aria-hidden', 'true');
    renderer.domElement.setAttribute('role', 'presentation');
    host.appendChild(renderer.domElement);

    scene.add(new THREE.HemisphereLight(0xfff5e9, 0x718f72, 2.2));
    const keyLight = new THREE.DirectionalLight(0xffffff, 2.6);
    keyLight.position.set(-3, 5, 8);
    scene.add(keyLight);
    const rimLight = new THREE.PointLight(0xffb67e, 20, 20);
    rimLight.position.set(4, 1, 5);
    scene.add(rimLight);

    const sphereGeometry = new THREE.SphereGeometry(1, 32, 24);
    const material = (color, roughness = 0.62) => new THREE.MeshStandardMaterial({ color, roughness, metalness: 0.02 });
    const dogFur = material('#cf8c5f');
    const dogLight = material('#f5d7bd');
    const dogDark = material('#603f32', 0.35);
    const catFur = material('#8b9d7e');
    const catLight = material('#dce2d1');
    const noseMaterial = material('#493c37', 0.3);
    const eyeMaterial = new THREE.MeshStandardMaterial({ color: '#30322f', roughness: 0.22 });
    const eyeShine = new THREE.MeshBasicMaterial({ color: '#ffffff' });

    function oval(parent, colorMaterial, position, scale, geometry = sphereGeometry) {
        const mesh = new THREE.Mesh(geometry, colorMaterial);
        mesh.position.set(...position);
        mesh.scale.set(...scale);
        mesh.castShadow = true;
        parent.add(mesh);
        return mesh;
    }

    function addEyes(parent, spacing = 0.29, y = 0.04) {
        for (const x of [-spacing, spacing]) {
            oval(parent, eyeMaterial, [x, y, 0.86], [0.075, 0.09, 0.055]);
            oval(parent, eyeShine, [x - 0.02, y + 0.035, 0.914], [0.022, 0.024, 0.012]);
        }
    }

    function makeDog() {
        const dog = new THREE.Group();
        oval(dog, dogFur, [0, -0.42, -0.12], [0.62, 0.45, 0.42]);
        oval(dog, dogFur, [0, 0.2, 0], [0.75, 0.72, 0.58]);
        oval(dog, dogFur, [-0.57, 0.43, 0.05], [0.27, 0.43, 0.2]).rotation.z = 0.42;
        oval(dog, dogFur, [0.57, 0.43, 0.05], [0.27, 0.43, 0.2]).rotation.z = -0.42;
        oval(dog, dogLight, [0, -0.06, 0.53], [0.4, 0.29, 0.2]);
        oval(dog, dogDark, [0, 0.03, 0.71], [0.095, 0.07, 0.07]);
        oval(dog, dogLight, [0, -0.46, 0.26], [0.27, 0.18, 0.18]);
        addEyes(dog, 0.29, 0.24);
        const collar = oval(dog, material('#ef8255', 0.4), [0, -0.39, 0.19], [0.55, 0.1, 0.43]);
        collar.scale.set(0.56, 0.09, 0.44);
        const tag = oval(dog, material('#f5c36b', 0.3), [0, -0.55, 0.57], [0.12, 0.14, 0.07]);
        tag.rotation.z = Math.PI;
        return dog;
    }

    function makeCat() {
        const cat = new THREE.Group();
        oval(cat, catFur, [0, -0.43, -0.14], [0.63, 0.44, 0.42]);
        const head = oval(cat, catFur, [0, 0.18, 0], [0.69, 0.66, 0.55]);
        const earGeometry = new THREE.ConeGeometry(0.36, 0.72, 4);
        const leftEar = oval(cat, catFur, [-0.43, 0.68, -0.02], [1, 1, 0.72], earGeometry);
        const rightEar = oval(cat, catFur, [0.43, 0.68, -0.02], [1, 1, 0.72], earGeometry);
        leftEar.rotation.z = -0.12;
        rightEar.rotation.z = 0.12;
        oval(cat, material('#d5a094'), [-0.43, 0.69, 0.18], [0.2, 0.34, 0.09], earGeometry);
        oval(cat, material('#d5a094'), [0.43, 0.69, 0.18], [0.2, 0.34, 0.09], earGeometry);
        oval(cat, catLight, [-0.2, -0.09, 0.45], [0.27, 0.19, 0.15]);
        oval(cat, catLight, [0.2, -0.09, 0.45], [0.27, 0.19, 0.15]);
        oval(cat, material('#c87670', 0.32), [0, -0.01, 0.58], [0.09, 0.065, 0.045]);
        addEyes(cat, 0.27, 0.23);
        oval(cat, material('#f5d8b7'), [0, -0.43, 0.29], [0.3, 0.16, 0.16]);
        return cat;
    }

    const pets = new THREE.Group();
    const dog = makeDog();
    dog.position.set(-1.02, -0.14, 0.2);
    dog.rotation.y = 0.12;
    dog.scale.setScalar(1.16);
    pets.add(dog);

    const cat = makeCat();
    cat.position.set(1.04, -0.2, -0.25);
    cat.rotation.y = -0.18;
    cat.scale.setScalar(1.1);
    pets.add(cat);
    scene.add(pets);

    const pawMaterial = material('#ef8255');
    const pawToeMaterial = material('#f3a36e');
    function makePaw(size, color, toes) {
        const group = new THREE.Group();
        const padMaterial = material(color);
        oval(group, padMaterial, [0, -0.04, 0], [0.34, 0.27, 0.12]);
        const toePositions = toes
            ? [[-0.3, 0.32], [-0.1, 0.45], [0.13, 0.43], [0.32, 0.25]]
            : [[-0.27, 0.26], [-0.09, 0.39], [0.14, 0.36], [0.31, 0.19]];
        for (const [x, y] of toePositions) {
            oval(group, pawToeMaterial, [x, y, 0], [0.12, 0.15, 0.09]);
        }
        group.scale.setScalar(size);
        return group;
    }

    const paws = [
        makePaw(0.62, '#ef8255', true),
        makePaw(0.48, '#718f72', false),
        makePaw(0.38, '#eaa071', false)
    ];
    paws[0].position.set(-2.7, 1.55, -0.2);
    paws[0].rotation.z = -0.32;
    paws[1].position.set(2.62, 1.52, -0.6);
    paws[1].rotation.z = 0.28;
    paws[2].position.set(2.4, -1.55, -0.8);
    paws[2].rotation.z = -0.14;
    for (const paw of paws) scene.add(paw);
    const pawBaseY = paws.map((paw) => paw.position.y);
    const pawBaseRotation = paws.map((paw) => paw.rotation.z);

    const root = host.closest('.hero-art');
    root?.classList.add('scene-ready');
    let visible = true;
    let lastTime = 0;
    let elapsed = 0;
    let animationFrame = 0;

    const resize = () => {
        const nextWidth = Math.max(host.clientWidth, 1);
        const nextHeight = Math.max(host.clientHeight, 1);
        camera.aspect = nextWidth / nextHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(nextWidth, nextHeight, false);
        renderStatic();
    };

    const renderStatic = () => renderer.render(scene, camera);
    const animate = (time) => {
        if (!visible || document.hidden || reduceMotion.matches) {
            animationFrame = 0;
            renderStatic();
            return;
        }
        const delta = Math.min((time - lastTime) / 1000 || 0, 0.05);
        lastTime = time;
        elapsed += delta;
        dog.position.y = -0.14 + Math.sin(elapsed * 1.2) * 0.055;
        dog.rotation.z = Math.sin(elapsed * 0.8) * 0.025;
        cat.position.y = -0.2 + Math.sin(elapsed * 1.1 + 0.8) * 0.05;
        cat.rotation.z = Math.sin(elapsed * 0.65 + 1.1) * 0.018;
        paws.forEach((paw, index) => {
            paw.position.y = pawBaseY[index] + Math.sin(elapsed * 1.5 + index * 1.8) * 0.055;
            paw.rotation.z = pawBaseRotation[index] + Math.cos(elapsed * 0.6 + index) * 0.035;
        });
        renderer.render(scene, camera);
        animationFrame = window.requestAnimationFrame(animate);
    };

    const start = () => {
        if (animationFrame) window.cancelAnimationFrame(animationFrame);
        animationFrame = 0;
        if (visible && !document.hidden && !reduceMotion.matches) {
            lastTime = performance.now();
            animationFrame = window.requestAnimationFrame(animate);
        } else {
            renderStatic();
        }
    };

    const intersection = new IntersectionObserver((entries) => {
        visible = entries[0]?.isIntersecting ?? true;
        start();
    }, { threshold: 0.05 });
    intersection.observe(host);
    document.addEventListener('visibilitychange', start);
    reduceMotion.addEventListener('change', start);
    new ResizeObserver(resize).observe(host);
    resize();
    start();
}
