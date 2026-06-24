(function () {
    'use strict';

    var INTERVALO_MS = 3000;

    var menu = [];
    var carrito = [];
    var tipoEntrega = 'mesa';
    var tabActiva = 'activos';
    var ultimosPedidos = [];

    /* ── Utilidades ── */

    function ir(url) {
        window.location.href = url;
    }

    function extraerIngredientes(item) {
        var fuente = item.ingredientes || item.descripcion || '';
        return fuente.split(',').map(function (s) { return s.trim(); }).filter(function (s) { return s.length > 0; });
    }

    /* ── Tipo de entrega ── */

    function setTipoEntrega(tipo) {
        tipoEntrega = tipo;
        document.getElementById('btn-mesa').classList.toggle('tipo-entrega__btn--activo', tipo === 'mesa');
        document.getElementById('btn-llevar').classList.toggle('tipo-entrega__btn--activo', tipo === 'llevar');
        document.getElementById('mesa-input').style.display = tipo === 'mesa' ? 'block' : 'none';
    }

    /* ── Tabs ── */

    function cambiarTab(tab) {
        tabActiva = tab;
        document.getElementById('tab-activos').classList.toggle('tabs__btn--activo', tab === 'activos');
        document.getElementById('tab-pagados').classList.toggle('tabs__btn--activo', tab === 'pagados');
        renderPedidos();
    }

    /* ── Menú ── */

    function cargarMenu() {
        fetch('/Operaciones/Recepcion/ObtenerMenu')
            .then(function (res) {
                if (res.status === 401) { ir('/Operaciones/Auth/Login'); return null; }
                if (!res.ok) throw new Error('HTTP ' + res.status);
                return res.json();
            })
            .then(function (datos) {
                if (!datos) return;
                menu = datos;
                renderMenu();
            })
            .catch(function (err) {
                console.error('Error cargando menú:', err);
                document.getElementById('menu-contenedor').innerHTML =
                    '<p class="mensaje-vacio" style="color:#dc2626;">No se pudo cargar el menú.</p>';
            });
    }

    function renderMenu() {
        var contenedor = document.getElementById('menu-contenedor');
        contenedor.innerHTML = '';

        var disponibles = menu.filter(function (i) { return i.activo; });
        var categorias = disponibles
            .map(function (i) { return i.categoria || 'Otros'; })
            .filter(function (v, i, a) { return a.indexOf(v) === i; });

        categorias.forEach(function (cat) {
            var titulo = document.createElement('div');
            titulo.className = 'categoria-titulo';
            titulo.textContent = cat;
            contenedor.appendChild(titulo);

            disponibles
                .filter(function (i) { return (i.categoria || 'Otros') === cat; })
                .forEach(function (item) {
                    var div = document.createElement('div');
                    div.className = 'menu-item' + (item.agotado ? ' menu-item--agotado' : '');
                    div.innerHTML =
                        '<div class="menu-item__info">' +
                        '<h3>' + item.platillo + '</h3>' +
                        '<p class="menu-item__desc">' + (item.descripcion || '') + '</p>' +
                        '<p class="menu-item__precio">$' + Number(item.precio).toFixed(2) + '</p>' +
                        '</div>' +
                        '<button class="menu-item__btn" data-item-id="' + item.id + '" ' + (item.agotado ? 'disabled' : '') + '>' +
                        (item.agotado ? 'Agotado' : 'Agregar') +
                        '</button>';
                    contenedor.appendChild(div);
                });
        });
    }

    /* ── Carrito ── */

    function agregarAlCarrito(itemId) {
        var item = menu.find(function (i) { return i.id === itemId; });
        if (!item) return;

        var existente = carrito.find(function (c) {
            return c.itemId === itemId && c.ingredientesQuitados.length === 0;
        });

        if (existente) {
            existente.cantidad++;
        } else {
            carrito.push({
                itemId: item.id,
                nombre: item.platillo,
                precioUnitario: item.precio,
                cantidad: 1,
                ingredientesDisponibles: extraerIngredientes(item),
                ingredientesQuitados: []
            });
        }
        renderCarrito();
    }

    function cambiarCantidad(index, delta) {
        carrito[index].cantidad += delta;
        if (carrito[index].cantidad <= 0) carrito.splice(index, 1);
        renderCarrito();
    }

    function toggleIngrediente(index, ingrediente) {
        var linea = carrito[index];
        var pos = linea.ingredientesQuitados.indexOf(ingrediente);
        if (pos >= 0) linea.ingredientesQuitados.splice(pos, 1);
        else linea.ingredientesQuitados.push(ingrediente);
        renderCarrito();
    }

    function quitarLinea(index) {
        carrito.splice(index, 1);
        renderCarrito();
    }

    function renderCarrito() {
        var cont = document.getElementById('carrito-lineas');
        var crearBtn = document.getElementById('crear-btn');
        var total = carrito.reduce(function (sum, l) { return sum + l.precioUnitario * l.cantidad; }, 0);

        if (carrito.length === 0) {
            cont.innerHTML = '<p class="mensaje-vacio">Aún no has agregado productos.</p>';
            crearBtn.disabled = true;
        } else {
            cont.innerHTML = '';
            carrito.forEach(function (linea, index) {
                var div = document.createElement('div');
                div.className = 'carrito-linea';

                var checks = linea.ingredientesDisponibles.map(function (ing) {
                    var quitado = linea.ingredientesQuitados.indexOf(ing) >= 0;
                    return '<label><input type="checkbox" data-index="' + index +
                        '" data-ing="' + ing.replace(/"/g, '&quot;') + '"' +
                        (quitado ? ' checked' : '') + '> Sin ' + ing + '</label>';
                }).join('');

                div.innerHTML =
                    '<div class="carrito-linea__nombre">' +
                    '<span>' + linea.cantidad + 'x ' + linea.nombre + '</span>' +
                    '<span>$' + (linea.precioUnitario * linea.cantidad).toFixed(2) + '</span>' +
                    '</div>' +
                    '<div class="carrito-linea__controles">' +
                    '<button data-accion="restar" data-index="' + index + '">−</button>' +
                    '<button data-accion="sumar" data-index="' + index + '">+</button>' +
                    '<button class="carrito-linea__eliminar" data-accion="quitar" data-index="' + index + '">Eliminar</button>' +
                    '</div>' +
                    (checks ? '<div class="carrito-linea__ingredientes">' + checks + '</div>' : '');

                cont.appendChild(div);
            });
            crearBtn.disabled = false;
        }

        document.getElementById('total-carrito').textContent = 'Total: $' + total.toFixed(2);
    }

    /* ── Crear pedido ── */

    function crearPedido() {
        var mesaInput = document.getElementById('mesa-input').value.trim();

        if (tipoEntrega === 'mesa' && !mesaInput) {
            alert('Escribe el número o nombre de la mesa.');
            return;
        }

        var mesaTexto = tipoEntrega === 'mesa' ? 'Mesa ' + mesaInput : 'Para llevar';

        var items = carrito.map(function (l) {
            var sufijo = l.ingredientesQuitados.length > 0
                ? ' (sin ' + l.ingredientesQuitados.join(', ') + ')'
                : '';
            return {
                itemId: l.itemId,
                nombre: l.nombre + sufijo,
                cantidad: l.cantidad,
                precioUnitario: l.precioUnitario
            };
        });

        fetch('/Operaciones/Recepcion/Crear', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ mesa: mesaTexto, items: items })
        })
            .then(function (res) {
                if (res.status === 401) { ir('/Operaciones/Auth/Login'); return null; }
                if (!res.ok) {
                    return res.text().then(function (t) {
                        alert('No se pudo crear el pedido (' + res.status + '). ' + t);
                    });
                }
                carrito = [];
                document.getElementById('mesa-input').value = '';
                renderCarrito();
                cargarPedidos();
            })
            .catch(function (err) {
                console.error('Error creando pedido:', err);
                alert('Error de red al crear el pedido.');
            });
    }

    /* ── Pedidos ── */

    function cargarPedidos() {
        fetch('/Operaciones/Recepcion/ObtenerPedidos')
            .then(function (res) {
                if (res.status === 401) { ir('/Operaciones/Auth/Login'); return null; }
                if (!res.ok) throw new Error('HTTP ' + res.status);
                return res.json();
            })
            .then(function (pedidos) {
                if (!pedidos) return;
                ultimosPedidos = pedidos;
                renderPedidos();
            })
            .catch(function (err) {
                console.error('Error cargando pedidos:', err);
            });
    }

    function renderPedidos() {
        var cont = document.getElementById('pedidos-grid');

        var filtrados = tabActiva === 'activos'
            ? ultimosPedidos.filter(function (p) { return p.estado !== 'Pagado'; })
            : ultimosPedidos.filter(function (p) { return p.estado === 'Pagado'; });

        if (filtrados.length === 0) {
            cont.innerHTML = '<p class="mensaje-vacio">' +
                (tabActiva === 'activos' ? 'No hay pedidos activos.' : 'Aún no hay pedidos pagados.') +
                '</p>';
            return;
        }

        cont.innerHTML = filtrados.map(function (p) {
            var itemsHtml = p.items.map(function (i) {
                return '<li>' + i.cantidad + 'x ' + i.nombre + '</li>';
            }).join('');

            var accion = p.estado !== 'Pagado'
                ? '<button class="pagar-btn" data-id="' + p.id + '">Marcar pagado</button>'
                : '<span class="pedido-card__pagado">✓ Pagado</span>';

            return '<div class="pedido-card pedido-card--' + p.estado + '">' +
                '<div class="pedido-card__header">' +
                '<span class="pedido-card__mesa">' + p.mesa +
                ' <span class="pedido-card__id">#' + p.id + '</span>' +
                '</span>' +
                '<span class="pedido-card__badge pedido-card__badge--' + p.estado + '">' + p.estado + '</span>' +
                '</div>' +
                '<ul class="pedido-card__items">' + itemsHtml + '</ul>' +
                '<div class="pedido-card__total">$' + p.total.toFixed(2) + '</div>' +
                accion +
                '</div>';
        }).join('');
    }

    function marcarPagado(id) {
        fetch('/Operaciones/Recepcion/Pagar', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'id=' + id
        })
            .then(function (res) {
                if (res.status === 401) { ir('/Operaciones/Auth/Login'); return; }
                if (!res.ok) {
                    alert('No se pudo marcar como pagado (código ' + res.status + ').');
                    return;
                }
                cargarPedidos();
            })
            .catch(function (err) {
                console.error('Error marcando pagado:', err);
                alert('Error de red al marcar como pagado.');
            });
    }

    /* ── Delegación de eventos ── */

    document.addEventListener('click', function (e) {
        var t = e.target;

        if (t.matches('.menu-item__btn') && t.dataset.itemId) {
            agregarAlCarrito(Number(t.dataset.itemId));
            return;
        }
        if (t.matches('[data-accion="restar"]')) {
            cambiarCantidad(Number(t.dataset.index), -1);
            return;
        }
        if (t.matches('[data-accion="sumar"]')) {
            cambiarCantidad(Number(t.dataset.index), 1);
            return;
        }
        if (t.matches('[data-accion="quitar"]')) {
            quitarLinea(Number(t.dataset.index));
            return;
        }
        if (t.matches('.pagar-btn') && t.dataset.id) {
            marcarPagado(Number(t.dataset.id));
            return;
        }
        if (t.matches('.tipo-entrega__btn') && t.dataset.tipo) {
            setTipoEntrega(t.dataset.tipo);
            return;
        }
        if (t.matches('.tabs__btn') && t.dataset.tab) {
            cambiarTab(t.dataset.tab);
            return;
        }
        if (t.matches('#crear-btn')) {
            crearPedido();
            return;
        }
    });

    document.addEventListener('change', function (e) {
        var t = e.target;
        if (t.matches('.carrito-linea__ingredientes input[type="checkbox"]')) {
            toggleIngrediente(Number(t.dataset.index), t.dataset.ing);
        }
    });

    /* ── Inicio ── */

    setTipoEntrega('mesa');
    cambiarTab('activos');
    cargarMenu();
    cargarPedidos();
    setInterval(cargarPedidos, INTERVALO_MS);

})();