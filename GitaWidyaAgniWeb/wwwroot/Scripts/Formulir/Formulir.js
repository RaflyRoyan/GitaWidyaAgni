// Formulir › Formulir. URL dari window.FormulirUrl (Views/Formulir/Formulir/Index.cshtml); helper global di js/site.js (AppUI).
window.Formulir = (function () {
    const URL = window.FormulirUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const STATUS = { DRAFT: 1, OPEN: 2, CLOSED: 3 };
    const JENIS = { EVALUASI: 1, PENDAFTARAN: 2, SURVEI: 3 };
    const TIPE_PILIHAN = ['3', '4'];
    let form = null;
    let formKey = null;

    $(function () { refresh(); });

    function refresh() {
        return AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Buat Formulir', URL.add, null, 'lg');
    }
    function save(isEdit) {
        AppUI.submitForm('#frmFormulir', isEdit ? URL.update : URL.simpanBaru, null, MODAL).done(refresh);
    }

    // Referensi mengikuti jenis.
    function formInit(e) {
        form = e.component;
        setTimeout(function () { showReference(form.option('formData').FormType); }, 0);
    }
    function showReference(type) {
        if (!form) return;
        form.itemOption('ActivityId', 'visible', type === JENIS.EVALUASI);
        form.itemOption('RecruitmentId', 'visible', type === JENIS.PENDAFTARAN);
        form.itemOption('Anonim', 'visible', type !== JENIS.PENDAFTARAN);
    }
    function typeChanged(e) {
        showReference(e.value);
    }

    /* ===== Pertanyaan ===== */

    function questions(row) {
        formKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Pertanyaan · ' + row.Title, URL.pertanyaan, { encKey: formKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }
    function reloadQuestions() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.pertanyaan, { encKey: formKey });
    }
    function questionTypeChanged() {
        $('#qOptionsWrap').prop('hidden', TIPE_PILIHAN.indexOf($('#qType').val()) < 0);
    }
    function resetQuestion() {
        $('#qKey').val('');
        $('#qText').val('');
        $('#qOptions').val('');
        $('#qSeq').val(0);
        $('#qWajib').prop('checked', false);
        $('#qType').val('1');
        $('#qSaveText').text('Tambah');
        $('#qBatal').prop('hidden', true);
        questionTypeChanged();
    }
    function editQuestion(el) {
        const d = $(el).data();
        $('#qKey').val(d.key);
        $('#qText').val(d.text);
        $('#qOptions').val(d.options || '');
        $('#qSeq').val(d.seq);
        $('#qWajib').prop('checked', String(d.wajib) === '1');
        $('#qType').val(String(d.type));
        $('#qSaveText').text('Simpan Perubahan');
        $('#qBatal').prop('hidden', false);
        questionTypeChanged();
        $('#qText').trigger('focus');
    }
    function saveQuestion() {
        const text = $.trim($('#qText').val() || '');
        if (!text) {
            AppUI.alertWarning('Isi teks pertanyaan.');
            return;
        }
        AppUI.postJson(URL.simpanPertanyaan, {
            Key: formKey,
            QuestionKey: $('#qKey').val() || null,
            Seq: parseInt($('#qSeq').val(), 10) || 0,
            QuestionText: text,
            QuestionType: parseInt($('#qType').val(), 10),
            Options: $('#qOptions').val() || null,
            Wajib: $('#qWajib').is(':checked')
        }).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadQuestions();
        });
    }
    function removeQuestion(key) {
        AppUI.confirmPost('Hapus pertanyaan?', 'Pertanyaan dihapus dari formulir.', URL.hapusPertanyaan, { Key: key }, 'Hapus', 'btn-danger').done(reloadQuestions);
    }

    /* ===== Jawaban ===== */

    function responses(row) {
        formKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Jawaban · ' + row.Title, URL.jawaban, { encKey: formKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }
    function viewResponse(key) {
        AppUI.loadPartial('#jawabanDetail', URL.jawabanDetail, { encKey: key });
    }
    function removeResponse(key) {
        AppUI.confirmPost('Hapus jawaban?', 'Jawaban dan isinya dihapus permanen.', URL.hapusJawaban, { Key: key }, 'Hapus', 'btn-danger').done(function () {
            AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.jawaban, { encKey: formKey });
        });
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Dibuka: 'success', Ditutup: 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        btn(container, 'btn-outline-success', 'ri-list-check-2', 'Pertanyaan', function () { questions(row); });
        if (row.ResponseCount > 0) {
            btn(container, 'btn-outline-info', 'ri-bar-chart-2-line', 'Jawaban & ringkasan', function () { responses(row); });
        }
        if (row.Status !== STATUS.OPEN) {
            btn(container, 'btn-outline-primary', 'ri-play-circle-line', 'Buka', function () {
                AppUI.confirmPost('Buka formulir?', row.Title + ' bisa diisi sesuai jadwal.', URL.buka, key, 'Buka', 'btn-primary').done(refresh);
            });
        } else {
            btn(container, 'btn-outline-secondary', 'ri-stop-circle-line', 'Tutup', function () {
                AppUI.confirmPost('Tutup formulir?', row.Title, URL.tutup, key, 'Tutup', 'btn-secondary').done(refresh);
            });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Formulir', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
        if (row.Status === STATUS.DRAFT) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus formulir?', row.Title + ' beserta pertanyaannya.', URL.delete, key, 'Hapus', 'btn-danger').done(refresh);
            });
        }
    }

    return {
        add: add, save: save, formInit: formInit, typeChanged: typeChanged,
        questionTypeChanged: questionTypeChanged, resetQuestion: resetQuestion, editQuestion: editQuestion, saveQuestion: saveQuestion, removeQuestion: removeQuestion,
        viewResponse: viewResponse, removeResponse: removeResponse, statusTemplate: statusTemplate, actionTemplate: actionTemplate
    };
})();
