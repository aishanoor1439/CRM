document.addEventListener('DOMContentLoaded', function () {
    const globeContainer = document.querySelector('.globe-container');
    if (!globeContainer) return;

    // Create orbiting nodes
    function createOrbitingNodes() {
        const orbitCount = 3;
        const nodesPerOrbit = [4, 6, 8];

        nodesPerOrbit.forEach((nodeCount, orbitIndex) => {
            const orbitRadius = [225, 175, 250]; // px
            const orbitClass = `orbit-${orbitIndex + 1}`;

            for (let i = 0; i < nodeCount; i++) {
                const angle = (i * 2 * Math.PI) / nodeCount;
                const x = orbitRadius[orbitIndex] * Math.cos(angle);
                const y = orbitRadius[orbitIndex] * Math.sin(angle);

                // Create node
                const node = document.createElement('div');
                node.className = `globe-node node-${orbitIndex}-${i}`;
                node.style.left = `calc(50% + ${x}px)`;
                node.style.top = `calc(50% + ${y}px)`;
                node.style.animationDelay = `${i * 0.5}s`;

                globeContainer.appendChild(node);

                // Create connection lines between nodes
                if (i > 0) {
                    createNodeConnection(`node-${orbitIndex}-${i - 1}`, `node-${orbitIndex}-${i}`);
                }
                if (i === nodeCount - 1) {
                    createNodeConnection(`node-${orbitIndex}-${i}`, `node-${orbitIndex}-0`);
                }

                // Create orbiting particles
                createOrbitingParticle(orbitRadius[orbitIndex], angle, i);
            }
        });
    }

    // Create connection between nodes
    function createNodeConnection(nodeId1, nodeId2) {
        const node1 = document.querySelector(`.${nodeId1}`);
        const node2 = document.querySelector(`.${nodeId2}`);

        if (!node1 || !node2) return;

        const rect1 = node1.getBoundingClientRect();
        const rect2 = node2.getBoundingClientRect();
        const containerRect = globeContainer.getBoundingClientRect();

        const x1 = rect1.left + rect1.width / 2 - containerRect.left;
        const y1 = rect1.top + rect1.height / 2 - containerRect.top;
        const x2 = rect2.left + rect2.width / 2 - containerRect.left;
        const y2 = rect2.top + rect2.height / 2 - containerRect.top;

        const length = Math.sqrt(Math.pow(x2 - x1, 2) + Math.pow(y2 - y1, 2));
        const angle = Math.atan2(y2 - y1, x2 - x1) * 180 / Math.PI;

        const connection = document.createElement('div');
        connection.className = 'node-connection';
        connection.style.left = `${x1}px`;
        connection.style.top = `${y1}px`;
        connection.style.width = `${length}px`;
        connection.style.transform = `rotate(${angle}deg)`;
        connection.style.animationDelay = `${Math.random() * 2}s`;

        globeContainer.appendChild(connection);
    }

    // Create orbiting particles
    function createOrbitingParticle(radius, startAngle, index) {
        const particle = document.createElement('div');
        particle.className = 'globe-particle';
        particle.style.setProperty('--radius', radius);
        particle.style.setProperty('--angle', startAngle);
        particle.style.animation = `orbitParticle ${10 + Math.random() * 10}s infinite linear`;
        particle.style.animationDelay = `${index * 0.3}s`;

        // Random color
        const colors = ['#00F5D4', '#6C63FF', '#00C896', '#4ECDC4'];
        particle.style.background = colors[Math.floor(Math.random() * colors.length)];

        globeContainer.appendChild(particle);
    }

    // Create data packets traveling between nodes
    function createDataPackets() {
        setInterval(() => {
            const nodes = document.querySelectorAll('.globe-node');
            if (nodes.length < 2) return;

            const fromNode = nodes[Math.floor(Math.random() * nodes.length)];
            const toNode = nodes[Math.floor(Math.random() * nodes.length)];

            if (fromNode !== toNode) {
                createDataPacket(fromNode, toNode);
            }
        }, 1000);
    }

    function createDataPacket(fromElement, toElement) {
        const packet = document.createElement('div');
        packet.className = 'data-packet';
        packet.style.position = 'absolute';
        packet.style.width = '8px';
        packet.style.height = '8px';
        packet.style.borderRadius = '50%';
        packet.style.background = '#00F5D4';
        packet.style.boxShadow = '0 0 15px #00F5D4';
        packet.style.zIndex = '5';

        const fromRect = fromElement.getBoundingClientRect();
        const toRect = toElement.getBoundingClientRect();
        const containerRect = globeContainer.getBoundingClientRect();

        const startX = fromRect.left + fromRect.width / 2 - containerRect.left;
        const startY = fromRect.top + fromRect.height / 2 - containerRect.top;
        const endX = toRect.left + toRect.width / 2 - containerRect.left;
        const endY = toRect.top + toRect.height / 2 - containerRect.top;

        packet.style.left = `${startX}px`;
        packet.style.top = `${startY}px`;

        globeContainer.appendChild(packet);

        const duration = 2;
        packet.animate([
            {
                transform: 'translate(0, 0) scale(1)',
                opacity: 1
            },
            {
                transform: `translate(${endX - startX}px, ${endY - startY}px) scale(0.5)`,
                opacity: 0
            }
        ], {
            duration: duration * 1000,
            easing: 'linear'
        });

        setTimeout(() => {
            if (packet.parentNode) {
                packet.parentNode.removeChild(packet);
            }
        }, duration * 1000);
    }

    // Interactive globe rotation on mouse move
    const globe = document.querySelector('.main-globe');
    if (globe) {
        document.addEventListener('mousemove', (e) => {
            const centerX = window.innerWidth / 2;
            const centerY = window.innerHeight / 2;
            const mouseX = e.clientX - centerX;
            const mouseY = e.clientY - centerY;

            const rotateY = mouseX / 100;
            const rotateX = -mouseY / 100;

            globe.style.transform = `translate(-50%, -50%) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;

            // Rotate orbits in opposite direction for parallax effect
            const orbits = document.querySelectorAll('.globe-orbit');
            orbits.forEach((orbit, index) => {
                const factor = index % 2 === 0 ? -0.5 : 0.3;
                orbit.style.transform = `translate(-50%, -50%) rotate(${rotateY * factor * 10}deg)`;
            });
        });
    }

    // Add CSS for orbiting particles
    const style = document.createElement('style');
    style.textContent = `
        @keyframes orbitParticle {
            0% {
                transform: rotate(var(--angle)) translateX(var(--radius)) rotate(calc(-1 * var(--angle)));
            }
            100% {
                transform: rotate(calc(var(--angle) + 360deg)) translateX(var(--radius)) rotate(calc(-1 * var(--angle) - 360deg));
            }
        }
        
        .globe-particle {
            position: absolute;
            left: 50%;
            top: 50%;
            transform-origin: center center;
        }
    `;
    document.head.appendChild(style);

    // Initialize globe animations
    createOrbitingNodes();
    createDataPackets();

    // Create background particles
    function createBackgroundParticles() {
        const background = document.querySelector('.background-elements');
        if (!background) return;

        for (let i = 0; i < 30; i++) {
            const particle = document.createElement('div');
            particle.style.position = 'absolute';
            particle.style.width = Math.random() * 4 + 1 + 'px';
            particle.style.height = particle.style.width;
            particle.style.background = Math.random() > 0.5 ? 'rgba(0, 245, 212, 0.3)' : 'rgba(108, 99, 255, 0.3)';
            particle.style.borderRadius = '50%';
            particle.style.left = Math.random() * 100 + '%';
            particle.style.top = Math.random() * 100 + '%';
            particle.style.opacity = Math.random() * 0.5 + 0.1;

            background.appendChild(particle);

            // Animate particle
            const duration = 10 + Math.random() * 20;
            const xDistance = (Math.random() - 0.5) * 200;
            const yDistance = (Math.random() - 0.5) * 200;

            particle.animate([
                {
                    transform: 'translate(0, 0)',
                    opacity: particle.style.opacity
                },
                {
                    transform: `translate(${xDistance}px, ${yDistance}px)`,
                    opacity: 0
                }
            ], {
                duration: duration * 1000,
                easing: 'linear'
            });

            // Loop animation
            setInterval(() => {
                particle.style.left = Math.random() * 100 + '%';
                particle.style.top = Math.random() * 100 + '%';
            }, duration * 1000);
        }
    }

    createBackgroundParticles();

    // Add hover effects to buttons
    const buttons = document.querySelectorAll('.cta-button');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', () => {
            button.style.transform = button.classList.contains('cta-primary')
                ? 'translateY(-5px) scale(1.05)'
                : 'translateY(-5px)';
        });

        button.addEventListener('mouseleave', () => {
            button.style.transform = 'translateY(0) scale(1)';
        });
    });

    // Add parallax effect to background shapes
    const shapes = document.querySelectorAll('.floating-shape');
    document.addEventListener('mousemove', (e) => {
        const mouseX = e.clientX / window.innerWidth - 0.5;
        const mouseY = e.clientY / window.innerHeight - 0.5;

        shapes.forEach((shape, index) => {
            const speed = index === 0 ? 0.02 : 0.01;
            shape.style.transform = `translate(${mouseX * 100}px, ${mouseY * 100}px) rotate(${mouseX * 20}deg)`;
        });
    });
});