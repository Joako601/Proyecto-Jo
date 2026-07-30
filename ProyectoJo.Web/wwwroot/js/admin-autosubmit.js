(function () {
    document.addEventListener('change', function (event) {
        var el = event.target;
        if (el && el.hasAttribute('data-autosubmit') && el.form) {
            el.form.submit();
        }
    });
})();
