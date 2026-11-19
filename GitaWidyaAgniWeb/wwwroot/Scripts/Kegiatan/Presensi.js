// Kegiatan › Presensi. URL dari window.PresensiUrl (Views/Kegiatan/Presensi/Index.cshtml); helper global di js/site.js (AppUI).
// QR digambar dengan qrcodejs (js/qrcode.min.js).
window.Presensi = (function () {
    const URL = window.PresensiUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const NAMA = { Sesi: 'Sesi Presensi', Jadwal: 'Jadwal Rutin', Notulen: 'Notulen' };
    const SESSION = { SCHEDULED: 1, OPEN: 2, CLOSED: 3, CANCELLED: 4 };
    const loaded = {};

    $(function () {
        refresh('Sesi');
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

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    /* ===== Sesi & QR ===== */

    let qrKey = null;

    function drawQr() {
        const $qr = $('#presensiQr');
        const box = document.getElementById('presensiQrCanvas');
        if (!box) return;
        box.innerHTML = '';
        new QRCode(box, { text: String($qr.data('token')), width: 320, height: 320, correctLevel: QRCode.CorrectLevel.M });
    }

    function showQr(row) {
        qrKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'QR Presensi', URL.qr, { encKey: qrKey }, 'lg').done(drawQr);
    }

    function open(row) {
        const text = row.Status === SESSION.OPEN ? 'Token QR lama tidak berlaku lagi.' : row.Name + ' dibuka dan QR ditampilkan.';
        AppUI.confirmPost(row.Status === SESSION.OPEN ? 'Ganti QR?' : 'Buka sesi?', text, URL.bukaSesi, { Key: row.EncDecrKey }, 'Buka', 'btn-primary')
            .done(function () {
                refresh('Sesi');
                showQr(row);
            });
    }

    function rotateQr() {
        AppUI.postJson(URL.bukaSesi, { Key: qrKey }).done(function () {
            AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.qr, { encKey: qrKey }).done(drawQr);
        });
    }

    function fullscreenQr() {
        const el = document.getElementById('presensiQr');
        if (el && el.requestFullscreen) el.requestFullscreen();
    }

    function close(row) {
        AppUI.confirmPost('Tutup sesi?', 'Anggota wajib hadir yang belum presensi dicatat Alpa.', URL.tutupSesi, { Key: row.EncDecrKey }, 'Tutup', 'btn-secondary')
            .done(function (resp) {
                if (resp && resp.Data) AppUI.alertSuccess(resp.Data + ' anggota dicatat Alpa.');
                refresh('Sesi');
            });
    }

    function cancel(row) {
        AppUI.reasonPost('Batalkan sesi ' + row.Name + '?', URL.batalSesi, { Key: row.EncDecrKey }, 'Keterangan', 'Batalkan', 'btn-danger')
            .done(function () { refresh('Sesi'); });
    }

    /* ===== Pantau langsung ===== */

    let monitorTimer = null;
    let monitorFilter = '';

    function monitor(row) {
        monitorFilter = '';
        AppUI.openModal(MODAL_LARGE, 'Pantau Langsung — ' + row.Name, URL.pantau, { encKey: row.EncDecrKey }).done(function () {
            bindMonitorFilter();
            applyMonitorFilter();
        });
        clearInterval(monitorTimer);
        monitorTimer = setInterval(refreshMonitor, 15000);
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () {
            clearInterval(monitorTimer);
            monitorTimer = null;
            refresh('Sesi');
        });
    }
    function refreshMonitor() {
        const key = $('#presensiPantau').data('key');
        if (!key) return;
        $.get(URL.pantauList, { encKey: key }).done(function (html) {
            if (typeof html !== 'string') return;
            $('#presensiPantauTarget').html(html);
            $('#pantauWaktu').text($('#pantauRingkas').data('waktu') || '');
            applyMonitorFilter();
        });
    }
    function bindMonitorFilter() {
        $('#presensiPantau [data-saring]').on('click', function () {
            monitorFilter = $(this).data('saring') || '';
            $('#presensiPantau [data-saring]').removeClass('active');
            $(this).addClass('active');
            applyMonitorFilter();
        });
    }
    function applyMonitorFilter() {
        $('#presensiPantauTarget tbody tr[data-kelompok]').each(function () {
            $(this).toggle(!monitorFilter || $(this).data('kelompok') === monitorFilter);
        });
    }

    /* ===== Cetak rekap kehadiran ===== */

    function recapReport() {
        const today = new Date();
        const first = new Date(today.getFullYear(), today.getMonth(), 1);
        const opsi = window.PresensiOpsi || { kegiatan: {}, section: {} };
        const title = 'Rekap Kehadiran';
        AppUI.promptInput({ title: title, text: 'Dari tanggal', input: 'date', value: AppUI.toIsoDate(first), required: true, label: 'Dari', confirmText: 'Lanjut' }).then(function (dari) {
            if (dari === null) return;
            AppUI.promptInput({ title: title, text: 'Sampai tanggal', input: 'date', value: AppUI.toIsoDate(today), required: true, label: 'Sampai', confirmText: 'Lanjut' }).then(function (sampai) {
                if (sampai === null) return;
                AppUI.promptInput({ title: title, text: 'Kegiatan', input: 'select', options: new Map([['', 'Semua kegiatan']].concat(Object.entries(opsi.kegiatan))), value: '', confirmText: 'Lanjut' }).then(function (kegiatan) {
                    if (kegiatan === null) return;
                    AppUI.promptInput({ title: title, text: 'Section', input: 'select', options: new Map([['', 'Semua section']].concat(Object.entries(opsi.section))), value: '', confirmText: 'Cetak' }).then(function (section) {
                        if (section === null) return;
                        AppUI.openReport(title, URL.cetakRekap + '?' + $.param({ dari: dari, sampai: sampai, kegiatan: kegiatan || '', section: section || '' }));
                    });
                });
            });
        });
    }

    /* ===== Rekap ===== */

    function recap(row) {
        AppUI.openModal(MODAL_LARGE, 'Rekap Presensi — ' + row.Name, URL.rekap, { encKey: row.EncDecrKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () { refresh('Sesi'); });
    }

    function refreshRecap() {
        AppUI.loadPartial('#presensiRekapTarget', URL.rekapList, { encKey: $('#presensiRekap').data('key') });
    }

    function manual() {
        const form = $('#frmManual').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.manual, $.extend({}, form.option('formData'), { Key: $('#presensiRekap').data('key') })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.getEditor('MemberId').reset();
            form.getEditor('Notes').reset();
            refreshRecap();
        });
    }

    function recordActionTemplate(container, options) {
        const row = options.data;
        if (row.CheckInAt) {
            btn(container, 'btn-outline-info', 'ri-camera-line', 'Foto masuk', function () { window.open(URL.foto + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        if (row.CheckOutAt) {
            btn(container, 'btn-outline-info', 'ri-camera-switch-line', 'Foto keluar', function () { window.open(URL.foto + '?keluar=true&key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah status', function () {
            const form = $('#frmManual').dxForm('instance');
            if (!form) return;
            form.updateData('MemberId', row.MemberId);
            form.updateData('Notes', row.Notes);
            form.getEditor('Status').focus();
        });
    }

    function generate() {
        const today = AppUI.toIsoDate(new Date());
        AppUI.promptInput({ title: 'Buat sesi dari jadwal', text: 'Tanggal awal', input: 'date', value: today, required: true, label: 'Dari', confirmText: 'Lanjut' })
            .then(function (from) {
                if (from === null) return;
                const end = new Date(from);
                end.setDate(end.getDate() + 30);
                AppUI.promptInput({ title: 'Buat sesi dari jadwal', text: 'Tanggal akhir', input: 'date', value: AppUI.toIsoDate(end), required: true, label: 'Sampai', confirmText: 'Buat Sesi' })
                    .then(function (to) {
                        if (to === null) return;
                        AppUI.postJson(URL.generate, { From: from, To: to }).done(function (resp) {
                            AppUI.alertSuccess((resp.Data || 0) + ' sesi dibuat.');
                            loaded.Sesi = false;
                            refresh('Sesi');
                        });
                    });
            });
    }

    /* ===== Notulen ===== */

    function saveMinute(isEdit) {
        const form = $('#frmNotulen').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.berkas = AppUI.dxFile('fileNotulen');
        if (isEdit && $('#chkHapusBerkasNotulen').is(':checked')) data.hapusBerkas = true;
        AppUI.postForm(isEdit ? URL.updateNotulen : URL.simpanNotulen, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Notulen');
        });
    }

    function minuteActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        if (row.FileName) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Unduh lampiran', function () { window.open(URL.berkasNotulen + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        if (row.Status === 1) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah notulen', function () { edit('Notulen', row); });
            btn(container, 'btn-outline-success', 'ri-check-double-line', 'Finalkan', function () {
                AppUI.confirmPost('Finalkan notulen?', 'Notulen Final tidak bisa diubah lagi.', URL.finalNotulen, key, 'Finalkan', 'btn-success').done(function () { refresh('Notulen'); });
            });
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus notulen?', row.Sesi, URL.hapusNotulen, key, 'Hapus', 'btn-danger').done(function () { refresh('Notulen'); });
            });
        }
    }

    /* ===== Template grid ===== */

    function sessionTemplate(container, options) {
        const d = options.data;
        $('<div class="gwa-cell-main">').text(d.Name).appendTo(container);
        const sub = [d.DariJadwal ? 'dari jadwal rutin' : null, d.Status === SESSION.SCHEDULED ? 'dibuka otomatis 30 menit sebelum mulai' : null, d.AdaNotulen ? 'ada notulen' : null].filter(Boolean).join(' · ');
        if (sub) $('<div class="gwa-cell-sub">').text(sub).appendTo(container);
    }

    function sessionActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === SESSION.SCHEDULED) {
            btn(container, 'btn-outline-primary', 'ri-qr-code-line', 'Buka sesi', function () { open(row); });
        } else if (row.Status === SESSION.OPEN) {
            btn(container, 'btn-outline-primary', 'ri-qr-code-line', 'Tampilkan QR', function () { showQr(row); });
        }
        if (row.Status !== SESSION.CANCELLED) {
            btn(container, 'btn-outline-info', 'ri-radar-line', 'Pantau langsung', function () { monitor(row); });
        }
        if (row.Status !== SESSION.SCHEDULED) {
            btn(container, 'btn-outline-success', 'ri-list-check-3', 'Rekap & presensi manual', function () { recap(row); });
        }
        if (row.Status === SESSION.OPEN) {
            btn(container, 'btn-outline-secondary', 'ri-lock-line', 'Tutup sesi', function () { close(row); });
        }
        if (row.Status === SESSION.SCHEDULED || row.Status === SESSION.OPEN) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah sesi', function () { edit('Sesi', row); });
            btn(container, 'btn-outline-danger', 'ri-close-circle-line', 'Batalkan sesi', function () { cancel(row); });
        }
    }

    function scheduleActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah jadwal', function () { edit('Jadwal', row); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan jadwal', function () {
                AppUI.confirmPost('Nonaktifkan jadwal?', row.Kegiatan + ' ' + row.HariText + ' ' + row.Jam, URL.hapusJadwal, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
                    .done(function () { refresh('Jadwal'); });
            });
        }
    }

    const modeTemplate = AppUI.badgeTemplate({ 'Tanpa QR': 'success', 'Wajib Scan QR': 'primary' });
    const sessionStatusTemplate =AppUI.badgeTemplate({ Terjadwal: 'secondary', Dibuka: 'success', Ditutup: 'dark', Dibatalkan: 'danger' });
    const recordStatusTemplate = AppUI.badgeTemplate({ Hadir: 'success', Terlambat: 'warning', Izin: 'info', Sakit: 'primary', Alpa: 'danger' });
    const minuteStatusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Final: 'success' });

    return {
        add: add, save: save, saveMinute: saveMinute, refreshMonitor: refreshMonitor, recapReport: recapReport, rotateQr: rotateQr, fullscreenQr: fullscreenQr, manual: manual, generate: generate,
        sessionTemplate: sessionTemplate, sessionActionTemplate: sessionActionTemplate, scheduleActionTemplate: scheduleActionTemplate,
        recordActionTemplate: recordActionTemplate, minuteActionTemplate: minuteActionTemplate, modeTemplate: modeTemplate,
        sessionStatusTemplate: sessionStatusTemplate, recordStatusTemplate: recordStatusTemplate, minuteStatusTemplate: minuteStatusTemplate
    };
})();
