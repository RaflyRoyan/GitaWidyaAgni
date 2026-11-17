// Organisasi › Kepengurusan. URL dari window.KepengurusanUrl (Views/Organisasi/Kepengurusan/Index.cshtml); helper global di js/site.js (AppUI).
window.Kepengurusan = (function () {
    const URL = window.KepengurusanUrl;
    const MODAL = 'staticBackdrop';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Penugasan', URL.add, null, 'lg');
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Penugasan', URL.edit, { encKey: encKey }, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmKepengurusan', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmKepengurusan', URL.update, null, MODAL).done(refresh);
    }

    function end(row) {
        AppUI.inputPost({
            title: 'Akhiri penugasan?', text: row.Anggota + ' sebagai ' + row.Jabatan + '. Penugasan yang diakhiri tidak bisa diubah lagi.',
            input: 'date', label: 'Tanggal selesai', value: AppUI.toIsoDate(new Date()), required: true, confirmText: 'Akhiri', confirmCss: 'btn-secondary'
        }, URL.akhiri, { Key: row.EncDecrKey }, 'EndDate').done(refresh);
    }

    function remove(row) {
        AppUI.confirmPost('Hapus penugasan?', 'Hapus penugasan ' + row.Anggota + ' sebagai ' + row.Jabatan + '?', URL.hapus, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger')
            .done(refresh);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        if (!row.Berjalan) return;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah penugasan', onClick: function () { edit(row.EncDecrKey); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-secondary', icon: 'ri-stop-circle-line', title: 'Akhiri penugasan', onClick: function () { end(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus penugasan', onClick: function () { remove(row); } }));
    }

    return { add: add, edit: edit, save: save, update: update, refresh: refresh, actionTemplate: actionTemplate };
})();
