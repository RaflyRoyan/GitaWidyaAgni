// Keuangan › Anggaran. URL dari window.AnggaranUrl (Views/Keuangan/Anggaran/Index.cshtml); helper global di js/site.js (AppUI).
window.Anggaran = (function () {
    const URL = window.AnggaranUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const STATUS = { DRAFT: 1, SUBMITTED: 2, APPROVED: 3, REJECTED: 4 };
    let detailKey = null;

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Anggaran', URL.add, null, 'lg');
    }

    function save() {
        AppUI.submitForm('#frmAnggaran', URL.simpanBaru, null, MODAL).done(refresh);
    }

    function update() {
        AppUI.submitForm('#frmAnggaran', URL.update, null, MODAL).done(refresh);
    }

    function post(title, text, url, row, label, css) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, label, css).done(refresh);
    }

    /* ===== Rincian ===== */

    function detail(row) {
        detailKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Rincian — ' + row.Name, URL.rincian, { encKey: detailKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', refresh);
    }

    function reloadDetail() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.rincian, { encKey: detailKey });
    }

    function addItem() {
        const form = $('#frmItemAnggaran').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahItem, $.extend({}, form.option('formData'), { Key: detailKey })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.getEditor('Name').reset();
            form.getEditor('UnitPrice').reset();
            AppUI.loadPartial('#anggaranItemTarget', URL.itemList, { encKey: detailKey });
        });
    }

    function itemActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({
            css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus item', onClick: function () {
                AppUI.confirmPost('Hapus item?', row.Name, URL.hapusItem, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger')
                    .done(function () { AppUI.loadPartial('#anggaranItemTarget', URL.itemList, { encKey: detailKey }); });
            }
        }));
    }

    function addDocument() {
        const file = AppUI.dxFile('fileDokumenAnggaran');
        if (!file) {
            AppUI.alertWarning('Pilih berkas terlebih dahulu.');
            return;
        }
        AppUI.postForm(URL.tambahDokumen + '?key=' + encodeURIComponent(detailKey), AppUI.toFormData({ berkas: file })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }

    function removeDocument(key) {
        AppUI.confirmPost('Hapus dokumen?', 'Berkas dihapus permanen.', URL.hapusDokumen, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Diajukan: 'warning', Disetujui: 'success', Ditolak: 'danger' });

    function absorptionTemplate(container, options) {
        const v = Math.max(0, options.value || 0);
        $('<div class="d-flex align-items-center gap-2">')
            .append($('<div class="progress flex-grow-1" style="height:6px;min-width:60px">')
                .append($('<div class="progress-bar">').addClass(v > 100 ? 'bg-danger' : 'bg-success').css('width', Math.min(v, 100) + '%')))
            .append($('<small class="text-muted">').text(v + '%'))
            .appendTo(container);
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        const editable = row.Status === STATUS.DRAFT || row.Status === STATUS.REJECTED;
        btn(container, 'btn-outline-success', 'ri-list-check-2', 'Rincian & dokumen', function () { detail(row); });
        if (editable) {
            btn(container, 'btn-outline-primary', 'ri-send-plane-line', 'Ajukan', function () { post('Ajukan anggaran?', row.Name + ' (' + row.JumlahItem + ' item).', URL.ajukan, row, 'Ajukan', 'btn-primary'); });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Anggaran', URL.edit, { encKey: row.EncDecrKey }, 'lg'); });
        }
        if (row.Status === STATUS.SUBMITTED) {
            btn(container, 'btn-outline-success', 'ri-check-double-line', 'Setujui', function () { post('Setujui anggaran?', row.Name, URL.setujui, row, 'Setujui', 'btn-success'); });
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Tolak', function () { post('Tolak anggaran?', row.Name + ' kembali bisa diubah penyusun.', URL.tolak, row, 'Tolak', 'btn-secondary'); });
        }
        if (row.Status === STATUS.DRAFT) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () { post('Hapus anggaran?', row.Name, URL.delete, row, 'Hapus', 'btn-danger'); });
        }
    }

    return {
        add: add, save: save, update: update, addItem: addItem, addDocument: addDocument, removeDocument: removeDocument,
        statusTemplate: statusTemplate, absorptionTemplate: absorptionTemplate, actionTemplate: actionTemplate, itemActionTemplate: itemActionTemplate
    };
})();
