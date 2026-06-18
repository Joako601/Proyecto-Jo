const reveal = () => {
    const items = document.querySelectorAll('.module-reveal');

    items.forEach(item => {
        const windowHeight = window.innerHeight;
        const elementTop = item.getBoundingClientRect().top;
        // Ajustamos a 100 para que se active más rápido al hacer scroll
        const elementVisible = 100;

        if (elementTop < windowHeight - elementVisible) {
            item.classList.add('active');
        }
    });
};

// Escuchamos el scroll y el cambio de tamaño de ventana
window.addEventListener('scroll', reveal);
window.addEventListener('resize', reveal);

// Ejecución inmediata al cargar para elementos que ya están en el viewport
window.addEventListener('load', reveal);

// Un pequeño "empujón" extra por si el DOM tarda en renderizar
setTimeout(reveal, 500);