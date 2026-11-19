// Inventaris › Peminjaman. URL dari window.PeminjamanUrl (Views/Inventaris/Peminjaman/Index.cshtml); helper global di js/site.js (AppUI).
window.Peminjaman = (function () {
    const URL = window.PeminjamanUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const STATUS = { PENDING: 1, APPROVED: 2, REJECTED: 3, BORROWED: 4, RETURNED: 5, OVERDUE: 6, CANCELLED: 7 };
    const PAY = { BELUM: 1, SEBAGIAN: 2, LUNAS: 3, TIDAK_ADA: 4 };
    let detailKey = null;

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL_LARGE, 'Catat Peminjaman', URL.add);
    }
    function save() {
        AppUI.submitForm('#frmPeminjaman', URL.simpanBaru, null, MODAL_LARGE).done(refresh);
    }
    function update() {
        AppUI.submitForm('#frmPeminjaman', URL.update, null, MODAL_LARGE).done(refresh);
    }
    function pay() {
        AppUI.submitForm('#frmBayar', URL.simpanBayar, null, MODAL).done(refresh);
    }

    /* ===== Detail ===== */

    function detail(row) {
        detailKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Peminjaman ' + row.RequestNumber, URL.detail, { encKey: detailKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }
    function reloadDetail() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.detail, { encKey: detailKey }).done(function () {
            $('#kodeKembali').trigger('focus');
        });
    }
    function addItem() {
        const form = $('#frmItemPinjam').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahItem, $.extend({}, form.option('formData'), { Key: detailKey })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function removeItem(key) {
        AppUI.confirmPost('Hapus item?', 'Item dihapus dari peminjaman.', URL.hapusItem, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }
    // Label QR berisi URL publik .../aset/{token}; scanner bisa memindai URL itu atau mengetik kode unit (server memetakan
    // token ke kode lewat AsetLabel.ResolveKode).
    function unitCode(text) {
        let s = $.trim(String(text || '')).split(/[?#]/)[0].replace(/\/+$/, '');
        s = s.substring(s.lastIndexOf('/') + 1);
        try { s = decodeURIComponent(s); } catch (e) { /* biarkan apa adanya */ }
        return $.trim(s).toUpperCase();
    }
    function returnUnit() {
        const code = unitCode($('#kodeKembali').val());
        if (!code) {
            AppUI.alertWarning('Isi kode unit.');
            return;
        }
        AppUI.postJson(URL.kembaliUnit, { Key: detailKey, UnitCode: code, Condition: Number($('#kondisiKembali').val()), Lost: $('#hilangKembali').is(':checked') })
            .done(function (resp) {
                AppUI.alertSuccess(code + ': ' + resp.Message);
                reloadDetail();
            });
    }
    function returnQuantity(itemKey, sisa) {
        AppUI.promptInput({ title: 'Jumlah kembali', text: 'Sisa belum kembali: ' + sisa, input: 'number', value: sisa, required: true, label: 'Kembali (baik / rusak)', confirmText: 'Lanjut' })
            .then(function (returned) {
                if (returned === null) return;
                AppUI.promptInput({ title: 'Jumlah hilang', input: 'number', value: 0, label: 'Hilang', confirmText: 'Lanjut' }).then(function (lost) {
                    if (lost === null) return;
                    AppUI.promptInput({ title: 'Kondisi barang kembali', input: 'select', options: { 1: 'Baik', 2: 'Rusak Ringan', 3: 'Rusak Berat' }, value: '1', required: true, confirmText: 'Simpan' })
                        .then(function (condition) {
                            if (condition === null) return;
                            AppUI.postJson(URL.kembaliJumlah, { Key: itemKey, Returned: Number(returned) || 0, Lost: Number(lost) || 0, Condition: Number(condition) }).done(function (resp) {
                                AppUI.alertSuccess(resp.Message);
                                reloadDetail();
                            });
                        });
                });
            });
    }
    /* ===== Cetak (DevExpress Reports) ===== */

    function printInvoice(row) {
        AppUI.openReport('Invoice ' + row.RequestNumber, URL.invoice + '?key=' + encodeURIComponent(row.EncDecrKey));
    }
    function printReceipt(row) {
        AppUI.openReport('Kuitansi ' + row.RequestNumber, URL.kuitansi + '?key=' + encodeURIComponent(row.EncDecrKey));
    }
    // Dari modal Detail: viewer dibuka di tab baru (modal layar penuh tidak ditumpuk di atas modal Detail).
    function printPaymentReceipt(transactionId) {
        window.open(URL.kuitansi + '?' + $.param({ key: detailKey, trx: transactionId }), '_blank');
    }

    function cancelPayment(transactionId) {
        AppUI.reasonPost('Batalkan pembayaran? Transaksinya di-void.', URL.batalBayar, { Key: detailKey, TransactionId: transactionId }, 'Keterangan', 'Batalkan', 'btn-danger').done(reloadDetail);
    }

    /* ===== Checkout ===== */

    function checkout(row) {
        AppUI.openModal(MODAL_LARGE, 'Serahkan Barang', URL.checkout, { encKey: row.EncDecrKey });
    }
    function saveCheckout() {
        const lines = [];
        let error = null;
        $('#pinjamCheckout .checkout-item').each(function () {
            const id = Number($(this).data('id'));
            if ($(this).data('unit') === 1 || $(this).data('unit') === '1') {
                const codes = ($('#unit' + id).dxTagBox('instance').option('value') || []).map(unitCode);
                if (codes.length !== Number($(this).data('qty'))) error = 'Jumlah kode unit harus ' + $(this).data('qty') + ' untuk setiap aset per unit.';
                lines.push({ BorrowItemId: id, UnitCodes: codes });
            } else {
                lines.push({ BorrowItemId: id, Condition: Number($('#kondisi' + id).val()) });
            }
        });
        if (error) {
            AppUI.alertWarning(error);
            return;
        }
        AppUI.postAndClose(URL.simpanCheckout, { Key: $('#pinjamCheckout').data('key'), Lines: lines }, MODAL_LARGE).done(refresh);
    }

    /* ===== Grid ===== */

    function borrowerTemplate(container, options) {
        const d = options.data;
        $('<div class="gwa-cell-main">').text(d.Peminjam).appendTo(container);
        $('<div class="gwa-cell-sub">').text(d.Jenis + ' · ' + d.Skema).appendTo(container);
    }

    const statusTemplate = AppUI.badgeTemplate({ Menunggu: 'secondary', Disetujui: 'primary', Ditolak: 'danger', Dipinjam: 'warning', Dikembalikan: 'success', Terlambat: 'danger', Dibatalkan: 'dark' });
    const paymentTemplate = AppUI.badgeTemplate({ 'Belum Bayar': 'warning', Sebagian: 'info', Lunas: 'success', 'Tidak Ada Tagihan': 'secondary' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        btn(container, 'btn-outline-info', 'ri-file-list-3-line', 'Detail', function () { detail(row); });
        if (row.Status === STATUS.PENDING) {
            btn(container, 'btn-outline-success', 'ri-check-double-line', 'Setujui', function () {
                AppUI.confirmPost('Setujui peminjaman?', row.RequestNumber + ' · ' + row.Peminjam, URL.setujui, key, 'Setujui', 'btn-success').done(refresh);
            });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL_LARGE, 'Ubah Peminjaman', URL.edit, { encKey: row.EncDecrKey }); });
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Tolak', function () {
                AppUI.reasonPost('Tolak peminjaman ' + row.RequestNumber + '?', URL.tolak, key, 'Keterangan', 'Tolak', 'btn-secondary').done(refresh);
            });
        }
        if (row.Status === STATUS.APPROVED) {
            btn(container, 'btn-outline-primary', 'ri-hand-heart-line', 'Serahkan barang', function () { checkout(row); });
        }
        if (row.Status === STATUS.PENDING || row.Status === STATUS.APPROVED) {
            btn(container, 'btn-outline-secondary', 'ri-forbid-line', 'Batalkan', function () {
                AppUI.reasonPost('Batalkan peminjaman ' + row.RequestNumber + '?', URL.batal, key, 'Keterangan', 'Batalkan', 'btn-secondary').done(refresh);
            });
        }
        if (row.Status !== STATUS.REJECTED && row.Status !== STATUS.CANCELLED && (row.Seikhlasnya || row.Bayar === PAY.BELUM || row.Bayar === PAY.SEBAGIAN)) {
            btn(container, 'btn-outline-dark', 'ri-money-dollar-circle-line', 'Catat pembayaran', function () { AppUI.openModal(MODAL, 'Pembayaran Peminjaman', URL.bayar, { encKey: row.EncDecrKey }, 'lg'); });
        }
        if (row.Status !== STATUS.REJECTED && row.Status !== STATUS.CANCELLED) {
            btn(container, 'btn-outline-primary', 'ri-file-text-line', 'Cetak invoice', function () { printInvoice(row); });
        }
        if (row.PaidAmount > 0) {
            btn(container, 'btn-outline-success', 'ri-receipt-line', 'Cetak kuitansi', function () { printReceipt(row); });
        }
        if (row.Status === STATUS.RETURNED && row.Tagihan > row.PaidAmount && !row.Seikhlasnya) {
            btn(container, 'btn-outline-info', 'ri-gift-line', 'Bebaskan denda', function () {
                AppUI.promptInput({ title: 'Bebaskan denda', input: 'number', label: 'Nominal dibebaskan', required: true, confirmText: 'Lanjut' }).then(function (amount) {
                    if (amount === null) return;
                    AppUI.inputPost({ title: 'Catatan pembebasan', input: 'textarea', label: 'Catatan', confirmText: 'Bebaskan' }, URL.bebasDenda, { Key: row.EncDecrKey, Amount: Number(amount) }, 'Keterangan').done(refresh);
                });
            });
        }
        if (row.Status === STATUS.PENDING || row.Status === STATUS.REJECTED || row.Status === STATUS.CANCELLED) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus peminjaman?', row.RequestNumber, URL.delete, key, 'Hapus', 'btn-danger').done(refresh);
            });
        }
    }

    return {
        add: add, save: save, update: update, pay: pay, addItem: addItem, removeItem: removeItem, returnUnit: returnUnit, returnQuantity: returnQuantity,
        cancelPayment: cancelPayment, printPaymentReceipt: printPaymentReceipt, saveCheckout: saveCheckout, borrowerTemplate: borrowerTemplate, statusTemplate: statusTemplate,
        paymentTemplate: paymentTemplate, actionTemplate: actionTemplate
    };
})();
