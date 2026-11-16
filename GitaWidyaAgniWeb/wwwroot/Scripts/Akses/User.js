// Akses › User. URL dari window.UserUrl (Views/Akses/User/Index.cshtml); helper global di js/site.js (AppUI).
window.User = (function () {
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', UserUrl.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah User', UserUrl.add);
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah User', UserUrl.edit, { encKey: encKey });
    }

    // POST User/SimpanBaru
    function save() {
        AppUI.submitForm('#frmUser', UserUrl.simpanBaru, null, MODAL).done(refresh);
    }

    // POST User/Update (encKey disimpan di TempData saat Edit dibuka)
    function update() {
        AppUI.submitForm('#frmUser', UserUrl.update, null, MODAL).done(refresh);
    }

    function deactivate(row) {
        AppUI.confirmPost('Nonaktifkan user?', 'User ' + row.Username + ' tidak bisa login lagi.', UserUrl.hapus, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refresh);
    }

    /* ===== Reset password ===== */

    function resetPassword(encKey) {
        AppUI.openModal(MODAL, 'Reset Password', UserUrl.resetPassword, { encKey: encKey });
    }

    function passwordTarget() {
        return $('#frmPassword').dxForm('instance').option('formData').PasswordBaru;
    }

    function savePassword() {
        AppUI.submitForm('#frmPassword', UserUrl.simpanPassword, null, MODAL);
    }

    /* ===== Role user ===== */

    function roles(row) {
        AppUI.openModal(MODAL_LARGE, 'Role User — ' + row.Username, UserUrl.role, { encKey: row.EncDecrKey });
    }

    function roleKey() {
        return $('#userRole').data('key');
    }

    function refreshRoles() {
        AppUI.loadPartial('#userRoleTarget', UserUrl.roleList, { encKey: roleKey() });
        refresh();
    }

    function addRole() {
        const form = $('#frmUserRole').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'), { Key: roleKey() });
        AppUI.postJson(UserUrl.tambahRole, data).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.resetValues();
            refreshRoles();
        });
    }

    function removeRole(row) {
        AppUI.confirmPost('Cabut role?', 'Cabut role ' + row.Nama + ' dari user ini?', UserUrl.hapusRole, { Key: row.EncDecrKey }, 'Cabut', 'btn-danger')
            .done(refreshRoles);
    }

    /* ===== Grid (teks selalu lewat .text()) ===== */

    function memberTemplate(container, options) {
        const d = options.data;
        if (!d.NamaAnggota) {
            $('<span class="text-muted">').text('-').appendTo(container);
            return;
        }
        $('<span class="gwa-cell-main">').text(d.NamaAnggota).appendTo(container);
        $('<span class="gwa-cell-sub">').text(d.NomorAnggota || '').appendTo(container);
    }

    function periodTemplate(container, options) {
        const d = options.data;
        if (!d.Periode) {
            $('<span class="text-muted">').text('Tanpa periode').appendTo(container);
            return;
        }
        $('<span class="me-1">').text(d.Periode).appendTo(container);
        $('<span class="badge fs-12">').addClass(d.StatusPeriode === 'Aktif' ? 'badge-soft-success' : 'badge-soft-secondary')
            .text(d.StatusPeriode).appendTo(container);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-shield-user-line', title: 'Role user', onClick: function () { roles(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-info', icon: 'ri-lock-password-line', title: 'Reset password', onClick: function () { resetPassword(row.EncDecrKey); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah user', onClick: function () { edit(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan user', onClick: function () { deactivate(row); } }));
        }
    }

    function roleActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Cabut role', onClick: function () { removeRole(row); } }));
    }

    return {
        add: add, edit: edit, save: save, update: update, refresh: refresh,
        savePassword: savePassword, passwordTarget: passwordTarget,
        addRole: addRole,
        memberTemplate: memberTemplate, periodTemplate: periodTemplate,
        actionTemplate: actionTemplate, roleActionTemplate: roleActionTemplate
    };
})();
