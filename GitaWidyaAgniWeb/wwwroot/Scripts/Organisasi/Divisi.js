// Organisasi › Divisi. URL dari window.DivisiUrl (Views/Organisasi/Divisi/Index.cshtml); helper global di js/site.js (AppUI).
window.Divisi = (function () {
    const URL = window.DivisiUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Divisi', URL.add, null, 'lg');
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Divisi', URL.edit, { encKey: encKey }, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmDivisi', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmDivisi', URL.update, null, MODAL).done(refresh);
    }

    function deactivate(row) {
        AppUI.confirmPost('Nonaktifkan divisi?', 'Divisi ' + row.Name + ' tidak bisa dipilih lagi.', URL.hapus, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refresh);
    }

    /* ===== Anggota divisi ===== */

    function members(row) {
        AppUI.openModal(MODAL_LARGE, 'Anggota Divisi — ' + row.Name, URL.anggota, { encKey: row.EncDecrKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }

    function memberKey() {
        return $('#divisiAnggota').data('key');
    }

    function refreshMembers() {
        AppUI.loadPartial('#divisiAnggotaTarget', URL.anggotaList, { encKey: memberKey() });
    }

    function addMember() {
        const form = $('#frmDivisiAnggota').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'), { Key: memberKey() });
        data.MemberRole = $.trim(data.MemberRole || '').toUpperCase();
        AppUI.postJson(URL.tambahAnggota, data).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.getEditor('MemberId').reset();
            refreshMembers();
        });
    }

    function removeMember(row) {
        AppUI.confirmPost('Keluarkan anggota?', row.Nama + ' dikeluarkan dari divisi (riwayat tetap tersimpan).', URL.keluarkanAnggota, { Key: row.EncDecrKey }, 'Keluarkan', 'btn-danger')
            .done(refreshMembers);
    }

    /* ===== Grid ===== */

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-group-line', title: 'Anggota divisi', onClick: function () { members(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah divisi', onClick: function () { edit(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan divisi', onClick: function () { deactivate(row); } }));
        }
    }

    function memberActionTemplate(container, options) {
        const row = options.data;
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-user-unfollow-line', title: 'Keluarkan', onClick: function () { removeMember(row); } }));
        }
    }

    return {
        add: add, edit: edit, save: save, update: update, refresh: refresh,
        addMember: addMember, actionTemplate: actionTemplate, memberActionTemplate: memberActionTemplate
    };
})();
