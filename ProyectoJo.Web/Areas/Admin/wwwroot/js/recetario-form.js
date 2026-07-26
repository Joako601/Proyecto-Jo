(function () {
    const tabla = document.getElementById('tablaIngredientes');
    const tbody = tabla?.querySelector('tbody');
    const btnAgregar = document.getElementById('btnAgregarIngrediente');
    const selectItem = document.querySelector('select[name="ItemId"]');
    const inputRendimiento = document.getElementById('inputRendimiento');

    const UNIDADES = [
        { value: 'Kilogramo', label: 'kg' },
        { value: 'Gramo', label: 'g' },
        { value: 'Litro', label: 'l' },
        { value: 'Mililitro', label: 'ml' },
        { value: 'Unidad', label: 'un' },
    ];

    if (!tabla || !tbody) return;

    function siguienteIndice() {
        return tbody.querySelectorAll('.fila-ingrediente').length;
    }

    function crearFila(indice) {
        const tr = document.createElement('tr');
        tr.className = 'fila-ingrediente';

        const opciones = UNIDADES.map(u => `<option value="${u.value}">${u.label}</option>`).join('');

        tr.innerHTML = `
            <td><input name="Ingredientes[${indice}].Nombre" class="form-control-sm" placeholder="Ej: Harina" /></td>
            <td><input name="Ingredientes[${indice}].Cantidad" class="form-control-sm input-cantidad" type="number" step="0.01" min="0" /></td>
            <td>
                <select name="Ingredientes[${indice}].Unidad" class="form-control-sm">
                    ${opciones}
                </select>
            </td>
            <td><input name="Ingredientes[${indice}].CostoUnitario" class="form-control-sm input-costo" type="number" step="0.01" min="0" /></td>
            <td class="subtotal-celda">$0.00</td>
            <td><button type="button" class="link-delete btn-quitar-fila">Quitar</button></td>
        `;
        return tr;
    }

    function reindexarFilas() {
        tbody.querySelectorAll('.fila-ingrediente').forEach((fila, i) => {
            fila.querySelectorAll('input, select').forEach(campo => {
                campo.name = campo.name.replace(/Ingredientes\[\d+\]/, `Ingredientes[${i}]`);
            });
        });
    }

    function recalcularTodo() {
        let costoTotal = 0;

        tbody.querySelectorAll('.fila-ingrediente').forEach(fila => {
            const cantidad = parseFloat(fila.querySelector('.input-cantidad')?.value) || 0;
            const costoUnitario = parseFloat(fila.querySelector('.input-costo')?.value) || 0;
            const subtotal = cantidad * costoUnitario;
            const celda = fila.querySelector('.subtotal-celda');
            if (celda) celda.textContent = `$${subtotal.toFixed(2)}`;
            costoTotal += subtotal;
        });

        const rendimiento = parseFloat(inputRendimiento?.value) || 1;
        const costoPorPorcion = rendimiento > 0 ? costoTotal / rendimiento : 0;

        const elTotal = document.getElementById('resumenCostoTotal');
        const elPorcion = document.getElementById('resumenCostoPorcion');
        const elMargen = document.getElementById('resumenMargen');

        if (elTotal) elTotal.textContent = `$${costoTotal.toFixed(2)}`;
        if (elPorcion) elPorcion.textContent = `$${costoPorPorcion.toFixed(2)}`;

        if (elMargen) {
            const precioVenta = parseFloat(selectItem?.selectedOptions?.[0]?.dataset?.precio) || 0;
            if (precioVenta > 0) {
                const margen = precioVenta - costoPorPorcion;
                const margenPct = (margen / precioVenta) * 100;
                elMargen.textContent = `${margenPct.toFixed(1)}% ($${margen.toFixed(2)})`;
            } else {
                elMargen.textContent = '—';
            }
        }
    }

    btnAgregar?.addEventListener('click', () => {
        tbody.appendChild(crearFila(siguienteIndice()));
    });

    tbody.addEventListener('click', (e) => {
        if (e.target.closest('.btn-quitar-fila')) {
            const fila = e.target.closest('.fila-ingrediente');
            if (tbody.querySelectorAll('.fila-ingrediente').length > 1) {
                fila.remove();
                reindexarFilas();
                recalcularTodo();
            }
        }
    });

    tabla.addEventListener('input', recalcularTodo);
    tabla.addEventListener('change', recalcularTodo);
    selectItem?.addEventListener('change', recalcularTodo);
    inputRendimiento?.addEventListener('input', recalcularTodo);

    recalcularTodo();
})();