// Keuangan › Iuran. URL dari window.IuranUrl (Views/Keuangan/Iuran/Index.cshtml); helper global di js/site.js (AppUI).
window.Iuran = (function () {
    const URL = window.IuranUrl;
    const MODAL = 'staticBackdrop';
    const BILL = { UNPAID: 1, PAID: 2, WAIVED: 3 };
    const loaded = {};

    $(function () {
        refresh('Tagihan');
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

    function post(tab, title, text, url, data, label, css) {
        AppUI.confirmPost(title, text, url, data, label, css).done(function () { refresh(tab); });
    }

    /* ===== Tagihan ===== */

    function generate() {
        AppUI.openModal(MODAL, 'Buat Tagihan Iuran', URL.generate, null, 'lg');
    }
    function saveGenerate() {
        const form = $('#frmGenerate').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.BillPeriod = data.BillPeriod ? $.trim(data.BillPeriod).toUpperCase() : null;
        AppUI.postJson(URL.buat, data).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess((resp.Data || 0) + ' tagihan dibuat.');
            refresh('Tagihan');
        });
    }
    function updateBill() {
        AppUI.submitForm('#frmTagihan', URL.updateTagihan, null, MODAL).done(function () { refresh('Tagihan'); });
    }
    function pay() {
        AppUI.submitForm('#frmBayar', URL.simpanBayar, null, MODAL).done(function () { refresh('Tagihan'); });
    }

    function billActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        const name = row.Anggota + ' · ' + row.Iuran + ' ' + row.BillPeriod;
        if (row.Status === BILL.UNPAID) {
            btn(container, 'btn-outline-success', 'ri-hand-coin-line', 'Catat bayar', function () { AppUI.openModal(MODAL, 'Bayar Iuran', URL.bayar, { encKey: row.EncDecrKey }, 'lg'); });
            btn(container, 'btn-outline-info', 'ri-gift-line', 'Bebaskan', function () { post('Tagihan', 'Bebaskan tagihan?', name, URL.bebaskan, key, 'Bebaskan', 'btn-info'); });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Tagihan', URL.editTagihan, { encKey: row.EncDecrKey }, 'lg'); });
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Tagihan', 'Hapus tagihan?', name, URL.hapusTagihan, key, 'Hapus', 'btn-danger'); });
        } else if (row.Status === BILL.PAID) {
            btn(container, 'btn-outline-secondary', 'ri-arrow-go-back-line', 'Batal bayar', function () {
                AppUI.reasonPost('Batalkan pembayaran ' + name + '?', URL.batalBayar, key, 'Keterangan', 'Batalkan', 'btn-secondary')
                    .done(function () { refresh('Tagihan'); });
            });
        } else if (row.Status === BILL.WAIVED) {
            btn(container, 'btn-outline-info', 'ri-arrow-go-back-line', 'Batal bebas', function () { post('Tagihan', 'Batalkan pembebasan?', name + ' kembali Belum Dibayar.', URL.batalBebas, key, 'Batalkan', 'btn-info'); });
        }
    }

    function dueTemplate(container, options) {
        if (!options.value) return;
        const span = $('<span>').text(new Date(options.value).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' })).appendTo(container);
        if (options.data.Terlambat) span.addClass('text-danger fw-semibold').attr('title', 'Lewat jatuh tempo');
    }

    const statusTemplate = AppUI.badgeTemplate({ 'Belum Dibayar': 'warning', Lunas: 'success', Dibebaskan: 'info' });

    /* ===== Pengaturan ===== */

    function add() {
        AppUI.openModal(MODAL, 'Tambah Iuran', URL.addPengaturan, null, 'lg');
    }
    function saveSetting(isEdit) {
        AppUI.submitForm('#frmPengaturan', isEdit ? URL.updatePengaturan : URL.simpanPengaturan, null, MODAL).done(function () { refresh('Pengaturan'); });
    }
    function settingActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Iuran', URL.editPengaturan, { encKey: row.EncDecrKey }, 'lg'); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () { post('Pengaturan', 'Nonaktifkan iuran?', row.Name, URL.hapusPengaturan, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger'); });
        }
    }

    return {
        generate: generate, saveGenerate: saveGenerate, updateBill: updateBill, pay: pay, add: add, saveSetting: saveSetting,
        billActionTemplate: billActionTemplate, settingActionTemplate: settingActionTemplate, dueTemplate: dueTemplate, statusTemplate: statusTemplate
    };
})();
