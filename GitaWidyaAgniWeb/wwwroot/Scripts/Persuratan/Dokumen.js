// Persuratan › Arsip Dokumen. URL dari window.DokumenUrl (Views/Persuratan/Dokumen/Index.cshtml); helper global di js/site.js (AppUI).
window.Dokumen = (function () {
    const URL = window.DokumenUrl;
    const MODAL = 'staticBackdrop';
    const NAMA = { Dokumen: 'Dokumen', Kategori: 'Kategori Dokumen' };
    const loaded = {};

    $(function () {
        refresh('Dokumen');
        $('[data-tab]').on('shown.bs.tab', function () {
            const tab = $(this).data('tab');
            if (!loaded[tab]) refresh(tab);
        });
    });

    function refresh(tab) {
        loaded[tab] = true;
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }

    function add(tab) {
        AppUI.openModal(MODAL, (tab === 'Dokumen' ? 'Unggah ' : 'Tambah ') + NAMA[tab], URL['add' + tab], null, tab === 'Dokumen' ? 'lg' : undefined);
    }
    function edit(tab, row) {
        AppUI.openModal(MODAL, 'Ubah ' + NAMA[tab], URL['edit' + tab], { encKey: row.EncDecrKey }, tab === 'Dokumen' ? 'lg' : undefined);
    }

    function saveDocument(isEdit) {
        const form = $('#frmDokumen').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.berkas = AppUI.dxFile('fileDokumen');
        if (!isEdit && !data.berkas) {
            AppUI.alertWarning('Pilih berkas dokumen.');
            return;
        }
        AppUI.postForm(isEdit ? URL.updateDokumen : URL.simpanDokumen, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Dokumen');
        });
    }
    function saveCategory(isEdit) {
        AppUI.submitForm('#frmKategori', isEdit ? URL.updateKategori : URL.simpanKategori, null, MODAL).done(function () { refresh('Kategori'); });
    }

    const visibilityTemplate = AppUI.badgeTemplate({ Publik: 'success', Anggota: 'info', Pengurus: 'warning' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }
    function fileUrl(row, view) {
        return URL.unduh + '?key=' + encodeURIComponent(row.EncDecrKey) + (view ? '&lihat=true' : '');
    }

    function documentActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-info', 'ri-eye-line', 'Lihat', function () { window.open(fileUrl(row, true), '_blank'); });
        btn(container, 'btn-outline-secondary', 'ri-download-2-line', 'Unduh', function () { window.location.href = fileUrl(row, false); });
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Dokumen', row); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus permanen', function () {
            AppUI.confirmPost('Hapus dokumen permanen?', row.Title + ' tidak bisa dikembalikan.', URL.hapusDokumen, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger').done(function () { refresh('Dokumen'); });
        });
    }

    function categoryActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Kategori', row); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () {
                AppUI.confirmPost('Nonaktifkan kategori?', row.Name, URL.hapusKategori, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger').done(function () { refresh('Kategori'); });
            });
        }
    }

    return {
        add: add, saveDocument: saveDocument, saveCategory: saveCategory,
        visibilityTemplate: visibilityTemplate, documentActionTemplate: documentActionTemplate, categoryActionTemplate: categoryActionTemplate
    };
})();
