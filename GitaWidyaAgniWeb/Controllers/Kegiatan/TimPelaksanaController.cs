using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Kegiatan.TimPelaksanaVM;

namespace GitaWidyaAgniWeb.Controllers.Kegiatan
{
    /// <summary>
    /// Kegiatan › Tim Pelaksana (/Kegiatan/TimPelaksana?activity=id): seksi, panitia, rekrutmen panitia, rundown, checklist,
    /// rombongan per kegiatan, serta master template checklist. Semua isi tab kegiatan = data panitia → Create / Edit / Delete;
    /// buka-tutup rekrutmen, terima / tolak lamaran, dan status checklist = Process.
    /// </summary>
    [Area("Kegiatan")]
    public class TimPelaksanaController : BaseController
    {
        private const string KEY = "TimPelaksana_KEY_EDIT_";

        public IActionResult Index(int? activity) => HalamanIndex(new VM.Index(activity));

        private IActionResult EditForm(string tab, string encKey, Func<object> buat)
        {
            TempData[KEY + tab] = encKey;
            return Bagian(buat, "Edit" + tab);
        }
        private string EditKeyTab(string tab) => EditKey(KEY + tab);

        // ---------- Seksi
        [AccessAs(Acc.Index)] public IActionResult ListSeksi(int activityId) => Bagian(() => new VM.ListSeksi(activityId));
        [AccessAs(Acc.Create)] public IActionResult AddSeksi(int activityId) => Bagian(() => new VM.AddSeksi(activityId));
        [AccessAs(Acc.Edit)] public IActionResult EditSeksi(string encKey) => EditForm("Seksi", encKey, () => new VM.EditSeksi(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanSeksi([FromBody] VM.NewSeksi m) => Aksi(u => VM.Method.SimpanSeksi(m, u), "Seksi disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateSeksi([FromBody] VM.NewSeksi m) => Aksi(u => VM.Method.UpdateSeksi(EditKeyTab("Seksi"), m, u), "Seksi diubah.", KEY + "Seksi");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusSeksi([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusSeksi(i.Key, u), "Seksi dihapus.");

        // ---------- Panitia
        [AccessAs(Acc.Index)] public IActionResult ListPanitia(int activityId) => Bagian(() => new VM.ListPanitia(activityId));
        [AccessAs(Acc.Create)] public IActionResult AddPanitia(int activityId) => Bagian(() => new VM.AddPanitia(activityId));
        [AccessAs(Acc.Edit)] public IActionResult EditPanitia(string encKey) => EditForm("Panitia", encKey, () => new VM.EditPanitia(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanPanitia([FromBody] VM.NewPanitia m) => Aksi(u => VM.Method.SimpanPanitia(m, u), "Panitia ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdatePanitia([FromBody] VM.NewPanitia m) => Aksi(u => VM.Method.UpdatePanitia(EditKeyTab("Panitia"), m, u), "Panitia diubah.", KEY + "Panitia");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult MundurPanitia([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.MundurPanitia(i.Key, u), "Panitia dicatat mundur.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult KeluarkanPanitia([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.KeluarkanPanitia(i.Key, u), "Panitia dikeluarkan.");

        // ---------- Rekrutmen panitia
        [AccessAs(Acc.Index)] public IActionResult ListRekrutmen(int activityId) => Bagian(() => new VM.ListRekrutmen(activityId));
        [AccessAs(Acc.Create)] public IActionResult AddRekrutmen(int activityId) => Bagian(() => new VM.AddRekrutmen(activityId));
        [AccessAs(Acc.Edit)] public IActionResult EditRekrutmen(string encKey) => EditForm("Rekrutmen", encKey, () => new VM.EditRekrutmen(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanRekrutmen([FromBody] VM.NewRekrutmen m) => Aksi(u => VM.Method.SimpanRekrutmen(m, u), "Rekrutmen panitia dibuat.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateRekrutmen([FromBody] VM.NewRekrutmen m) => Aksi(u => VM.Method.UpdateRekrutmen(EditKeyTab("Rekrutmen"), m, u), "Rekrutmen panitia diubah.", KEY + "Rekrutmen");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult BukaRekrutmen([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadRekrutmen(i.Key).Open(u).Ok(); }, "Rekrutmen panitia dibuka.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult TutupRekrutmen([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadRekrutmen(i.Key).Close(u).Ok(); }, "Rekrutmen panitia ditutup.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusRekrutmen([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadRekrutmen(i.Key).Delete(u).Ok(); }, "Rekrutmen panitia dihapus.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult TerimaLamaran([FromBody] VM.TerimaInput i) => Aksi(u => VM.Method.TerimaLamaran(i, u), "Lamaran diterima; anggota masuk panitia sebagai Staf.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult TolakLamaran([FromBody] GwaVM.AlasanInput i) => Aksi(u => VM.Method.TolakLamaran(i, u), "Lamaran ditolak.");

        // Pilihan seksi kegiatan (JSON) untuk dialog terima lamaran.
        [AccessAs(Acc.Index)]
        public IActionResult OpsiSeksi(int activityId)
            => Json(GWALib.Lib.Transaksi.TimPelaksana.CommitteeSection.GetDataList(activityId).OrderBy(x => x.SeqNo).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)));

        // ---------- Rundown
        [AccessAs(Acc.Index)] public IActionResult ListRundown(int activityId) => Bagian(() => new VM.ListRundown(activityId));
        [AccessAs(Acc.Create)] public IActionResult AddRundown(int activityId) => Bagian(() => new VM.AddRundown(activityId));
        [AccessAs(Acc.Edit)] public IActionResult EditRundown(string encKey) => EditForm("Rundown", encKey, () => new VM.EditRundown(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanRundown([FromBody] VM.NewRundown m) => Aksi(u => VM.Method.SimpanRundown(m, u), "Rundown disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateRundown([FromBody] VM.NewRundown m) => Aksi(u => VM.Method.UpdateRundown(EditKeyTab("Rundown"), m, u), "Rundown diubah.", KEY + "Rundown");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusRundown([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusRundown(i.Key, u), "Rundown dihapus.");

        // ---------- Checklist
        [AccessAs(Acc.Index)] public IActionResult ListChecklist(int activityId) => Bagian(() => new VM.ListChecklist(activityId));
        [AccessAs(Acc.Create)] public IActionResult AddChecklist(int activityId) => Bagian(() => new VM.AddChecklist(activityId));
        [AccessAs(Acc.Edit)] public IActionResult EditChecklist(string encKey) => EditForm("Checklist", encKey, () => new VM.EditChecklist(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanChecklist([FromBody] VM.NewChecklist m) => Aksi(u => VM.Method.SimpanChecklist(m, u), "Checklist disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateChecklist([FromBody] VM.NewChecklist m) => Aksi(u => VM.Method.UpdateChecklist(EditKeyTab("Checklist"), m, u), "Checklist diubah.", KEY + "Checklist");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult StatusChecklist([FromBody] GwaVM.StatusInput i) => Aksi(u => VM.Method.StatusChecklist(i, u), "Status checklist diperbarui.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusChecklist([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusChecklist(i.Key, u), "Checklist dihapus.");
        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult GenerateChecklist([FromBody] VM.GenerateInput i) => Aksi(u => (object)VM.Method.GenerateChecklist(i, u), "Checklist dibuat dari template.");

        // ---------- Rombongan (FormData: surat izin)
        [AccessAs(Acc.Index)] public IActionResult ListRombongan(int activityId) => Bagian(() => new VM.ListRombongan(activityId));
        [AccessAs(Acc.Create)] public IActionResult AddRombongan(int activityId) => Bagian(() => new VM.AddRombongan(activityId));
        [AccessAs(Acc.Edit)] public IActionResult EditRombongan(string encKey) => EditForm("Rombongan", encKey, () => new VM.EditRombongan(encKey));

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanRombongan([FromForm] VM.NewRombongan m, IFormFile? izin)
        {
            var bytes = await BacaBerkasAsync(izin);
            return Aksi(u => VM.Method.SimpanRombongan(m, bytes, izin, u), "Anggota rombongan disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateRombongan([FromForm] VM.NewRombongan m, IFormFile? izin, bool hapusIzin = false)
        {
            var bytes = await BacaBerkasAsync(izin);
            return Aksi(u => VM.Method.UpdateRombongan(EditKeyTab("Rombongan"), m, bytes, izin, hapusIzin, u), "Anggota rombongan diubah.", KEY + "Rombongan");
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusRombongan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadRombongan(i.Key).Delete(u).Ok(); }, "Anggota rombongan dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult SuratIzin(string key)
        {
            try
            {
                var file = VM.Method.LoadRombongan(key).GetPermissionFile();
                return Berkas(file?.Data, file?.FileName, file?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Template checklist
        [AccessAs(Acc.Index)] public IActionResult ListTemplate() => Bagian(() => new VM.ListTemplate());
        [AccessAs(Acc.Create)] public IActionResult AddTemplate() => Bagian(() => new VM.AddTemplate());
        [AccessAs(Acc.Edit)] public IActionResult EditTemplate(string encKey) => EditForm("Template", encKey, () => new VM.EditTemplate(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanTemplate([FromBody] VM.NewTemplate m) => Aksi(u => VM.Method.SimpanTemplate(m, u), "Template disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateTemplate([FromBody] VM.NewTemplate m) => Aksi(u => VM.Method.UpdateTemplate(EditKeyTab("Template"), m, u), "Template diubah.", KEY + "Template");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusTemplate([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusTemplate(i.Key, u), "Template dinonaktifkan.");
        [AccessAs(Acc.Index)] public IActionResult Item(string encKey) => Bagian(() => new VM.Item(encKey));
        [AccessAs(Acc.Index)] public IActionResult ItemList(string encKey) => Bagian(() => new VM.ItemList(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahItem([FromBody] VM.NewItem m) => Aksi(u => VM.Method.TambahItem(m, u), "Item ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusItem([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusItem(i.Key, u), "Item dihapus.");
    }
}
