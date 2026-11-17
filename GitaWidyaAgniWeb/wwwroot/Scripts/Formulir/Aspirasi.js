// Formulir › Aspirasi. URL dari window.AspirasiUrl (Views/Formulir/Aspirasi/Index.cshtml); helper global di js/site.js (AppUI).
window.Aspirasi = (function () {
    const URL = window.AspirasiUrl;
    const MODAL = 'staticBackdrop';
    const STATUS = { BARU: 1, DIPROSES: 2, SELESAI: 3, DITOLAK: 4 };

    $(function () { refresh(); });

    function refresh() {
        return AppUI.loadPartial('#target', URL.list);
    }

    function detail(row) {
        AppUI.openModal(MODAL, 'Aspirasi', URL.detail, { encKey: row.EncDecrKey }, 'lg');
    }

    function respond() {
        const status = parseInt($('#aspStatus').val(), 10);
        const text = $.trim($('#aspTanggapan').val() || '');
        if (status !== STATUS.DIPROSES && !text) {
            AppUI.alertWarning('Isi tanggapan untuk status Selesai / Ditolak.');
            return;
        }
        AppUI.postAndClose(URL.tanggapi, { Key: $('#aspirasiDetail').data('key'), Status: status, Keterangan: text || null }, MODAL).done(refresh);
    }

    const statusTemplate = AppUI.badgeTemplate({ Baru: 'warning', Diproses: 'info', Selesai: 'success', Ditolak: 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-info', 'ri-chat-check-line', 'Baca & tanggapi', function () { detail(row); });
        if (row.AttachmentFileName) {
            btn(container, 'btn-outline-secondary', 'ri-attachment-2', 'Lampiran', function () { window.open(URL.lampiran + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        if (row.Status === STATUS.DITOLAK) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus aspirasi?', row.Title, URL.delete, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger').done(refresh);
            });
        }
    }

    return { respond: respond, statusTemplate: statusTemplate, actionTemplate: actionTemplate };
})();
