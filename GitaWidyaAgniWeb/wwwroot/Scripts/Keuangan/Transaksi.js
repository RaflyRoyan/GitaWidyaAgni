// Keuangan › Transaksi. URL dari window.TransaksiUrl (Views/Keuangan/Transaksi/Index.cshtml); helper global di js/site.js (AppUI).
window.Transaksi = (function () {
    const URL = window.TransaksiUrl;
    const MODAL = 'staticBackdrop';
    const NAMA = { Transaksi: 'Transaksi', Akun: 'Akun Kas', Kategori: 'Kategori' };
    const STATUS = { DRAFT: 1, POSTED: 2, VOID: 3 };
    const BALANCE = { OPEN: 1, CLOSED: 2 };
    const loaded = {};
    let attachmentKey = null;

    $(function () {
        refresh('Transaksi');
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
        AppUI.openModal(MODAL, 'Tambah ' + NAMA[tab], URL['add' + tab], null, 'lg');
    }

    function edit(tab, row) {
        AppUI.openModal(MODAL, 'Ubah ' + NAMA[tab], URL['edit' + tab], { encKey: row.EncDecrKey }, 'lg');
    }

    function save(tab, isEdit) {
        AppUI.submitForm('#frm' + tab, URL[(isEdit ? 'update' : 'simpan') + tab], null, MODAL).done(function () { refresh(tab); });
    }

    function post(tab, title, text, url, data, label, css) {
        return AppUI.confirmPost(title, text, url, data, label, css).done(function () { refresh(tab); });
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    // Ganti jenis → ganti daftar kategori (window.TransaksiKategori dari _FormTransaksi).
    function typeChanged(e) {
        const form = $('#frmTransaksi').dxForm('instance');
        if (!form || !window.TransaksiKategori) return;
        const editor = form.getEditor('CategoryId');
        editor.option('dataSource', window.TransaksiKategori[e.value] || []);
        if (e.previousValue !== undefined && e.previousValue !== null) editor.option('value', null);
    }

    /* ===== Lampiran ===== */

    function attachments(row) {
        attachmentKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'Lampiran — ' + row.TransactionNumber, URL.lampiran, { encKey: attachmentKey }, 'lg');
        $('#' + MODAL).one('hidden.bs.modal', function () { refresh('Transaksi'); });
    }
    function reloadAttachments() {
        AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.lampiran, { encKey: attachmentKey });
    }
    function addAttachment() {
        const file = AppUI.dxFile('fileLampiran');
        if (!file) {
            AppUI.alertWarning('Pilih berkas terlebih dahulu.');
            return;
        }
        AppUI.postForm(URL.tambahLampiran + '?key=' + encodeURIComponent(attachmentKey), AppUI.toFormData({ berkas: file })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadAttachments();
        });
    }
    function removeAttachment(key) {
        AppUI.confirmPost('Hapus lampiran?', 'Berkas dihapus permanen.', URL.hapusLampiran, { Key: key }, 'Hapus', 'btn-danger').done(reloadAttachments);
    }

    /* ===== Saldo ===== */

    function openBalance() {
        const period = $('#saldoPeriode').dxSelectBox('instance').option('value');
        if (!period) {
            AppUI.alertWarning('Pilih periode terlebih dahulu.');
            return;
        }
        AppUI.confirmPost('Buka saldo periode?', 'Baris saldo dibuat untuk semua akun aktif yang belum punya. Saldo awal = saldo akhir periode sebelumnya.',
            URL.bukaSaldo, { PeriodId: period }, 'Buka', 'btn-primary').done(function (resp) {
                if (resp && resp.Data !== undefined) AppUI.alertSuccess(resp.Data + ' akun dibuka.');
                refresh('Saldo');
            });
    }

    function balanceActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        if (row.Status === BALANCE.OPEN) {
            btn(container, 'btn-outline-info', 'ri-refresh-line', 'Hitung ulang', function () { post('Saldo', 'Hitung ulang saldo?', row.Akun + ' · ' + row.Periode, URL.segarkanSaldo, key, 'Hitung', 'btn-info'); });
            btn(container, 'btn-outline-secondary', 'ri-lock-line', 'Tutup saldo', function () { post('Saldo', 'Tutup saldo periode?', 'Transaksi akun ini di periode tersebut terkunci. Ditolak bila masih ada transaksi Draft.', URL.tutupSaldo, key, 'Tutup', 'btn-secondary'); });
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Saldo', 'Hapus baris saldo?', row.Akun + ' · ' + row.Periode, URL.hapusSaldo, key, 'Hapus', 'btn-danger'); });
        } else {
            btn(container, 'btn-outline-warning', 'ri-lock-unlock-line', 'Buka lagi', function () { post('Saldo', 'Buka lagi saldo?', 'Ditolak bila periode berikutnya sudah punya saldo untuk akun ini.', URL.bukaLagiSaldo, key, 'Buka Lagi', 'btn-warning'); });
        }
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Diposting: 'success', Dibatalkan: 'danger' });
    const balanceStatusTemplate = AppUI.badgeTemplate({ Dibuka: 'success', Ditutup: 'dark' });
    const typeTemplate = AppUI.badgeTemplate({ Pemasukan: 'success', Pengeluaran: 'danger' });

    function transactionActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        btn(container, 'btn-outline-info', 'ri-attachment-2', 'Lampiran (' + row.JumlahLampiran + ')', function () { attachments(row); });
        if (row.Status === STATUS.DRAFT) {
            btn(container, 'btn-outline-primary', 'ri-check-double-line', 'Posting', function () { post('Transaksi', 'Posting transaksi?', row.TransactionNumber + ' masuk ke saldo.', URL.postingTransaksi, key, 'Posting', 'btn-primary'); });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Transaksi', row); });
        }
        if (row.Status !== STATUS.VOID && !row.Sumber) {
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Batalkan', function () {
                AppUI.reasonPost('Batalkan transaksi ' + row.TransactionNumber + '?', URL.batalTransaksi, key, 'Keterangan', 'Batalkan', 'btn-secondary')
                    .done(function () { refresh('Transaksi'); });
            });
        }
        if (row.Status === STATUS.DRAFT) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Transaksi', 'Hapus transaksi?', row.TransactionNumber, URL.hapusTransaksi, key, 'Hapus', 'btn-danger'); });
        }
    }

    function masterActionTemplate(tab) {
        return function (container, options) {
            const row = options.data;
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit(tab, row); });
            if (row.Aktif) {
                btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () {
                    post(tab, 'Nonaktifkan ' + NAMA[tab].toLowerCase() + '?', row.Name, URL['hapus' + tab], { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger');
                });
            }
        };
    }

    return {
        add: add, save: save, typeChanged: typeChanged, addAttachment: addAttachment, removeAttachment: removeAttachment, openBalance: openBalance,
        statusTemplate: statusTemplate, balanceStatusTemplate: balanceStatusTemplate, typeTemplate: typeTemplate,
        transactionActionTemplate: transactionActionTemplate, balanceActionTemplate: balanceActionTemplate, masterActionTemplate: masterActionTemplate
    };
})();
