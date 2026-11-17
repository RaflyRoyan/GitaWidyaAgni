// E-Voting. URL dari window.EVotingUrl (Views/EVoting/EVoting/Index.cshtml); helper global di js/site.js (AppUI).
window.EVoting = (function () {
    const URL = window.EVotingUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const STATUS = { DRAFT: 1, REGISTRATION: 2, OPEN: 3, CLOSED: 4, FINALIZED: 5 };
    let electionKey = null;

    $(function () { refresh(); });

    function refresh() {
        return AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Buat Pemilihan', URL.add, null, 'lg');
    }
    function save(isEdit) {
        AppUI.submitForm('#frmEVoting', isEdit ? URL.update : URL.simpanBaru, null, MODAL).done(refresh);
    }

    /* ===== Kelola ===== */

    function manage(row) {
        electionKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Kelola · ' + row.Title, URL.kelola, { encKey: electionKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }
    // Muat ulang isi modal, tetap di tab yang sedang aktif.
    function reload() {
        const active = $('#kelolaEVoting .nav-link.active').data('bs-target');
        const xhr = AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.kelola, { encKey: electionKey });
        $.when(xhr).done(function () {
            if (active) {
                const tab = document.querySelector('#kelolaEVoting [data-bs-target="' + active + '"]');
                if (tab) bootstrap.Tab.getOrCreateInstance(tab).show();
            }
        });
    }
    function notify(resp) {
        AppUI.alertSuccess(resp.Message);
        reload();
    }

    function resetCandidate() {
        $('#kdKey').val('');
        $('#kdNomor').val('');
        $('#kdVisi').val('');
        $('#kdMisi').val('');
        $('#kdHapusFoto').prop('checked', false);
        $('#kdHapusFotoWrap, #kdBatal').prop('hidden', true);
        $('#kdAnggotaWrap').prop('hidden', false);
        $('#kdSaveText').text('Daftarkan Kandidat');
        const up = $('#kdFoto').dxFileUploader('instance');
        if (up) up.reset();
    }
    function editCandidate(el) {
        const d = $(el).data();
        $('#kdKey').val(d.key);
        $('#kdNomor').val(d.nomor || '');
        $('#kdVisi').val(d.visi || '');
        $('#kdMisi').val(d.misi || '');
        $('#kdHapusFotoWrap, #kdBatal').prop('hidden', false);
        $('#kdAnggotaWrap').prop('hidden', true);
        $('#kdSaveText').text('Simpan Perubahan');
        $('#kdNomor').trigger('focus');
    }
    function saveCandidate() {
        const key = $('#kdKey').val();
        const memberId = AppUI.dxValue('kdAnggota');
        if (!key && !memberId) {
            AppUI.alertWarning('Pilih anggota.');
            return;
        }
        const data = {
            Key: electionKey,
            CandidateKey: key || null,
            MemberId: memberId || 0,
            CandidateNumber: $('#kdNomor').val() || null,
            Vision: $('#kdVisi').val() || null,
            Mission: $('#kdMisi').val() || null,
            foto: AppUI.dxFile('kdFoto')
        };
        if (key && $('#kdHapusFoto').is(':checked')) data.hapusFoto = true;
        AppUI.postForm(URL.simpanKandidat, AppUI.toFormData(data)).done(notify);
    }
    function candidateAction(action, key) {
        const labels = {
            verifikasiKandidat: ['Verifikasi kandidat?', 'Verifikasi', 'btn-success'],
            tolakKandidat: ['Tolak kandidat?', 'Tolak', 'btn-danger'],
            mundurKandidat: ['Kandidat mengundurkan diri?', 'Mundur', 'btn-secondary'],
            hapusKandidat: ['Hapus kandidat?', 'Hapus', 'btn-danger']
        };
        const l = labels[action];
        AppUI.confirmPost(l[0], '', URL[action], { Key: key }, l[1], l[2]).done(reload);
    }

    function generateVoters() {
        const types = $('.ev-tipe:checked').map(function () { return parseInt(this.value, 10); }).get();
        if (types.length === 0) {
            AppUI.alertWarning('Pilih minimal satu tipe anggota.');
            return;
        }
        AppUI.postJson(URL.tambahPemilih, { Key: electionKey, MemberTypes: types }).done(notify);
    }
    function addVoters() {
        const ids = AppUI.dxValue('evAnggota') || [];
        if (ids.length === 0) {
            AppUI.alertWarning('Pilih anggota.');
            return;
        }
        AppUI.postJson(URL.tambahPemilih, { Key: electionKey, MemberIds: ids }).done(notify);
    }
    function setEligibility(key, status) {
        AppUI.postJson(URL.hakPilih, { Key: key, Status: status }).done(notify);
    }
    function removeVoter(key) {
        AppUI.confirmPost('Hapus pemilih?', '', URL.hapusPemilih, { Key: key }, 'Hapus', 'btn-danger').done(reload);
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({
        'Draft': 'secondary', 'Pendaftaran': 'info', 'Pemungutan Suara Dibuka': 'success', 'Ditutup': 'warning', 'Final': 'dark'
    });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }
    function step(row, url, title, text, label, css) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, label, css).done(refresh);
    }

    function actionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-success', 'ri-settings-3-line', 'Kelola kandidat, pemilih & hasil', function () { manage(row); });
        switch (row.Status) {
            case STATUS.DRAFT:
                btn(container, 'btn-outline-info', 'ri-user-add-line', 'Buka pendaftaran kandidat', function () {
                    step(row, URL.pendaftaran, 'Buka pendaftaran kandidat?', row.Title, 'Buka', 'btn-info');
                });
            // falls through
            case STATUS.REGISTRATION:
                btn(container, 'btn-outline-primary', 'ri-play-circle-line', 'Buka pemungutan suara', function () {
                    step(row, URL.buka, 'Buka pemungutan suara?', 'Kandidat terverifikasi diberi nomor urut. Kandidat dan jadwal terkunci.', 'Buka', 'btn-primary');
                });
                break;
            case STATUS.OPEN:
                btn(container, 'btn-outline-secondary', 'ri-stop-circle-line', 'Tutup pemungutan', function () {
                    step(row, URL.tutup, 'Tutup pemungutan suara?', row.VotedCount + ' dari ' + row.VoterCount + ' pemilih sudah memilih.', 'Tutup', 'btn-secondary');
                });
                break;
            case STATUS.CLOSED:
                btn(container, 'btn-outline-dark', 'ri-award-line', 'Finalisasi hasil', function () {
                    step(row, URL.finalisasi, 'Finalisasi hasil?', 'Suara didekripsi dan dihitung. Pemilihan terkunci setelah final.', 'Finalisasi', 'btn-dark');
                });
                break;
        }
        if (row.Status !== STATUS.FINALIZED) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Pemilihan', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
        }
        if (row.Status === STATUS.DRAFT || row.Status === STATUS.REGISTRATION) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                step(row, URL.delete, 'Hapus pemilihan?', row.Title + ' beserta kandidat dan pemilihnya.', 'Hapus', 'btn-danger');
            });
        }
    }

    return {
        add: add, save: save, resetCandidate: resetCandidate, editCandidate: editCandidate, saveCandidate: saveCandidate, candidateAction: candidateAction,
        generateVoters: generateVoters, addVoters: addVoters, setEligibility: setEligibility, removeVoter: removeVoter,
        statusTemplate: statusTemplate, actionTemplate: actionTemplate
    };
})();
