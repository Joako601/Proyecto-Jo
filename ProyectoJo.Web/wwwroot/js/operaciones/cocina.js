(function () {
    'use strict';

    function obtenerTokenAntiforgery() {
        var meta = document.querySelector('meta[name="request-verification-token"]');
        return meta ? meta.getAttribute('content') : '';
    }

    function formatHora(fechaIso) {
        var d = new Date(fechaIso);
        return d.toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });
    }

    function crearTarjeta(pedido) {
        var div = document.createElement('div');
        div.className = 'pedido' + (pedido.estado === 'Preparado' ? ' pedido--preparado' : '');

        var itemsHtml = pedido.items
            .map(function (i) { return '<li>' + i.cantidad + 'x ' + i.nombre + '</li>'; })
            .join('');

        var accionHtml = pedido.estado === 'Pendiente'
            ? '<button class="pedido__accion" data-id="' + pedido.id + '">Marcar como Preparado</button>'
            : '<span class="pedido__listo">✓ Preparado — esperando pago</span>';

        div.innerHTML =
            '<div class="pedido__header">' +
            '<span class="pedido__mesa">' + pedido.mesa + '</span>' +
            '<span class="pedido__hora">' + formatHora(pedido.fechaCreacion) + '</span>' +
            '</div>' +
            '<ul class="pedido__items">' + itemsHtml + '</ul>' +
            accionHtml;

        return div;
    }

    function setEstadoConexion(estado) {
        var el = document.getElementById('estado-conexion');
        if (!el) return;
        var textos = {
            conectado: '',
            reconectando: 'Reconectando…',
            desconectado: 'Sin conexión — usa el botón de refresco'
        };
        el.textContent = textos[estado] || '';
    }

    function renderPedidos(pedidos) {
        var colPendiente = document.getElementById('col-pendiente');
        var colPreparado = document.getElementById('col-preparado');

        colPendiente.innerHTML = '';
        colPreparado.innerHTML = '';

        var pendientes = pedidos.filter(function (p) { return p.estado === 'Pendiente'; });
        var preparados = pedidos.filter(function (p) { return p.estado === 'Preparado'; });

        if (pendientes.length === 0) {
            colPendiente.innerHTML = '<p class="mensaje-vacio">Sin pedidos pendientes</p>';
        } else {
            pendientes.forEach(function (p) { colPendiente.appendChild(crearTarjeta(p)); });
        }

        if (preparados.length === 0) {
            colPreparado.innerHTML = '<p class="mensaje-vacio">Nada en espera de pago.</p>';
        } else {
            preparados.forEach(function (p) { colPreparado.appendChild(crearTarjeta(p)); });
        }
    }

    function cargarPedidos() {
        fetch('/Operaciones/Cocina/ObtenerPedidos')
            .then(function (res) {
                if (res.status === 401) {
                    window.location.href = '/Operaciones/Auth/Login';
                    return null;
                }
                if (res.status === 403) {
                    setEstadoConexion('desconectado');
                    return null;
                }
                if (!res.ok) throw new Error('HTTP ' + res.status);
                return res.json();
            })
            .then(function (pedidos) {
                if (pedidos) renderPedidos(pedidos);
            })
            .catch(function () {
                setEstadoConexion('desconectado');
            });
    }

    function marcarPreparado(id) {
        fetch('/Operaciones/Cocina/CambiarEstado', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': obtenerTokenAntiforgery()
            },
            body: 'id=' + id + '&nuevoEstado=Preparado'
        })
            .then(function (res) {
                if (res.status === 401) {
                    window.location.href = '/Operaciones/Auth/Login';
                    return;
                }
                if (!res.ok) {
                    alert('No se pudo actualizar el pedido. Código: ' + res.status);
                }
            })
            .catch(function () {
                alert('Error de red al actualizar el pedido.');
            });
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/pedidos')
        .withAutomaticReconnect()
        .build();

    connection.on('PedidoNuevo', function (pedido) {
        cargarPedidos();
    });

    connection.on('PedidoActualizado', function (pedido) {
        cargarPedidos();
    });

    connection.on('Desconectar', function () {
        window.location.href = '/Operaciones/Auth/Salir';
    });

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
                return connection.invoke('UnirseAGrupo', 'Cocina');
            })
            .catch(function (err) {
                console.error('Error iniciando SignalR en Cocina:', err);
                setEstadoConexion('desconectado');
            });
    }

    document.addEventListener('click', function (e) {
        if (e.target && e.target.matches('.pedido__accion')) {
            marcarPreparado(e.target.dataset.id);
        }

        if (e.target && e.target.matches('#btn-refrescar')) {
            cargarPedidos();
        }
    });

    cargarPedidos();
    iniciarSignalR();

})();