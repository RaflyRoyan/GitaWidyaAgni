// LPJ. URL dari window.LpjUrl (Views/Lpj/Lpj/Index.cshtml); helper global di js/site.js (AppUI).
window.Lpj = (function () {
    const URL = window.LpjUrl;
    const MODAL = 'staticBackdropLarge';
    const JENIS = { KEGIATAN: 1, KEPENGURUSAN: 2 };
    const STATUS = { DRAFT: 1, FINAL: 2 };

    $(function () { refresh(); });

    function refresh() {
        return AppUI.loadPartial('#target', URL.list);
    }

    function add() {
        AppUI.promptInput({
            title: 'Buat LPJ',
            input: 'select',
            options: { 1: 'LPJ Kegiatan', 2: 'LPJ Kepengurusan' },
            value: String(JENIS.KEGIATAN),
            required: true,
            confirmText: 'Lanjut'
        }).then(function (jenis) {
            if (jenis === null) return;
            const title = Number(jenis) === JENIS.KEPENGURUSAN ? 'Buat LPJ Kepengurusan' : 'Buat LPJ Kegiatan';
            AppUI.openModal(MODAL, title, URL.add, { jenis: jenis });
        });
    }
    function save(isEdit) {
        AppUI.submitForm('#frmLpj', isEdit ? URL.update : URL.simpanBaru, null, MODAL).done(refresh);
    }

    function preview(row) {
        AppUI.openReport('LPJ — ' + row.Title, URL.pratinjau + '?key=' + encodeURIComponent(row.EncDecrKey));
    }
    function finalize(row) {
        AppUI.confirm('Finalisasi LPJ?', 'Dokumen dibuat dari data saat ini dan disimpan sebagai PDF; LPJ terkunci sampai dibuka lagi.', 'Finalisasi', 'btn-success')
            .then(function (ok) {
                if (!ok) return;
                AppUI.postJson(URL.finalisasi, { Key: row.EncDecrKey }).done(function (resp) {
                    const extra = resp.Data && resp.Data.HandoverAttached ? ' PDF juga dipasang sebagai berkas LPJ serah terima periode.' : '';
                    AppUI.alertSuccess(resp.Message + extra);
                    refresh();
                });
            });
    }

    const statusTemplate = AppUI.badgeTemplate({ Draft: 'warning', Final: 'success' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function actionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        btn(container, 'btn-outline-info', 'ri-eye-line', 'Pratinjau dokumen', function () { preview(row); });
        if (row.Status === STATUS.DRAFT) {
            btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah narasi', function () {
                AppUI.openModal(MODAL, 'Ubah LPJ', URL.edit, { encKey: row.EncDecrKey });
            });
            btn(container, 'btn-outline-success', 'ri-checkbox-circle-line', 'Finalisasi', function () { finalize(row); });
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus LPJ?', row.Title, URL.delete, key, 'Hapus', 'btn-danger').done(refresh);
            });
        } else {
            btn(container, 'btn-outline-primary', 'ri-file-download-line', 'Unduh PDF final', function () {
                window.location.href = URL.unduh + '?key=' + encodeURIComponent(row.EncDecrKey);
            });
            btn(container, 'btn-outline-secondary', 'ri-lock-unlock-line', 'Buka lagi', function () {
                AppUI.confirmPost('Buka lagi LPJ?', 'Status kembali Draft dan PDF final dibuang.', URL.bukaLagi, key, 'Buka lagi', 'btn-secondary').done(refresh);
            });
        }
    }

    return { add: add, save: save, statusTemplate: statusTemplate, actionTemplate: actionTemplate };
})();
