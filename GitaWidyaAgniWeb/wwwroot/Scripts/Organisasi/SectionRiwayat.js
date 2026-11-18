// Modal riwayat section (dipakai halaman Organisasi › Section dan Anggota › Data Anggota).
// URL dari window.SectionRiwayatUrl; isi modal = Views/Organisasi/Section/_Anggota (mode section) atau _Riwayat (mode member).
window.SectionRiwayat = (function () {
    const URL = window.SectionRiwayatUrl;

    function box() {
        return $('#sectionRiwayat');
    }

    function refresh() {
        const el = box();
        const isSection = el.data('mode') === 'section';
        AppUI.loadPartial('#sectionRiwayatTarget', isSection ? URL.sectionList : URL.memberList,
            isSection ? { encKey: el.data('key') } : { memberKey: el.data('key') });
    }

    function move() {
        const form = $('#frmSectionPindah').dxForm('instance');
        if (!form.validate().isValid) return;
        const el = box();
        const data = $.extend({}, form.option('formData'));
        if (el.data('mode') === 'section') {
            data.SectionKey = el.data('key');
        } else {
            data.MemberKey = el.data('key');
        }
        AppUI.postJson(URL.pindah, data).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            const editor = form.getEditor(el.data('mode') === 'section' ? 'MemberId' : 'SectionId');
            if (editor) editor.reset();
            form.getEditor('Note').reset();
            refresh();
        });
    }

    function remove(row) {
        const text = row.Aktif
            ? 'Section aktif dihapus dan section sebelumnya dibuka lagi.'
            : 'Baris riwayat ini dihapus permanen.';
        AppUI.confirmPost('Hapus riwayat section?', text, URL.hapus, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger')
            .done(refresh);
    }

    function rowPrepared(e) {
        if (e.rowType === 'data' && e.data.Aktif) {
            $(e.rowElement).addClass('fw-medium');
        }
    }

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus riwayat (salah input)', onClick: function () { remove(row); } }));
    }

    return { move: move, refresh: refresh, rowPrepared: rowPrepared, actionTemplate: actionTemplate };
})();
