(function () {
    'use strict';

    // ---------- Preview + subida de imagen ----------
    const urlInput = document.getElementById('ImagenUrl');
    const previewImg = document.getElementById('banner-preview-img');
    const placeholder = document.getElementById('banner-preview-placeholder');
    const dropzone = document.getElementById('banner-dropzone');
    const fileInput = document.getElementById('banner-file-input');
    const uploadStatus = document.getElementById('banner-upload-status');

    function actualizarPreview() {
        if (!urlInput || !previewImg || !placeholder) return;
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

    if (urlInput && previewImg && placeholder) {
        previewImg.addEventListener('error', function () {
            previewImg.hidden = true;
            placeholder.hidden = false;
        });

        urlInput.addEventListener('input', actualizarPreview);
        actualizarPreview();
    }

    function subirArchivo(file) {
        if (!file || !urlInput) return;

        if (!file.type.startsWith('image/')) {
            if (uploadStatus) uploadStatus.textContent = 'El archivo debe ser una imagen.';
            return;
        }

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const formData = new FormData();
        formData.append('archivo', file);
        if (tokenInput) formData.append('__RequestVerificationToken', tokenInput.value);

        if (uploadStatus) uploadStatus.textContent = 'Subiendo imagen...';

        fetch('/Admin/Promociones/SubirImagen', {
            method: 'POST',
            body: formData
        })
            .then(function (res) {
                return res.json().then(function (data) {
                    return { ok: res.ok, data: data };
                });
            })
            .then(function (resultado) {
                if (!resultado.ok) {
                    if (uploadStatus) uploadStatus.textContent = resultado.data.error || 'No se pudo subir la imagen.';
                    return;
                }
                urlInput.value = resultado.data.url;
                if (uploadStatus) uploadStatus.textContent = 'Imagen subida correctamente.';
                actualizarPreview();
            })
            .catch(function () {
                if (uploadStatus) uploadStatus.textContent = 'Error de red al subir la imagen.';
            });
    }

    if (dropzone && fileInput) {
        dropzone.addEventListener('click', function () {
            fileInput.click();
        });

        dropzone.addEventListener('dragover', function (e) {
            e.preventDefault();
            dropzone.classList.add('banner-dropzone--activo');
        });

        dropzone.addEventListener('dragleave', function () {
            dropzone.classList.remove('banner-dropzone--activo');
        });

        dropzone.addEventListener('drop', function (e) {
            e.preventDefault();
            dropzone.classList.remove('banner-dropzone--activo');
            if (e.dataTransfer.files && e.dataTransfer.files[0]) {
                subirArchivo(e.dataTransfer.files[0]);
            }
        });

        fileInput.addEventListener('change', function () {
            if (fileInput.files && fileInput.files[0]) {
                subirArchivo(fileInput.files[0]);
            }
        });
    }

    // ---------- Mostrar/ocultar según Tipo de descuento ----------
    const tipoSelect = document.getElementById('TipoDescuento');
    const campoDescuento = document.getElementById('campo-descuento');
    const campoPlatillos = document.getElementById('campo-platillos');
    const campoAplicarPlatillos = document.getElementById('campo-aplicar-platillos');
    const chkAplicarPlatillos = document.getElementById('aplicarPlatillos');

    function actualizarCampos() {
        if (!tipoSelect) return;
        const esAnuncio = tipoSelect.value === 'Ninguno';
        const aplicarAPlatillos = !!(chkAplicarPlatillos && chkAplicarPlatillos.checked);

        if (esAnuncio) {
            if (campoAplicarPlatillos) campoAplicarPlatillos.style.display = '';
            if (campoPlatillos) campoPlatillos.style.display = aplicarAPlatillos ? '' : 'none';
            if (campoDescuento) campoDescuento.style.display = aplicarAPlatillos ? '' : 'none';
        } else {
            if (campoAplicarPlatillos) campoAplicarPlatillos.style.display = 'none';
            if (campoPlatillos) campoPlatillos.style.display = '';
            if (campoDescuento) campoDescuento.style.display = '';
        }
    }

    if (tipoSelect) {
        tipoSelect.addEventListener('change', actualizarCampos);
        actualizarCampos();
    }

    if (chkAplicarPlatillos) {
        chkAplicarPlatillos.addEventListener('change', actualizarCampos);
    }

    // ---------- Siempre activa / fechas ----------
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