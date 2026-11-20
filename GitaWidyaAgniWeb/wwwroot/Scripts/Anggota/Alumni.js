// Anggota › Alumni. URL dari window.AlumniUrl (Views/Anggota/Alumni/Index.cshtml); helper global di js/site.js (AppUI).
window.Alumni = (function () {
    const URL = window.AlumniUrl;
    const MODAL = 'staticBackdrop';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Profil Alumni', URL.add, null, 'lg');
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Profil Alumni', URL.edit, { encKey: encKey }, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmAlumni', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmAlumni', URL.update, null, MODAL).done(refresh);
    }

    function remove(row) {
        AppUI.confirmPost('Hapus profil alumni?', 'Profil alumni ' + row.Nama + ' dihapus (data anggota tetap ada).', URL.hapus, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger')
            .done(refresh);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah profil', onClick: function () { edit(row.EncDecrKey); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus profil', onClick: function () { remove(row); } }));
    }

    /* ===== Undangan alumni ===== */

    function invite() {
        AppUI.openModal(MODAL, 'Undang Alumni', URL.undang, null, 'lg');
    }
    function sendInvite() {
        const form = document.getElementById('frmUndang');
        if (!form.reportValidity()) return;
        const body = {
            Judul: $('#undJudul').val(),
            Pesan: $('#undPesan').val(),
            Kegiatan: $('#undKegiatan').val() || null,
            Angkatan: $('#undAngkatan').val() || null,
            KirimWa: $('#undWa').is(':checked')
        };
        AppUI.postJson(URL.kirimUndangan, body).done(function (r) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(r.Data || r.Message);
        });
    }

    return { add: add, save: save, update: update, refresh: refresh, actionTemplate: actionTemplate, invite: invite, sendInvite: sendInvite };
})();
