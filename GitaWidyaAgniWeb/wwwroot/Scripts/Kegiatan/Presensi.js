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
        if (tab === 'Statistik') return refreshStats();
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }

    /* ===== Statistik keaktifan ===== */

    const STAT_COLOR = { Hadir: '#1f7a45', Terlambat: '#e8b52a', Izin: '#2f5f9e', Sakit: '#7aa0d4', Alpa: '#c8202a', Line: '#9a6b00' };

    $(function () {
        $('#frmStatistik select').on('change', refreshStats);
        $('#targetStatistik').on('click', '[data-anggota]', function () {
            $('#statAnggota').val(String($(this).data('anggota')));
            refreshStats();
        });
    });

    function refreshStats() {
        loaded.Statistik = true;
        const q = { bulan: $('#statBulan').val(), section: $('#statSection').val(), anggota: $('#statAnggota').val() };
        return AppUI.loadPartial('#targetStatistik', URL.statistik, q).done(drawStats);
    }

    function drawStats() {
        const raw = document.getElementById('statistikData');
        if (!raw) return;
        const data = JSON.parse(raw.textContent);
        const pct = function (v) { return (v === null || v === undefined) ? '–' : Number(v).toLocaleString('id-ID', { maximumFractionDigits: 1 }) + '%'; };
        if ($('#chartStatBulan').length) {
            $('#chartStatBulan').dxChart({
                dataSource: data.Months,
                commonSeriesSettings: { argumentField: 'Bulan', type: 'stackedBar', barPadding: 0.35 },
                series: ['Hadir', 'Terlambat', 'Izin', 'Sakit', 'Alpa'].map(function (k) {
                    return { valueField: k, name: k, color: STAT_COLOR[k] };
                }).concat([{
                    type: 'spline', valueField: 'Persen', name: '% Hadir', axis: 'persen', color: STAT_COLOR.Line, width: 3,
                    point: { visible: true, size: 8 }, label: { visible: true, customizeText: function (e) { return pct(e.value); }, backgroundColor: STAT_COLOR.Line }
                }]),
                valueAxis: [
                    { name: 'jumlah', allowDecimals: false, title: { text: 'Catatan' }, grid: { color: '#ece4d3' } },
                    {
                        name: 'persen', position: 'right', min: 0, max: 100, showZero: true, title: { text: '% Hadir' },
                        label: { customizeText: function (e) { return e.value + '%'; } },
                        constantLines: [{ value: data.Batas, color: STAT_COLOR.Alpa, dashStyle: 'dash', width: 1.5, label: { text: 'Batas ' + data.Batas + '%', font: { color: STAT_COLOR.Alpa } } }]
                    }
                ],
                argumentAxis: { grid: { visible: false } },
                legend: { verticalAlignment: 'top', horizontalAlignment: 'center', itemTextPosition: 'right' },
                tooltip: {
                    enabled: true, shared: true,
                    customizeTooltip: function (e) {
                        const d = e.point.data;
                        return { text: e.argumentText + '\nHadir ' + d.Hadir + ' · Telat ' + d.Terlambat + '\nIzin ' + d.Izin + ' · Sakit ' + d.Sakit + ' · Alpa ' + d.Alpa + '\nKehadiran ' + pct(d.Persen) };
                    }
                }
            });
        }
        if ($('#chartStatSection').length) {
            $('#chartStatSection').dxChart({
                dataSource: data.Sections,
                rotated: true,
                series: [{ argumentField: 'Name', valueField: 'Persen', type: 'bar', name: '% Hadir', barPadding: 0.3, label: { visible: true, customizeText: function (e) { return pct(e.value); } } }],
                customizePoint: function () { return { color: this.value < data.Batas ? STAT_COLOR.Alpa : STAT_COLOR.Hadir }; },
                valueAxis: {
                    min: 0, max: 100, label: { customizeText: function (e) { return e.value + '%'; } }, grid: { color: '#ece4d3' },
                    constantLines: [{ value: data.Batas, color: STAT_COLOR.Alpa, dashStyle: 'dash', width: 1.5 }]
                },
                argumentAxis: { inverted: true },
                legend: { visible: false },
                tooltip: {
                    enabled: true,
                    customizeTooltip: function (e) { const d = e.point.data; return { text: d.Name + ': ' + pct(d.Persen) + '\n' + d.Members + ' anggota · ' + d.Sessions + ' catatan' }; }
                }
            });
        }
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
        AppUI.promptForm({
            title: title, confirmText: 'Cetak', fields: [
                { type: 'daterange', names: ['dari', 'sampai'], label: 'Rentang tanggal', value: [AppUI.toIsoDate(first), AppUI.toIsoDate(today)], required: true },
                { name: 'kegiatan', label: 'Kegiatan', type: 'select', options: [['', 'Semua kegiatan']].concat(Object.entries(opsi.kegiatan)), value: '' },
                { name: 'section', label: 'Section', type: 'select', options: [['', 'Semua section']].concat(Object.entries(opsi.section)), value: '' }
            ]
        }).then(function (v) {
            if (!v) return;
            AppUI.openReport(title, URL.cetakRekap + '?' + $.param(v));
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
        const today = new Date();
        const end = new Date();
        end.setDate(end.getDate() + 30);
        AppUI.promptForm({
            title: 'Buat sesi dari jadwal', confirmText: 'Buat Sesi', fields: [
                { type: 'daterange', names: ['From', 'To'], label: 'Rentang tanggal', value: [AppUI.toIsoDate(today), AppUI.toIsoDate(end)], required: true }
            ]
        }).then(function (v) {
            if (!v) return;
            AppUI.postJson(URL.generate, v).done(function (resp) {
                AppUI.alertSuccess((resp.Data || 0) + ' sesi dibuat.');
                loaded.Sesi = false;
                refresh('Sesi');
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
        add: add, save: save, saveMinute: saveMinute, refreshMonitor: refreshMonitor, recapReport: recapReport, refreshStats: refreshStats, rotateQr: rotateQr, fullscreenQr: fullscreenQr, manual: manual, generate: generate,
        sessionTemplate: sessionTemplate, sessionActionTemplate: sessionActionTemplate, scheduleActionTemplate: scheduleActionTemplate,
        recordActionTemplate: recordActionTemplate, minuteActionTemplate: minuteActionTemplate, modeTemplate: modeTemplate,
        sessionStatusTemplate: sessionStatusTemplate, recordStatusTemplate: recordStatusTemplate, minuteStatusTemplate: minuteStatusTemplate
    };
})();
