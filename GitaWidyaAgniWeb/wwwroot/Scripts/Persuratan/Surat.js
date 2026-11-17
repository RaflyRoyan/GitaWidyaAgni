// Persuratan › Surat. URL dari window.SuratUrl (Views/Persuratan/Surat/Index.cshtml); helper global di js/site.js (AppUI).
window.Surat = (function () {
    const URL = window.SuratUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const ARAH = { MASUK: 1, KELUAR: 2 };
    const STATUS = { DRAFT: 1, WAITING: 2, APPROVED: 3, REJECTED: 4, SENT: 5, RECEIVED: 6, ARCHIVED: 7 };
    const TAB_ARAH = { Keluar: ARAH.KELUAR, Masuk: ARAH.MASUK };
    const loaded = {};
    let currentArah = ARAH.KELUAR;
    let detailKey = null;

    $(function () {
        refresh('Keluar');
        $('[data-tab]').on('shown.bs.tab', function () {
            const tab = $(this).data('tab');
            if (!loaded[tab]) refresh(tab);
        });
    });

    function refresh(tab) {
        loaded[tab] = true;
        if (tab === 'Template') return AppUI.loadPartial('#targetTemplate', URL.listTemplate);
        return AppUI.loadPartial('#target' + tab, URL.listSurat, { arah: TAB_ARAH[tab] });
    }
    function refreshArah(arah) {
        refresh(arah === ARAH.MASUK ? 'Masuk' : 'Keluar');
    }

    /* ===== Surat ===== */

    function add(arah) {
        currentArah = arah;
        AppUI.openModal(MODAL, arah === ARAH.MASUK ? 'Catat Surat Masuk' : 'Buat Surat Keluar', URL.addSurat, { arah: arah }, 'lg');
    }
    function edit(row, arah) {
        currentArah = arah;
        AppUI.openModal(MODAL, 'Ubah Surat', URL.editSurat, { encKey: row.EncDecrKey }, 'lg');
    }
    function save(isEdit) {
        const form = $('#frmSurat').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        if (data.LetterType) data.LetterType = String(data.LetterType).toUpperCase();
        data.berkas = AppUI.dxFile('fileSurat');
        if (isEdit && $('#chkHapusBerkasSurat').is(':checked')) data.hapusBerkas = true;
        AppUI.postForm(isEdit ? URL.updateSurat : URL.simpanSurat, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refreshArah(currentArah);
        });
    }

    /* ===== Detail & disposisi ===== */

    function detail(row, arah) {
        detailKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Surat ' + (row.LetterNumber || row.Subject), URL.detail, { encKey: detailKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () { refreshArah(arah); });
    }
    function reloadDetail() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.detail, { encKey: detailKey });
    }
    function addDisposition() {
        const memberId = AppUI.dxValue('disposisiTujuan');
        const instruction = $.trim($('#disposisiInstruksi').val() || '');
        if (!memberId || !instruction) {
            AppUI.alertWarning('Pilih anggota dan isi instruksi.');
            return;
        }
        AppUI.postJson(URL.tambahDisposisi, { Key: detailKey, ToMemberId: memberId, Instruction: instruction, DueDate: $('#disposisiTenggat').val() || null }).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function doneDisposition(key) {
        AppUI.postJson(URL.selesaiDisposisi, { Key: key }).done(function (resp) { AppUI.alertSuccess(resp.Message); reloadDetail(); });
    }
    function reopenDisposition(key) {
        AppUI.postJson(URL.bukaDisposisi, { Key: key }).done(function (resp) { AppUI.alertSuccess(resp.Message); reloadDetail(); });
    }
    function removeDisposition(key) {
        AppUI.confirmPost('Hapus disposisi?', 'Disposisi yang masih terbuka dihapus.', URL.hapusDisposisi, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }

    /* ===== Template ===== */

    function addTemplate() {
        AppUI.openModal(MODAL, 'Tambah Template Surat', URL.addTemplate, null, 'lg');
    }
    function saveTemplate(isEdit) {
        const form = $('#frmTemplateSurat').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        if (data.LetterType) data.LetterType = String(data.LetterType).toUpperCase();
        data.berkas = AppUI.dxFile('fileTemplateSurat');
        if (isEdit && $('#chkHapusBerkasTemplate').is(':checked')) data.hapusBerkas = true;
        AppUI.postForm(isEdit ? URL.updateTemplate : URL.simpanTemplate, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Template');
        });
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({
        'Draft': 'secondary', 'Menunggu Persetujuan': 'warning', 'Disetujui': 'primary', 'Ditolak': 'danger',
        'Terkirim': 'success', 'Diterima': 'info', 'Diarsipkan': 'dark'
    });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function letterActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        const masuk = row.Masuk;
        const arah = masuk ? ARAH.MASUK : ARAH.KELUAR;
        const done = function () { refreshArah(arah); };

        btn(container, 'btn-outline-info', 'ri-file-list-3-line', 'Detail, persetujuan & disposisi', function () { detail(row, arah); });
        if (!masuk) {
            btn(container, 'btn-outline-dark', 'ri-printer-line', 'Cetak surat', function () { report('Surat — ' + row.Subject, URL.cetak, row); });
        }
        if (masuk || row.Status === STATUS.APPROVED || row.Status === STATUS.SENT || row.Status === STATUS.ARCHIVED) {
            btn(container, 'btn-outline-secondary', 'ri-file-paper-2-line', 'Lembar disposisi', function () { report('Lembar Disposisi — ' + row.Subject, URL.lembarDisposisi, row); });
        }
        if (row.FileName) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Unduh berkas', function () { window.open(URL.berkasSurat + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        switch (row.Status) {
            case STATUS.DRAFT:
            case STATUS.REJECTED:
                btn(container, 'btn-outline-primary', 'ri-send-plane-line', 'Ajukan persetujuan', function () {
                    AppUI.confirmPost('Ajukan surat?', row.Subject, URL.ajukan, key, 'Ajukan', 'btn-primary').done(done);
                });
                btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit(row, arah); });
                btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                    AppUI.confirmPost('Hapus surat?', row.Subject, URL.hapusSurat, key, 'Hapus', 'btn-danger').done(done);
                });
                break;
            case STATUS.WAITING:
                btn(container, 'btn-outline-success', 'ri-check-double-line', 'Setujui', function () {
                    AppUI.inputPost({ title: 'Setujui surat?', text: row.Subject, input: 'textarea', label: 'Catatan (opsional)', confirmText: 'Setujui' }, URL.setujui, key, 'Keterangan').done(done);
                });
                btn(container, 'btn-outline-danger', 'ri-close-circle-line', 'Tolak', function () {
                    AppUI.reasonPost('Tolak surat ' + row.Subject + '?', URL.tolak, key, 'Keterangan', 'Tolak', 'btn-danger').done(done);
                });
                break;
            case STATUS.APPROVED:
                btn(container, 'btn-outline-primary', 'ri-mail-check-line', 'Tandai terkirim', function () {
                    AppUI.confirmPost('Tandai terkirim?', row.LetterNumber + ' · ' + row.Subject, URL.kirim, key, 'Terkirim', 'btn-primary').done(done);
                });
                btn(container, 'btn-outline-warning', 'ri-upload-2-line', 'Unggah berkas final', function () { edit(row, arah); });
                break;
            case STATUS.SENT:
                btn(container, 'btn-outline-secondary', 'ri-archive-line', 'Arsipkan', function () {
                    AppUI.confirmPost('Arsipkan surat?', row.Subject, URL.arsip, key, 'Arsipkan', 'btn-secondary').done(done);
                });
                break;
            case STATUS.RECEIVED:
                btn(container, 'btn-outline-secondary', 'ri-archive-line', 'Arsipkan', function () {
                    AppUI.confirmPost('Arsipkan surat?', row.Subject, URL.arsip, key, 'Arsipkan', 'btn-secondary').done(done);
                });
                btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit(row, arah); });
                btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                    AppUI.confirmPost('Hapus surat masuk?', row.Subject, URL.hapusSurat, key, 'Hapus', 'btn-danger').done(done);
                });
                break;
        }
    }

    /* ===== Cetak (DevExpress Reports di modal layar penuh) ===== */

    function report(title, url, row) {
        AppUI.openReport(title, url + '?key=' + encodeURIComponent(row.EncDecrKey));
    }
    // Agenda surat (1 = masuk, 2 = keluar) untuk rentang tanggal; tanggal kosong = tanpa batas.
    function agenda(arah) {
        const label = arah === ARAH.MASUK ? 'Agenda Surat Masuk' : 'Agenda Surat Keluar';
        const today = new Date();
        const first = new Date(today.getFullYear(), 0, 1);
        AppUI.promptInput({ title: label, text: 'Dari tanggal (kosongkan = semua)', input: 'date', value: AppUI.toIsoDate(first), confirmText: 'Lanjut' }).then(function (dari) {
            if (dari === null) return;
            AppUI.promptInput({ title: label, text: 'Sampai tanggal (kosongkan = sekarang)', input: 'date', value: AppUI.toIsoDate(today), confirmText: 'Cetak' }).then(function (sampai) {
                if (sampai === null) return;
                AppUI.openReport(label, URL.agenda + '?' + $.param({ arah: arah, dari: dari || '', sampai: sampai || '' }));
            });
        });
    }

    function templateActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-dark', 'ri-eye-line', 'Pratinjau di kop surat', function () { report('Template — ' + row.Name, URL.cetakTemplate, row); });
        if (row.FileName) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Unduh .docx', function () { window.open(URL.berkasTemplate + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Template Surat', URL.editTemplate, { encKey: row.EncDecrKey }, 'lg'); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () {
                AppUI.confirmPost('Nonaktifkan template?', row.Name, URL.hapusTemplate, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger').done(function () { refresh('Template'); });
            });
        }
    }

    return {
        add: add, save: save, agenda: agenda, addDisposition: addDisposition, doneDisposition: doneDisposition, reopenDisposition: reopenDisposition,
        removeDisposition: removeDisposition, addTemplate: addTemplate, saveTemplate: saveTemplate,
        statusTemplate: statusTemplate, letterActionTemplate: letterActionTemplate, templateActionTemplate: templateActionTemplate
    };
})();
