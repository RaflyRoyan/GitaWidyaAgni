// Organisasi › Section. URL dari window.SectionUrl (Views/Organisasi/Section/Index.cshtml); modal anggota memakai SectionRiwayat.js.
window.Section = (function () {
    const URL = window.SectionUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Section', URL.add, null, 'lg');
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Section', URL.edit, { encKey: encKey }, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmSection', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmSection', URL.update, null, MODAL).done(refresh);
    }

    function deactivate(row) {
        AppUI.confirmPost('Nonaktifkan section?', 'Section ' + row.Name + ' tidak bisa dipilih lagi. Pindahkan dulu semua anggotanya.', URL.hapus, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refresh);
    }

    function members(row) {
        AppUI.openModal(MODAL_LARGE, 'Anggota Section — ' + row.Name, URL.anggota, { encKey: row.EncDecrKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-group-line', title: 'Anggota & riwayat section', onClick: function () { members(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah section', onClick: function () { edit(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan section', onClick: function () { deactivate(row); } }));
        }
    }

    return { add: add, edit: edit, save: save, update: update, refresh: refresh, actionTemplate: actionTemplate };
})();
