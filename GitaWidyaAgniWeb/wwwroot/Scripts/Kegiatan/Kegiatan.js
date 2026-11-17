// Kegiatan › Daftar Kegiatan. URL dari window.KegiatanUrl (Views/Kegiatan/Kegiatan/Index.cshtml); helper global di js/site.js (AppUI).
// Tab Kegiatan (#targetKegiatan) dan Laporan (#targetLaporan).
window.Kegiatan = (function () {
    const URL = window.KegiatanUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const NAMA = { Kegiatan: 'Kegiatan', Laporan: 'Laporan Kegiatan' };
    const STATUS = { PLANNED: 1, ONGOING: 2, DONE: 3, CANCELLED: 4 };
    const REG = { REGISTERED: 1, CONFIRMED: 2, CANCELLED: 3 };
    const COLLAB = { DIUNDANG: 1, DITERIMA: 2, DITOLAK: 3 };
    const REPORT = { DRAFT: 1, SUBMITTED: 2, REVISION: 3, APPROVED: 4 };
    const loaded = {};

    $(function () {
        refresh('Kegiatan');
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
        AppUI.openModal(tab === 'Kegiatan' ? MODAL_LARGE : MODAL, 'Tambah ' + NAMA[tab], URL['add' + tab], null, tab === 'Kegiatan' ? null : 'lg');
    }

    function edit(tab, row) {
        AppUI.openModal(tab === 'Kegiatan' ? MODAL_LARGE : MODAL, 'Ubah ' + NAMA[tab], URL['edit' + tab], { encKey: row.EncDecrKey }, tab === 'Kegiatan' ? null : 'lg');
    }

    function save(tab, isEdit) {
        AppUI.submitForm('#frm' + tab, URL[(isEdit ? 'update' : 'simpan') + tab], null, tab === 'Kegiatan' ? MODAL_LARGE : MODAL)
            .done(function () { refresh(tab); });
    }

    function post(tab, title, text, url, data, label, css) {
        AppUI.confirmPost(title, text, url, data, label, css).done(function () { refresh(tab); });
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    /* ===== Peserta ===== */

    function participants(row) {
        AppUI.openModal(MODAL_LARGE, 'Peserta — ' + row.Title, URL.peserta, { encKey: row.EncDecrKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () { refresh('Kegiatan'); });
    }
    function refreshParticipants() {
        AppUI.loadPartial('#kegiatanPesertaTarget', URL.pesertaList, { encKey: $('#kegiatanPeserta').data('key') });
    }
    function addParticipant() {
        const form = $('#frmPeserta').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'), { Key: $('#kegiatanPeserta').data('key') });
        data.ParticipantRole = $.trim(data.ParticipantRole || '').toUpperCase();
        AppUI.postJson(URL.tambahPeserta, data).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.getEditor('MemberId').reset();
            refreshParticipants();
        });
    }
    function participantActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === REG.REGISTERED) {
            btn(container, 'btn-outline-success', 'ri-check-line', 'Konfirmasi', function () {
                AppUI.confirmPost('Konfirmasi peserta?', row.Nama, URL.konfirmasiPeserta, { Key: row.EncDecrKey }, 'Konfirmasi', 'btn-success').done(refreshParticipants);
            });
        }
        if (row.Status !== REG.CANCELLED) {
            btn(container, 'btn-outline-danger', 'ri-close-line', 'Batalkan pendaftaran', function () {
                AppUI.confirmPost('Batalkan pendaftaran?', row.Nama, URL.batalPeserta, { Key: row.EncDecrKey }, 'Batalkan', 'btn-danger').done(refreshParticipants);
            });
        }
    }

    /* ===== Kolaborator ===== */

    function collaborators(row) {
        AppUI.openModal(MODAL_LARGE, 'Kolaborator — ' + row.Title, URL.kolaborator, { encKey: row.EncDecrKey });
    }
    function refreshCollaborators() {
        AppUI.loadPartial('#kegiatanKolaboratorTarget', URL.kolaboratorList, { encKey: $('#kegiatanKolaborator').data('key') });
    }
    function addCollaborator() {
        const form = $('#frmKolaborator').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahKolaborator, $.extend({}, form.option('formData'), { Key: $('#kegiatanKolaborator').data('key') })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.getEditor('ExternalPartyId').reset();
            refreshCollaborators();
        });
    }
    function respond(row, status, label, css) {
        AppUI.inputPost({ title: label + ' — ' + row.Nama, input: 'textarea', label: 'Catatan', value: row.Notes, confirmText: label, confirmCss: css },
            URL.responKolaborator, { Key: row.EncDecrKey, Status: status }, 'Keterangan').done(refreshCollaborators);
    }
    function collabActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === COLLAB.DIUNDANG) {
            btn(container, 'btn-outline-success', 'ri-check-line', 'Mitra menerima', function () { respond(row, COLLAB.DITERIMA, 'Diterima', 'btn-success'); });
            btn(container, 'btn-outline-secondary', 'ri-close-line', 'Mitra menolak', function () { respond(row, COLLAB.DITOLAK, 'Ditolak', 'btn-secondary'); });
        }
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
            AppUI.confirmPost('Hapus kolaborator?', row.Nama, URL.hapusKolaborator, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger').done(refreshCollaborators);
        });
    }

    /* ===== Laporan ===== */

    function saveReport(isEdit) {
        const form = $('#frmLaporan').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.berkas = AppUI.dxFile('fileLaporan');
        if (isEdit && $('#chkHapusBerkasLaporan').is(':checked')) data.hapusBerkas = true;
        AppUI.postForm(isEdit ? URL.updateLaporan : URL.simpanLaporan, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Laporan');
        });
    }
    function reportActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        if (row.FileName) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Unduh berkas', function () { window.open(URL.berkasLaporan + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        if (row.Status === REPORT.DRAFT || row.Status === REPORT.REVISION) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah laporan', function () { edit('Laporan', row); });
            btn(container, 'btn-outline-primary', 'ri-send-plane-line', 'Ajukan', function () { post('Laporan', 'Ajukan laporan?', row.Title, URL.ajukanLaporan, key, 'Ajukan', 'btn-primary'); });
        }
        if (row.Status === REPORT.SUBMITTED) {
            btn(container, 'btn-outline-success', 'ri-check-double-line', 'Setujui', function () { post('Laporan', 'Setujui laporan?', row.Title, URL.setujuiLaporan, key, 'Setujui', 'btn-success'); });
            btn(container, 'btn-outline-secondary', 'ri-arrow-go-back-line', 'Minta revisi', function () { post('Laporan', 'Minta revisi?', row.Title + ' dikembalikan ke penyusun.', URL.revisiLaporan, key, 'Minta Revisi', 'btn-secondary'); });
        }
        if (row.Status === REPORT.DRAFT) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Laporan', 'Hapus laporan?', row.Title, URL.hapusLaporan, key, 'Hapus', 'btn-danger'); });
        }
    }

    /* ===== Grid kegiatan ===== */

    function titleTemplate(container, options) {
        const d = options.data;
        const main = $('<div class="gwa-cell-main">').text(d.Title).appendTo(container);
        if (d.Disorot) main.prepend($('<i class="ri-star-fill text-warning me-1" title="Disorot di landing page" aria-label="Disorot">'));
        const sub = [d.Induk ? 'Bagian dari ' + d.Induk : null, d.Program].filter(Boolean).join(' · ');
        if (sub) $('<div class="gwa-cell-sub">').text(sub).appendTo(container);
    }

    const statusTemplate = AppUI.badgeTemplate({ Direncanakan: 'secondary', Berjalan: 'primary', Selesai: 'success', Dibatalkan: 'dark' });
    const participantStatusTemplate = AppUI.badgeTemplate({ Terdaftar: 'info', Terkonfirmasi: 'success', Dibatalkan: 'danger' });
    const collabStatusTemplate = AppUI.badgeTemplate({ Diundang: 'warning', Diterima: 'success', Ditolak: 'danger' });
    const reportStatusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Diajukan: 'warning', Revisi: 'danger', Disetujui: 'success' });

    function actionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        const open = row.Status === STATUS.PLANNED || row.Status === STATUS.ONGOING;
        btn(container, 'btn-outline-success', 'ri-group-line', 'Peserta', function () { participants(row); });
        btn(container, 'btn-outline-info', 'ri-handshake-line', 'Kolaborator / mitra', function () { collaborators(row); });
        if (row.Status === STATUS.PLANNED) {
            btn(container, 'btn-outline-primary', 'ri-play-circle-line', 'Mulai kegiatan', function () {
                post('Kegiatan', 'Mulai kegiatan?', row.Title + ' menjadi Berjalan.', URL.statusKegiatan, { Key: row.EncDecrKey, Status: STATUS.ONGOING }, 'Mulai', 'btn-primary');
            });
        } else if (row.Status === STATUS.ONGOING) {
            btn(container, 'btn-outline-primary', 'ri-checkbox-circle-line', 'Selesaikan kegiatan', function () {
                post('Kegiatan', 'Selesaikan kegiatan?', 'Data turunan ' + row.Title + ' (panitia, rundown, presensi) terkunci setelah Selesai.', URL.statusKegiatan, { Key: row.EncDecrKey, Status: STATUS.DONE }, 'Selesai', 'btn-success');
            });
        }
        if (open) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah kegiatan', function () { edit('Kegiatan', row); });
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Batalkan', function () {
                post('Kegiatan', 'Batalkan kegiatan?', row.Title + ' menjadi Dibatalkan.', URL.statusKegiatan, { Key: row.EncDecrKey, Status: STATUS.CANCELLED }, 'Batalkan', 'btn-secondary');
            });
        }
        if (row.Status === STATUS.PLANNED) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus kegiatan', function () { post('Kegiatan', 'Hapus kegiatan?', row.Title + ' dihapus permanen.', URL.hapusKegiatan, key, 'Hapus', 'btn-danger'); });
        }
    }

    return {
        add: add, save: save, saveReport: saveReport, addParticipant: addParticipant, addCollaborator: addCollaborator,
        titleTemplate: titleTemplate, statusTemplate: statusTemplate, participantStatusTemplate: participantStatusTemplate,
        collabStatusTemplate: collabStatusTemplate, reportStatusTemplate: reportStatusTemplate,
        actionTemplate: actionTemplate, participantActionTemplate: participantActionTemplate,
        collabActionTemplate: collabActionTemplate, reportActionTemplate: reportActionTemplate
    };
})();
