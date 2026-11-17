// Inventaris › Aset. URL dari window.AsetUrl (Views/Inventaris/Aset/Index.cshtml); helper global di js/site.js (AppUI).
// Label QR unit dicetak lewat DevExpress Reports (Reports/LabelQrUnitReport.cs, viewer LabelQr di modal layar penuh); isi QR = kode unit.
window.Aset = (function () {
    const URL = window.AsetUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const NAMA = { Aset: 'Aset', Perawatan: 'Perawatan', Inspeksi: 'Inspeksi', Kategori: 'Kategori Aset' };
    const STATUS = { AVAILABLE: 1, BORROWED: 2, MAINTENANCE: 3, DISPOSED: 4 };
    const MT = { SCHEDULED: 1, PROGRESS: 2, DONE: 3 };
    const loaded = {};
    let currentKey = null;

    $(function () {
        refresh('Aset');
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
        AppUI.openModal(tab === 'Aset' ? MODAL_LARGE : MODAL, 'Tambah ' + NAMA[tab], URL['add' + tab], null, tab === 'Aset' ? null : 'lg');
    }

    function edit(tab, row) {
        AppUI.openModal(tab === 'Aset' ? MODAL_LARGE : MODAL, 'Ubah ' + NAMA[tab], URL['edit' + tab], { encKey: row.EncDecrKey }, tab === 'Aset' ? null : 'lg');
    }

    function save(tab, isEdit) {
        const form = $('#frm' + tab).dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        ['AssetCode', 'UnitPrefix', 'UnitCode'].forEach(function (f) { if (data[f]) data[f] = $.trim(data[f]).toUpperCase(); });
        AppUI.postAndClose(URL[(isEdit ? 'update' : 'simpan') + tab], data, tab === 'Aset' ? MODAL_LARGE : MODAL).done(function () { refresh(tab); });
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function post(tab, title, text, url, data, label, css) {
        AppUI.confirmPost(title, text, url, data, label, css).done(function () { refresh(tab); });
    }

    /* ===== Unit & label ===== */

    function units(row) {
        currentKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Unit — ' + row.Name, URL.unit, { encKey: currentKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () { refresh('Aset'); });
    }
    function reloadUnits() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.unit, { encKey: currentKey });
    }
    function addUnit() {
        const form = $('#frmUnit').dxForm('instance');
        AppUI.postJson(URL.tambahUnit, $.extend({}, form.option('formData'), { Key: currentKey })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadUnits();
        });
    }
    function removeUnit(key, code) {
        AppUI.confirmPost('Hapus unit ' + code + '?', 'Jumlah aset berkurang dan mutasi keluar dicatat. Nomor unit tidak dipakai ulang.', URL.hapusUnit, { Key: key }, 'Hapus', 'btn-danger').done(reloadUnits);
    }

    // Server menyiapkan data label (unit ditandai tercetak), lalu viewer DevExpress Reports dibuka di modal layar penuh.
    // Cetak / export PDF dari toolbar viewer.
    function printLabels() {
        const keys = $('.chk-unit:checked').map(function () { return $(this).val(); }).get();
        if (!keys.length) {
            AppUI.alertWarning('Centang unit yang labelnya akan dicetak.');
            return;
        }
        AppUI.postJson(URL.cetakLabel, { AsetKey: currentKey, Keys: keys }).done(function (resp) {
            reloadUnits();
            AppUI.openReport('Label QR — ' + $('#asetUnit .gwa-mform-info-title').text(), URL.labelQr + '?id=' + encodeURIComponent(resp.Data));
        });
    }

    /* ===== Foto ===== */

    function photos(row) {
        currentKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'Foto — ' + row.Name, URL.foto, { encKey: currentKey }, 'lg');
        $('#' + MODAL).one('hidden.bs.modal', function () { refresh('Aset'); });
    }
    function reloadPhotos() {
        AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.foto, { encKey: currentKey });
    }
    function addPhoto() {
        const file = AppUI.dxFile('fileAsetFoto');
        if (!file) {
            AppUI.alertWarning('Pilih foto terlebih dahulu.');
            return;
        }
        AppUI.postForm(URL.tambahFoto + '?key=' + encodeURIComponent(currentKey), AppUI.toFormData({ foto: file })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadPhotos();
        });
    }
    function removePhoto(seq) {
        AppUI.confirmPost('Hapus foto?', 'Foto dihapus permanen.', URL.hapusFoto, { Key: currentKey, Seq: seq }, 'Hapus', 'btn-danger').done(reloadPhotos);
    }

    /* ===== Mutasi, perawatan, inspeksi ===== */

    function saveMovement() {
        AppUI.submitForm('#frmMutasi', URL.simpanMutasi, null, MODAL).done(function () { refresh('Aset'); loaded.Mutasi = false; });
    }
    function finish() {
        AppUI.submitForm('#frmSelesai', URL.simpanSelesai, null, MODAL).done(function () { refresh('Perawatan'); loaded.Aset = false; });
    }
    function saveInspection() {
        const form = $('#frmInspeksi').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        if (data.UnitCode) data.UnitCode = $.trim(data.UnitCode).toUpperCase();
        data.foto = AppUI.dxFile('fileInspeksi');
        AppUI.postForm(URL.simpanInspeksi, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Inspeksi');
            loaded.Aset = false;
        });
    }

    /* ===== Template grid ===== */

    function nameTemplate(container, options) {
        const d = options.data;
        const wrap = $('<div class="d-flex align-items-center gap-2">').appendTo(container);
        if (d.FotoSeq) {
            $('<img class="rounded" width="40" height="40" style="object-fit:cover" loading="lazy" alt="">')
                .attr('src', URL.gambar + '?key=' + encodeURIComponent(d.EncDecrKey) + '&seq=' + d.FotoSeq).appendTo(wrap);
        }
        $('<div>').append($('<div class="gwa-cell-main">').text(d.Name)).append($('<div class="gwa-cell-sub">').text(d.Brand || '')).appendTo(wrap);
    }
    function qtyTemplate(container, options) {
        container.text(options.data.Quantity + ' ' + options.data.Unit);
    }

    const statusTemplate = AppUI.badgeTemplate({ Tersedia: 'success', Dipinjam: 'warning', Perawatan: 'info', Dihapuskan: 'dark' });
    const conditionTemplate = AppUI.badgeTemplate({ Baik: 'success', 'Rusak Ringan': 'warning', 'Rusak Berat': 'danger', Hilang: 'dark' });
    const maintenanceStatusTemplate = AppUI.badgeTemplate({ Terjadwal: 'secondary', Dikerjakan: 'primary', Selesai: 'success' });

    function assetActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === STATUS.DISPOSED) return;
        if (row.PerUnit) {
            btn(container, 'btn-outline-success', 'ri-qr-code-line', 'Unit & label QR', function () { units(row); });
        } else {
            btn(container, 'btn-outline-primary', 'ri-arrow-left-right-line', 'Mutasi stok', function () { AppUI.openModal(MODAL, 'Mutasi Stok', URL.mutasi, { encKey: row.EncDecrKey }, 'lg'); });
        }
        btn(container, 'btn-outline-info', 'ri-image-line', 'Foto', function () { photos(row); });
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Aset', row); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapuskan aset', function () {
            post('Aset', 'Hapuskan aset?', row.Name + ' berstatus Dihapuskan dan unit yang tersisa ikut dihapus. Ditolak bila masih dipinjam.', URL.hapusAset, { Key: row.EncDecrKey }, 'Hapuskan', 'btn-danger');
        });
    }

    function maintenanceActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        if (row.Status === MT.SCHEDULED) {
            btn(container, 'btn-outline-primary', 'ri-play-circle-line', 'Mulai', function () { post('Perawatan', 'Mulai perawatan?', 'Aset / unit berstatus Perawatan dan tidak bisa dipinjam.', URL.mulaiPerawatan, key, 'Mulai', 'btn-primary'); });
        }
        if (row.Status !== MT.DONE) {
            btn(container, 'btn-outline-success', 'ri-checkbox-circle-line', 'Selesai', function () { AppUI.openModal(MODAL, 'Selesaikan Perawatan', URL.selesai, { encKey: row.EncDecrKey }, 'lg'); });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Perawatan', row); });
        }
        if (row.Status === MT.SCHEDULED) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Perawatan', 'Hapus perawatan?', row.Description, URL.hapusPerawatan, key, 'Hapus', 'btn-danger'); });
        }
    }

    function inspectionActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-info', 'ri-camera-line', 'Foto', function () { window.open(URL.fotoInspeksi + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        if (!row.DariPengembalian) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Inspeksi', 'Hapus inspeksi?', row.Aset, URL.hapusInspeksi, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger'); });
        }
    }

    function categoryActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Kategori', row); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () { post('Kategori', 'Nonaktifkan kategori?', row.Name, URL.hapusKategori, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger'); });
        }
    }

    return {
        add: add, save: save, addUnit: addUnit, removeUnit: removeUnit, printLabels: printLabels, addPhoto: addPhoto, removePhoto: removePhoto,
        saveMovement: saveMovement, finish: finish, saveInspection: saveInspection,
        nameTemplate: nameTemplate, qtyTemplate: qtyTemplate, statusTemplate: statusTemplate, conditionTemplate: conditionTemplate,
        maintenanceStatusTemplate: maintenanceStatusTemplate, assetActionTemplate: assetActionTemplate,
        maintenanceActionTemplate: maintenanceActionTemplate, inspectionActionTemplate: inspectionActionTemplate, categoryActionTemplate: categoryActionTemplate
    };
})();
