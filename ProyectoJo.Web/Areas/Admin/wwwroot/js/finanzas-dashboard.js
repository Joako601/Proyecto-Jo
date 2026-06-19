document.addEventListener('DOMContentLoaded', () => {

    const mesesLabels = JSON.parse(document.getElementById('data-meses').textContent);
    const dataIngresosAnio = JSON.parse(document.getElementById('data-ingresos-anio').textContent);
    const catLabels = JSON.parse(document.getElementById('data-cat-labels').textContent);
    const catData = JSON.parse(document.getElementById('data-cat-data').textContent);
    const catIngLabels = JSON.parse(document.getElementById('data-cat-ing-labels').textContent);
    const catIngData = JSON.parse(document.getElementById('data-cat-ing-data').textContent);

    // Gráfica de área — tendencia de ventas del año en curso
    const ctxTendencia = document.getElementById('graficaTendenciaAnual').getContext('2d');
    new Chart(ctxTendencia, {
        type: 'line',
        data: {
            labels: mesesLabels,
            datasets: [{
                label: 'Ventas',
                data: dataIngresosAnio,
                borderColor: '#2a7a2a',
                backgroundColor: 'rgba(42, 122, 42, 0.15)',
                fill: true,
                tension: 0.3,
                pointRadius: 4,
                pointHoverRadius: 7,
                pointBackgroundColor: '#2a7a2a'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                duration: 1200,
                easing: 'easeOutQuart'
            },
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        title: ctx => ctx[0].label,
                        label: ctx => ' $' + ctx.parsed.y.toLocaleString('es-MX', { minimumFractionDigits: 2 })
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { callback: val => '$' + val.toLocaleString('es-MX') }
                }
            }
        }
    });

    // Gráfica de dona — egresos por categoría
    if (catLabels.length > 0) {
        const ctxDona = document.getElementById('graficaDona').getContext('2d');
        new Chart(ctxDona, {
            type: 'doughnut',
            data: {
                labels: catLabels,
                datasets: [{
                    data: catData,
                    backgroundColor: ['#c0392b', '#e67e22', '#b5a478', '#7f8c8d', '#2c3e50'],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { animateRotate: true, animateScale: true },
                plugins: {
                    legend: { position: 'right' },
                    tooltip: {
                        callbacks: {
                            label: ctx => ' $' + ctx.parsed.toLocaleString('es-MX', { minimumFractionDigits: 2 })
                        }
                    }
                }
            }
        });
    }

    // Gráfica de dona — ingresos por categoría
    if (catIngLabels.length > 0) {
        const ctxDonaIngresos = document.getElementById('graficaDonaIngresos').getContext('2d');
        new Chart(ctxDonaIngresos, {
            type: 'doughnut',
            data: {
                labels: catIngLabels,
                datasets: [{
                    data: catIngData,
                    backgroundColor: ['#2a7a2a', '#4caf50', '#81c784', '#a5d6a7', '#558b2f'],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { animateRotate: true, animateScale: true },
                plugins: {
                    legend: { position: 'right' },
                    tooltip: {
                        callbacks: {
                            label: ctx => ' $' + ctx.parsed.toLocaleString('es-MX', { minimumFractionDigits: 2 })
                        }
                    }
                }
            }
        });
    }
});