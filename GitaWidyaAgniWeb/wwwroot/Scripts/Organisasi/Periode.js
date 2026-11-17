// Organisasi › Periode. URL dari window.PeriodeUrl (Views/Organisasi/Periode/Index.cshtml); helper global di js/site.js (AppUI).
// Tab Periode (#targetPeriode) dan Serah Terima (#targetSerahTerima); tab dimuat saat pertama dibuka.
window.Periode = (function () {
    const URL = window.PeriodeUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const PERIOD = { DRAFT: 1, ACTIVE: 2, CLOSED: 3 };
    const HANDOVER = { DRAFT: 1, DIAJUKAN: 2, DISETUJUI: 3, DITOLAK: 4 };
    const loaded = {};

    $(function () {
        refresh('Periode');
        $('[data-tab]').on('shown.bs.tab', function () {
            const tab = $(this).data('tab');
            if (!loaded[tab]) refresh(tab);
        });
    });

    function refresh(tab) {
        loaded[tab] = true;
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }

    /* ===== Periode ===== */

    function addPeriode() {
        AppUI.openModal(MODAL, 'Tambah Periode', URL.addPeriode, null, 'lg');
    }
    function editPeriode(encKey) {
        AppUI.openModal(MODAL, 'Ubah Periode', URL.editPeriode, { encKey: encKey }, 'lg');
    }
    function savePeriode() {
        AppUI.submitForm('#frmPeriode', URL.simpanPeriode, null, MODAL).done(function () { refresh('Periode'); });
    }
    function updatePeriode() {
        AppUI.submitForm('#frmPeriode', URL.updatePeriode, null, MODAL).done(function () { refresh('Periode'); });
    }
    function removePeriode(row) {
        AppUI.confirmPost('Hapus periode?', 'Periode ' + row.Name + ' dihapus permanen.', URL.hapusPeriode, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger')
            .done(function () { refresh('Periode'); });
    }

    const periodStatusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Aktif: 'success', Active: 'success', Ditutup: 'dark', Closed: 'dark' });

    function periodActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === PERIOD.CLOSED) return;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah periode', onClick: function () { editPeriode(row.EncDecrKey); } }));
        if (row.Status === PERIOD.DRAFT) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus periode', onClick: function () { removePeriode(row); } }));
        }
    }

    /* ===== Serah terima ===== */

    function addHandover() {
        AppUI.openModal(MODAL, 'Tambah Serah Terima', URL.addSerahTerima, null, 'lg');
    }
    function editHandover(encKey) {
        AppUI.openModal(MODAL, 'Ubah Serah Terima', URL.editSerahTerima, { encKey: encKey }, 'lg');
    }
    function saveHandover(isEdit) {
        const form = $('#frmSerahTerima').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.lpj = AppUI.dxFile('fileLpj');
        if (isEdit && $('#chkHapusLpj').is(':checked')) data.hapusLpj = true;
        AppUI.postForm(isEdit ? URL.updateSerahTerima : URL.simpanSerahTerima, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('SerahTerima');
        });
    }
    function post(title, text, url, row, label, css) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, label, css).done(function () {
            refresh('SerahTerima');
            refresh('Periode');
        });
    }
    function reject(row) {
        AppUI.reasonPost('Tolak serah terima?', URL.tolak, { Key: row.EncDecrKey }, 'Keterangan', 'Tolak', 'btn-danger')
            .done(function () { refresh('SerahTerima'); });
    }

    /* ===== Item ===== */

    function items(row) {
        AppUI.openModal(MODAL_LARGE, 'Item Serah Terima', URL.item, { encKey: row.EncDecrKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () { refresh('SerahTerima'); });
    }
    function itemKey() {
        return $('#serahTerimaItem').data('key');
    }
    function refreshItems() {
        AppUI.loadPartial('#serahTerimaItemTarget', URL.itemList, { encKey: itemKey() });
    }
    function addItem() {
        const form = $('#frmSerahTerimaItem').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahItem, $.extend({}, form.option('formData'), { Key: itemKey() })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.resetValues();
            form.updateData('ItemType', 8);
            refreshItems();
        });
    }
    function removeItem(row) {
        AppUI.confirmPost('Hapus item?', row.Description, URL.hapusItem, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger').done(refreshItems);
    }
    function itemActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus item', onClick: function () { removeItem(row); } }));
    }

    const handoverStatusTemplate = AppUI.badgeTemplate({ Draft: 'secondary', Diajukan: 'warning', Disetujui: 'success', Ditolak: 'danger' });

    function handoverActionTemplate(container, options) {
        const row = options.data;
        const editable = row.Status === HANDOVER.DRAFT || row.Status === HANDOVER.DITOLAK;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-list-check-2', title: 'Item serah terima', onClick: function () { items(row); } }));
        if (row.LpjFileName) {
            container.append(AppUI.actionButton({ css: 'btn-outline-info', icon: 'ri-file-download-line', title: 'Unduh LPJ', onClick: function () { window.open(URL.lpj + '?encKey=' + encodeURIComponent(row.EncDecrKey), '_blank'); } }));
        }
        if (editable) {
            container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah', onClick: function () { editHandover(row.EncDecrKey); } }));
            container.append(AppUI.actionButton({
                css: 'btn-outline-primary', icon: 'ri-send-plane-line', title: 'Ajukan',
                onClick: function () { post('Ajukan serah terima?', 'Setelah diajukan data tidak bisa diubah sampai ditolak.', URL.ajukan, row, 'Ajukan', 'btn-primary'); }
            }));
        }
        if (row.Status === HANDOVER.DIAJUKAN) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-success', icon: 'ri-check-double-line', title: 'Setujui',
                onClick: function () { post('Setujui serah terima?', 'Periode ' + row.FromPeriod + ' ditutup dan ' + row.ToPeriod + ' menjadi aktif. Tindakan ini tidak bisa dibatalkan.', URL.setujui, row, 'Setujui', 'btn-success'); }
            }));
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-close-circle-line', title: 'Tolak', onClick: function () { reject(row); } }));
        }
        if (row.Status === HANDOVER.DRAFT) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus',
                onClick: function () { post('Hapus serah terima?', 'Data serah terima dan itemnya dihapus.', URL.hapusSerahTerima, row, 'Hapus', 'btn-danger'); }
            }));
        }
    }

    return {
        addPeriode: addPeriode, savePeriode: savePeriode, updatePeriode: updatePeriode,
        periodStatusTemplate: periodStatusTemplate, periodActionTemplate: periodActionTemplate,
        addHandover: addHandover, saveHandover: saveHandover, addItem: addItem,
        handoverStatusTemplate: handoverStatusTemplate, handoverActionTemplate: handoverActionTemplate, itemActionTemplate: itemActionTemplate
    };
})();
