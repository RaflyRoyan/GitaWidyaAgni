// Akses › Role. URL dari window.RoleUrl (Views/Akses/Role/Index.cshtml); helper di Scripts/Shared/Gwa.js.

function Tambah() {
    gwaModal(RoleUrl.add, null, "Tambah Role");
}
function fnUbah(encKey) {
    gwaModal(RoleUrl.edit, { encKey: encKey }, "Ubah Role");
}
function fnSimpan() {
    var isEdit = $("#frmRole").data("edit") == 1;
    var data = { Code: gwaVal("Code").toUpperCase(), Name: gwaVal("Name"), Description: gwaValNull("Description") };
    if (!data.Code) return gwaWarning("Kode role harap diisi");
    if (!data.Name) return gwaWarning("Nama role harap diisi");
    if (isEdit) data.IsActive = gwaId("IsActive");
    gwaPost(isEdit ? RoleUrl.update : RoleUrl.simpan, data);
}
function fnNonaktif(encKey, nama) {
    gwaHapus(RoleUrl.hapus, encKey, "Nonaktifkan role " + nama + "? User dengan role ini kehilangan aksesnya.");
}

// ---- hak akses (dua TreeList berseleksi rekursif)
function fnAkses(encKey, nama) {
    gwaModal(RoleUrl.akses, { encKey: encKey }, "Hak Akses — " + gwaEsc(nama), "penuh");
}
function fnRenderNodeAksi(container, options) {
    var d = options.data;
    if (d.ActionId) $("<span>").addClass("gwa-code").text(d.Nama).appendTo(container);
    else $("<span>").addClass("fw-semibold").text(d.Nama).appendTo(container);
}
function fnRenderNodeMenu(container, options) {
    var d = options.data;
    if (d.Ikon) $("<i>").addClass(d.Ikon + " me-1").appendTo(container);
    $("<span>").addClass(d.ParentId ? "" : "fw-semibold").text(d.Nama).appendTo(container);
}
function fnSimpanAkses() {
    var aksi = DevExpress.ui.dxTreeList.getInstance(document.getElementById("treeAksi"));
    var menu = DevExpress.ui.dxTreeList.getInstance(document.getElementById("treeMenu"));
    var data = {
        Key: $("#frmAkses").data("key"),
        ActionIds: aksi.getSelectedRowsData("leavesOnly").filter(function (x) { return x.ActionId; }).map(function (x) { return x.ActionId; }),
        Menus: menu.getSelectedRowsData("leavesOnly").filter(function (x) { return x.MenuValue; }).map(function (x) { return x.MenuValue; })
    };
    gwaPost(RoleUrl.simpanAkses, data, { grid: false });
}
