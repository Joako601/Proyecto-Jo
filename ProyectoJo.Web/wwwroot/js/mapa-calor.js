(function () {
    'use strict';

    function leerDatos(id) {
        var el = document.getElementById(id);
        return el ? JSON.parse(el.textContent) : [];
    }

    var tcTeal = 'rgba(47, 110, 98, 0.85)';
    var tcMustard = 'rgba(217, 164, 65, 0.85)';
    var tcBrick = 'rgba(178, 58, 46, 0.85)';

    var params = new URLSearchParams(window.location.search);
    var scrollTarget = params.get('scroll');
    if (scrollTarget) {
        var el = document.getElementById(scrollTarget);
        if (el) {
            requestAnimationFrame(function () {
                el.scrollIntoView({ behavior: 'smooth', block: 'start' });
            });
        }
    }

    document.querySelectorAll('form[data-scroll-target]').forEach(function (form) {
        form.addEventListener('submit', function () {
            var hidden = document.createElement('input');
            hidden.type = 'hidden';
            hidden.name = 'scroll';
            hidden.value = form.dataset.scrollTarget;
            form.appendChild(hidden);
        });
    });

    new Chart(document.getElementById('graficaHoraspico'), {
        type: 'bar',
        data: {
            labels: leerDatos('mc-data-horas-labels'),
            datasets: [{ label: 'Pedidos pagados', data: leerDatos('mc-data-horas-data'), backgroundColor: tcTeal, borderRadius: 4 }]
        },
        options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
    });

    new Chart(document.getElementById('graficaDiasSemana'), {
        type: 'bar',
        data: {
            labels: leerDatos('mc-data-dias-labels'),
            datasets: [{ label: 'Pedidos pagados', data: leerDatos('mc-data-dias-data'), backgroundColor: tcMustard, borderRadius: 4 }]
        },
        options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
    });

    new Chart(document.getElementById('graficaMeses'), {
        type: 'bar',
        data: {
            labels: leerDatos('mc-data-meses-labels'),
            datasets: [{ label: 'Pedidos pagados', data: leerDatos('mc-data-meses-data'), backgroundColor: tcBrick, borderRadius: 4 }]
        },
        options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
    });
})();
