(function () {
    const select = document.getElementById('selectCategoria');
    const inputNueva = document.getElementById('inputCategoriaNueva');

    if (!select || !inputNueva) return;

    const VALOR_NUEVA = '__nueva__';

    function actualizarEstado() {
        var esNueva = select.value === VALOR_NUEVA;
        inputNueva.style.display = esNueva ? 'block' : 'none';
        inputNueva.required = esNueva;

        // Solo uno de los dos controles debe viajar al servidor como "Categoria"
        if (esNueva) {
            select.removeAttribute('name');
            inputNueva.setAttribute('name', 'Categoria');
        } else {
            select.setAttribute('name', 'Categoria');
            inputNueva.removeAttribute('name');
        }
    }

    select.addEventListener('change', actualizarEstado);
    actualizarEstado();
})();