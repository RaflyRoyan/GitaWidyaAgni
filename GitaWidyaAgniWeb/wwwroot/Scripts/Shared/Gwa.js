// Helper bersama halaman pengelolaan GWA (pola SurabayaTaxWeb Scripts/Kantor/Kantor.js).
// Pola: modal partial (GET) -> kirim JSON / FormData (POST) -> Response { Status, Message, Data } -> refresh grid DevExtreme.
// Status 1 = sukses, 0 = gagal (GWAShared.Enums.ResponseStatus).

var GWA_MODAL = { kecil: "#staticBackdrop", besar: "#staticBackdropLarge", penuh: "#staticBackdropLargeScrollable" };

// ---- token antiforgery & sesi ----
function gwaToken() {
    return $('meta[name="csrf-token"]').attr("content") || "";
}
$.ajaxSetup({ headers: { "X-Requested-With": "XMLHttpRequest" } });
$(document).ajaxSend(function (e, xhr, settings) {
    if ((settings.type || "GET").toUpperCase() !== "GET") xhr.setRequestHeader("RequestVerificationToken", gwaToken());
});
$(document).ajaxError(function (e, xhr) {
    if (xhr.status === 401) {
        window.location.href = "/Login?returnUrl=" + encodeURIComponent(location.pathname + location.search);
    } else if (xhr.status === 403) {
        gwaWarning("Anda tidak punya hak akses untuk aksi ini.");
    }
});

function gwaLoading(on) {
    $("#loading1").toggle(!!on);
}

// ---- notifikasi ----
function gwaWarning(pesan) {
    Swal.fire({ icon: "warning", title: '<b style="color:#c8202a">Peringatan</b>', text: pesan, confirmButtonColor: "#9a6b00" });
}
function gwaToast(pesan) {
    Swal.fire({ toast: true, position: "top-end", icon: "success", title: pesan, showConfirmButton: false, timer: 1800 });
}
function gwaKonfirmasi(teks, lanjut, bahaya) {
    Swal.fire({
        title: "Apakah anda yakin?", text: teks, icon: "question", showCancelButton: true, reverseButtons: true,
        confirmButtonText: "Ya, lanjutkan", cancelButtonText: "Batal", confirmButtonColor: bahaya ? "#c8202a" : "#9a6b00"
    }).then(function (r) { if (r.isConfirmed) lanjut(); });
}

// ---- modal partial ----
// ukuran: "kecil" (default) | "besar" | "penuh" (xl + scroll)
function gwaModal(url, data, judul, ukuran, setelah) {
    var modal = GWA_MODAL[ukuran || "kecil"];
    $.ajax({
        type: "GET", url: url, data: data || {},
        beforeSend: function () { gwaLoading(true); },
        complete: function () { gwaLoading(false); },
        success: function (html) {
            if (html && html.Status !== undefined) {
                gwaWarning(html.Message);
                return;
            }
            $(modal + " .modal-title").html(judul);
            $(modal + " .modal-body").html(html);
            $(modal).modal("show");
            if (setelah) setelah($(modal));
        },
        error: function (xhr) { if (xhr.status !== 401 && xhr.status !== 403) gwaWarning("Terjadi kesalahan saat memuat data."); }
    });
}
function gwaTutupModal() {
    $.each(GWA_MODAL, function (k, v) { $(v).modal("hide"); });
}

// ---- kirim data ----
// opsi: { konfirmasi: "teks", bahaya: true, sukses: fn(resp), tutupModal: true, grid: "tblGrid" | ["a","b"] | false }
function gwaPost(url, data, opsi) {
    gwaKirim(url, JSON.stringify(data || {}), "application/json; charset=utf-8", opsi);
}
// Untuk form berisi berkas: FormData (field biasa + berkas).
function gwaPostForm(url, formData, opsi) {
    gwaKirim(url, formData, false, opsi);
}
function gwaKirim(url, body, contentType, opsi) {
    opsi = opsi || {};
    var kirim = function () {
        $.ajax({
            type: "POST", url: url, data: body, contentType: contentType, processData: contentType !== false, dataType: "json",
            beforeSend: function () { gwaLoading(true); },
            complete: function () { gwaLoading(false); },
            success: function (resp) {
                if (resp.Status != 1) {
                    gwaWarning(resp.Message);
                    return;
                }
                if (opsi.tutupModal !== false) gwaTutupModal();
                gwaToast(resp.Message || "Tersimpan");
                if (opsi.grid !== false) [].concat(opsi.grid || "tblGrid").forEach(gwaRefresh);
                if (opsi.sukses) opsi.sukses(resp);
            },
            error: function (xhr) { if (xhr.status !== 401 && xhr.status !== 403) gwaWarning("Terjadi kesalahan saat mengirim data."); }
        });
    };
    if (!opsi.konfirmasi) return kirim();
    gwaKonfirmasi(opsi.konfirmasi, kirim, opsi.bahaya);
}
// Minta alasan (wajib) lalu POST; alasan dikirim sebagai field namaField (default "Keterangan").
function gwaPostAlasan(url, data, judul, opsi, namaField) {
    Swal.fire({
        title: judul, input: "textarea", inputPlaceholder: "Tuliskan alasan...", showCancelButton: true,
        confirmButtonText: "Simpan", cancelButtonText: "Batal", confirmButtonColor: "#9a6b00",
        inputValidator: function (v) { if (!v || !v.trim()) return "Alasan harap diisi"; }
    }).then(function (r) {
        if (!r.isConfirmed) return;
        data[namaField || "Keterangan"] = r.value.trim();
        gwaPost(url, data, opsi);
    });
}
// GET JSON { Status, Data } (mis. detail untuk form).
function gwaGet(url, data, sukses) {
    $.ajax({
        type: "GET", url: url, data: data || {}, dataType: "json",
        beforeSend: function () { gwaLoading(true); },
        complete: function () { gwaLoading(false); },
        success: function (resp) {
            if (resp.Status != 1) return gwaWarning(resp.Message);
            sukses(resp.Data);
        }
    });
}

function gwaRefresh(gridId) {
    var el = document.getElementById(gridId);
    if (!el) return;
    var grid = DevExpress.ui.dxDataGrid.getInstance(el) || DevExpress.ui.dxTreeList.getInstance(el);
    if (grid) grid.refresh();
}
// Nonaktifkan / hapus baris master (POST { Key }).
function gwaHapus(url, key, teks, grid) {
    gwaPost(url, { Key: key }, { konfirmasi: teks, bahaya: true, tutupModal: false, grid: grid });
}

// ---- nilai form (widget DevExtreme dikenali otomatis, fallback input HTML biasa) ----
var GWA_WIDGET = ["dxSelectBox", "dxTagBox", "dxDateBox", "dxNumberBox", "dxTextBox", "dxTextArea", "dxCheckBox", "dxSwitch", "dxFileUploader", "dxHtmlEditor", "dxRadioGroup", "dxColorBox", "dxLookup"];

function gwaWidget(id) {
    var el = document.getElementById(id);
    if (!el) return null;
    for (var i = 0; i < GWA_WIDGET.length; i++) {
        var kelas = DevExpress.ui[GWA_WIDGET[i]];
        var inst = kelas && kelas.getInstance(el);
        if (inst) return inst;
    }
    return null;
}
// nilai string (DateBox: date -> "yyyy-MM-dd", time -> "HH:mm", datetime -> "yyyy-MM-ddTHH:mm:ss")
function gwaVal(id) {
    var w = gwaWidget(id);
    var v = w ? w.option("value") : $("#" + id).val();
    if (v instanceof Date) {
        var tipe = w && w.option("type");
        v = tipe === "date" ? gwaIso(v).substring(0, 10) : tipe === "time" ? gwaIso(v).substring(11, 16) : gwaIso(v);
    }
    if (Array.isArray(v)) v = v.join(",");
    return v == null ? "" : String(v).trim();
}
function gwaValNull(id) {
    var v = gwaVal(id);
    return v === "" ? null : v;
}
function gwaSet(id, nilai) {
    var w = gwaWidget(id);
    if (w) w.option("value", nilai);
    else $("#" + id).val(nilai);
}
function gwaCek(id) {
    var w = gwaWidget(id);
    return w ? w.option("value") === true : $("#" + id).is(":checked");
}
function gwaAngka(id) {
    var w = gwaWidget(id);
    if (w && typeof w.option("value") === "number") return w.option("value");
    var v = gwaVal(id).replace(",", ".");
    if (v === "") return null;
    var n = Number(v);
    return isNaN(n) ? null : n;
}
// SelectBox angka (id) -> number atau null
function gwaId(id) {
    var v = gwaDx(id);
    return v === null || v === undefined || v === "" ? null : Number(v);
}
// nilai mentah widget (array untuk TagBox, Date untuk DateBox)
function gwaDx(id) {
    var w = gwaWidget(id);
    return w ? w.option("value") : null;
}
// berkas pertama dari FileUploader (uploadMode "useForm") atau input file biasa
function gwaFile(id) {
    var w = gwaWidget(id);
    if (w) { var f = w.option("value"); return f && f.length ? f[0] : null; }
    var el = document.getElementById(id);
    return el && el.files && el.files.length ? el.files[0] : null;
}
// Validasi grup validasi DevExtreme (ValidationGroup) bila ada; true = lolos.
function gwaValid(group) {
    try {
        var r = DevExpress.validationEngine.validateGroup(group);
        if (!r.isValid) {
            var first = r.brokenRules && r.brokenRules[0];
            gwaWarning(first ? first.message : "Lengkapi isian yang wajib.");
        }
        return r.isValid;
    } catch (e) { return true; }
}
// Objek -> FormData (nilai null dikirim kosong).
function gwaFormData(obj) {
    var fd = new FormData();
    Object.keys(obj || {}).forEach(function (k) {
        var v = obj[k];
        if (v === undefined) return;
        if (v === null) fd.append(k, "");
        else if (v instanceof File || v instanceof Blob) fd.append(k, v);
        else if (typeof v === "boolean") fd.append(k, v ? "true" : "false");
        else fd.append(k, v);
    });
    return fd;
}
function gwaIso(d) {
    if (!d) return null;
    if (!(d instanceof Date)) d = new Date(d);
    var p = function (n) { return (n < 10 ? "0" : "") + n; };
    return d.getFullYear() + "-" + p(d.getMonth() + 1) + "-" + p(d.getDate()) + "T" + p(d.getHours()) + ":" + p(d.getMinutes()) + ":" + p(d.getSeconds());
}
function gwaEsc(v) {
    return $("<div>").text(v == null ? "" : String(v)).html();
}

// ---- render kolom grid ----
function gwaRenderAktif(container, options) {
    var aktif = options.value === 1 || options.value === true;
    $("<span>").addClass("badge " + (aktif ? "badge-soft-success" : "badge-soft-secondary")).text(aktif ? "Aktif" : "Tidak Aktif").appendTo(container);
}
// Badge dari teks status (kolom *Text). Warna per teks bisa ditimpa lewat GWA_BADGE.
var GWA_BADGE = {
    "aktif": "success", "tidak aktif": "secondary", "draft": "secondary", "disetujui": "success", "ditolak": "danger", "diajukan": "info",
    "selesai": "success", "batal": "danger", "dibatalkan": "danger", "gagal": "danger", "terkirim": "success", "menunggu": "warning",
    "berjalan": "info", "revisi": "warning", "lunas": "success", "belum lunas": "warning", "ditutup": "dark", "buka": "success", "tutup": "dark"
};
function gwaBadgeHtml(teks, warna) {
    if (teks === null || teks === undefined || teks === "") return "";
    var c = warna || GWA_BADGE[String(teks).toLowerCase()] || "primary";
    return '<span class="badge badge-soft-' + c + '">' + gwaEsc(teks) + "</span>";
}
function gwaRenderBadge(container, options) {
    $(gwaBadgeHtml(options.value)).appendTo(container);
}
function gwaRenderKode(container, options) {
    if (options.value) $("<span>").addClass("gwa-code").text(options.value).appendTo(container);
}
function gwaRenderIkon(container, options) {
    if (!options.value) return;
    $("<i>").addClass(options.value + " fs-18 align-middle me-1").appendTo(container);
    $("<small>").addClass("text-muted").text(options.value).appendTo(container);
}
function gwaRp(n) {
    return "Rp " + Number(n || 0).toLocaleString("id-ID", { maximumFractionDigits: 0 });
}

// Export grid ke Excel. Pakai: .OnExporting("function(e){ gwaExportExcel(e, 'NamaFile'); }")
function gwaExportExcel(e, namaFile) {
    var workbook = new ExcelJS.Workbook();
    var worksheet = workbook.addWorksheet("Data");
    DevExpress.excelExporter.exportDataGrid({ component: e.component, worksheet: worksheet, autoFilterEnabled: true })
        .then(function () { return workbook.xlsx.writeBuffer(); })
        .then(function (buffer) { saveAs(new Blob([buffer], { type: "application/octet-stream" }), namaFile + ".xlsx"); });
    e.cancel = true;
}

// Grid di tab Bootstrap yang tersembunyi perlu menghitung ulang ukuran saat tab dibuka.
$(document).on("shown.bs.tab", function (e) {
    var target = $(e.target).attr("data-bs-target");
    $(target).find(".dx-datagrid, .dx-treelist").parent().each(function () {
        var g = DevExpress.ui.dxDataGrid.getInstance(this) || DevExpress.ui.dxTreeList.getInstance(this);
        if (g) g.updateDimensions();
    });
});
// Grid / TreeList di dalam modal dibuat saat modal masih tersembunyi; hitung ulang ukurannya saat modal tampil.
$(document).on("shown.bs.modal", function (e) {
    $(e.target).find(".dx-datagrid, .dx-treelist").parent().each(function () {
        var g = DevExpress.ui.dxDataGrid.getInstance(this) || DevExpress.ui.dxTreeList.getInstance(this);
        if (g) g.updateDimensions();
    });
});
