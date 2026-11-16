// Pengaturan Sistem. URL dari window.PengaturanUrl (Views/Pengaturan/Pengaturan/Index.cshtml); helper di Scripts/Shared/Gwa.js.
var P = window.PengaturanUrl;

function fnRenderRingkas(container, options) {
    var v = options.value || "";
    $("<span>").attr("title", v).text(v.length > 90 ? v.substring(0, 90) + "…" : v).appendTo(container);
}

// ================================================================ Pengaturan situs
function fnRenderNilaiSetting(container, options) {
    var d = options.data;
    if (d.HasImage) {
        $("<a>").attr({ href: P.gambarSetting + "?encKey=" + encodeURIComponent(d.EncDecrKey), target: "_blank", title: "Lihat gambar" })
            .addClass("me-1").append($("<i>").addClass("ri-image-line")).appendTo(container);
    }
    fnRenderRingkas(container, options);
}
function TambahSetting() {
    gwaModal(P.addSetting, null, "Tambah Pengaturan", "besar");
}
function fnUbahSetting(encKey) {
    gwaModal(P.editSetting, { encKey: encKey }, "Ubah Pengaturan", "besar");
}
function fnSimpanSetting() {
    var isEdit = $("#frmSetting").data("edit") == 1;
    var data = {
        SettingGroup: gwaVal("SettingGroup").toUpperCase(), SettingKey: gwaVal("SettingKey"), SettingValue: gwaValNull("SettingValue"),
        Description: gwaValNull("SettingDescription"), HapusGambar: isEdit && gwaWidget("HapusGambar") ? gwaCek("HapusGambar") : false,
        image: gwaFile("SettingImage")
    };
    if (!data.SettingGroup) return gwaWarning("Grup harap diisi");
    if (!data.SettingKey) return gwaWarning("Kunci harap diisi");
    gwaPostForm(isEdit ? P.updateSetting : P.simpanSetting, gwaFormData(data), { grid: "tblSetting" });
}
function fnHapusSetting(encKey, kunci) {
    gwaHapus(P.hapusSetting, encKey, "Hapus pengaturan " + kunci + "? Landing page yang memakainya kembali ke nilai bawaan.", "tblSetting");
}

// ================================================================ Format nomor
function TambahFormat() {
    gwaModal(P.addFormat, null, "Tambah Format Nomor", "besar");
}
function fnUbahFormat(encKey) {
    gwaModal(P.editFormat, { encKey: encKey }, "Ubah Format Nomor", "besar");
}
function fnSimpanFormat() {
    var isEdit = $("#frmFormat").data("edit") == 1;
    var data = {
        DocumentType: gwaVal("DocumentType"), NumberFormat: gwaVal("NumberFormat"), SeqPadding: gwaAngka("SeqPadding") || 0,
        ResetPeriod: gwaId("ResetPeriod"), Description: gwaValNull("FormatDescription")
    };
    if (!data.DocumentType) return gwaWarning("Jenis dokumen harap diisi");
    if (!data.NumberFormat) return gwaWarning("Format harap diisi");
    if (data.NumberFormat.indexOf("{seq}") < 0) return gwaWarning("Format wajib memuat {seq}");
    if (isEdit) data.IsActive = gwaId("FormatIsActive");
    gwaPost(isEdit ? P.updateFormat : P.simpanFormat, data, { grid: "tblFormat" });
}
function fnHapusFormat(encKey, jenis) {
    gwaHapus(P.hapusFormat, encKey, "Nonaktifkan format " + jenis + "? Dokumen jenis ini memakai nomor cadangan.", "tblFormat");
}

// ================================================================ Alur persetujuan + langkah
// Master-detail: tabel langkah per alur.
function fnTemplateLangkah(container, options) {
    var flow = options.data;
    $("<div>").addClass("fw-semibold mb-2").text("Langkah " + flow.Name).appendTo(container);
    $("<div>").dxDataGrid({
        dataSource: flow.Steps,
        showBorders: true,
        showRowLines: true,
        columnAutoWidth: true,
        noDataText: "Belum ada langkah. Tambahkan lewat tombol + di baris alur.",
        columns: [
            { dataField: "StepLevel", caption: "Level", width: 70, alignment: "center", sortOrder: "asc" },
            { dataField: "StepName", caption: "Nama langkah" },
            { dataField: "Jenis", caption: "Berdasarkan" },
            { dataField: "Penyetuju", caption: "Penyetuju" },
            { dataField: "Revisi", caption: "Revisi" },
            {
                caption: "", width: 100, alignment: "center", cssClass: "gwa-grid-actions",
                cellTemplate: function (c, o) {
                    $("<button>").attr({ type: "button", title: "Ubah" }).addClass("btn btn-sm btn-soft-primary btn-icon")
                        .append($("<i>").addClass("ri-edit-2-line")).on("click", function () { fnUbahStep(o.data.EncDecrKey); }).appendTo(c);
                    $("<button>").attr({ type: "button", title: "Hapus" }).addClass("btn btn-sm btn-soft-danger btn-icon")
                        .append($("<i>").addClass("ri-delete-bin-line")).on("click", function () { fnHapusStep(o.data.EncDecrKey, o.data.StepName); }).appendTo(c);
                }
            }
        ]
    }).appendTo(container);
}
function TambahFlow() {
    gwaModal(P.addFlow, null, "Tambah Alur Persetujuan", "besar");
}
function fnUbahFlow(encKey) {
    gwaModal(P.editFlow, { encKey: encKey }, "Ubah Alur Persetujuan", "besar");
}
function fnSimpanFlow() {
    var isEdit = $("#frmFlow").data("edit") == 1;
    var data = { DocumentType: gwaVal("FlowDocumentType"), Name: gwaVal("FlowName"), Description: gwaValNull("FlowDescription"), IsActive: gwaId("FlowIsActive") };
    if (!data.DocumentType) return gwaWarning("Jenis dokumen harap diisi");
    if (!data.Name) return gwaWarning("Nama alur harap diisi");
    gwaPost(isEdit ? P.updateFlow : P.simpanFlow, data, { grid: "tblFlow" });
}
function fnHapusFlow(encKey, nama) {
    gwaHapus(P.hapusFlow, encKey, "Nonaktifkan alur " + nama + "? Dokumen jenis ini tidak bisa diajukan sampai ada alur aktif.", "tblFlow");
}
function fnTambahStep(flowKey) {
    gwaModal(P.addStep, { flowKey: flowKey }, "Tambah Langkah", "besar");
}
function fnUbahStep(encKey) {
    gwaModal(P.editStep, { encKey: encKey }, "Ubah Langkah", "besar");
}
// Tampilkan pilihan penyetuju sesuai jenis (1 Role, 2 Jabatan, 3 User tertentu).
function fnTampilPenyetuju(jenis) {
    $("#frmStep [data-penyetuju]").each(function () { $(this).toggle(Number($(this).data("penyetuju")) === Number(jenis)); });
}
function fnJenisPenyetuju(e) {
    fnTampilPenyetuju(e.value);
}
function fnSimpanStep() {
    var isEdit = $("#frmStep").data("edit") == 1;
    var jenis = gwaId("ApproverType");
    var data = {
        StepLevel: gwaAngka("StepLevel") || 0, StepName: gwaVal("StepName"), ApproverType: jenis, AllowRevision: gwaId("AllowRevision"),
        RoleId: jenis === 1 ? gwaId("StepRoleId") : null, PositionId: jenis === 2 ? gwaId("StepPositionId") : null, UserId: jenis === 3 ? gwaId("StepUserId") : null
    };
    if (!data.StepName) return gwaWarning("Nama langkah harap diisi");
    if (!data.RoleId && !data.PositionId && !data.UserId) return gwaWarning("Pilih penyetuju");
    gwaPost(isEdit ? P.updateStep : P.simpanStep, data, { grid: "tblFlow" });
}
function fnHapusStep(encKey, nama) {
    gwaHapus(P.hapusStep, encKey, "Hapus langkah " + nama + "?", "tblFlow");
}

// ================================================================ Template pesan
function TambahTemplate() {
    gwaModal(P.addTemplate, null, "Tambah Template Pesan", "besar");
}
function fnUbahTemplate(encKey) {
    gwaModal(P.editTemplate, { encKey: encKey }, "Ubah Template Pesan", "besar");
}
function fnSimpanTemplate() {
    var isEdit = $("#frmTemplate").data("edit") == 1;
    var data = {
        TemplateCode: gwaVal("TemplateCode"), Channel: gwaId("Channel"), Subject: gwaValNull("Subject"),
        Body: gwaVal("Body"), Description: gwaValNull("TemplateDescription")
    };
    if (!data.TemplateCode) return gwaWarning("Kode template harap diisi");
    if (!data.Body) return gwaWarning("Isi template harap diisi");
    if (isEdit) data.IsActive = gwaId("TemplateIsActive");
    gwaPost(isEdit ? P.updateTemplate : P.simpanTemplate, data, { grid: "tblTemplate" });
}
function fnHapusTemplate(encKey, kode) {
    gwaHapus(P.hapusTemplate, encKey, "Nonaktifkan template " + kode + "?", "tblTemplate");
}

// ================================================================ Antrian pesan
var STATUS_PESAN = { 1: "warning", 2: "success", 3: "danger", 4: "secondary" };
function fnRenderStatusPesan(container, options) {
    $(gwaBadgeHtml(options.value, STATUS_PESAN[options.data.Status])).appendTo(container);
    if (options.data.LastError) $("<div>").addClass("small text-danger").text(options.data.LastError).appendTo(container);
}
function fnDetailOutbox(encKey) {
    gwaModal(P.detailOutbox, { encKey: encKey }, "Detail Pesan", "besar");
}
function fnRetryOutbox(encKey) {
    gwaPost(P.retryOutbox, { Key: encKey }, { konfirmasi: "Kirim ulang pesan ini?", tutupModal: false, grid: "tblOutbox" });
}
function fnCancelOutbox(encKey) {
    gwaPost(P.cancelOutbox, { Key: encKey }, { konfirmasi: "Batalkan pesan ini?", bahaya: true, tutupModal: false, grid: "tblOutbox" });
}

// ================================================================ Log & sequence
function fnDetailLog(encKey) {
    gwaModal(P.detailLog, { encKey: encKey }, "Detail Log", "besar");
}
function fnUbahSequence(encKey) {
    gwaModal(P.editSequence, { encKey: encKey }, "Ubah Sequence");
}
function fnSimpanSequence() {
    var v = gwaAngka("LastId");
    if (v === null || v < 0) return gwaWarning("Id terakhir harap diisi");
    gwaPost(P.updateSequence, { LastId: v }, { grid: "tblSequence" });
}
