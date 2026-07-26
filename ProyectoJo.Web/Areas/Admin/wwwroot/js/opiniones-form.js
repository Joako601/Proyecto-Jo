(function () {
    const picker = document.getElementById('estrellasPicker');
    const inputCalificacion = document.getElementById('inputCalificacion');
    const textoValor = document.getElementById('estrellasValorTexto');

    if (picker && inputCalificacion) {
        const estrellas = Array.from(picker.querySelectorAll('.estrella'));

        function pintar(valor) {
            estrellas.forEach((estrella, i) => {
                const posicion = i + 1;
                estrella.classList.remove('estrella--llena', 'estrella--media', 'bi-star', 'bi-star-fill', 'bi-star-half');

                if (valor >= posicion) {
                    estrella.classList.add('bi-star-fill', 'estrella--llena');
                } else if (valor >= posicion - 0.5) {
                    estrella.classList.add('bi-star-half', 'estrella--media');
                } else {
                    estrella.classList.add('bi-star');
                }
            });

            if (textoValor) {
                textoValor.textContent = valor > 0 ? `${valor.toFixed(1)} / 5` : 'Sin calificar';
            }
        }

        function calcularValorDesdeClick(estrella, evento) {
            const posicion = parseInt(estrella.dataset.posicion, 10);
            const rect = estrella.getBoundingClientRect();
            const mitad = rect.left + rect.width / 2;
            const clickIzquierda = evento.clientX < mitad;
            return clickIzquierda ? posicion - 0.5 : posicion;
        }

        estrellas.forEach(estrella => {
            estrella.addEventListener('click', (e) => {
                const valor = calcularValorDesdeClick(estrella, e);
                inputCalificacion.value = valor.toFixed(1);
                pintar(valor);
            });

            estrella.addEventListener('mousemove', (e) => {
                const valor = calcularValorDesdeClick(estrella, e);
                pintar(valor);
            });
        });

        picker.addEventListener('mouseleave', () => {
            pintar(parseFloat(inputCalificacion.value) || 0);
        });

        const valorInicial = parseFloat(picker.dataset.valor) || 0;
        inputCalificacion.value = valorInicial > 0 ? valorInicial.toFixed(1) : '';
        pintar(valorInicial);
    }

    document.querySelectorAll('.semaforo-opcion input[type="radio"]').forEach(radio => {
        radio.addEventListener('change', () => {
            document.querySelectorAll('.semaforo-opcion').forEach(op => op.classList.remove('activo'));
            if (radio.checked) {
                radio.closest('.semaforo-opcion')?.classList.add('activo');
            }
        });

        if (radio.checked) {
            radio.closest('.semaforo-opcion')?.classList.add('activo');
        }
    });
})();