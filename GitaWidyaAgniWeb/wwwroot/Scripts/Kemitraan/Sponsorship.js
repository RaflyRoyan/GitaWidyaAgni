// Kemitraan › Sponsorship. URL dari window.SponsorshipUrl (Views/Kemitraan/Sponsorship/Index.cshtml); helper global di js/site.js (AppUI).
window.Sponsorship = (function () {
    const URL = window.SponsorshipUrl;
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    const NAMA = { Sponsor: 'Sponsorship', Paket: 'Paket Sponsorship' };
    const STATUS = { AKTIF: 1, SELESAI: 2, BATAL: 3 };
    const loaded = {};
    let detailKey = null;

    $(function () {
        refresh('Sponsor');
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

    function save(tab, isEdit) {
        AppUI.submitForm('#frm' + tab, URL[(isEdit ? 'update' : 'simpan') + tab], null, MODAL).done(function () { refresh(tab); });
    }

    // Ganti mitra → muat kontak mitra.
    function partyChanged(e) {
        const form = $('#frmSponsor').dxForm('instance');
        if (!form || !e.value) return;
        $.getJSON(URL.opsiKontak, { partyId: e.value }).done(function (items) {
            const editor = form.getEditor('ContactId');
            editor.option('dataSource', items || []);
            if (e.previousValue) editor.option('value', null);
        });
    }

    function receive() {
        AppUI.submitForm('#frmBayar', URL.simpanTerima, null, MODAL).done(function () { refresh('Sponsor'); });
    }

    /* ===== Detail ===== */

    function detail(row) {
        detailKey = row.EncDecrKey;
        AppUI.openModal(MODAL_LARGE, 'Sponsorship ' + row.Nomor, URL.detail, { encKey: detailKey });
        $('#' + MODAL_LARGE).one('hidden.bs.modal', function () { refresh('Sponsor'); });
    }
    function reloadDetail() {
        AppUI.loadPartial('#' + MODAL_LARGE + ' .modal-body', URL.detail, { encKey: detailKey });
    }
    function addBenefit() {
        const title = $.trim($('#benefitJudul').val() || '');
        if (!title) {
            AppUI.alertWarning('Isi judul benefit.');
            return;
        }
        AppUI.postJson(URL.tambahBenefit, { Key: detailKey, Title: title, Description: $('#benefitUraian').val() }).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function removeBenefit(key) {
        AppUI.confirmPost('Hapus benefit?', 'Benefit dihapus dari sponsorship.', URL.hapusBenefit, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }
    function undoneBenefit(key) {
        AppUI.postJson(URL.belumBenefit, { Key: key }).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function doneBenefit(key) {
        Swal.fire({
            title: 'Benefit sudah diberikan?',
            html: '<input type="file" id="swalBukti" class="form-control mb-2" aria-label="Bukti (opsional)"><input id="swalCatatan" class="form-control" placeholder="Catatan (opsional)" aria-label="Catatan">',
            showCancelButton: true,
            confirmButtonText: 'Tandai Sudah',
            cancelButtonText: 'Batal',
            customClass: { confirmButton: 'btn btn-success me-2', cancelButton: 'btn btn-light' },
            buttonsStyling: false,
            preConfirm: function () {
                return { file: document.getElementById('swalBukti').files[0] || null, notes: document.getElementById('swalCatatan').value };
            }
        }).then(function (r) {
            if (!r.isConfirmed) return;
            AppUI.postForm(URL.selesaiBenefit + '?key=' + encodeURIComponent(key), AppUI.toFormData({ bukti: r.value.file, notes: r.value.notes })).done(function (resp) {
                AppUI.alertSuccess(resp.Message);
                reloadDetail();
            });
        });
    }
    function addMou() {
        const data = {
            Key: detailKey,
            MouNumber: $.trim($('#mouNomor').val() || '') || null,
            SignedDate: $('#mouTtd').val() || null,
            StartDate: $('#mouMulai').val() || null,
            EndDate: $('#mouAkhir').val() || null,
            dokumen: AppUI.dxFile('fileMou')
        };
        AppUI.postForm(URL.tambahMou, AppUI.toFormData(data)).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            reloadDetail();
        });
    }
    function removeMou(key) {
        AppUI.confirmPost('Hapus MoU?', 'Ditolak bila MoU dipakai item serah terima.', URL.hapusMou, { Key: key }, 'Hapus', 'btn-danger').done(reloadDetail);
    }

    /* ===== Paket ===== */

    function reloadPackage() {
        AppUI.loadPartial('#' + MODAL + ' .modal-body', URL.editPaket, { encKey: $('#paketForm').data('key') });
    }
    function addPackageBenefit() {
        const title = $.trim($('#paketBenefit').val() || '');
        if (!title) return;
        AppUI.postJson(URL.tambahBenefitPaket, { Key: $('#paketForm').data('key'), Title: title }).done(reloadPackage);
    }
    function removePackageBenefit(key) {
        AppUI.postJson(URL.hapusBenefitPaket, { Key: key }).done(reloadPackage);
    }

    /* ===== Grid ===== */

    const statusTemplate = AppUI.badgeTemplate({ Aktif: 'primary', Selesai: 'success', Batal: 'dark' });

    function btn(container, css, icon, title, onClick) {
        container.append(AppUI.actionButton({ css: css, icon: icon, title: title, onClick: onClick }));
    }

    function sponsorActionTemplate(container, options) {
        const row = options.data;
        const key = { Key: row.EncDecrKey };
        btn(container, 'btn-outline-info', 'ri-file-list-3-line', 'Benefit, MoU & dana', function () { detail(row); });
        if (row.Status !== STATUS.AKTIF) return;
        if (row.Dana && row.ReceivedAmount < row.Amount) {
            btn(container, 'btn-outline-dark', 'ri-money-dollar-circle-line', 'Catat dana masuk', function () { AppUI.openModal(MODAL, 'Dana Sponsor Masuk', URL.terima, { encKey: row.EncDecrKey }, 'lg'); });
        }
        btn(container, 'btn-outline-success', 'ri-checkbox-circle-line', 'Selesai', function () {
            AppUI.confirmPost('Selesaikan sponsorship?', 'Semua benefit harus sudah diberikan.', URL.selesai, key, 'Selesai', 'btn-success').done(function () { refresh('Sponsor'); });
        });
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah', function () { AppUI.openModal(MODAL, 'Ubah Sponsorship', URL.editSponsor, { encKey: row.EncDecrKey }, 'lg'); });
        if (row.ReceivedAmount === 0) {
            btn(container, 'btn-outline-secondary', 'ri-close-circle-line', 'Batalkan', function () {
                AppUI.reasonPost('Batalkan sponsorship ' + row.Nomor + '?', URL.batal, key, 'Keterangan', 'Batalkan', 'btn-secondary').done(function () { refresh('Sponsor'); });
            });
            btn(container, 'btn-outline-danger', 'ri-delete-bin-line', 'Hapus', function () {
                AppUI.confirmPost('Hapus sponsorship?', row.Nomor + ' beserta benefitnya.', URL.hapusSponsor, key, 'Hapus', 'btn-danger').done(function () { refresh('Sponsor'); });
            });
        }
    }

    function packageActionTemplate(container, options) {
        const row = options.data;
        btn(container, 'btn-outline-warning', 'ri-edit-2-line', 'Ubah & benefit', function () {
            AppUI.openModal(MODAL, 'Ubah Paket', URL.editPaket, { encKey: row.EncDecrKey }, 'lg');
            $('#' + MODAL).one('hidden.bs.modal', function () { refresh('Paket'); });
        });
        if (row.Aktif) {
            btn(container, 'btn-outline-danger', 'ri-forbid-line', 'Nonaktifkan', function () {
                AppUI.confirmPost('Nonaktifkan paket?', row.Name, URL.hapusPaket, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger').done(function () { refresh('Paket'); });
            });
        }
    }

    return {
        add: add, save: save, partyChanged: partyChanged, receive: receive, addBenefit: addBenefit, removeBenefit: removeBenefit,
        undoneBenefit: undoneBenefit, doneBenefit: doneBenefit, addMou: addMou, removeMou: removeMou,
        addPackageBenefit: addPackageBenefit, removePackageBenefit: removePackageBenefit,
        statusTemplate: statusTemplate, sponsorActionTemplate: sponsorActionTemplate, packageActionTemplate: packageActionTemplate
    };
})();
