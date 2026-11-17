// Merchandise. URL dari window.MerchandiseUrl (Views/Merchandise/Merchandise/Index.cshtml); helper global di js/site.js (AppUI).
window.Merchandise = (function () {
    const URL = window.MerchandiseUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const ORDER = { DIPESAN: 1, SIAP: 2, SELESAI: 3, BATAL: 4 };
    const PAY = { BELUM: 1, SEBAGIAN: 2, LUNAS: 3 };
    const loaded = {};
    let currentKey = null;

    $(function () {
        refresh('Pesanan');
        $('[data-tab]').on('shown.bs.tab', function () {
            const tab = $(this).data('tab');
            if (!loaded[tab]) refresh(tab);
        });
    });

    function refresh(tab) {
        loaded[tab] = true;
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    /* ===== Produk ===== */

    function addProduct() {
        AppUI.openModal(MODAL, 'Tambah Produk', URL.addProduk, null, 'lg');
    }
    function saveProduct(isEdit) {
        const form = $('#frmProduk').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.foto = AppUI.dxFile('fileProduk');
        if (isEdit && $('#chkHapusFotoProduk').is(':checked')) data.hapusFoto = true;
        AppUI.postForm(isEdit ? URL.updateProduk : URL.simpanProduk, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Produk');
        });
    }
    function variants(row) {
        currentKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'Varian — ' + row.Name, URL.varian, { encKey: currentKey }, 'lg');
        $('#' + MODAL).one('hidden.bs.modal', function () { refresh('Produk'); });
    }
    function reloadVariants() {
        AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.varian, { encKey: currentKey });
    }
    function addVariant() {
        const form = $('#frmVarian').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahVarian, $.extend({}, form.option('formData'), { Key: currentKey })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadVariants();
        });
    }
    function removeVariant(key) {
        AppUI.confirmPost('Nonaktifkan varian?', 'Varian tidak bisa dipesan lagi.', URL.hapusVarian, { Key: key }, 'Nonaktifkan', 'btn-danger').done(reloadVariants);
    }
    function saveStock() {
        AppUI.submitForm('#frmStok', URL.simpanStok, null, MODAL).done(function () { refresh('Produk'); loaded.Mutasi = false; });
    }

    function productTemplate(container, options) {
        const d = options.data;
        const wrap = $('<div class="d-flex align-items-center gap-2">').appendTo(container);
        if (d.HasPhoto) {
            $('<img class="rounded" width="40" height="40" style="object-fit:cover" loading="lazy" alt="">')
                .attr('src', URL.fotoProduk + '?key=' + encodeURIComponent(d.EncDecrKey)).appendTo(wrap);
        }
        $('<div class="gwa-cell-main">').text(d.Name).appendTo(wrap);
    }
    function stockTemplate(container, options) {
        const d = options.data;
        if (!d.Dilacak) {
            $('<span class="text-muted">').text('-').appendTo(container);
            return;
        }
        $('<span>').addClass(d.Stock <= 0 ? 'text-danger fw-semibold' : '').text(d.Stock + ' ' + d.Unit).appendTo(container);
    }
    function productActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-success', 'ri-list-check-2', 'Varian', function () { variants(row); });
        if (row.Dilacak) {
            btn(container, 'btn-outline-primary', 'ri-stack-line', 'Mutasi stok', function () { AppUI.openModal(MODAL, 'Mutasi Stok — ' + row.Name, URL.stok, { encKey: row.EncDecrKey }, 'lg'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Produk', URL.editProduk, { encKey: row.EncDecrKey }, 'lg'); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () {
                AppUI.confirmPost('Nonaktifkan produk?', row.Name, URL.hapusProduk, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger').done(function () { refresh('Produk'); });
            });
        }
    }

    /* ===== Pesanan ===== */

    function addOrder() {
        AppUI.openModal(MODAL_LARGE, 'Catat Pesanan', URL.addPesanan).done(addOrderLine);
    }
    function addOrderLine() {
        const tpl = document.getElementById('tplBarisPesanan');
        if (tpl) $('#barisPesanan').append(tpl.content.cloneNode(true));
    }
    function saveOrder() {
        const form = $('#frmPesanan').dxForm('instance');
        if (!form.validate().isValid) return;
        const items = [];
        $('#barisPesanan .baris-pesanan').each(function () {
            const p = $(this).find('.p-produk').val();
            if (p) items.push({ Product: p, Quantity: Number($(this).find('.p-qty').val()) || 0 });
        });
        if (!items.length) {
            AppUI.alertWarning('Tambahkan minimal satu barang.');
            return;
        }
        AppUI.postAndClose(URL.simpanPesanan, $.extend({}, form.option('formData'), { Items: items }), MODAL_LARGE).done(function () {
            refresh('Pesanan');
            loaded.Produk = false;
            loaded.Mutasi = false;
        });
    }
    function pay() {
        AppUI.submitForm('#frmBayar', URL.simpanBayar, null, MODAL).done(function () { refresh('Pesanan'); });
    }
    function detail(row) {
        currentKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'Pesanan ' + row.OrderNumber, URL.detail, { encKey: currentKey }, 'lg');
        $('#' + MODAL).one('hidden.bs.modal', function () { refresh('Pesanan'); });
    }
    function cancelPayment(transactionId) {
        AppUI.reasonPost('Batalkan pembayaran? Transaksinya di-void.', URL.batalBayar, { Key: currentKey, TransactionId: transactionId }, 'Keterangan', 'Batalkan', 'btn-danger')
            .done(function () { AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.detail, { encKey: currentKey }); });
    }

    const orderStatusTemplate = AppUI.badgeTemplate({ Dipesan: 'secondary', 'Siap Diambil': 'primary', Selesai: 'success', Batal: 'dark' });
    const paymentTemplate = AppUI.badgeTemplate({ 'Belum Bayar': 'warning', Sebagian: 'info', Lunas: 'success' });

    function orderActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        const open = row.Status === ORDER.DIPESAN || row.Status === ORDER.SIAP;
        btn(container, 'btn-outline-info', 'ri-file-list-3-line', 'Detail & pembayaran', function () { detail(row); });
        if (open && row.Bayar !== PAY.LUNAS) {
            btn(container, 'btn-outline-dark', 'ri-money-dollar-circle-line', 'Catat pembayaran', function () { AppUI.openModal(MODAL, 'Pembayaran Pesanan', URL.bayar, { encKey: row.EncDecrKey }, 'lg'); });
        }
        if (row.Status === ORDER.DIPESAN) {
            btn(container, 'btn-outline-primary', 'ri-inbox-unarchive-line', 'Siap diambil', function () {
                AppUI.confirmPost('Pesanan siap diambil?', row.OrderNumber, URL.siap, key, 'Siap', 'btn-primary').done(function () { refresh('Pesanan'); });
            });
        }
        if (open) {
            btn(container, 'btn-outline-success', 'ri-checkbox-circle-line', 'Selesai', function () {
                AppUI.confirmPost('Pesanan selesai?', 'Harus sudah Lunas.', URL.selesai, key, 'Selesai', 'btn-success').done(function () { refresh('Pesanan'); });
            });
            if (row.PaidAmount === 0) {
                btn(container, 'btn-outline-danger', 'ri-close-circle-line', 'Batalkan', function () {
                    AppUI.reasonPost('Batalkan pesanan ' + row.OrderNumber + '? Stok dikembalikan.', URL.batal, key, 'Keterangan', 'Batalkan', 'btn-danger')
                        .done(function () { refresh('Pesanan'); loaded.Produk = false; });
                });
            }
        }
    }

    return {
        addProduct: addProduct, saveProduct: saveProduct, addVariant: addVariant, removeVariant: removeVariant, saveStock: saveStock,
        addOrder: addOrder, addOrderLine: addOrderLine, saveOrder: saveOrder, pay: pay, cancelPayment: cancelPayment,
        productTemplate: productTemplate, stockTemplate: stockTemplate, productActionTemplate: productActionTemplate,
        orderStatusTemplate: orderStatusTemplate, paymentTemplate: paymentTemplate, orderActionTemplate: orderActionTemplate
    };
})();
