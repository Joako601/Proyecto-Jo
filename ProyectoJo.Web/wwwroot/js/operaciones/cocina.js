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

    function parseModificadores(nombre) {
        var m = /^(.*?)\s*\(sin (.+)\)$/.exec(nombre);
        if (!m) return { base: nombre, mods: null };
        return { base: m[1], mods: 'Sin ' + m[2] };
    }

    function calcularUrgencia(fechaIso) {
        var minutos = (Date.now() - new Date(fechaIso).getTime()) / 60000;
        if (minutos >= 20) return 'urgente';
        if (minutos >= 10) return 'aviso';
        return null;
    }

    function ordenarPorFecha(lista) {
        return lista.slice().sort(function (a, b) {
            return new Date(a.fechaCreacion) - new Date(b.fechaCreacion);
        });
    }

    function crearTarjeta(pedido) {
        var div = document.createElement('div');
        var urgencia = pedido.estado === 'Pendiente' ? calcularUrgencia(pedido.fechaCreacion) : null;
        div.className = 'pedido'
            + (pedido.estado === 'Preparado' ? ' pedido--preparado' : '')
            + (urgencia ? ' pedido--' + urgencia : '');
        div.dataset.fecha = pedido.fechaCreacion;

        var itemsHtml = pedido.items
            .map(function (i) {
                var partes = parseModificadores(i.nombre);
                var modHtml = partes.mods ? '<span class="pedido__mod">' + partes.mods + '</span>' : '';
                return '<li>' + i.cantidad + 'x ' + partes.base + modHtml + '</li>';
            })
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

    function actualizarUrgenciaEnPantalla() {
        var tarjetas = document.querySelectorAll('#col-pendiente .pedido');
        tarjetas.forEach(function (t) {
            var fecha = t.dataset.fecha;
            if (!fecha) return;
            var urgencia = calcularUrgencia(fecha);
            t.classList.remove('pedido--aviso', 'pedido--urgente');
            if (urgencia) t.classList.add('pedido--' + urgencia);
        });
    }

    function actualizarContador(id, n) {
        var el = document.getElementById(id);
        if (el) el.textContent = n > 0 ? '(' + n + ')' : '';
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

        var pendientes = ordenarPorFecha(pedidos.filter(function (p) { return p.estado === 'Pendiente'; }));
        var preparados = ordenarPorFecha(pedidos.filter(function (p) { return p.estado === 'Preparado'; }));

        actualizarContador('contador-pendiente', pendientes.length);
        actualizarContador('contador-preparado', preparados.length);

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
    setInterval(actualizarUrgenciaEnPantalla, 30000);

})();