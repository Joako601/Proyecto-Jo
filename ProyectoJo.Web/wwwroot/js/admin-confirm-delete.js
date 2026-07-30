(function () {
    document.addEventListener('submit', function (event) {
        var form = event.target;
        if (!(form instanceof HTMLFormElement)) return;
        var mensaje = form.dataset.confirmDelete;
        if (mensaje && !window.confirm(mensaje)) {
            event.preventDefault();
        }
    });
})();
