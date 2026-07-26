(function () {
    'use strict';

    const urlInput = document.getElementById('ImagenUrl');
    const previewImg = document.getElementById('banner-preview-img');
    const placeholder = document.getElementById('banner-preview-placeholder');

    if (urlInput && previewImg && placeholder) {
        function actualizarPreview() {
            const url = urlInput.value.trim();
            if (!url) {
                previewImg.hidden = true;
                placeholder.hidden = false;
                return;
            }
            previewImg.src = url;
            previewImg.hidden = false;
            placeholder.hidden = true;
        }

        previewImg.addEventListener('error', function () {
            previewImg.hidden = true;
            placeholder.hidden = false;
        });

        urlInput.addEventListener('input', actualizarPreview);
        actualizarPreview(); 
    }

    const tipoSelect = document.getElementById('TipoDescuento');
    const campoImagen = document.getElementById('campo-imagen');
    const campoDescuento = document.getElementById('campo-descuento');
    const campoPlatillos = document.getElementById('campo-platillos');

    function actualizarCampos() {
        if (!tipoSelect) return;
        const esAnuncio = tipoSelect.value === 'Ninguno';
        if (campoImagen) campoImagen.style.display = esAnuncio ? '' : 'none';
        if (campoDescuento) campoDescuento.style.display = esAnuncio ? 'none' : '';
        if (campoPlatillos) campoPlatillos.style.display = esAnuncio ? 'none' : '';
    }

    if (tipoSelect) {
        tipoSelect.addEventListener('change', actualizarCampos);
        actualizarCampos();
    }

    const siempreActiva = document.getElementById('siempreActiva');
    const fechaInicio = document.getElementById('FechaInicio');
    const fechaFin = document.getElementById('FechaFin');

    function actualizarFechas() {
        if (!siempreActiva || !fechaInicio || !fechaFin) return;
        const permanente = siempreActiva.checked;
        fechaInicio.disabled = permanente;
        fechaFin.disabled = permanente;
        if (permanente) {
            fechaInicio.value = '';
            fechaFin.value = '';
        }
    }

    if (siempreActiva) {
        siempreActiva.addEventListener('change', actualizarFechas);
        actualizarFechas();
    }
})();