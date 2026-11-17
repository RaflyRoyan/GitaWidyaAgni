// Kegiatan › Galeri. URL dari window.GaleriUrl (Views/Kegiatan/Galeri/Index.cshtml); helper global di js/site.js (AppUI).
window.Galeri = (function () {
    const URL = window.GaleriUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const STATUS = { DRAFT: 1, PUBLISHED: 2, ARCHIVED: 3 };
    let photoKey = null;

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Galeri', URL.add, null, 'lg');
    }

    function edit(row) {
        AppUI.openModal(MODAL, 'Ubah Galeri', URL.edit, { encKey: row.EncDecrKey }, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmGaleri', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmGaleri', URL.update, null, MODAL).done(refresh);
    }

    function post(title, text, url, row, label, css) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, label, css).done(refresh);
    }

    /* ===== Foto ===== */

    function photos(row) {
        photoKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Foto — ' + row.Title, URL.foto, { encKey: photoKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }

    function reloadPhotos() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.foto, { encKey: photoKey });
    }

    function addPhotos() {
        const el = document.getElementById('fileGaleri');
        const up = el && DevExpress.ui.dxFileUploader.getInstance(el);
        const files = up ? up.option('value') : [];
        if (!files || !files.length) {
            AppUI.alertWarning('Pilih foto terlebih dahulu.');
            return;
        }
        const fd = new FormData();
        files.forEach(function (f) { fd.append('foto', f); });
        AppUI.postForm(URL.tambahFoto + '?key=' + encodeURIComponent(photoKey), fd).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadPhotos();
        });
    }

    function setCover(id) {
        AppUI.postJson(URL.sampul, { Key: photoKey, Id: id }).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadPhotos();
        });
    }

    function caption(id, button) {
        AppUI.inputPost({ title: 'Keterangan foto', input: 'text', label: 'Keterangan', value: $(button).data('caption') || '' },
            URL.keterangan, { Key: photoKey, Id: id }, 'Caption').done(reloadPhotos);
    }

    function removePhoto(id) {
        AppUI.confirmPost('Hapus foto?', 'Foto dihapus permanen. Bila foto ini sampul, sampul pindah ke foto berikutnya.', URL.hapusFoto, { Key: photoKey, Id: id }, 'Hapus', 'btn-danger')
            .done(reloadPhotos);
    }

    /* ===== Grid ===== */

    function titleTemplate(container, options) {
        const d = options.data;
        const wrap = $('<div class="d-flex align-items-center gap-2">').appendTo(container);
        if (d.CoverId) {
            $('<img class="rounded" width="56" height="42" style="object-fit:cover" loading="lazy" alt="">')
                .attr('src', URL.gambar + '?key=' + encodeURIComponent(d.EncDecrKey) + '&id=' + d.CoverId)
                .appendTo(wrap);
        }
        const text = $('<div>').appendTo(wrap);
        const main = $('<div class="gwa-cell-main">').text(d.Title).appendTo(text);
        if (d.Disorot) main.prepend($('<i class="ri-star-fill text-warning me-1" title="Disorot" aria-label="Disorot">'));
    }

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Terbit: 'success', Diarsipkan: 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-success', 'ri-image-add-line', 'Foto galeri', function () { photos(row); });
        if (row.Status !== STATUS.PUBLISHED) {
            btn(container, 'btn-outline-primary', 'ri-global-line', 'Terbitkan', function () { post('Terbitkan galeri?', row.Title + ' tampil di landing page bila Publik.', URL.terbitkan, row, 'Terbitkan', 'btn-primary'); });
        } else {
            btn(container, 'btn-outline-secondary', 'ri-archive-line', 'Arsipkan', function () { post('Arsipkan galeri?', row.Title + ' tidak tampil lagi di landing page.', URL.arsipkan, row, 'Arsipkan', 'btn-secondary'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah galeri', function () { edit(row); });
        if (row.Status === STATUS.DRAFT) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus galeri', function () { post('Hapus galeri?', row.Title + ' dan semua fotonya dihapus.', URL.delete, row, 'Hapus', 'btn-danger'); });
        }
    }

    return {
        add: add, save: save, update: update, addPhotos: addPhotos, setCover: setCover, caption: caption, removePhoto: removePhoto,
        titleTemplate: titleTemplate, statusTemplate: statusTemplate, actionTemplate: actionTemplate
    };
})();
