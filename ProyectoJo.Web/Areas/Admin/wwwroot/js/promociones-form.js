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

    // ---------- Descuento: switch independiente de imagen/platillos ----------
    // El campo real que usa el backend es el hidden #TipoDescuento. El switch
    // "aplicarDescuento" y el select "tipoDescuentoReal" son solo UI: entre los
    // dos deciden qué valor tiene ese hidden. La imagen y los platillos nunca
    // dependen de esto, así que no se tocan acá.
    const chkAplicarDescuento = document.getElementById('aplicarDescuento');
    const campoTipoReal = document.getElementById('campo-tipo-descuento-real');
    const tipoDescuentoRealSelect = document.getElementById('tipoDescuentoReal');
    const tipoDescuentoHidden = document.getElementById('TipoDescuento');
    const campoDescuento = document.getElementById('campo-descuento');
    const valorDescuentoUnidad = document.getElementById('valorDescuentoUnidad');

    function actualizarUnidad() {
        if (!valorDescuentoUnidad || !tipoDescuentoRealSelect) return;
        if (tipoDescuentoRealSelect.value === 'Porcentaje') {
            valorDescuentoUnidad.textContent = '(%)';
        } else if (tipoDescuentoRealSelect.value === 'MontoFijo') {
            valorDescuentoUnidad.textContent = '($)';
        } else {
            valorDescuentoUnidad.textContent = '';
        }
    }

    function actualizarDescuento() {
        const aplica = !!(chkAplicarDescuento && chkAplicarDescuento.checked);

        if (campoTipoReal) campoTipoReal.style.display = aplica ? '' : 'none';
        if (campoDescuento) campoDescuento.style.display = aplica ? '' : 'none';

        if (tipoDescuentoHidden) {
            tipoDescuentoHidden.value = aplica
                ? (tipoDescuentoRealSelect ? tipoDescuentoRealSelect.value : 'Porcentaje')
                : 'Ninguno';
        }

        actualizarUnidad();
    }

    if (chkAplicarDescuento) {
        chkAplicarDescuento.addEventListener('change', actualizarDescuento);
    }
    if (tipoDescuentoRealSelect) {
        tipoDescuentoRealSelect.addEventListener('change', actualizarDescuento);
    }
    actualizarDescuento();

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