(function () {
    function sincronizarAreas(checkboxGeneral) {
        var form = checkboxGeneral.form;
        if (!form) return;
        var fieldset = form.querySelector('.usuarios-form__areas');
        if (!fieldset) return;

        var deshabilitar = checkboxGeneral.checked;
        fieldset.querySelectorAll('input[type="checkbox"]').forEach(function (input) {
            input.disabled = deshabilitar;
            if (deshabilitar) input.checked = false;
        });
        fieldset.classList.toggle('usuarios-form__areas--disabled', deshabilitar);
    }

    document.querySelectorAll('input[name="general"]').forEach(sincronizarAreas);

    document.addEventListener('change', function (event) {
        var el = event.target;
        if (el && el.name === 'general') {
            sincronizarAreas(el);
        }
    });
})();
