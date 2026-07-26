(function () {
    'use strict';

    var modalEl = document.getElementById('modalVigenciaPromo');
    if (!modalEl || typeof bootstrap === 'undefined') return;

    var modal = new bootstrap.Modal(modalEl);
    var paso1 = document.getElementById('modalVigenciaPaso1');
    var paso2 = document.getElementById('modalVigenciaPaso2');
    var nombreSpan1 = document.getElementById('modalVigenciaNombre');
    var nombreSpan2 = document.getElementById('modalVigenciaNombre2');
    var permanenteId = document.getElementById('permanenteId');
    var fechaId = document.getElementById('fechaId');
    var inputInicio = document.getElementById('inputFechaInicio');
    var inputFin = document.getElementById('inputFechaFin');
    var btnMostrarCambiarFecha = document.getElementById('btnMostrarCambiarFecha');
    var btnVolverPaso1 = document.getElementById('btnVolverPaso1');

    function mostrarPaso1() {
        paso1.classList.remove('d-none');
        paso2.classList.add('d-none');
    }

    document.querySelectorAll('.btn-estado--fecha').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var id = btn.dataset.id;
            var titulo = btn.dataset.titulo || '';
            var fechaInicio = btn.dataset.fechaInicio || '';
            var fechaFin = btn.dataset.fechaFin || '';

            nombreSpan1.textContent = titulo;
            nombreSpan2.textContent = titulo;
            permanenteId.value = id;
            fechaId.value = id;
            inputInicio.value = fechaInicio;
            inputFin.value = fechaFin;

            mostrarPaso1();
            modal.show();
        });
    });

    if (btnMostrarCambiarFecha) {
        btnMostrarCambiarFecha.addEventListener('click', function () {
            paso1.classList.add('d-none');
            paso2.classList.remove('d-none');
        });
    }

    if (btnVolverPaso1) {
        btnVolverPaso1.addEventListener('click', mostrarPaso1);
    }

    modalEl.addEventListener('hidden.bs.modal', mostrarPaso1);
})();