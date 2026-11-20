// Prestasi › Lomba & Prestasi. URL dari window.PrestasiUrl (Views/Prestasi/Prestasi/Index.cshtml); helper global di js/site.js (AppUI).
window.Prestasi = (function () {
    const URL = window.PrestasiUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const NAMA = { Lomba: 'Lomba', Prestasi: 'Prestasi', Materi: 'Materi', Poin: 'Poin Manual', Aturan: 'Aturan Poin' };
    const LOMBA = { UPCOMING: 1, REGISTRATION: 2, ONGOING: 3, FINISHED: 4, CANCELLED: 5 };
    const LOMBA_TEXT = { 1: 'Akan Datang', 2: 'Pendaftaran', 3: 'Berlangsung', 4: 'Selesai', 5: 'Dibatalkan' };
    const loaded = {};
    let pesertaKey = null;

    $(function () {
        refresh('Lomba');
        $('[data-tab]').on('shown.bs.tab', function () {
            const tab = $(this).data('tab');
            if (!loaded[tab]) refresh(tab);
        });
    });

    function refresh(tab) {
        loaded[tab] = true;
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }
    function done(tab) {
        return function () { refresh(tab); };
    }

    function add(tab) {
        AppUI.openModal(MODAL, 'Tambah ' + NAMA[tab], URL['add' + tab], null, tab === 'Poin' || tab === 'Aturan' ? undefined : 'lg');
    }
    function edit(tab, row) {
        AppUI.openModal(MODAL, 'Ubah ' + NAMA[tab], URL['edit' + tab], { encKey: row.EncDecrKey }, tab === 'Aturan' ? undefined : 'lg');
    }

    // Form dengan berkas → FormData.
    function saveWithFiles(formId, tab, isEdit, files, flags) {
        const form = $(formId).dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        Object.keys(files || {}).forEach(function (k) { data[k] = AppUI.dxFile(files[k]); });
        Object.keys(flags || {}).forEach(function (k) { if (isEdit && $(flags[k]).is(':checked')) data[k] = true; });
        AppUI.postForm(URL[(isEdit ? 'update' : 'simpan') + tab], AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh(tab);
        });
    }

    function saveCompetition(isEdit) {
        saveWithFiles('#frmLomba', 'Lomba', isEdit, { poster: 'filePoster' }, { hapusPoster: '#chkHapusPoster' });
    }
    function saveAchievement(isEdit) {
        saveWithFiles('#frmPrestasi', 'Prestasi', isEdit, { sertifikat: 'fileSertifikatPrestasi', foto: 'fileFotoPrestasi' },
            { hapusSertifikat: '#chkHapusSertifikat', hapusFoto: '#chkHapusFotoPrestasi' });
    }
    function saveMaterial(isEdit) {
        saveWithFiles('#frmMateri', 'Materi', isEdit, { berkas: 'fileMateri' });
    }
    function savePoint() {
        AppUI.submitForm('#frmPoin', URL.simpanPoin, null, MODAL).done(done('Poin'));
    }
    function saveRule(isEdit) {
        AppUI.submitForm('#frmAturan', isEdit ? URL.updateAturan : URL.simpanAturan, null, MODAL).done(done('Aturan'));
    }

    /* ===== Lomba ===== */

    function changeStatus(row) {
        const options = {};
        if (row.Status !== LOMBA.FINISHED && row.Status !== LOMBA.CANCELLED) {
            for (let s = row.Status + 1; s <= LOMBA.FINISHED; s++) options[s] = LOMBA_TEXT[s];
            options[LOMBA.CANCELLED] = LOMBA_TEXT[LOMBA.CANCELLED];
        }
        AppUI.promptInput({ title: 'Ubah status lomba', text: row.Name + ' · sekarang ' + row.StatusText, input: 'select', options: options, required: true, label: 'Status baru', confirmText: 'Ubah' })
            .then(function (value) {
                if (!value) return;
                AppUI.postJson(URL.statusLomba, { Key: row.EncDecrKey, Status: parseInt(value, 10) }).done(function (resp) {
                    AppUI.alertSuccess(resp.Message);
                    refresh('Lomba');
                });
            });
    }

    function participants(row) {
        pesertaKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Peserta ' + row.Name, URL.peserta, { encKey: pesertaKey });
    }
    function reloadParticipants() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.peserta, { encKey: pesertaKey });
    }
    function registerParticipant() {
        const memberId = AppUI.dxValue('pesertaAnggota');
        if (!memberId) {
            AppUI.alertWarning('Pilih anggota.');
            return;
        }
        AppUI.postJson(URL.daftarPeserta, { Key: pesertaKey, MemberId: memberId, TeamName: $.trim($('#pesertaTim').val() || '') || null, ParticipantRole: $('#pesertaPeran').val() })
            .done(function (resp) { AppUI.alertSuccess(resp.Message); reloadParticipants(); });
    }
    function participate(key) {
        AppUI.postJson(URL.ikutPeserta, { Key: key }).done(function (resp) { AppUI.alertSuccess(resp.Message); reloadParticipants(); });
    }
    function withdraw(key) {
        AppUI.confirmPost('Peserta mengundurkan diri?', 'Peserta bisa didaftarkan lagi selama pendaftaran dibuka.', URL.mundurPeserta, { Key: key }, 'Mundur', 'btn-danger').done(reloadParticipants);
    }

    /* ===== Poin ===== */

    function generate(title, url, optionsUrl, label) {
        $.getJSON(optionsUrl).done(function (items) {
            const options = {};
            (items || []).forEach(function (x) { options[x.Value] = x.Text; });
            AppUI.promptInput({ title: title, input: 'select', options: options, required: true, label: label, confirmText: 'Generate' }).then(function (value) {
                if (!value) return;
                AppUI.postJson(url, { Key: '', Status: parseInt(value, 10) }).done(function (resp) {
                    AppUI.alertSuccess(resp.Message);
                    refresh('Poin');
                });
            });
        });
    }
    function generateActivity() {
        generate('Generate poin Hadir & Panitia', URL.generateKegiatan, URL.opsiKegiatan, 'Kegiatan');
    }
    function generatePeriod() {
        generate('Generate poin Pengurus', URL.generatePeriode, URL.opsiPeriode, 'Periode');
    }
    function leaderboard(periodId) {
        if (periodId === undefined) {
            AppUI.openModal(MODAL, 'Peringkat Poin Anggota', URL.peringkat, null, 'lg');
        } else {
            AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.peringkat, periodId ? { periodId: periodId } : null);
        }
    }

    // Rekap poin: periode (bawaan aktif), lalu section (kosong = semua); viewer DevExpress (ekspor PDF / Excel dari toolbar viewer).
    function pointReport() {
        $.when($.getJSON(URL.opsiPeriode), $.getJSON(URL.opsiSection)).done(function (p, s) {
            const periods = p[0] || [];
            if (periods.length === 0) { AppUI.alertWarning('Belum ada periode kepengurusan.'); return; }
            const active = periods.find(function (x) { return x.Text.indexOf('(aktif)') >= 0; }) || periods[0];
            const title = 'Rekap Poin Keaktifan';
            AppUI.promptInput({ title: title, text: 'Periode', input: 'select', options: new Map(periods.map(function (x) { return [String(x.Value), x.Text]; })), value: String(active.Value), confirmText: 'Lanjut' }).then(function (periode) {
                if (!periode) return;
                AppUI.promptInput({ title: title, text: 'Section', input: 'select', options: new Map([['', 'Semua section']].concat((s[0] || []).map(function (x) { return [String(x.Value), x.Text]; }))), value: '', confirmText: 'Cetak' }).then(function (section) {
                    if (section === null) return;
                    AppUI.openReport(title, URL.cetakRekapPoin + '?' + $.param({ periode: periode, section: section || '' }));
                });
            });
        });
    }

    /* ===== Grid ===== */

    const competitionStatusTemplate = AppUI.badgeTemplate({ 'Akan Datang': 'secondary', 'Pendaftaran': 'info', 'Berlangsung': 'primary', 'Selesai': 'success', 'Dibatalkan': 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }
    function remove(tab, url, row, title, text, label) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, label || 'Hapus', 'btn-danger').done(done(tab));
    }

    function competitionActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-success', 'ri-team-line', 'Peserta', function () { participants(row); });
        if (row.Status !== LOMBA.FINISHED && row.Status !== LOMBA.CANCELLED) {
            btn(container, 'btn-outline-primary', 'ri-arrow-right-circle-line', 'Ubah status', function () { changeStatus(row); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Lomba', row); });
        if (row.Status === LOMBA.UPCOMING) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                remove('Lomba', URL.hapusLomba, row, 'Hapus lomba?', row.Name + '. Ditolak bila sudah dipakai data lain.');
            });
        }
    }

    function achievementActionTemplate(container, options) {
        const row = options.data;
        if (row.CertificateFileName) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Sertifikat', function () { window.open(URL.sertifikatPrestasi + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Prestasi', row); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
            remove('Prestasi', URL.hapusPrestasi, row, 'Hapus prestasi?', row.Title + '. Poin prestasinya ikut dihapus.');
        });
    }

    function materialActionTemplate(container, options) {
        const row = options.data;
        if (row.FileName) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Unduh berkas', function () { window.open(URL.berkasMateri + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        } else if (row.Url) {
            btn(container, 'btn-outline-info', 'ri-external-link-line', 'Buka tautan', function () { window.open(row.Url, '_blank', 'noopener'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Materi', row); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
            remove('Materi', URL.hapusMateri, row, 'Hapus materi?', row.Title);
        });
    }

    function pointActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus poin', function () {
            remove('Poin', URL.hapusPoin, row, 'Hapus poin?', row.MemberName + ' · ' + row.Points + ' poin (' + row.Sumber + ')');
        });
    }

    function ruleActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Aturan', row); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () {
                remove('Aturan', URL.hapusAturan, row, 'Nonaktifkan aturan?', row.Sumber + ' · ' + row.Points + ' poin', 'Nonaktifkan');
            });
        }
    }

    return {
        add: add, saveCompetition: saveCompetition, saveAchievement: saveAchievement, saveMaterial: saveMaterial, savePoint: savePoint, saveRule: saveRule,
        registerParticipant: registerParticipant, participate: participate, withdraw: withdraw,
        generateActivity: generateActivity, generatePeriod: generatePeriod, leaderboard: leaderboard, pointReport: pointReport,
        competitionStatusTemplate: competitionStatusTemplate, competitionActionTemplate: competitionActionTemplate,
        achievementActionTemplate: achievementActionTemplate, materialActionTemplate: materialActionTemplate,
        pointActionTemplate: pointActionTemplate, ruleActionTemplate: ruleActionTemplate
    };
})();
