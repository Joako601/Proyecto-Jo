document.addEventListener("DOMContentLoaded", function () {
    // Obtenemos todos los botones y las tarjetas del menú
    const filterButtons = document.querySelectorAll('.filter-btn');
    const menuItems = document.querySelectorAll('.menu-item-card');

    filterButtons.forEach(button => {
        button.addEventListener('click', () => {

            // 1. Quitar la clase 'active' de todos los botones para reiniciar el estilo
            filterButtons.forEach(btn => btn.classList.remove('active'));

            // 2. Agregar 'active' solo al botón que se hizo clic
            button.classList.add('active');

            // 3. Obtener la categoría que queremos filtrar (ej: "panuchos")
            const filterValue = button.getAttribute('data-filter');

            // 4. Recorrer cada platillo para decidir si se oculta o se muestra
            menuItems.forEach(item => {
                const itemCategory = item.getAttribute('data-category');

                // Si el filtro es "todos" o si la categoría coincide
                if (filterValue === 'todos' || itemCategory.includes(filterValue)) {
                    item.style.display = 'block';

                    // Pequeño retraso para que la animación CSS surta efecto
                    setTimeout(() => {
                        item.style.opacity = '1';
                        item.style.transform = 'translateY(0)';
                    }, 50);
                } else {
                    // Si no coincide, lo desvanecemos y luego lo ocultamos del layout
                    item.style.opacity = '0';
                    item.style.transform = 'translateY(20px)';

                    setTimeout(() => {
                        item.style.display = 'none';
                    }, 400); // 400ms coincide con el tiempo de tu CSS transition
                }
            });
        });
    });
});