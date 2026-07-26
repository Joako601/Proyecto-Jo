(function () {
    const tabla = document.getElementById('tablaIngredientesMenu');
    const tbody = tabla?.querySelector('tbody');
    const btnAgregar = document.getElementById('btnAgregarIngredienteMenu');
    const inputOculto = document.getElementById('inputIngredientesOculto');

    if (!tabla || !tbody || !inputOculto) return;

    function siguienteIndice() {
        return tbody.querySelectorAll('.fila-ingrediente-menu').length;
    }

    function crearFila(nombre) {
        const tr = document.createElement('tr');
        tr.className = 'fila-ingrediente-menu';
        tr.innerHTML = `
            <td><input class="form-control-sm input-nombre-ingrediente" placeholder="Ej: Cebolla" value="${nombre ? nombre.replace(/"/g, '&quot;') : ''}" /></td>
            <td><button type="button" class="link-delete btn-quitar-fila-menu">Quitar</button></td>
        `;
        return tr;
    }

    function sincronizarOculto() {
        const nombres = Array.from(tbody.querySelectorAll('.input-nombre-ingrediente'))
            .map(input => input.value.trim())
            .filter(nombre => nombre.length > 0);
        inputOculto.value = nombres.join(', ');
    }

    const valorInicial = (inputOculto.value || '')
        .split(',')
        .map(s => s.trim())
        .filter(s => s.length > 0);

    if (valorInicial.length === 0) {
        tbody.appendChild(crearFila(''));
    } else {
        valorInicial.forEach(nombre => tbody.appendChild(crearFila(nombre)));
    }

    btnAgregar?.addEventListener('click', () => {
        tbody.appendChild(crearFila(''));
        sincronizarOculto();
    });

    tbody.addEventListener('click', (e) => {
        if (e.target.closest('.btn-quitar-fila-menu')) {
            const fila = e.target.closest('.fila-ingrediente-menu');
            if (tbody.querySelectorAll('.fila-ingrediente-menu').length > 1) {
                fila.remove();
            } else {
                fila.querySelector('.input-nombre-ingrediente').value = '';
            }
            sincronizarOculto();
        }
    });

    tabla.addEventListener('input', sincronizarOculto);

    tabla.closest('form')?.addEventListener('submit', sincronizarOculto);

    sincronizarOculto();
})();