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

    // ---------- Tipo de descuento ----------
    // "tipoDescuentoUi" (Banner / Porcentaje / MontoFijo) es la selección
    // principal. Si eligen Porcentaje o MontoFijo ahí, ese es el descuento y
    // el input de valor aparece directo. Si eligen Banner, se muestra el
    // bloque de imagen del banner (label, dropzone, URL y vista previa), y
    // además puede aparecer el switch "Aplicar también a platillos
    // específicos": si lo activan, eligen ahí un tipo de descuento
    // (Porcentaje/MontoFijo) para esos platillos puntuales, y recién ahí
    // aparece el input de valor.
    const tipoDescuentoUiSelect = document.getElementById('tipoDescuentoUi');
    const tipoDescuentoHidden = document.getElementById('TipoDescuento');
    const campoImagen = document.getElementById('campo-imagen');
    const campoAplicarPlatillos = document.getElementById('campo-aplicar-platillos');
    const chkAplicarDescuento = document.getElementById('aplicarDescuento');
    const campoTipoReal = document.getElementById('campo-tipo-descuento-real');
    const tipoDescuentoRealSelect = document.getElementById('tipoDescuentoReal');
    const campoDescuento = document.getElementById('campo-descuento');
    const valorDescuentoUnidad = document.getElementById('valorDescuentoUnidad');

    function actualizarTipoDescuento() {
        if (!tipoDescuentoUiSelect) return;
        const tipoUi = tipoDescuentoUiSelect.value; // Banner | Porcentaje | MontoFijo
        const esBanner = tipoUi === 'Banner';
        const aplicaAPlatillos = !!(chkAplicarDescuento && chkAplicarDescuento.checked);

        // El bloque de imagen del banner solo se muestra si el tipo es Banner
        if (campoImagen) campoImagen.style.display = esBanner ? '' : 'none';

        // El switch "aplicar a platillos específicos" solo tiene sentido con Banner
        if (campoAplicarPlatillos) campoAplicarPlatillos.style.display = esBanner ? '' : 'none';
        if (!esBanner && chkAplicarDescuento) chkAplicarDescuento.checked = false;

        // El segundo select (Porcentaje/MontoFijo) solo aparece con Banner + switch activado
        const mostrarTipoReal = esBanner && aplicaAPlatillos;
        if (campoTipoReal) campoTipoReal.style.display = mostrarTipoReal ? '' : 'none';

        // Tipo efectivo: el elegido arriba si no es Banner, o el del segundo select si aplica a platillos
        let tipoEfectivo = 'Ninguno';
        if (!esBanner) {
            tipoEfectivo = tipoUi;
        } else if (aplicaAPlatillos) {
            tipoEfectivo = tipoDescuentoRealSelect ? tipoDescuentoRealSelect.value : 'Porcentaje';
        }

        if (tipoDescuentoHidden) tipoDescuentoHidden.value = tipoEfectivo;

        const mostrarValor = tipoEfectivo !== 'Ninguno';
        if (campoDescuento) campoDescuento.style.display = mostrarValor ? '' : 'none';

        if (valorDescuentoUnidad) {
            valorDescuentoUnidad.textContent = tipoEfectivo === 'Porcentaje' ? '(%)'
                : tipoEfectivo === 'MontoFijo' ? '($)'
                    : '';
        }
    }

    if (tipoDescuentoUiSelect) {
        tipoDescuentoUiSelect.addEventListener('change', actualizarTipoDescuento);
    }
    if (chkAplicarDescuento) {
        chkAplicarDescuento.addEventListener('change', actualizarTipoDescuento);
    }
    if (tipoDescuentoRealSelect) {
        tipoDescuentoRealSelect.addEventListener('change', actualizarTipoDescuento);
    }
    actualizarTipoDescuento();

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