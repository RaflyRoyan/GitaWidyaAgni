// Pengadaan. URL dari window.PengadaanUrl (Views/Pengadaan/Pengadaan/Index.cshtml); helper global di js/site.js (AppUI).
window.Pengadaan = (function () {
    const URL = window.PengadaanUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const S = { DRAFT: 1, DIAJUKAN: 2, REVISI: 3, DISETUJUI: 4, DITOLAK: 5, DIBELI: 6, SEBAGIAN: 7, SELESAI: 8, BATAL: 9 };
    const LANGSUNG = 1;
    let detailKey = null;

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Buat Pengadaan', URL.add, null, 'lg');
    }
    function save() {
        AppUI.submitForm('#frmPengadaan', URL.simpanBaru, null, MODAL).done(refresh);
    }
    function update() {
        AppUI.submitForm('#frmPengadaan', URL.update, null, MODAL).done(refresh);
    }
    function pay() {
        AppUI.submitForm('#frmBayar', URL.simpanBayar, null, MODAL).done(refresh);
    }

    function decide(row, url, label, css, required) {
        AppUI.inputPost({ title: label + ' — ' + row.RequestNumber, text: row.Title, input: 'textarea', label: 'Catatan', required: required, confirmText: label, confirmCss: css },
            url, { Key: row.EncDecrKey }, 'Keterangan').done(refresh);
    }

    function buy(row) {
        if (row.Metode === LANGSUNG) {
            AppUI.confirmPost('Tandai dibeli?', row.RequestNumber + ' · pembayaran dicatat terpisah.', URL.beli, { Key: row.EncDecrKey }, 'Beli', 'btn-dark').done(refresh);
            return;
        }
        $.getJSON(URL.opsiPengajuanDana).done(function (items) {
            if (!items || !items.length) {
                AppUI.alertWarning('Belum ada pengajuan dana Disetujui / Dicairkan.');
                return;
            }
            const options = {};
            items.forEach(function (x) { options[x.Value] = x.Text; });
            AppUI.inputPost({ title: 'Pilih pengajuan dana', text: row.Title, input: 'select', options: options, required: true, confirmText: 'Beli', confirmCss: 'btn-dark' },
                URL.beli, { Key: row.EncDecrKey }, 'FundingRequestId').done(refresh);
        });
    }

    /* ===== Detail ===== */

    function detail(row) {
        detailKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Pengadaan ' + row.RequestNumber, URL.detail, { encKey: detailKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }
    function reloadDetail() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.detail, { encKey: detailKey });
    }
    function addItem() {
        const form = $('#frmItemPengadaan').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahItem, $.extend({}, form.option('formData'), { Key: detailKey })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function removeItem(key) {
        AppUI.confirmPost('Hapus barang?', 'Barang dan penawarannya dihapus.', URL.hapusItem, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }
    function actualPrice(key, price) {
        AppUI.inputPost({ title: 'Harga aktual per satuan', input: 'number', value: price, required: true, label: 'Harga (Rp)', confirmText: 'Simpan' },
            URL.hargaAktual, { Key: key }, 'Price').done(reloadDetail);
    }
    function addQuotation() {
        const form = $('#frmPenawaran').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'), { Key: detailKey });
        data.berkas = AppUI.dxFile('filePenawaran');
        AppUI.postForm(URL.tambahPenawaran, AppUI.toFormData(data)).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function selectQuotation(key) {
        AppUI.postJson(URL.pilihPenawaran, { Key: key }).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function removeQuotation(key) {
        AppUI.confirmPost('Hapus penawaran?', 'Penawaran dihapus permanen.', URL.hapusPenawaran, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }
    function removeReceipt(key) {
        AppUI.confirmPost('Hapus penerimaan?', 'Jumlah diterima dikurangi. Ditolak bila sudah dibuatkan aset.', URL.hapusTerima, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }
    function cancelPayment(transactionId) {
        AppUI.reasonPost('Batalkan pembayaran? Transaksinya di-void.', URL.batalBayar, { Key: detailKey, TransactionId: transactionId }, 'Keterangan', 'Batalkan', 'btn-danger').done(reloadDetail);
    }

    /* ===== Penerimaan ===== */

    function receive(row) {
        AppUI.openModal(MODAL_LARGE, 'Terima Barang', URL.terima, { encKey: row.EncDecrKey });
    }
    function saveReceipt() {
        const lines = [];
        $('#pengadaanTerima .terima-line').each(function () {
            lines.push({
                ItemId: Number($(this).data('id')),
                Quantity: Number($(this).find('.t-qty').val()) || 0,
                Condition: Number($(this).find('.t-cond').val()),
                AssetCode: $.trim($(this).find('.t-code').val() || '') || null
            });
        });
        AppUI.postAndClose(URL.simpanTerima, {
            Key: $('#pengadaanTerima').data('key'),
            ReceiptDate: $('#tglTerima').val(),
            ReceivedByMemberId: $('#penerima').dxSelectBox('instance').option('value'),
            Notes: $('#catatanTerima').val(),
            Lines: lines
        }, MODAL_LARGE).done(refresh);
    }

    /* ===== Grid ===== */

    function titleTemplate(container, options) {
        const d = options.data;
        $('<div class="gwa-cell-main">').text(d.Title).appendTo(container);
        if (d.LebihAnggaran) $('<div class="gwa-cell-sub text-danger">').text('Melebihi sisa anggaran').appendTo(container);
    }

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Diajukan: 'warning', Revisi: 'info', Disetujui: 'primary', Ditolak: 'danger', Dibeli: 'dark', 'Diterima Sebagian': 'info', Selesai: 'success', Batal: 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        const s = row.Status;
        btn(container, 'btn-outline-info', 'ri-file-list-3-line', 'Detail', function () { detail(row); });
        if (s === S.DRAFT || s === S.REVISI) {
            btn(container, 'btn-outline-primary', 'ri-send-plane-line', 'Ajukan', function () {
                AppUI.confirmPost('Ajukan pengadaan?', row.RequestNumber + ' · minimal satu barang.', URL.ajukan, key, 'Ajukan', 'btn-primary').done(refresh);
            });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Pengadaan', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
        }
        if (s === S.DIAJUKAN) {
            btn(container, 'btn-outline-success', 'ri-check-double-line', 'Setujui', function () { decide(row, URL.setujui, 'Setujui', 'btn-success', false); });
            btn(container, 'btn-outline-secondary', 'ri-arrow-go-back-line', 'Revisi', function () { decide(row, URL.revisi, 'Minta Revisi', 'btn-secondary', true); });
            btn(container, 'btn-outline-danger', 'ri-close-circle-line', 'Tolak', function () { decide(row, URL.tolak, 'Tolak', 'btn-danger', true); });
        }
        if (s === S.DISETUJUI) {
            btn(container, 'btn-outline-dark', 'ri-shopping-bag-line', 'Beli', function () { buy(row); });
        }
        if ((s === S.DIBELI || s === S.SEBAGIAN || s === S.SELESAI) && row.Metode === LANGSUNG) {
            btn(container, 'btn-outline-dark', 'ri-money-dollar-circle-line', 'Catat pembayaran', function () { AppUI.openModal(MODAL, 'Pembayaran Pengadaan', URL.bayar, { encKey: row.EncDecrKey }, 'lg'); });
        }
        if (s === S.DIBELI || s === S.SEBAGIAN) {
            btn(container, 'btn-outline-success', 'ri-inbox-archive-line', 'Terima barang', function () { receive(row); });
        }
        if (s === S.DRAFT || s === S.DIAJUKAN || s === S.REVISI || s === S.DISETUJUI || s === S.DIBELI) {
            btn(container, 'btn-outline-secondary', 'ri-forbid-line', 'Batalkan', function () {
                AppUI.reasonPost('Batalkan pengadaan ' + row.RequestNumber + '?', URL.batal, key, 'Keterangan', 'Batalkan', 'btn-secondary').done(refresh);
            });
        }
        if (s === S.DRAFT) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus pengadaan?', row.RequestNumber + ' beserta barang dan penawarannya.', URL.delete, key, 'Hapus', 'btn-danger').done(refresh);
            });
        }
    }

    return {
        add: add, save: save, update: update, pay: pay, addItem: addItem, removeItem: removeItem, actualPrice: actualPrice, addQuotation: addQuotation,
        selectQuotation: selectQuotation, removeQuotation: removeQuotation, removeReceipt: removeReceipt, cancelPayment: cancelPayment, saveReceipt: saveReceipt,
        titleTemplate: titleTemplate, statusTemplate: statusTemplate, actionTemplate: actionTemplate
    };
})();
