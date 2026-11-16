// Akses › Role. URL dari window.RoleUrl (Views/Akses/Role/Index.cshtml); helper global di js/site.js (AppUI).
window.Role = (function () {
    const MODAL = 'staticBackdrop';
    const MODAL_FULL = 'staticBackdropFullScreen';
    const SUPERADMIN = 'SUPERADMIN';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', RoleUrl.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Role', RoleUrl.add, null, 'lg');
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Role', RoleUrl.edit, { encKey: encKey }, 'lg');
    }

    // Kode role selalu huruf besar.
    function formData() {
        const data = $.extend({}, $('#frmRole').dxForm('instance').option('formData'));
        data.Code = $.trim(data.Code || '').toUpperCase();
        return data;
    }

    function submit(url) {
        const form = $('#frmRole').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postAndClose(url, formData(), MODAL).done(refresh);
    }

    // POST Role/SimpanBaru
    function save() {
        submit(RoleUrl.simpanBaru);
    }

    // POST Role/Update (encKey disimpan di TempData saat Edit dibuka)
    function update() {
        submit(RoleUrl.update);
    }

    function deactivate(row) {
        AppUI.confirmPost('Nonaktifkan role?', 'User dengan role ' + row.Name + ' kehilangan aksesnya.', RoleUrl.hapus, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refresh);
    }

    /* ===== Hak akses (dua TreeList berseleksi rekursif) ===== */

    function access(row) {
        AppUI.openModal(MODAL_FULL, 'Hak Akses — ' + row.Name, RoleUrl.akses, { encKey: row.EncDecrKey });
    }

    function saveAccess() {
        const aksi = $('#treeAksi').dxTreeList('instance');
        const menu = $('#treeMenu').dxTreeList('instance');
        const data = {
            Key: $('#roleAkses').data('key'),
            ActionIds: aksi.getSelectedRowsData('leavesOnly').filter(function (x) { return x.ActionId; }).map(function (x) { return x.ActionId; }),
            Menus: menu.getSelectedRowsData('leavesOnly').filter(function (x) { return x.MenuValue; }).map(function (x) { return x.MenuValue; })
        };
        AppUI.postAndClose(RoleUrl.simpanAkses, data, MODAL_FULL);
    }

    function nodeActionTemplate(container, options) {
        const d = options.data;
        if (d.ActionId) $('<span class="gwa-code">').text(d.Nama).appendTo(container);
        else $('<span class="fw-semibold">').text(d.Nama).appendTo(container);
    }

    function nodeMenuTemplate(container, options) {
        const d = options.data;
        if (d.Ikon) $('<i>').addClass(d.Ikon + ' me-1').attr('aria-hidden', 'true').appendTo(container);
        $('<span>').addClass(d.ParentId ? '' : 'fw-semibold').text(d.Nama).appendTo(container);
    }

    /* ===== Grid ===== */

    function actionTemplate(container, options) {
        const row = options.data;
        const superAdmin = row.Code === SUPERADMIN;
        if (!superAdmin) {
            container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-shield-keyhole-line', title: 'Hak akses', onClick: function () { access(row); } }));
        }
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah role', onClick: function () { edit(row.EncDecrKey); } }));
        if (row.Aktif && !superAdmin) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan role', onClick: function () { deactivate(row); } }));
        }
    }

    return {
        add: add, edit: edit, save: save, update: update, refresh: refresh,
        saveAccess: saveAccess, nodeActionTemplate: nodeActionTemplate, nodeMenuTemplate: nodeMenuTemplate,
        actionTemplate: actionTemplate
    };
})();
