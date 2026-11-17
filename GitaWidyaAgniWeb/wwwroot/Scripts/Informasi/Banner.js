// Informasi › Banner. URL dari window.BannerUrl (Views/Informasi/Banner/Index.cshtml); helper global di js/site.js (AppUI).
window.Banner = (function () {
    const URL = window.BannerUrl;
    const MODAL = 'staticBackdrop';

    $(function () { refresh(); });

    function refresh() {
        return AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Banner', URL.add, null, 'lg');
    }
    function save(isEdit) {
        const form = $('#frmBanner').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.gambar = AppUI.dxFile('fileBanner');
        if (!isEdit && !data.gambar) {
            AppUI.alertWarning('Pilih gambar banner.');
            return;
        }
        AppUI.postForm(isEdit ? URL.update : URL.simpanBaru, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh();
        });
    }

    function imageTemplate(container, options) {
        $('<img>', { src: URL.gambar + '?key=' + encodeURIComponent(options.data.EncDecrKey), alt: '', loading: 'lazy' })
            .css({ width: 100, height: 56, objectFit: 'cover', borderRadius: 6 })
            .appendTo(container);
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Banner', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus permanen', function () {
            AppUI.confirmPost('Hapus banner?', row.Title + ' dihapus permanen.', URL.delete, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger').done(refresh);
        });
    }

    return { add: add, save: save, imageTemplate: imageTemplate, actionTemplate: actionTemplate };
})();
