(function () {
    var sidebar = document.getElementById('adminSidebar');
    var toggle = document.querySelector('.admin-sidebar-toggle');
    var backdrop = document.querySelector('.sidebar-backdrop');

    function abrir() {
        sidebar.classList.add('sidebar--open');
        backdrop.classList.add('sidebar-backdrop--visible');
        toggle.setAttribute('aria-expanded', 'true');
        document.body.style.overflow = 'hidden';
    }

    function cerrar() {
        sidebar.classList.remove('sidebar--open');
        backdrop.classList.remove('sidebar-backdrop--visible');
        toggle.setAttribute('aria-expanded', 'false');
        document.body.style.overflow = '';
    }

    if (toggle && sidebar && backdrop) {
        toggle.addEventListener('click', function () {
            sidebar.classList.contains('sidebar--open') ? cerrar() : abrir();
        });
        backdrop.addEventListener('click', cerrar);
        sidebar.querySelectorAll('.nav-link').forEach(function (link) {
            link.addEventListener('click', cerrar);
        });
        window.addEventListener('resize', function () {
            if (window.innerWidth >= 992) cerrar();
        });
    }
})();
