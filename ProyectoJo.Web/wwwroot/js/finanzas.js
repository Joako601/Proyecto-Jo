document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.form-eliminar').forEach(form => {
        form.addEventListener('submit', e => {
            if (!confirm('¿Eliminar este movimiento? Esta acción no se puede deshacer.')) {
                e.preventDefault();
            }
        });
    });
});