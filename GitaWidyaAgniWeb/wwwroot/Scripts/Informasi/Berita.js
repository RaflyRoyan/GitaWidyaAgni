// Informasi › Berita. URL dari window.BeritaUrl (Views/Informasi/Berita/Index.cshtml); helper global di js/site.js (AppUI).
window.Berita = (function () {
    const URL = window.BeritaUrl;
    const MODAL = 'staticBackdrop';
    const STATUS = { DRAFT: 1, PUBLISHED: 2, ARCHIVED: 3 };

    $(function () { refresh(); });

    function refresh() {
        return AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tulis Berita', URL.add, null, 'lg');
    }
    function save(isEdit) {
        const form = $('#frmBerita').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.sampul = AppUI.dxFile('fileSampul');
        if (isEdit && $('#chkHapusSampul').is(':checked')) data.hapusSampul = true;
        AppUI.postForm(isEdit ? URL.update : URL.simpanBaru, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh();
        });
    }

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Terbit: 'success', Diarsipkan: 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }
    function step(row, url, title, label, css) {
        AppUI.confirmPost(title, row.Title, url, { Key: row.EncDecrKey }, label, css).done(refresh);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        if (row.Status === STATUS.PUBLISHED && row.Publik) {
            btn(container, 'btn-outline-info', 'ri-external-link-line', 'Lihat di situs', function () { window.open(URL.situs + encodeURIComponent(row.Slug), '_blank'); });
        }
        if (row.Status !== STATUS.PUBLISHED) {
            btn(container, 'btn-outline-success', 'ri-send-plane-line', 'Terbitkan', function () { step(row, URL.terbitkan, 'Terbitkan berita?', 'Terbitkan', 'btn-success'); });
        } else {
            btn(container, 'btn-outline-secondary', 'ri-arrow-go-back-line', 'Tarik ke Draft', function () { step(row, URL.tarik, 'Tarik berita ke Draft?', 'Tarik', 'btn-secondary'); });
            btn(container, 'btn-outline-dark', 'ri-archive-line', 'Arsipkan', function () { step(row, URL.arsipkan, 'Arsipkan berita?', 'Arsipkan', 'btn-dark'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Berita', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
        if (row.Status !== STATUS.PUBLISHED) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { step(row, URL.delete, 'Hapus berita?', 'Hapus', 'btn-danger'); });
        }
    }

    return { add: add, save: save, statusTemplate: statusTemplate, actionTemplate: actionTemplate };
})();
