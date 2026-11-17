// Keuangan › Hutang. URL dari window.HutangUrl (Views/Keuangan/Hutang/Index.cshtml); helper global di js/site.js (AppUI).
window.Hutang = (function () {
    const URL = window.HutangUrl;
    const MODAL = 'staticBackdrop';
    const STATUS = { BELUM: 1, SEBAGIAN: 2, LUNAS: 3, DIHAPUSKAN: 4 };
    let historyKey = null;

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Catat Hutang', URL.add, null, 'lg');
    }
    function save() {
        AppUI.submitForm('#frmHutang', URL.simpanBaru, null, MODAL).done(refresh);
    }
    function update() {
        AppUI.submitForm('#frmHutang', URL.update, null, MODAL).done(refresh);
    }
    function pay() {
        AppUI.submitForm('#frmBayar', URL.simpanBayar, null, MODAL).done(refresh);
    }

    /* ===== Riwayat ===== */

    function history(row) {
        historyKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'Riwayat — ' + row.DebtNumber, URL.riwayat, { encKey: historyKey }, 'lg');
        $('#' + MODAL).one('hidden.bs.modal', refresh);
    }
    function reloadHistory() {
        AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.riwayat, { encKey: historyKey });
    }
    function removePayment(key) {
        AppUI.reasonPost('Hapus pembayaran? Transaksi pengeluarannya di-void.', URL.hapusBayar, { Key: key }, 'Keterangan', 'Hapus', 'btn-danger').done(reloadHistory);
    }
    function addAttachment() {
        const file = AppUI.dxFile('fileLampiranHutang');
        if (!file) {
            AppUI.alertWarning('Pilih berkas terlebih dahulu.');
            return;
        }
        AppUI.postForm(URL.tambahLampiran + '?key=' + encodeURIComponent(historyKey), AppUI.toFormData({ berkas: file })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadHistory();
        });
    }
    function removeAttachment(key) {
        AppUI.confirmPost('Hapus lampiran?', 'Berkas dihapus permanen.', URL.hapusLampiran, { Key: key }, 'Hapus', 'btn-danger').done(reloadHistory);
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ 'Belum Bayar': 'warning', Sebagian: 'info', Lunas: 'success', Dihapuskan: 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        const open = row.Status === STATUS.BELUM || row.Status === STATUS.SEBAGIAN;
        if (open) {
            btn(container, 'btn-outline-success', 'ri-hand-coin-line', 'Catat pembayaran', function () { AppUI.openModal(MODAL, 'Bayar Hutang', URL.bayar, { encKey: row.EncDecrKey }, 'lg'); });
        }
        btn(container, 'btn-outline-info', 'ri-history-line', 'Riwayat & lampiran', function () { history(row); });
        if (open) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Hutang', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
            btn(container, 'btn-outline-secondary', 'ri-eraser-line', 'Hapuskan', function () {
                AppUI.reasonPost('Hapuskan hutang ' + row.DebtNumber + '? Tindakan ini final.', URL.hapuskan, { Key: row.EncDecrKey }, 'Keterangan', 'Hapuskan', 'btn-secondary').done(refresh);
            });
        }
        if (row.Status === STATUS.BELUM && row.PaidAmount === 0) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus hutang?', row.DebtNumber + (row.Tunai ? '. Transaksi pemasukan pinjamannya di-void.' : ''), URL.delete, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger').done(refresh);
            });
        }
    }

    return {
        add: add, save: save, update: update, pay: pay, removePayment: removePayment, addAttachment: addAttachment, removeAttachment: removeAttachment,
        statusTemplate: statusTemplate, actionTemplate: actionTemplate
    };
})();
