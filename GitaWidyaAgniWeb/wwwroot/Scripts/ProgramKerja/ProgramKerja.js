// Program Kerja. URL dari window.ProgramKerjaUrl (Views/ProgramKerja/ProgramKerja/Index.cshtml): <aksi><Tab>, mis. listProgram, simpanTugas.
// Tiap tab punya grid di #target<Tab>, dimuat saat tab pertama dibuka. Helper global di js/site.js (AppUI).
window.ProgramKerja = (function () {
    const URL = window.ProgramKerjaUrl;
    const MODAL = 'staticBackdrop';
    const NAMA = { Program: 'Program', Tugas: 'Tugas', Timeline: 'Rencana Timeline' };
    const PROGRAM = { PLANNED: 1, ONGOING: 2, DONE: 3, CANCELLED: 4 };
    const TASK = { BELUM: 1, PROSES: 2, SELESAI: 3, BATAL: 4 };
    const PLAN = { RENCANA: 1, TERJADWAL: 2, SELESAI: 3, BATAL: 4 };
    const loaded = {};

    $(function () {
        refresh('Program');
        $('[data-tab]').on('shown.bs.tab', function () {
            const tab = $(this).data('tab');
            if (!loaded[tab]) refresh(tab);
        });
    });

    function refresh(tab) {
        loaded[tab] = true;
        return AppUI.loadPartial('#target' + tab, URL['list' + tab]);
    }

    function add(tab) {
        AppUI.openModal(MODAL, 'Tambah ' + NAMA[tab], URL['add' + tab], null, 'lg');
    }

    function edit(tab, row) {
        AppUI.openModal(MODAL, 'Ubah ' + NAMA[tab], URL['edit' + tab], { encKey: row.EncDecrKey }, 'lg');
    }

    function save(tab, isEdit) {
        AppUI.submitForm('#frm' + tab, URL[(isEdit ? 'update' : 'simpan') + tab], null, MODAL).done(function () {
            refresh(tab);
            if (tab === 'Tugas') loaded.Program = false;
        });
    }

    function setStatus(tab, row, status, title, text, label, css) {
        AppUI.confirmPost(title, text, URL['status' + tab], { Key: row.EncDecrKey, Status: status }, label, css).done(function () {
            refresh(tab);
            if (tab === 'Tugas') loaded.Program = false;
        });
    }

    function remove(tab, row) {
        AppUI.confirmPost('Hapus ' + NAMA[tab].toLowerCase() + '?', row.Title + ' dihapus permanen.', URL['hapus' + tab], { Key: row.EncDecrKey }, 'Hapus', 'btn-danger')
            .done(function () { refresh(tab); });
    }

    function link(row) {
        $.getJSON(URL.opsiKegiatan).done(function (items) {
            if (!items || !items.length) {
                AppUI.alertWarning('Belum ada kegiatan yang direncanakan / berjalan.');
                return;
            }
            const options = {};
            items.forEach(function (x) { options[x.Value] = x.Text; });
            AppUI.inputPost({
                title: 'Tautkan kegiatan', text: row.Title, input: 'select', options: options, placeholder: 'Pilih kegiatan...',
                label: 'Kegiatan', required: true, confirmText: 'Tautkan'
            }, URL.tautKegiatan, { Key: row.EncDecrKey }, 'ActivityId').done(function () { refresh('Timeline'); });
        });
    }

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    /* ===== Template grid ===== */

    function progressTemplate(container, options) {
        const v = options.value || 0;
        const d = options.data;
        $('<div class="d-flex align-items-center gap-2">')
            .append($('<div class="progress flex-grow-1" style="height:6px;min-width:60px">')
                .append($('<div class="progress-bar bg-success">').css('width', v + '%').attr({ role: 'progressbar', 'aria-valuenow': v, 'aria-valuemin': 0, 'aria-valuemax': 100 })))
            .append($('<small class="text-muted text-nowrap">').text(d.TugasSelesai + '/' + d.JumlahTugas))
            .appendTo(container);
    }

    const programStatusTemplate = AppUI.badgeTemplate({ Direncanakan: 'secondary', Berjalan: 'primary', Selesai: 'success', Dibatalkan: 'dark' });
    const taskStatusTemplate = AppUI.badgeTemplate({ Belum: 'secondary', Proses: 'primary', Selesai: 'success', Batal: 'dark' });
    const timelineStatusTemplate = AppUI.badgeTemplate({ Rencana: 'secondary', Terjadwal: 'primary', Selesai: 'success', Batal: 'dark' });
    const priorityTemplate = AppUI.badgeTemplate({ Rendah: 'info', Sedang: 'warning', Tinggi: 'danger' });

    function taskTitleTemplate(container, options) {
        const d = options.data;
        const main = $('<div class="gwa-cell-main">').text(d.Title).appendTo(container);
        if (d.Milestone) main.prepend($('<i class="ri-flag-2-fill text-warning me-1" title="Milestone" aria-label="Milestone">'));
        if (d.Terlambat) $('<div class="gwa-cell-sub text-danger">').text('Lewat tenggat').appendTo(container);
    }

    function taskRowPrepared(e) {
        if (e.rowType === 'data' && e.data.Status === TASK.BATAL) e.rowElement.addClass('bg-soft-danger');
    }

    function programActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === PROGRAM.PLANNED) {
            btn(container, 'btn-outline-primary', 'ri-play-circle-line', 'Mulai program', function () { setStatus('Program', row, PROGRAM.ONGOING, 'Mulai program?', row.Title + ' menjadi Berjalan.', 'Mulai', 'btn-primary'); });
        } else if (row.Status === PROGRAM.ONGOING) {
            btn(container, 'btn-outline-primary', 'ri-checkbox-circle-line', 'Selesaikan program', function () { setStatus('Program', row, PROGRAM.DONE, 'Selesaikan program?', row.Title + ' menjadi Selesai.', 'Selesai', 'btn-success'); });
        }
        if (row.Status === PROGRAM.PLANNED || row.Status === PROGRAM.ONGOING) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah program', function () { edit('Program', row); });
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Batalkan program', function () { setStatus('Program', row, PROGRAM.CANCELLED, 'Batalkan program?', row.Title + ' menjadi Dibatalkan.', 'Batalkan', 'btn-secondary'); });
        }
        if (row.Status === PROGRAM.PLANNED) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus program', function () { remove('Program', row); });
        }
    }

    function taskActionTemplate(container, options) {
        const row = options.data;
        if (row.Status === TASK.BELUM) {
            btn(container, 'btn-outline-primary', 'ri-play-circle-line', 'Mulai dikerjakan', function () { setStatus('Tugas', row, TASK.PROSES, 'Mulai tugas?', row.Title, 'Proses', 'btn-primary'); });
        } else if (row.Status === TASK.PROSES) {
            btn(container, 'btn-outline-primary', 'ri-checkbox-circle-line', 'Tandai selesai', function () { setStatus('Tugas', row, TASK.SELESAI, 'Tugas selesai?', row.Title, 'Selesai', 'btn-success'); });
        } else if (row.Status === TASK.SELESAI) {
            btn(container, 'btn-outline-primary', 'ri-arrow-go-back-line', 'Buka lagi', function () { setStatus('Tugas', row, TASK.PROSES, 'Buka lagi tugas?', row.Title + ' kembali ke Proses.', 'Buka', 'btn-primary'); });
        }
        if (row.Status !== TASK.SELESAI && row.Status !== TASK.BATAL) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah tugas', function () { edit('Tugas', row); });
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Batalkan tugas', function () { setStatus('Tugas', row, TASK.BATAL, 'Batalkan tugas?', row.Title, 'Batalkan', 'btn-secondary'); });
        }
        btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus tugas', function () { remove('Tugas', row); });
    }

    function timelineActionTemplate(container, options) {
        const row = options.data;
        const open = row.Status === PLAN.RENCANA || row.Status === PLAN.TERJADWAL;
        if (open) {
            btn(container, 'btn-outline-success', 'ri-links-line', 'Tautkan kegiatan', function () { link(row); });
            btn(container, 'btn-outline-primary', 'ri-check-line', 'Tandai selesai', function () { setStatus('Timeline', row, PLAN.SELESAI, 'Tandai selesai?', row.Title, 'Selesai', 'btn-success'); });
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah rencana', function () { edit('Timeline', row); });
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Batalkan', function () { setStatus('Timeline', row, PLAN.BATAL, 'Batalkan rencana?', row.Title, 'Batalkan', 'btn-secondary'); });
        }
        if (row.Status === PLAN.RENCANA) {
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus rencana', function () { remove('Timeline', row); });
        }
    }

    return {
        add: add, save: save,
        progressTemplate: progressTemplate, programStatusTemplate: programStatusTemplate, taskStatusTemplate: taskStatusTemplate,
        timelineStatusTemplate: timelineStatusTemplate, priorityTemplate: priorityTemplate, taskTitleTemplate: taskTitleTemplate,
        taskRowPrepared: taskRowPrepared, programActionTemplate: programActionTemplate, taskActionTemplate: taskActionTemplate,
        timelineActionTemplate: timelineActionTemplate
    };
})();
