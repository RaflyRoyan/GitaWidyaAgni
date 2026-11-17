// Organisasi › Jabatan. URL dari window.JabatanUrl (Views/Organisasi/Jabatan/Index.cshtml); helper global di js/site.js (AppUI).
window.Jabatan = (function () {
    const URL = window.JabatanUrl;
    const MODAL = 'staticBackdrop';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Jabatan', URL.add, null, 'lg');
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Jabatan', URL.edit, { encKey: encKey }, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmJabatan', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmJabatan', URL.update, null, MODAL).done(refresh);
    }

    function deactivate(row) {
        AppUI.confirmPost('Nonaktifkan jabatan?', 'Jabatan ' + row.Name + ' tidak bisa dipilih lagi untuk penugasan baru.', URL.hapus, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refresh);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah jabatan', onClick: function () { edit(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan jabatan', onClick: function () { deactivate(row); } }));
        }
    }

    return { add: add, edit: edit, save: save, update: update, refresh: refresh, actionTemplate: actionTemplate };
})();
