// Organisasi › Profil Organisasi. URL dari window.OrganisasiUrl (Views/Organisasi/Organisasi/Index.cshtml); helper global di js/site.js (AppUI).
window.Organisasi = (function () {
    const URL = window.OrganisasiUrl;

    $(function () {
        refreshPhotos();
    });

    function save() {
        const form = $('#frmOrganisasi').dxForm('instance');
        if (!form.validate().isValid) return;
        const data = $.extend({}, form.option('formData'));
        data.Code = $.trim(data.Code || '').toUpperCase();
        data.logo = AppUI.dxFile('fileLogo');
        data.sk = AppUI.dxFile('fileSk');
        if ($('#chkHapusLogo').is(':checked')) data.hapusLogo = true;
        if ($('#chkHapusSk').is(':checked')) data.hapusSk = true;
        AppUI.postForm(URL.simpan, AppUI.toFormData(data)).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            setTimeout(function () { window.location.reload(); }, 900);
        });
    }

    function refreshPhotos() {
        AppUI.loadPartial('#targetFoto', URL.foto);
    }

    function addPhoto() {
        const file = AppUI.dxFile('fileFoto');
        if (!file) {
            AppUI.alertWarning('Pilih foto terlebih dahulu.');
            return;
        }
        AppUI.postForm(URL.tambahFoto, AppUI.toFormData({ foto: file })).done(function (resp) {
            AppUI.alertSuccess(resp.Message);
            $('#fileFoto').dxFileUploader('instance').reset();
            refreshPhotos();
        });
    }

    function removePhoto(seq) {
        AppUI.confirmPost('Hapus foto?', 'Foto #' + seq + ' dihapus dari profil organisasi.', URL.hapusFoto, { SeqPhoto: seq }, 'Hapus', 'btn-danger')
            .done(refreshPhotos);
    }

    return { save: save, addPhoto: addPhoto, removePhoto: removePhoto };
})();
