// Kemitraan › Mitra. URL dari window.MitraUrl (Views/Kemitraan/Mitra/Index.cshtml); helper global di js/site.js (AppUI).
window.Mitra = (function () {
    const URL = window.MitraUrl;
    const MODAL = 'staticBackdrop';
    let contactKey = null;

    $(function () {
        refresh();
    });

    function refresh() {
        AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.openModal(MODAL, 'Tambah Mitra', URL.add, null, 'lg');
    }

    function save(isEdit) {
        const form = $('#frmMitra').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.logo = AppUI.dxFile('fileLogoMitra');
        if (isEdit && $('#chkHapusLogoMitra').is(':checked')) data.hapusLogo = true;
        AppUI.postForm(isEdit ? URL.update : URL.simpanBaru, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh();
        });
    }

    /* ===== Kontak ===== */

    function contacts(row) {
        contactKey = row.EncDecrKey;
        AppUI.openModal(MODAL, 'Kontak — ' + row.Name, URL.kontak, { encKey: contactKey }, 'lg');
        $('#' + MODAL).one('hidden.bs.modal', refresh);
    }
    function reloadContacts() {
        AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.kontak, { encKey: contactKey });
    }
    function addContact() {
        const form = $('#frmKontak').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahKontak, $.extend({}, form.option('formData'), { Key: contactKey })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadContacts();
        });
    }
    function primaryContact(key) {
        AppUI.postJson(URL.utamaKontak, { Key: key }).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadContacts();
        });
    }
    function removeContact(key) {
        AppUI.confirmPost('Nonaktifkan kontak?', 'Kontak tidak dipakai lagi.', URL.hapusKontak, { Key: key }, 'Nonaktifkan', 'btn-danger').done(reloadContacts);
    }

    /* ===== Grid ===== */

    function nameTemplate(container, options) {
        const d = options.data;
        const wrap = $('<div class="d-flex align-items-center gap-2">').appendTo(container);
        if (d.HasLogo) {
            $('<img class="rounded bg-white border" width="36" height="36" style="object-fit:contain" loading="lazy" alt="">')
                .attr('src', URL.logo + '?key=' + encodeURIComponent(d.EncDecrKey)).appendTo(wrap);
        }
        $('<div class="gwa-cell-main">').text(d.Name).appendTo(wrap);
    }

    const statusTemplate = AppUI.badgeTemplate({ Aktif: 'success', 'Tidak Aktif': 'secondary', Blacklist: 'danger' });

    function actionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-contacts-book-line', title: 'Kontak', onClick: function () { contacts(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah', onClick: function () { AppUI.openModal(MODAL, 'Ubah Mitra', URL.edit, { encKey: row.EncDecrKey }, 'lg'); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan', onClick: function () {
                    AppUI.confirmPost('Nonaktifkan mitra?', row.Name, URL.delete, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger').done(refresh);
                }
            }));
        }
    }

    return { add: add, save: save, addContact: addContact, primaryContact: primaryContact, removeContact: removeContact, nameTemplate: nameTemplate, statusTemplate: statusTemplate, actionTemplate: actionTemplate };
})();
