// Anggota › Rekrutmen. URL dari window.RekrutmenUrl (Views/Anggota/Rekrutmen/Index.cshtml); helper global di js/site.js (AppUI).
window.Rekrutmen = (function () {
    const URL = window.RekrutmenUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const STATUS = { DRAFT: 1, OPEN: 2, CLOSED: 3 };
    const APPLICANT = { SUBMITTED: 1, SCREENING: 2, INTERVIEW: 3, ACCEPTED: 4, REJECTED: 5 };

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Rekrutmen', URL.add, null, 'lg');
    }

    function edit(encKey) {
        AppUI.openModal(MODAL, 'Ubah Rekrutmen', URL.edit, { encKey: encKey }, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmRekrutmen', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmRekrutmen', URL.update, null, MODAL).done(refresh);
    }

    function post(title, text, url, row, label, css) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, label, css).done(refresh);
    }

    /* ===== Pendaftar ===== */

    function applicants(row) {
        AppUI.openModal(MODAL_LARGE, 'Pendaftar — ' + row.Title, URL.pendaftar, { encKey: row.EncDecrKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }

    function refreshApplicants() {
        AppUI.loadPartial('#rekrutmenPendaftarTarget', URL.pendaftarList, { encKey: $('#rekrutmenPendaftar').data('key') });
    }

    // Status baru + catatan opsional (Ditolak: catatan wajib).
    function review(row, status, label, css, extra) {
        AppUI.inputPost({
            title: label + ' — ' + row.Fullname,
            text: status === APPLICANT.ACCEPTED ? 'Pendaftar akan dibuatkan data anggota baru.' : '',
            input: 'textarea', label: 'Catatan', value: row.Notes, required: status === APPLICANT.REJECTED,
            confirmText: label, confirmCss: css
        }, URL.review, $.extend({ Key: row.EncDecrKey, Status: status }, extra), 'Notes').done(refreshApplicants);
    }

    // Terima: pendaftar yang belum anggota wajib diberi section awal (semua anggota punya section).
    function accept(row) {
        if (row.SudahAnggota) {
            review(row, APPLICANT.ACCEPTED, 'Terima', 'btn-success');
            return;
        }
        AppUI.promptInput({
            title: 'Section — ' + row.Fullname, text: 'Pilih section awal anggota baru.',
            input: 'select', options: window.RekrutmenSection, required: true, confirmText: 'Lanjut', confirmCss: 'btn-success'
        }).then(function (sectionId) {
            if (sectionId === null) return;
            review(row, APPLICANT.ACCEPTED, 'Terima', 'btn-success', { SectionId: parseInt(sectionId, 10) });
        });
    }

    function applicantActionTemplate(container, options) {
        const row = options.data;
        if (row.Status >= APPLICANT.ACCEPTED) return;
        if (row.Status < APPLICANT.SCREENING) {
            container.append(AppUI.actionButton({ css: 'btn-outline-info', icon: 'ri-file-search-line', title: 'Seleksi berkas', onClick: function () { review(row, APPLICANT.SCREENING, 'Seleksi Berkas', 'btn-info'); } }));
        }
        if (row.Status < APPLICANT.INTERVIEW) {
            container.append(AppUI.actionButton({ css: 'btn-outline-primary', icon: 'ri-chat-voice-line', title: 'Wawancara', onClick: function () { review(row, APPLICANT.INTERVIEW, 'Wawancara', 'btn-primary'); } }));
        }
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-check-line', title: 'Terima', onClick: function () { accept(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-close-line', title: 'Tolak', onClick: function () { review(row, APPLICANT.REJECTED, 'Tolak', 'btn-danger'); } }));
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Dibuka: 'success', Ditutup: 'dark' });
    const applicantStatusTemplate = AppUI.badgeTemplate({ Terkirim: 'secondary', 'Seleksi Berkas': 'info', Wawancara: 'primary', Diterima: 'success', Ditolak: 'danger' });

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-team-line', title: 'Pendaftar', onClick: function () { applicants(row); } }));
        if (row.Status !== STATUS.OPEN) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-primary', icon: 'ri-door-open-line', title: 'Buka pendaftaran',
                onClick: function () { post('Buka pendaftaran?', row.Title + ' tampil di halaman publik dan menerima pendaftar.', URL.buka, row, 'Buka', 'btn-primary'); }
            }));
        } else {
            container.append(AppUI.actionButton({
                css: 'btn-outline-secondary', icon: 'ri-door-closed-line', title: 'Tutup pendaftaran',
                onClick: function () { post('Tutup pendaftaran?', 'Pendaftar baru tidak diterima lagi.', URL.tutup, row, 'Tutup', 'btn-secondary'); }
            }));
        }
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah rekrutmen', onClick: function () { edit(row.EncDecrKey); } }));
        if (row.Status === STATUS.DRAFT && row.JumlahPendaftar === 0) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus rekrutmen',
                onClick: function () { post('Hapus rekrutmen?', row.Title + ' dihapus permanen.', URL.hapus, row, 'Hapus', 'btn-danger'); }
            }));
        }
    }

    return {
        add: add, save: save, update: update, refresh: refresh,
        statusTemplate: statusTemplate, applicantStatusTemplate: applicantStatusTemplate,
        actionTemplate: actionTemplate, applicantActionTemplate: applicantActionTemplate
    };
})();
