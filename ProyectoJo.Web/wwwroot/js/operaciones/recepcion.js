(function () {
    'use strict';

    function obtenerTokenAntiforgery() {
        var meta = document.querySelector('meta[name="request-verification-token"]');
        return meta ? meta.getAttribute('content') : '';
    }

    var menu = [];
    var carrito = [];
    var tipoEntrega = 'mesa';
    var tabActiva = 'activos';
    var ultimosPedidos = [];
    var categoriaActiva = 'Todos';
    var textoBusqueda = '';

    function ir(url) {
        window.location.href = url;
    }

    function extraerIngredientes(item) {
        var fuente = item.ingredientes || item.descripcion || '';
        return fuente.split(',').map(function (s) { return s.trim(); }).filter(function (s) { return s.length > 0; });
    }

    function setEstadoConexion(estado) {
        var el = document.getElementById('estado-conexion');
        if (!el) return;
        var textos = {
            conectado: '',
            reconectando: '🔄 Reconectando…',
            desconectado: '⚠ Sin conexión — usa el botón de refresco'
        };
        el.textContent = textos[estado] || '';
    }

    function setTipoEntrega(tipo) {
        tipoEntrega = tipo;
        document.getElementById('btn-mesa').classList.toggle('tipo-entrega__btn--activo', tipo === 'mesa');
        document.getElementById('btn-llevar').classList.toggle('tipo-entrega__btn--activo', tipo === 'llevar');
        document.getElementById('mesa-input').style.display = tipo === 'mesa' ? 'block' : 'none';
    }

    function cambiarTab(tab) {
        tabActiva = tab;
        document.getElementById('tab-activos').classList.toggle('tabs__btn--activo', tab === 'activos');
        document.getElementById('tab-pagados').classList.toggle('tabs__btn--activo', tab === 'pagados');
        renderPedidos();
    }

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
                renderChips();
                renderMenu();
            })
            .catch(function (err) {
                console.error('Error cargando menú:', err);
                document.getElementById('menu-grid').innerHTML =
                    '<p class="mensaje-vacio" style="color:var(--tc-brick);">No se pudo cargar el menú.</p>';
            });
    }

    function categoriasDisponibles() {
        return menu
            .filter(function (i) { return i.activo; })
            .map(function (i) { return i.categoria || 'Otros'; })
            .filter(function (v, i, a) { return a.indexOf(v) === i; });
    }

    function renderChips() {
        var cont = document.getElementById('chips-categoria');
        var categorias = ['Todos'].concat(categoriasDisponibles());

        if (categorias.indexOf(categoriaActiva) === -1) categoriaActiva = 'Todos';

        cont.innerHTML = categorias.map(function (cat) {
            return '<button class="chip' + (cat === categoriaActiva ? ' chip--activo' : '') +
                '" data-categoria="' + cat.replace(/"/g, '&quot;') + '">' + cat + '</button>';
        }).join('');
    }

    function cantidadEnCarrito(itemId) {
        return carrito
            .filter(function (l) { return l.itemId === itemId; })
            .reduce(function (sum, l) { return sum + l.cantidad; }, 0);
    }

    function renderMenu() {
        var contenedor = document.getElementById('menu-grid');
        var disponibles = menu.filter(function (i) { return i.activo; });

        if (categoriaActiva !== 'Todos') {
            disponibles = disponibles.filter(function (i) { return (i.categoria || 'Otros') === categoriaActiva; });
        }

        if (textoBusqueda) {
            var q = textoBusqueda.toLowerCase();
            disponibles = disponibles.filter(function (i) {
                return (i.platillo || '').toLowerCase().indexOf(q) !== -1;
            });
        }

        if (disponibles.length === 0) {
            contenedor.innerHTML = '<p class="mensaje-vacio">No se encontraron platillos.</p>';
            return;
        }

        contenedor.innerHTML = disponibles.map(function (item) {
            var cant = cantidadEnCarrito(item.id);
            return '<button class="menu-card' + (item.agotado ? ' menu-card--agotado' : '') +
                '" data-item-id="' + item.id + '" ' + (item.agotado ? 'disabled' : '') + '>' +
                (cant > 0 ? '<span class="menu-card__badge">' + cant + '</span>' : '') +
                '<p class="menu-card__nombre">' + item.platillo + '</p>' +
                (item.agotado
                    ? '<p class="menu-card__agotado-label">Agotado</p>'
                    : '<p class="menu-card__precio">$' + Number(item.precio).toFixed(2) + '</p>') +
                '</button>';
        }).join('');
    }

    function agregarAlCarrito(itemId) {
        var item = menu.find(function (i) { return i.id === itemId; });
        if (!item || item.agotado) return;

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
        renderMenu();
    }

    function cambiarCantidad(index, delta) {
        carrito[index].cantidad += delta;
        if (carrito[index].cantidad <= 0) carrito.splice(index, 1);
        renderCarrito();
        renderMenu();
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
        renderMenu();
    }

    function abrirCarrito() {
        document.getElementById('panel-carrito').classList.add('panel-carrito--abierto');
        document.getElementById('carrito-overlay').classList.add('carrito-overlay--visible');
    }

    function cerrarCarrito() {
        document.getElementById('panel-carrito').classList.remove('panel-carrito--abierto');
        document.getElementById('carrito-overlay').classList.remove('carrito-overlay--visible');
    }

    function renderCarrito() {
        var cont = document.getElementById('carrito-lineas');
        var crearBtn = document.getElementById('crear-btn');
        var total = carrito.reduce(function (sum, l) { return sum + l.precioUnitario * l.cantidad; }, 0);
        var items = carrito.reduce(function (sum, l) { return sum + l.cantidad; }, 0);

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
        document.getElementById('barra-mobile-items').textContent = items + (items === 1 ? ' item' : ' items');
        document.getElementById('barra-mobile-total').textContent = '$' + total.toFixed(2);
    }

    function enviarPedido(mesaTexto, items) {
        fetch('/Operaciones/Recepcion/Crear', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': obtenerTokenAntiforgery()
            },
            body: JSON.stringify({ mesa: mesaTexto, items: items })
        })
            .then(function (res) {
                if (res.status === 401) { ir('/Operaciones/Auth/Login'); return null; }
                if (!res.ok) {
                    return res.json().then(function (data) {
                        mostrarModalError(data.error || 'No se pudo crear el pedido.');
                    });
                }
                return res.json().then(function () {
                    carrito = [];
                    document.getElementById('mesa-input').value = '';
                    renderCarrito();
                    renderMenu();
                    cerrarCarrito();
                });
            })
            .catch(function (err) {
                console.error('Error creando pedido:', err);
                mostrarModalError('Error de red al crear el pedido.');
            });
    }

    function mostrarModalError(mensaje) {
        var modal = document.getElementById('modal-confirmacion');
        document.getElementById('modal-titulo').textContent = 'Error';
        document.getElementById('modal-cuerpo').textContent = mensaje;
        document.getElementById('modal-confirmar').style.display = 'none';
        document.getElementById('modal-cancelar').textContent = 'Cerrar';
        modal.style.display = 'flex';
    }

    function validarYMostrarModal(mesaTexto) {
        var itemsDisponibles = [];
        var itemsProblema = [];
        var itemsAjustados = [];

        carrito.forEach(function (l) {
            var itemMenu = menu.find(function (m) { return m.id === l.itemId; });
            var sufijo = l.ingredientesQuitados.length > 0
                ? ' (sin ' + l.ingredientesQuitados.join(', ') + ')'
                : '';

            if (!itemMenu || !itemMenu.activo) {
                itemsProblema.push({ nombre: l.nombre, motivo: 'Ya no está disponible en el menú' });
                return;
            }

            if (itemMenu.agotado) {
                itemsProblema.push({ nombre: l.nombre, motivo: 'Sin stock en este momento' });
                return;
            }

            var cantidadFinal = l.cantidad;

            if (itemMenu.stockMaximo != null && l.cantidad > itemMenu.stockMaximo) {
                if (itemMenu.stockMaximo <= 0) {
                    itemsProblema.push({ nombre: l.nombre, motivo: 'Sin stock en este momento' });
                    return;
                }
                itemsAjustados.push({
                    nombre: l.nombre,
                    cantidadSolicitada: l.cantidad,
                    cantidadFinal: itemMenu.stockMaximo
                });
                cantidadFinal = itemMenu.stockMaximo;
            }

            itemsDisponibles.push({
                itemId: l.itemId,
                nombre: l.nombre + sufijo,
                cantidad: cantidadFinal,
                precioUnitario: l.precioUnitario
            });
        });

        if (itemsProblema.length === 0 && itemsAjustados.length === 0) {
            enviarPedido(mesaTexto, itemsDisponibles);
            return;
        }

        var modal = document.getElementById('modal-confirmacion');

        if (itemsDisponibles.length === 0) {
            document.getElementById('modal-titulo').textContent = 'Sin productos disponibles';
            document.getElementById('modal-cuerpo').innerHTML =
                'Ninguno de los productos del pedido está disponible ahora mismo:<br><br>' +
                itemsProblema.map(function (p) {
                    return '• <b>' + p.nombre + '</b>: ' + p.motivo;
                }).join('<br>') +
                '<br><br>Revisá el menú y armá el pedido de nuevo.';
            document.getElementById('modal-confirmar').style.display = 'none';
            document.getElementById('modal-cancelar').textContent = 'Cerrar';
            modal.style.display = 'flex';
            return;
        }

        var bloques = [];

        if (itemsProblema.length > 0) {
            bloques.push(
                '<span style="color:var(--tc-brick);font-weight:600;">❌ No disponibles (no se envían):</span><br>' +
                itemsProblema.map(function (p) {
                    return '&nbsp;&nbsp;• <b>' + p.nombre + '</b>: ' + p.motivo;
                }).join('<br>')
            );
        }

        if (itemsAjustados.length > 0) {
            bloques.push(
                '<span style="color:var(--tc-mustard);font-weight:600;">⚠️ Cantidad ajustada por stock:</span><br>' +
                itemsAjustados.map(function (p) {
                    return '&nbsp;&nbsp;• <b>' + p.nombre + '</b>: pediste ' + p.cantidadSolicitada +
                        ', solo se puede preparar <b>' + p.cantidadFinal + '</b>';
                }).join('<br>')
            );
        }

        bloques.push(
            '<span style="color:var(--tc-teal);font-weight:600;">✅ Se envían a cocina:</span><br>' +
            itemsDisponibles.map(function (p) {
                return '&nbsp;&nbsp;• <b>' + p.nombre + '</b> ×' + p.cantidad;
            }).join('<br>')
        );

        document.getElementById('modal-titulo').textContent = 'Revisá el pedido antes de enviarlo';
        document.getElementById('modal-cuerpo').innerHTML =
            bloques.join('<br><br>') +
            '<br><br>¿Confirmás el pedido con estos ajustes, o preferís cancelar y editar el carrito?';
        document.getElementById('modal-confirmar').style.display = '';
        document.getElementById('modal-confirmar').textContent = 'Confirmar y enviar';
        document.getElementById('modal-cancelar').textContent = 'Cancelar y editar';

        document.getElementById('modal-confirmar').onclick = function () {
            cerrarModal();
            enviarPedido(mesaTexto, itemsDisponibles);
        };

        modal.style.display = 'flex';
    }

    function crearPedido() {
        var mesaInput = document.getElementById('mesa-input').value.trim();

        if (tipoEntrega === 'mesa' && !mesaInput) {
            alert('Escribe el número o nombre de la mesa.');
            return;
        }

        var mesaTexto = tipoEntrega === 'mesa' ? 'Mesa ' + mesaInput : 'Para llevar';

        fetch('/Operaciones/Recepcion/ObtenerMenu')
            .then(function (res) {
                if (res.status === 401) { ir('/Operaciones/Auth/Login'); return null; }
                if (!res.ok) throw new Error('HTTP ' + res.status);
                return res.json();
            })
            .then(function (menuActualizado) {
                if (!menuActualizado) return;
                menu = menuActualizado;
                renderChips();
                renderMenu();
                validarYMostrarModal(mesaTexto);
            })
            .catch(function (err) {
                console.error('Error refrescando menú:', err);
                validarYMostrarModal(mesaTexto);
            });
    }

    function cerrarModal() {
        document.getElementById('modal-confirmacion').style.display = 'none';
        document.getElementById('modal-confirmar').onclick = null;
    }

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
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': obtenerTokenAntiforgery()
            },
            body: 'id=' + id
        })
            .then(function (res) {
                if (res.status === 401) { ir('/Operaciones/Auth/Login'); return null; }
                if (!res.ok) {
                    alert('No se pudo marcar como pagado (código ' + res.status + ').');
                    return null;
                }
                return res.json();
            })
            .then(function (data) {
                if (data && data.advertencia) {
                    alert(data.advertencia);
                }
            })
            .catch(function (err) {
                console.error('Error marcando pagado:', err);
                alert('Error de red al marcar como pagado.');
            });
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/pedidos')
        .withAutomaticReconnect()
        .build();

    connection.on('PedidoNuevo', function () { cargarPedidos(); });
    connection.on('PedidoActualizado', function () { cargarPedidos(); });

    connection.onreconnecting(function () {
        setEstadoConexion('reconectando');
    });

    connection.onreconnected(function () {
        setEstadoConexion('conectado');
        cargarPedidos();
    });

    connection.onclose(function () {
        setEstadoConexion('desconectado');
    });

    function iniciarSignalR() {
        connection.start()
            .then(function () {
                setEstadoConexion('conectado');
                return connection.invoke('UnirseAGrupo', 'Recepcion');
            })
            .catch(function (err) {
                console.error('Error iniciando SignalR en Recepción:', err);
                setEstadoConexion('desconectado');
            });
    }

    document.addEventListener('click', function (e) {
        var t = e.target;
        var tarjeta = t.closest('.menu-card');

        if (tarjeta && tarjeta.dataset.itemId && !tarjeta.disabled) {
            agregarAlCarrito(Number(tarjeta.dataset.itemId));
            return;
        }
        if (t.matches('.chip') && t.dataset.categoria) {
            categoriaActiva = t.dataset.categoria;
            renderChips();
            renderMenu();
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
        if (t.matches('#btn-refrescar')) {
            cargarPedidos();
            return;
        }
        if (t.matches('#btn-ver-pedido')) {
            abrirCarrito();
            return;
        }
        if (t.matches('#cerrar-carrito') || t.matches('#carrito-overlay')) {
            cerrarCarrito();
            return;
        }
        if (t.matches('#modal-cancelar') || t.matches('#modal-overlay')) {
            cerrarModal();
            return;
        }
    });

    document.addEventListener('change', function (e) {
        var t = e.target;
        if (t.matches('.carrito-linea__ingredientes input[type="checkbox"]')) {
            toggleIngrediente(Number(t.dataset.index), t.dataset.ing);
        }
    });

    document.addEventListener('input', function (e) {
        if (e.target.matches('#buscador')) {
            textoBusqueda = e.target.value.trim();
            renderMenu();
        }
    });

    setTipoEntrega('mesa');
    cambiarTab('activos');
    cargarMenu();
    cargarPedidos();
    iniciarSignalR();

})();