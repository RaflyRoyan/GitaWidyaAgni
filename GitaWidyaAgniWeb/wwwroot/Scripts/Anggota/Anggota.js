// Anggota › Data Anggota. URL dari window.AnggotaUrl (Views/Anggota/Anggota/Index.cshtml); helper global di js/site.js (AppUI).
window.Anggota = (function () {
    const URL = window.AnggotaUrl;
    const MODAL = 'staticBackdropLarge';

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Anggota', URL.add);
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Anggota', URL.edit, { encKey: encKey });
    }

    function formData() {
        const data = $.extend({}, $('#frmAnggota').dxForm('instance').option('formData'));
        data.MemberNumber = data.MemberNumber ? $.trim(data.MemberNumber).toUpperCase() : null;
        return data;
    }

    function submit(url) {
        const form = $('#frmAnggota').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postAndClose(url, formData(), MODAL).done(refresh);
    }

    function save() { submit(URL.simpanBaru); }
    function update() { submit(URL.update); }

    function deactivate(row) {
        AppUI.confirmPost('Nonaktifkan anggota?', row.Fullname + ' ditandai Tidak Aktif dan tanggal keluar diisi hari ini.', URL.hapus, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refresh);
    }

    /* ===== Foto ===== */

    let photoKey = null;

    function photos(row) {
        photoKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'Foto — ' + row.Fullname, URL.foto, { encKey: row.EncDecrKey });
        $('#' + MODAL).one('hidden.bs.modal', refresh);
    }

    function reloadPhotos() {
        AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.foto, { encKey: photoKey });
    }

    function addPhoto() {
        const file = AppUI.dxFile('fileAnggotaFoto');
        if (!file) {
            AppUI.alertWarning('Pilih foto terlebih dahulu.');
            return;
        }
        AppUI.postForm(URL.tambahFoto + '?key=' + encodeURIComponent(photoKey), AppUI.toFormData({ foto: file })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadPhotos();
        });
    }

    function removePhoto(id) {
        AppUI.confirmPost('Hapus foto?', 'Foto dihapus permanen.', URL.hapusFoto, { Key: photoKey, Id: id }, 'Hapus', 'btn-danger')
            .done(reloadPhotos);
    }

    /* ===== Grid ===== */

    function nameTemplate(container, options) {
        const d = options.data;
        const wrap = $('<div class="d-flex align-items-center gap-2">').appendTo(container);
        if (d.FotoId) {
            $('<img class="rounded-circle" width="32" height="32" style="object-fit:cover" loading="lazy">')
                .attr('alt', '')
                .attr('src', URL.gambar + '?key=' + encodeURIComponent(d.EncDecrKey) + '&id=' + d.FotoId)
                .appendTo(wrap);
        } else {
            $('<span class="avatar-xs"><span class="avatar-title rounded-circle bg-soft-primary text-primary fw-semibold">')
                .find('.avatar-title').text((d.Fullname || '?').charAt(0).toUpperCase()).end()
                .appendTo(wrap);
        }
        $('<div>')
            .append($('<div class="gwa-cell-main">').text(d.Fullname))
            .append($('<div class="gwa-cell-sub">').text(d.Kategori))
            .appendTo(wrap);
    }

    const statusTemplate = AppUI.badgeTemplate({ Aktif: 'success', Alumni: 'info', 'Tidak Aktif': 'danger' });

    // Riwayat & pindah section (modal bersama dari Organisasi › Section, Scripts/Organisasi/SectionRiwayat.js).
    function sections(row) {
        AppUI.openModal('staticBackdropLarge', 'Riwayat Section — ' + row.Fullname, window.SectionRiwayatUrl.riwayat, { memberKey: row.EncDecrKey });
        $('#staticBackdropLarge').one('hidden.bs.modal', refresh);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-info', icon: 'ri-image-line', title: 'Foto anggota', onClick: function () { photos(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-music-2-line', title: 'Riwayat section', onClick: function () { sections(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah anggota', onClick: function () { edit(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan anggota', onClick: function () { deactivate(row); } }));
        }
    }

    return {
        add: add, save: save, update: update, refresh: refresh, addPhoto: addPhoto, removePhoto: removePhoto,
        nameTemplate: nameTemplate, statusTemplate: statusTemplate, actionTemplate: actionTemplate
    };
})();
