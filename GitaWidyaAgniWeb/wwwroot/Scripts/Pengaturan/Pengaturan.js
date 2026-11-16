// Pengaturan Sistem. URL dari window.PengaturanUrl (Views/Pengaturan/Pengaturan/Index.cshtml); helper global di js/site.js (AppUI).
// Tiap tab punya grid _List<Tab> di #target<Tab>, dimuat saat tab pertama kali dibuka dan dimuat ulang setelah simpan.
window.Pengaturan = (function () {
    const URL = window.PengaturanUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const OUTBOX_COLOR = { 1: 'warning', 2: 'success', 3: 'danger', 4: 'secondary' };
    const JOB_COLOR = { Berjalan: 'info', Sukses: 'success', Selesai: 'success', Gagal: 'danger' };
    const loaded = {};

    $(function () {
        refresh('Setting');
        $('[data-tab]').on('shown.bs.tab', function () {
            const tab = $(this).data('tab');
            if (!loaded[tab]) refresh(tab);
        });
    });

    function refresh(tab) {
        loaded[tab] = true;
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }

    function deleteRow(title, text, url, row, tab, confirmText) {
        AppUI.confirmPost(title, text, url, { Key: row.EncDecrKey }, confirmText || 'Nonaktifkan', 'btn-danger').done(function () { refresh(tab); });
    }

    /* ===== Pengaturan situs (FormData karena ada gambar) ===== */

    function addSetting() {
        AppUI.openModal(MODAL, 'Tambah Pengaturan', URL.addSetting, null, 'lg');
    }
    function editSetting(encKey) {
        AppUI.openModal(MODAL, 'Ubah Pengaturan', URL.editSetting, { encKey: encKey }, 'lg');
    }
    function saveSetting(isEdit) {
        const form = $('#frmSetting').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.SettingGroup = $.trim(data.SettingGroup || '').toUpperCase();
        data.image = AppUI.dxFile('fileSettingImage');
        AppUI.postForm(isEdit ? URL.updateSetting : URL.simpanSetting, AppUI.toFormData(data)).done(function (resp) {
            AppUI.closeModal(MODAL);
            AppUI.alertSuccess(resp.Message);
            refresh('Setting');
        });
    }
    function settingValueTemplate(container, options) {
        const d = options.data;
        if (d.HasImage) {
            $('<a class="me-1" target="_blank" rel="noopener">')
                .attr({ href: URL.gambarSetting + '?encKey=' + encodeURIComponent(d.EncDecrKey), title: 'Lihat gambar', 'aria-label': 'Lihat gambar' })
                .append($('<i class="ri-image-line" aria-hidden="true">')).appendTo(container);
        }
        shortTemplate(container, options);
    }
    function settingActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah pengaturan', onClick: function () { editSetting(row.EncDecrKey); } }));
        container.append(AppUI.actionButton({
            css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus pengaturan',
            onClick: function () { deleteRow('Hapus pengaturan?', 'Landing page yang memakai ' + row.SettingKey + ' kembali ke nilai bawaan.', URL.hapusSetting, row, 'Setting', 'Hapus'); }
        }));
    }

    /* ===== Format nomor ===== */

    function addFormat() {
        AppUI.openModal(MODAL, 'Tambah Format Nomor', URL.addFormat, null, 'lg');
    }
    function editFormat(encKey) {
        AppUI.openModal(MODAL, 'Ubah Format Nomor', URL.editFormat, { encKey: encKey }, 'lg');
    }
    function saveFormat() {
        AppUI.submitForm('#frmFormat', URL.simpanFormat, null, MODAL).done(function () { refresh('Format'); });
    }
    function updateFormat() {
        AppUI.submitForm('#frmFormat', URL.updateFormat, null, MODAL).done(function () { refresh('Format'); });
    }
    function formatActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah format', onClick: function () { editFormat(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan format',
                onClick: function () { deleteRow('Nonaktifkan format?', 'Dokumen ' + row.DocumentType + ' memakai nomor cadangan.', URL.hapusFormat, row, 'Format'); }
            }));
        }
    }

    /* ===== Alur persetujuan + langkah ===== */

    function addFlow() {
        AppUI.openModal(MODAL, 'Tambah Alur Persetujuan', URL.addFlow, null, 'lg');
    }
    function editFlow(encKey) {
        AppUI.openModal(MODAL, 'Ubah Alur Persetujuan', URL.editFlow, { encKey: encKey }, 'lg');
    }
    function saveFlow() {
        AppUI.submitForm('#frmFlow', URL.simpanFlow, null, MODAL).done(function () { refresh('Flow'); });
    }
    function updateFlow() {
        AppUI.submitForm('#frmFlow', URL.updateFlow, null, MODAL).done(function () { refresh('Flow'); });
    }

    function addStep(flowKey) {
        AppUI.openModal(MODAL_LARGE, 'Tambah Langkah', URL.addStep, { flowKey: flowKey });
    }
    function editStep(encKey) {
        AppUI.openModal(MODAL_LARGE, 'Ubah Langkah', URL.editStep, { encKey: encKey });
    }
    // Hanya pilihan penyetuju sesuai jenis (1 Role, 2 Jabatan, 3 User tertentu) yang tampil dan wajib.
    function stepFormChanged(e) {
        if (e.dataField && e.dataField !== 'ApproverType') return;
        const form = e.component;
        const jenis = Number(form.option('formData').ApproverType);
        form.itemOption('RoleId', 'visible', jenis === 1);
        form.itemOption('PositionId', 'visible', jenis === 2);
        form.itemOption('UserId', 'visible', jenis === 3);
    }
    function stepData() {
        const form = $('#frmStep').dxForm('instance');
        if (!form.validate().isValid) return null;
        const data = $.extend({}, form.option('formData'));
        const jenis = Number(data.ApproverType);
        if (jenis !== 1) data.RoleId = null;
        if (jenis !== 2) data.PositionId = null;
        if (jenis !== 3) data.UserId = null;
        if (!data.RoleId && !data.PositionId && !data.UserId) {
            AppUI.alertWarning('Pilih penyetuju.');
            return null;
        }
        return data;
    }
    function saveStep() {
        const data = stepData();
        if (data) AppUI.postAndClose(URL.simpanStep, data, MODAL_LARGE).done(function () { refresh('Flow'); });
    }
    function updateStep() {
        const data = stepData();
        if (data) AppUI.postAndClose(URL.updateStep, data, MODAL_LARGE).done(function () { refresh('Flow'); });
    }

    // Master-detail: grid langkah per alur.
    function stepTemplate(container, options) {
        const flow = options.data;
        $('<div class="gwa-mform-section-title mb-2">').text('Langkah ' + flow.Name).appendTo(container);
        $('<div>').dxDataGrid({
            dataSource: flow.Steps,
            keyExpr: 'EncDecrKey',
            elementAttr: { class: 'gwa-list-grid' },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            columnAutoWidth: true,
            noDataText: 'Belum ada langkah. Tambahkan lewat tombol + di baris alur.',
            columns: [
                { dataField: 'StepLevel', caption: 'Level', alignment: 'center', sortOrder: 'asc' },
                { dataField: 'StepName', caption: 'Nama Langkah' },
                { dataField: 'Jenis', caption: 'Berdasarkan' },
                { dataField: 'Penyetuju', caption: 'Penyetuju' },
                { dataField: 'Revisi', caption: 'Revisi' },
                {
                    caption: 'Aksi', alignment: 'center', cssClass: 'gwa-grid-actions',
                    cellTemplate: function (c, o) {
                        c.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah langkah', onClick: function () { editStep(o.data.EncDecrKey); } }));
                        c.append(AppUI.actionButton({
                            css: 'btn-outline-danger', icon: 'ri-delete-bin-line', title: 'Hapus langkah',
                            onClick: function () { deleteRow('Hapus langkah?', 'Langkah ' + o.data.StepName + ' dihapus dari alur.', URL.hapusStep, o.data, 'Flow', 'Hapus'); }
                        }));
                    }
                }
            ]
        }).appendTo(container);
    }
    function flowActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-add-circle-line', title: 'Tambah langkah', onClick: function () { addStep(row.EncDecrKey); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah alur', onClick: function () { editFlow(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan alur',
                onClick: function () { deleteRow('Nonaktifkan alur?', 'Dokumen ' + row.DocumentType + ' tidak bisa diajukan sampai ada alur aktif.', URL.hapusFlow, row, 'Flow'); }
            }));
        }
    }

    /* ===== Template pesan ===== */

    function addTemplate() {
        AppUI.openModal(MODAL, 'Tambah Template Pesan', URL.addTemplate, null, 'lg');
    }
    function editTemplate(encKey) {
        AppUI.openModal(MODAL, 'Ubah Template Pesan', URL.editTemplate, { encKey: encKey }, 'lg');
    }
    function saveTemplate() {
        AppUI.submitForm('#frmTemplate', URL.simpanTemplate, null, MODAL).done(function () { refresh('Template'); });
    }
    function updateTemplate() {
        AppUI.submitForm('#frmTemplate', URL.updateTemplate, null, MODAL).done(function () { refresh('Template'); });
    }
    function templateActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah template', onClick: function () { editTemplate(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan template',
                onClick: function () { deleteRow('Nonaktifkan template?', 'Template ' + row.TemplateCode + ' (' + row.Kanal + ') tidak dipakai lagi.', URL.hapusTemplate, row, 'Template'); }
            }));
        }
    }

    /* ===== Antrian pesan ===== */

    function outboxStatusTemplate(container, options) {
        $('<span class="badge fs-12">').addClass('badge-soft-' + (OUTBOX_COLOR[options.data.Status] || 'info')).text(options.value).appendTo(container);
        if (options.data.LastError) $('<span class="gwa-cell-sub text-danger">').text(options.data.LastError).appendTo(container);
    }
    function outboxActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({
            css: 'btn-outline-secondary', icon: 'ri-eye-line', title: 'Lihat pesan',
            onClick: function () { AppUI.openModal(MODAL_LARGE, 'Detail Pesan', URL.detailOutbox, { encKey: row.EncDecrKey }); }
        }));
        if (row.Status === 3) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-warning', icon: 'ri-restart-line', title: 'Kirim ulang',
                onClick: function () {
                    AppUI.confirmPost('Kirim ulang pesan?', 'Pesan ke ' + row.Recipient + ' dijadwalkan ulang.', URL.retryOutbox, { Key: row.EncDecrKey }, 'Kirim ulang')
                        .done(function () { refresh('Outbox'); });
                }
            }));
        }
        if (row.Status === 1 || row.Status === 3) {
            container.append(AppUI.actionButton({
                css: 'btn-outline-danger', icon: 'ri-close-circle-line', title: 'Batalkan pesan',
                onClick: function () { deleteRow('Batalkan pesan?', 'Pesan ke ' + row.Recipient + ' tidak akan dikirim.', URL.cancelOutbox, row, 'Outbox', 'Batalkan'); }
            }));
        }
    }

    /* ===== Log & sequence ===== */

    function jobStatusTemplate(container, options) {
        if (!options.value) return;
        $('<span class="badge fs-12">').addClass('badge-soft-' + (JOB_COLOR[options.value] || 'secondary')).text(options.value).appendTo(container);
    }
    function logActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({
            css: 'btn-outline-secondary', icon: 'ri-eye-line', title: 'Detail log',
            onClick: function () { AppUI.openModal(MODAL_LARGE, 'Detail Log', URL.detailLog, { encKey: row.EncDecrKey }); }
        }));
    }
    function sequenceActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({
            css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah id terakhir',
            onClick: function () { AppUI.openModal(MODAL, 'Ubah Sequence', URL.editSequence, { encKey: row.EncDecrKey }); }
        }));
    }
    function updateSequence() {
        AppUI.submitForm('#frmSequence', URL.updateSequence, null, MODAL).done(function () { refresh('Sequence'); });
    }

    // Teks panjang dipotong 90 karakter; teks lengkap di tooltip.
    function shortTemplate(container, options) {
        const v = options.value || '';
        $('<span>').attr('title', v).text(v.length > 90 ? v.substring(0, 90) + '…' : v).appendTo(container);
    }

    return {
        addSetting: addSetting, saveSetting: saveSetting, settingValueTemplate: settingValueTemplate, settingActionTemplate: settingActionTemplate,
        addFormat: addFormat, saveFormat: saveFormat, updateFormat: updateFormat, formatActionTemplate: formatActionTemplate,
        addFlow: addFlow, saveFlow: saveFlow, updateFlow: updateFlow, flowActionTemplate: flowActionTemplate,
        stepTemplate: stepTemplate, stepFormChanged: stepFormChanged, saveStep: saveStep, updateStep: updateStep,
        addTemplate: addTemplate, saveTemplate: saveTemplate, updateTemplate: updateTemplate, templateActionTemplate: templateActionTemplate,
        outboxStatusTemplate: outboxStatusTemplate, outboxActionTemplate: outboxActionTemplate,
        jobStatusTemplate: jobStatusTemplate, logActionTemplate: logActionTemplate,
        sequenceActionTemplate: sequenceActionTemplate, updateSequence: updateSequence,
        shortTemplate: shortTemplate
    };
})();
