// Kegiatan › Tim Pelaksana. URL dari window.TimPelaksanaUrl (Views/Kegiatan/TimPelaksana/Index.cshtml): <aksi><Tab>, mis. listSeksi.
// Tab kegiatan memuat data kegiatan terpilih (#pilihKegiatan); tab Template tidak bergantung kegiatan. Helper global di js/site.js (AppUI).
window.TimPelaksana = (function () {
    const URL = window.TimPelaksanaUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const NAMA = { Seksi: 'Seksi', Panitia: 'Panitia', Rekrutmen: 'Rekrutmen Panitia', Rundown: 'Acara Rundown', Checklist: 'Checklist', Rombongan: 'Anggota Rombongan', Template: 'Template Checklist' };
    const PER_KEGIATAN = ['Seksi', 'Panitia', 'Rekrutmen', 'Rundown', 'Checklist', 'Rombongan'];
    const TASK = { BELUM: 1, PROSES: 2, SELESAI: 3, BATAL: 4 };
    const APP = { DAFTAR: 1, DITERIMA: 2, DITOLAK: 3, MUNDUR: 4 };
    let loaded = {};
    let current = 'Seksi';

    $(function () {
        refresh('Seksi');
        $('[data-tab]').on('shown.bs.tab', function () {
            current = $(this).data('tab');
            if (!loaded[current]) refresh(current);
        });
    });

    function activityId() {
        return $('#pilihKegiatan').dxSelectBox('instance').option('value');
    }

    function changeActivity() {
        loaded = { Template: loaded.Template };
        const id = activityId();
        if (window.history && window.history.replaceState) {
            window.history.replaceState(null, '', window.location.pathname + (id ? '?activity=' + id : ''));
        }
        refresh(current);
    }

    function refresh(tab) {
        loaded[tab] = true;
        if (PER_KEGIATAN.indexOf(tab) >= 0) {
            const id = activityId();
            if (!id) {
                $('#target' + tab).html('<p class="text-muted text-center mb-0 py-4">Pilih kegiatan terlebih dahulu.</p>');
                return;
            }
            return AppUI.loadPartial('#target' + tab, URL['list' + tab], { activityId: id });
        }
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }

    function needActivity() {
        if (activityId()) return true;
        AppUI.alertWarning('Pilih kegiatan terlebih dahulu.');
        return false;
    }

    function add(tab) {
        if (PER_KEGIATAN.indexOf(tab) >= 0 && !needActivity()) return;
        AppUI.openModal(MODAL, 'Tambah ' + NAMA[tab], URL['add' + tab], PER_KEGIATAN.indexOf(tab) >= 0 ? { activityId: activityId() } : null, 'lg');
    }

    function edit(tab, row) {
        AppUI.openModal(MODAL, 'Ubah ' + NAMA[tab], URL['edit' + tab], { encKey: row.EncDecrKey }, 'lg');
    }

    function save(tab, isEdit) {
        AppUI.submitForm('#frm' + tab, URL[(isEdit ? 'update' : 'simpan') + tab], null, MODAL).done(function () {
            refresh(tab);
            if (tab === 'Panitia' || tab === 'Seksi') loaded[tab === 'Panitia' ? 'Seksi' : 'Panitia'] = false;
        });
    }

    function post(tab, title, text, url, data, label, css) {
        return AppUI.confirmPost(title, text, url, data, label, css).done(function () { refresh(tab); });
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    /* ===== Rekrutmen panitia ===== */

    function recruitment(action) {
        const $info = $('#rekrutmenPanitia');
        const key = { Key: $info.data('key') };
        if (action === 'ubah') {
            AppUI.openModal(MODAL, 'Ubah Rekrutmen Panitia', URL.editRekrutmen, { encKey: key.Key }, 'lg');
        } else if (action === 'buka') {
            post('Rekrutmen', 'Buka rekrutmen panitia?', 'Anggota bisa melamar dari Area Anggota.', URL.bukaRekrutmen, key, 'Buka', 'btn-primary');
        } else if (action === 'tutup') {
            post('Rekrutmen', 'Tutup rekrutmen panitia?', 'Lamaran baru tidak diterima lagi.', URL.tutupRekrutmen, key, 'Tutup', 'btn-secondary');
        } else if (action === 'hapus') {
            post('Rekrutmen', 'Hapus rekrutmen panitia?', 'Rekrutmen dihapus permanen.', URL.hapusRekrutmen, key, 'Hapus', 'btn-danger');
        }
    }

    function accept(row) {
        $.getJSON(URL.opsiSeksi, { activityId: activityId() }).done(function (items) {
            const options = {};
            (items || []).forEach(function (x) { options[x.Value] = x.Text; });
            AppUI.inputPost({
                title: 'Terima ' + row.Nama + '?', text: 'Kosongkan seksi untuk memakai pilihan 1 (' + row.Pilihan1 + ').',
                input: 'select', options: options, placeholder: 'Pilihan 1', confirmText: 'Terima', confirmCss: 'btn-success'
            }, URL.terimaLamaran, { Key: row.EncDecrKey }, 'SectionId').done(function () {
                refresh('Rekrutmen');
                loaded.Panitia = false;
            });
        });
    }

    function lamaranActionTemplate(container, options) {
        const row = options.data;
        if (row.Status !== APP.DAFTAR) return;
        btn(container, 'btn-outline-success', 'ri-check-line', 'Terima', function () { accept(row); });
        btn(container, 'btn-outline-danger', 'ri-close-line', 'Tolak', function () {
            AppUI.reasonPost('Tolak lamaran ' + row.Nama + '?', URL.tolakLamaran, { Key: row.EncDecrKey }, 'Keterangan', 'Tolak', 'btn-danger')
                .done(function () { refresh('Rekrutmen'); });
        });
    }

    /* ===== Checklist ===== */

    function generate() {
        const template = $('#pilihTemplate').dxSelectBox('instance').option('value');
        if (!template) {
            AppUI.alertWarning('Pilih template terlebih dahulu.');
            return;
        }
        AppUI.confirmPost('Buat checklist dari template?', 'Item yang sudah pernah dibuat dari template ini dilewati.', URL.generateChecklist,
            { ActivityId: activityId(), TemplateId: template }, 'Buat', 'btn-success').done(function (resp) {
                if (resp && resp.Data !== undefined) AppUI.alertSuccess(resp.Data + ' item checklist dibuat.');
                refresh('Checklist');
            });
    }

    function setTask(row, status, title, label, css) {
        post('Checklist', title, row.Title, URL.statusChecklist, { Key: row.EncDecrKey, Status: status }, label, css);
    }

    function checklistActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === TASK.BELUM) btn(container, 'btn-outline-primary', 'ri-play-circle-line', 'Mulai', function () { setTask(row, TASK.PROSES, 'Mulai dikerjakan?', 'Proses', 'btn-primary'); });
        if (row.Status === TASK.PROSES) btn(container, 'btn-outline-primary', 'ri-checkbox-circle-line', 'Selesai', function () { setTask(row, TASK.SELESAI, 'Tandai selesai?', 'Selesai', 'btn-success'); });
        if (row.Status === TASK.SELESAI) btn(container, 'btn-outline-primary', 'ri-arrow-go-back-line', 'Buka lagi', function () { setTask(row, TASK.PROSES, 'Buka lagi?', 'Buka', 'btn-primary'); });
        if (row.Status !== TASK.SELESAI && row.Status !== TASK.BATAL) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Checklist', row); });
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Batalkan', function () { setTask(row, TASK.BATAL, 'Batalkan pekerjaan?', 'Batalkan', 'btn-secondary'); });
        }
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
            post('Checklist', 'Hapus checklist?', row.Title, URL.hapusChecklist, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger');
        });
    }

    function checklistTitleTemplate(container, options) {
        const d = options.data;
        const main = $('<div class="gwa-cell-main">').text(d.Title).appendTo(container);
        if (d.Milestone) main.prepend($('<i class="ri-flag-2-fill text-warning me-1" title="Milestone" aria-label="Milestone">'));
        if (d.Terlambat) $('<div class="gwa-cell-sub text-danger">').text('Lewat tenggat').appendTo(container);
    }

    function checklistRowPrepared(e) {
        if (e.rowType === 'data' && e.data.Status === TASK.BATAL) e.rowElement.addClass('bg-soft-danger');
    }

    /* ===== Rombongan ===== */

    function saveTrip(isEdit) {
        const form = $('#frmRombongan').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.izin = AppUI.dxFile('fileIzin');
        if (isEdit && $('#chkHapusIzin').is(':checked')) data.hapusIzin = true;
        AppUI.postForm(isEdit ? URL.updateRombongan : URL.simpanRombongan, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Rombongan');
        });
    }

    function rombonganActionTemplate(container, options) {
        const row = options.data;
        if (row.IzinFile) {
            btn(container, 'btn-outline-info', 'ri-file-download-line', 'Unduh surat izin', function () { window.open(URL.suratIzin + '?key=' + encodeURIComponent(row.EncDecrKey), '_blank'); });
        }
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { edit('Rombongan', row); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
            post('Rombongan', 'Hapus dari rombongan?', row.Nama, URL.hapusRombongan, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger');
        });
    }

    /* ===== Template ===== */

    function items(row) {
        AppUI.openModal(MODAL_LARGE, 'Item Template — ' + row.Name, URL.item, { encKey: row.EncDecrKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () { refresh('Template'); });
    }
    function refreshItems() {
        AppUI.loadPartial('#templateItemTarget', URL.itemList, { encKey: $('#templateItem').data('key') });
    }
    function addItem() {
        const form = $('#frmItem').dxForm('instance');
        if (!form.validate().isValid) return;
        AppUI.postJson(URL.tambahItem, $.extend({}, form.option('formData'), { Key: $('#templateItem').data('key') })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            form.getEditor('Title').reset();
            refreshItems();
        });
    }
    function itemActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus item', function () {
            AppUI.confirmPost('Hapus item?', row.Title, URL.hapusItem, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger').done(refreshItems);
        });
    }

    function templateActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-success', 'ri-list-check-2', 'Item template', function () { items(row); });
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah template', function () { edit('Template', row); });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () {
                post('Template', 'Nonaktifkan template?', row.Name, URL.hapusTemplate, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger');
            });
        }
    }

    /* ===== Grid lain ===== */

    function seksiActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah seksi', function () { edit('Seksi', row); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus seksi', function () {
            post('Seksi', 'Hapus seksi?', row.Name, URL.hapusSeksi, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger');
        });
    }

    function panitiaActionTemplate(container, options) {
        const row = options.data;
        if (!row.Aktif) return;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah peran / seksi', function () { edit('Panitia', row); });
        btn(container, 'btn-outline-secondary', 'ri-logout-box-r-line', 'Catat mundur', function () {
            post('Panitia', 'Catat mundur?', row.Nama + ' mundur dari kepanitiaan.', URL.mundurPanitia, { Key: row.EncDecrKey }, 'Mundur', 'btn-secondary');
        });
        btn(container, 'btn-outline-danger', 'ri-user-unfollow-line', 'Keluarkan', function () {
            post('Panitia', 'Keluarkan dari panitia?', row.Nama, URL.keluarkanPanitia, { Key: row.EncDecrKey }, 'Keluarkan', 'btn-danger');
        });
    }

    function rundownActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah acara', function () { edit('Rundown', row); });
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus acara', function () {
            post('Rundown', 'Hapus acara?', row.Title, URL.hapusRundown, { Key: row.EncDecrKey }, 'Hapus', 'btn-danger');
        });
    }

    const memberStatusTemplate = AppUI.badgeTemplate({ Aktif: 'success', Mundur: 'secondary', Dikeluarkan: 'danger' });
    const applicationStatusTemplate = AppUI.badgeTemplate({ Daftar: 'info', Diterima: 'success', Ditolak: 'danger', Mundur: 'secondary' });
    const taskStatusTemplate = AppUI.badgeTemplate({ Belum: 'secondary', Proses: 'primary', Selesai: 'success', Batal: 'dark' });
    const priorityTemplate = AppUI.badgeTemplate({ Rendah: 'info', Sedang: 'warning', Tinggi: 'danger' });
    const permissionTemplate = AppUI.badgeTemplate({ 'Belum Ada': 'warning', Ada: 'success', 'Tidak Perlu': 'secondary' });

    return {
        changeActivity: changeActivity, add: add, save: save, saveTrip: saveTrip, recruitment: recruitment, generate: generate, addItem: addItem,
        seksiActionTemplate: seksiActionTemplate, panitiaActionTemplate: panitiaActionTemplate, lamaranActionTemplate: lamaranActionTemplate,
        rundownActionTemplate: rundownActionTemplate, checklistActionTemplate: checklistActionTemplate, checklistTitleTemplate: checklistTitleTemplate,
        checklistRowPrepared: checklistRowPrepared, rombonganActionTemplate: rombonganActionTemplate, templateActionTemplate: templateActionTemplate,
        itemActionTemplate: itemActionTemplate, memberStatusTemplate: memberStatusTemplate, applicationStatusTemplate: applicationStatusTemplate,
        taskStatusTemplate: taskStatusTemplate, priorityTemplate: priorityTemplate, permissionTemplate: permissionTemplate
    };
})();
