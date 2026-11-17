// Prestasi › Sertifikat. URL dari window.SertifikatUrl (Views/Prestasi/Sertifikat/Index.cshtml); helper global di js/site.js (AppUI).
window.Sertifikat = (function () {
    const URL = window.SertifikatUrl;
    const MODAL = 'staticBackdrop';
    const JENIS = { PESERTA: 1, PANITIA: 2, PEMATERI: 3, PENGURUS: 4, PRESTASI: 5, LAINNYA: 6 };
    const STATUS = { TERBIT: 1, DICABUT: 2 };
    let form = null;

    $(function () { refresh(); });

    function refresh() {
        return AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Terbitkan Sertifikat', URL.add, null, 'lg');
    }

    function save(isEdit) {
        const f = $('#frmSertifikat').dxForm('instance');
        if (!f.validate().isValid) return;
        const data = $.extend({}, f.option('formData'));
        data.berkas = AppUI.dxFile('fileSertifikat');
        if (isEdit && $('#chkHapusBerkasSertifikat').is(':checked')) data.hapusBerkas = true;
        AppUI.postForm(isEdit ? URL.update : URL.simpanBaru, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh();
        });
    }

    /* ===== Referensi mengikuti jenis ===== */

    function formInit(e) {
        form = e.component;
    }
    function showReference(type) {
        if (!form) return;
        const kegiatan = type === JENIS.PESERTA || type === JENIS.PANITIA || type === JENIS.PEMATERI;
        form.itemOption('ActivityId', 'visible', kegiatan);
        form.itemOption('AssignmentId', 'visible', type === JENIS.PENGURUS);
        form.itemOption('AchievementId', 'visible', type === JENIS.PRESTASI);
    }
    function typeChanged(e) {
        showReference(e.value);
    }
    function memberChanged(e) {
        if (!form || !e.value) return;
        $.getJSON(URL.opsiReferensi, { memberId: e.value }).done(function (d) {
            form.getEditor('AssignmentId').option({ dataSource: d.Pengurus || [], value: null, placeholder: (d.Pengurus || []).length ? 'Pilih jabatan' : 'Anggota belum punya jabatan' });
            form.getEditor('AchievementId').option({ dataSource: d.Prestasi || [], value: null, placeholder: (d.Prestasi || []).length ? 'Pilih prestasi' : 'Anggota belum punya prestasi' });
        });
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ Terbit: 'success', Dicabut: 'danger' });

    function rowPrepared(e) {
        if (e.rowType === 'data' && e.data.Status === STATUS.DICABUT) e.rowElement.addClass('bg-soft-danger');
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function copyLink(row) {
        const link = URL.verifikasi + '?kode=' + encodeURIComponent(row.VerificationCode);
        const ok = function () { AppUI.alertSuccess('Tautan verifikasi disalin.'); };
        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(link).then(ok, function () { window.prompt('Salin tautan:', link); });
        } else {
            window.prompt('Salin tautan:', link);
        }
    }

    function actionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        if (row.FileName) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Unduh berkas', function () { window.open(URL.unduh + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        btn(container, 'btn-outline-secondary', 'ri-qr-code-line', 'Salin tautan verifikasi', function () { copyLink(row); });
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Sertifikat', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
        if (row.Status === STATUS.TERBIT) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Cabut', function () {
                AppUI.confirmPost('Cabut sertifikat?', row.CertificateNumber + ' · ' + row.MemberName + '. Verifikasi publik akan menampilkan Dicabut.', URL.cabut, key, 'Cabut', 'btn-danger').done(refresh);
            });
        } else {
            btn(container, 'btn-outline-success', 'ri-refresh-line', 'Pulihkan', function () {
                AppUI.confirmPost('Pulihkan sertifikat?', row.CertificateNumber, URL.pulihkan, key, 'Pulihkan', 'btn-success').done(refresh);
            });
        }
    }

    return {
        add: add, save: save, formInit: formInit, typeChanged: typeChanged, memberChanged: memberChanged,
        statusTemplate: statusTemplate, rowPrepared: rowPrepared, actionTemplate: actionTemplate
    };
})();
