// Akses › User. URL dari window.UserUrl (Views/Akses/User/Index.cshtml); helper di Scripts/Shared/Gwa.js.

function fnRenderAnggota(container, options) {
    var d = options.data;
    if (!d.NamaAnggota) return $("<span>").addClass("text-muted").text("-").appendTo(container);
    $("<div>").addClass("fw-medium").text(d.NamaAnggota).appendTo(container);
    $("<small>").addClass("text-muted").text(d.NomorAnggota || "").appendTo(container);
}
function fnRenderPeriode(container, options) {
    var d = options.data;
    if (!d.Periode) return $("<span>").addClass("text-muted").text("Tanpa periode").appendTo(container);
    $("<span>").text(d.Periode + " ").appendTo(container);
    $(gwaBadgeHtml(d.StatusPeriode, d.StatusPeriode === "Aktif" ? "success" : "secondary")).appendTo(container);
}

function Tambah() {
    gwaModal(UserUrl.add, null, "Tambah User");
}
function fnUbah(encKey) {
    gwaModal(UserUrl.edit, { encKey: encKey }, "Ubah User");
}
function fnSimpan() {
    var isEdit = $("#frmUser").data("edit") == 1;
    var data = { Username: gwaVal("Username") };
    if (!data.Username) return gwaWarning("Username harap diisi");
    if (isEdit) {
        data.IsActive = gwaId("IsActive");
    } else {
        data.Password = gwaVal("Password");
        if (data.Password.length < 8) return gwaWarning("Password minimal 8 karakter");
    }
    gwaPost(isEdit ? UserUrl.update : UserUrl.simpan, data);
}
function fnNonaktif(encKey, username) {
    gwaHapus(UserUrl.hapus, encKey, "Nonaktifkan user " + username + "? User tidak bisa login lagi.");
}

// ---- reset password
function fnResetPassword(encKey) {
    gwaModal(UserUrl.resetPassword, { encKey: encKey }, "Reset Password");
}
function fnSimpanPassword() {
    var data = { Key: $("#frmPassword").data("key"), PasswordBaru: gwaVal("PasswordBaru"), UlangiPassword: gwaVal("UlangiPassword") };
    if (data.PasswordBaru.length < 8) return gwaWarning("Password baru minimal 8 karakter");
    if (data.PasswordBaru !== data.UlangiPassword) return gwaWarning("Ulangi password tidak sama");
    gwaPost(UserUrl.simpanPassword, data, { grid: false });
}

// ---- role user
function fnRole(encKey, username) {
    gwaModal(UserUrl.role, { encKey: encKey }, "Role — " + gwaEsc(username), "besar");
}
function fnTambahRole() {
    var data = { Key: $("#frmRole").data("key"), RoleId: gwaId("RoleId"), PeriodId: gwaId("PeriodId") };
    if (!data.RoleId) return gwaWarning("Pilih role");
    gwaPost(UserUrl.tambahRole, data, {
        tutupModal: false, grid: ["tblRole", "tblGrid"],
        sukses: function () { gwaSet("RoleId", null); gwaSet("PeriodId", null); }
    });
}
function fnHapusRole(encKey, nama) {
    gwaPost(UserUrl.hapusRole, { Key: encKey }, { konfirmasi: "Cabut role " + nama + " dari user ini?", bahaya: true, tutupModal: false, grid: ["tblRole", "tblGrid"] });
}
