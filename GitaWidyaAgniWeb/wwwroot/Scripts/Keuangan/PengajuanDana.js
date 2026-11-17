// Keuangan › Pengajuan Dana. URL dari window.PengajuanDanaUrl (Views/Keuangan/PengajuanDana/Index.cshtml); helper global di js/site.js (AppUI).
window.PengajuanDana = (function () {
    const URL = window.PengajuanDanaUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const STATUS = { DRAFT: 1, SUBMITTED: 2, APPROVED: 3, REJECTED: 4, DISBURSED: 5 };
    const LPJ = { BELUM: 1, DIAJUKAN: 2, SELESAI: 3 };
    let detailKey = null;

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Buat Pengajuan Dana', URL.add, null, 'lg');
    }
    function save() {
        AppUI.submitForm('#frmPengajuan', URL.simpanBaru, null, MODAL).done(refresh);
    }
    function update() {
        AppUI.submitForm('#frmPengajuan', URL.update, null, MODAL).done(refresh);
    }
    function disburse() {
        AppUI.submitForm('#frmBayar', URL.simpanCair, null, MODAL).done(refresh);
    }
    function settle() {
        AppUI.submitForm('#frmBayar', URL.simpanSelesai, null, MODAL).done(refresh);
    }

    function post(title, text, url, row, label, css) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, label, css).done(refresh);
    }

    // Keputusan dengan catatan (wajib untuk tolak / revisi).
    function decide(row, url, label, css, required) {
        AppUI.inputPost({ title: label + ' — ' + row.RequestNumber, text: row.Purpose, input: 'textarea', label: 'Catatan', required: required, confirmText: label, confirmCss: css },
            url, { Key: row.EncDecrKey }, 'Keterangan').done(refresh);
    }

    /* ===== Detail & realisasi ===== */

    function detail(row) {
        detailKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Pengajuan ' + row.RequestNumber, URL.detail, { encKey: detailKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }
    function reloadDetail() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.detail, { encKey: detailKey });
    }
    function addRealization() {
        const form = $('#frmRealisasi').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'), { Key: detailKey });
        data.bukti = AppUI.dxFile('fileBukti');
        AppUI.postForm(URL.tambahRealisasi, AppUI.toFormData(data)).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function removeRealization(key) {
        AppUI.confirmPost('Hapus realisasi?', 'Baris realisasi dan buktinya dihapus.', URL.hapusRealisasi, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Diajukan: 'warning', Disetujui: 'primary', Ditolak: 'danger', Dicairkan: 'success' });
    const lpjTemplate = function (container, options) {
        if (options.data.Status !== STATUS.DISBURSED) return;
        AppUI.badgeTemplate({ Belum: 'secondary', Diajukan: 'warning', Selesai: 'success' })(container, options);
    };

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-info', 'ri-file-list-3-line', 'Detail, persetujuan & realisasi', function () { detail(row); });
        if (row.Status === STATUS.DRAFT) {
            btn(container, 'btn-outline-primary', 'ri-send-plane-line', 'Ajukan', function () { post('Ajukan pengajuan dana?', row.RequestNumber + ' · ' + AppUI.rupiah(row.Amount), URL.ajukan, row, 'Ajukan', 'btn-primary'); });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Pengajuan Dana', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Hapus pengajuan?', row.RequestNumber, URL.delete, row, 'Hapus', 'btn-danger'); });
        }
        if (row.Status === STATUS.SUBMITTED) {
            btn(container, 'btn-outline-success', 'ri-check-double-line', 'Setujui langkah ini', function () { decide(row, URL.setujui, 'Setujui', 'btn-success', false); });
            btn(container, 'btn-outline-secondary', 'ri-arrow-go-back-line', 'Minta revisi', function () { decide(row, URL.revisi, 'Minta Revisi', 'btn-secondary', true); });
            btn(container, 'btn-outline-danger', 'ri-close-circle-line', 'Tolak', function () { decide(row, URL.tolak, 'Tolak', 'btn-danger', true); });
        }
        if (row.Status === STATUS.APPROVED) {
            btn(container, 'btn-outline-dark', 'ri-bank-line', 'Cairkan', function () { AppUI.openModal(MODAL, 'Cairkan Dana', URL.cair, { encKey: row.EncDecrKey }, 'lg'); });
        }
        if (row.Status === STATUS.DISBURSED) {
            if (row.Lpj === LPJ.BELUM) {
                btn(container, 'btn-outline-primary', 'ri-send-plane-line', 'Ajukan LPJ', function () { post('Ajukan LPJ?', 'Realisasi tidak bisa diubah setelah diajukan.', URL.ajukanLpj, row, 'Ajukan', 'btn-primary'); });
            } else if (row.Lpj === LPJ.DIAJUKAN) {
                btn(container, 'btn-outline-success', 'ri-check-double-line', 'Selesaikan LPJ', function () { AppUI.openModal(MODAL, 'Selesaikan LPJ', URL.selesai, { encKey: row.EncDecrKey }, 'lg'); });
                btn(container, 'btn-outline-secondary', 'ri-arrow-go-back-line', 'Kembalikan LPJ', function () { post('Kembalikan LPJ?', 'Pengaju bisa memperbaiki realisasi.', URL.kembalikanLpj, row, 'Kembalikan', 'btn-secondary'); });
            }
        }
    }

    return {
        add: add, save: save, update: update, disburse: disburse, settle: settle, addRealization: addRealization, removeRealization: removeRealization,
        statusTemplate: statusTemplate, lpjTemplate: lpjTemplate, actionTemplate: actionTemplate
    };
})();
