// Akses › Menu. URL dari window.MenuUrl (Views/Akses/Menu/Index.cshtml); helper di Scripts/Shared/Gwa.js.

function fnRenderNamaIkon(container, options) {
    if (options.data.Icon) $("<i>").addClass(options.data.Icon + " me-1").appendTo(container);
    $("<span>").text(options.value).appendTo(container);
}

// ================================================================ Controller
function TambahController() {
    gwaModal(MenuUrl.addController, null, "Tambah Controller");
}
function fnUbahController(encKey) {
    gwaModal(MenuUrl.editController, { encKey: encKey }, "Ubah Controller");
}
function fnSimpanController() {
    var isEdit = $("#frmController").data("edit") == 1;
    var data = { Name: gwaVal("CtlName"), IndexAction: gwaVal("CtlIndexAction"), Description: gwaValNull("CtlDescription") };
    if (!data.Name) return gwaWarning("Nama controller harap diisi");
    if (!data.IndexAction) return gwaWarning("Aksi halaman utama harap diisi");
    if (isEdit) data.IsActive = gwaId("CtlIsActive");
    gwaPost(isEdit ? MenuUrl.updateController : MenuUrl.simpanController, data, { grid: "tblController" });
}
function fnHapusController(encKey, nama) {
    gwaHapus(MenuUrl.hapusController, encKey, "Nonaktifkan controller " + nama + "?", "tblController");
}
function fnAksi(encKey, nama) {
    gwaModal(MenuUrl.aksi, { encKey: encKey }, "Daftar Aksi — " + gwaEsc(nama), "besar");
}
function fnSimpanAksi() {
    var grid = DevExpress.ui.dxDataGrid.getInstance(document.getElementById("tblAksi"));
    grid.saveEditData().then(function () {
        var rows = grid.getVisibleRows().filter(function (r) { return r.rowType === "data"; }).map(function (r) {
            return { Action: (r.data.Action || "").trim(), NeedAuth: r.data.NeedAuth, Description: r.data.Description || null };
        }).filter(function (x) { return x.Action; });
        if (!rows.length) return gwaWarning("Minimal satu aksi (aksi halaman utama wajib ada)");
        gwaPost(MenuUrl.simpanAksi, { Key: $("#frmAksi").data("key"), Rows: rows }, { grid: "tblController" });
    });
}

// ================================================================ Header menu
function TambahHeader() {
    gwaModal(MenuUrl.addHeader, null, "Tambah Header Menu", "besar");
}
function fnUbahHeader(encKey) {
    gwaModal(MenuUrl.editHeader, { encKey: encKey }, "Ubah Header Menu", "besar");
}
function fnSimpanHeader() {
    var isEdit = $("#frmHeader").data("edit") == 1;
    var data = {
        Name: gwaVal("HdrName"), Section: gwaValNull("HdrSection"), ControllerId: gwaId("HdrControllerId"),
        Icon: gwaValNull("HdrIcon"), Seq: gwaAngka("HdrSeq") || 0, Description: gwaValNull("HdrDescription")
    };
    if (!data.Name) return gwaWarning("Nama header harap diisi");
    if (isEdit) data.IsActive = gwaId("HdrIsActive");
    gwaPost(isEdit ? MenuUrl.updateHeader : MenuUrl.simpanHeader, data, { grid: "tblHeader" });
}
function fnHapusHeader(encKey, nama) {
    gwaHapus(MenuUrl.hapusHeader, encKey, "Nonaktifkan header " + nama + "?", "tblHeader");
}

// Sub menu dalam grup: grid berseleksi + seret untuk urutan.
function fnDetail(encKey, nama) {
    gwaModal(MenuUrl.detail, { encKey: encKey }, "Sub Menu — " + gwaEsc(nama), "besar");
}
function fnInitDetail() {
    $("#tblDetail").dxDataGrid({
        dataSource: window.detailItems,
        keyExpr: "Id",
        showBorders: true,
        showRowLines: true,
        height: 420,
        scrolling: { mode: "standard" },
        paging: { enabled: false },
        selection: { mode: "multiple", showCheckBoxesMode: "always", allowSelectAll: false },
        selectedRowKeys: window.detailItems.filter(function (x) { return x.Dipilih; }).map(function (x) { return x.Id; }),
        rowDragging: {
            allowReordering: true,
            showDragIcons: true,
            onReorder: function (e) {
                var visible = e.component.getVisibleRows();
                var toIndex = window.detailItems.findIndex(function (x) { return x.Id === visible[e.toIndex].data.Id; });
                var fromIndex = window.detailItems.findIndex(function (x) { return x.Id === e.itemData.Id; });
                window.detailItems.splice(fromIndex, 1);
                window.detailItems.splice(toIndex, 0, e.itemData);
                e.component.refresh();
            }
        },
        columns: [
            { dataField: "Nama", caption: "Sub menu", cellTemplate: function (c, o) { if (o.data.Ikon) $("<i>").addClass(o.data.Ikon + " me-1").appendTo(c); $("<span>").text(o.value).appendTo(c); } },
            { dataField: "Controller", caption: "Controller", cellTemplate: gwaRenderKode }
        ]
    });
}
function fnSimpanDetail() {
    var grid = DevExpress.ui.dxDataGrid.getInstance(document.getElementById("tblDetail"));
    var pilih = grid.getSelectedRowKeys();
    var ids = window.detailItems.map(function (x) { return x.Id; }).filter(function (id) { return pilih.indexOf(id) >= 0; });
    gwaPost(MenuUrl.simpanDetail, { Key: $("#frmDetail").data("key"), SubMenuIds: ids }, { grid: ["tblHeader", "tblSub"] });
}

// ================================================================ Sub menu
function TambahSub() {
    gwaModal(MenuUrl.addSub, null, "Tambah Sub Menu", "besar");
}
function fnUbahSub(encKey) {
    gwaModal(MenuUrl.editSub, { encKey: encKey }, "Ubah Sub Menu", "besar");
}
function fnSimpanSub() {
    var isEdit = $("#frmSub").data("edit") == 1;
    var data = { Name: gwaVal("SubName"), ControllerId: gwaId("SubControllerId"), Icon: gwaValNull("SubIcon"), Description: gwaValNull("SubDescription") };
    if (!data.Name) return gwaWarning("Nama sub menu harap diisi");
    if (!data.ControllerId) return gwaWarning("Pilih controller");
    if (isEdit) data.IsActive = gwaId("SubIsActive");
    gwaPost(isEdit ? MenuUrl.updateSub : MenuUrl.simpanSub, data, { grid: ["tblSub", "tblHeader"] });
}
function fnHapusSub(encKey, nama) {
    gwaHapus(MenuUrl.hapusSub, encKey, "Nonaktifkan sub menu " + nama + "?", "tblSub");
}
