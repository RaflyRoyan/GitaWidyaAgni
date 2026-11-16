using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Pengaturan;
using GWALib.Lib.Master.Information;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Pengaturan
{
    /// <summary>
    /// Pengaturan Sistem (/Pengaturan/Pengaturan). Tab: situs, format nomor, alur persetujuan, template pesan,
    /// antrian pesan (Process), log job, log aktivitas, sequence. Logika di Models/Pengaturan/PengaturanVM.Method.
    /// </summary>
    [Area("Pengaturan")]
    public class PengaturanController : BaseController
    {
        private const string KEY_SETTING = "Pengaturan_KEY_EDIT_SETTING";
        private const string KEY_FORMAT = "Pengaturan_KEY_EDIT_FORMAT";
        private const string KEY_FLOW = "Pengaturan_KEY_EDIT_FLOW";
        private const string KEY_STEP = "Pengaturan_KEY_EDIT_STEP";
        private const string KEY_FLOW_STEP = "Pengaturan_KEY_FLOW_STEP";
        private const string KEY_TEMPLATE = "Pengaturan_KEY_EDIT_TEMPLATE";
        private const string KEY_SEQUENCE = "Pengaturan_KEY_EDIT_SEQUENCE";

        public IActionResult Index() => HalamanIndex(new PengaturanVM.Index());

        // ================================================================ Pengaturan situs
        [AccessAs(Acc.Index)]
        public IActionResult ListSetting() => Bagian(() => new PengaturanVM.ListSetting());

        [AccessAs(Acc.Create)]
        public IActionResult AddSetting() => Bagian(() => new PengaturanVM.AddSetting());

        [AccessAs(Acc.Edit)]
        public IActionResult EditSetting(string encKey)
        {
            TempData[KEY_SETTING] = encKey;
            return Bagian(() => new PengaturanVM.EditSetting(encKey, Url.Action("GambarSetting", new { encKey })!));
        }

        // FormData (ada berkas gambar).
        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanSetting([FromForm] PengaturanVM.NewSetting model, IFormFile? image)
        {
            var bytes = await BacaBerkasAsync(image);
            return Aksi(user => PengaturanVM.Method.SimpanSetting(model, bytes, user), "Pengaturan berhasil disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateSetting([FromForm] PengaturanVM.NewSetting model, IFormFile? image)
        {
            var bytes = await BacaBerkasAsync(image);
            return Aksi(user => PengaturanVM.Method.UpdateSetting(EditKey(KEY_SETTING), model, bytes, user), "Pengaturan berhasil diubah.", KEY_SETTING);
        }

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteSetting([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusSetting(input.Key, user), "Pengaturan dihapus.");

        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GambarSetting(string encKey)
        {
            try
            {
                var image = LibExtensions.Load<SiteSetting>(encKey).GetImage();
                return FileOrNotFound(image?.Data, image?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ================================================================ Format nomor
        [AccessAs(Acc.Index)]
        public IActionResult ListFormat() => Bagian(() => new PengaturanVM.ListFormat());

        [AccessAs(Acc.Create)]
        public IActionResult AddFormat() => Bagian(() => new PengaturanVM.AddFormat());

        [AccessAs(Acc.Edit)]
        public IActionResult EditFormat(string encKey)
        {
            TempData[KEY_FORMAT] = encKey;
            return Bagian(() => new PengaturanVM.EditFormat(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanFormat([FromBody] PengaturanVM.NewFormat model)
            => Aksi(user => PengaturanVM.Method.SimpanFormat(model, user), "Format nomor berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateFormat([FromBody] PengaturanVM.NewFormat model)
            => Aksi(user => PengaturanVM.Method.UpdateFormat(EditKey(KEY_FORMAT), model, user), "Format nomor berhasil diubah.", KEY_FORMAT);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteFormat([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusFormat(input.Key, user), "Format nomor dinonaktifkan.");

        // ================================================================ Alur persetujuan + langkah
        [AccessAs(Acc.Index)]
        public IActionResult ListFlow() => Bagian(() => new PengaturanVM.ListFlow());

        [AccessAs(Acc.Create)]
        public IActionResult AddFlow() => Bagian(() => new PengaturanVM.AddFlow());

        [AccessAs(Acc.Edit)]
        public IActionResult EditFlow(string encKey)
        {
            TempData[KEY_FLOW] = encKey;
            return Bagian(() => new PengaturanVM.EditFlow(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanFlow([FromBody] PengaturanVM.NewFlow model)
            => Aksi(user => PengaturanVM.Method.SimpanFlow(model, user), "Alur persetujuan berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateFlow([FromBody] PengaturanVM.NewFlow model)
            => Aksi(user => PengaturanVM.Method.UpdateFlow(EditKey(KEY_FLOW), model, user), "Alur persetujuan berhasil diubah.", KEY_FLOW);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteFlow([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusFlow(input.Key, user), "Alur persetujuan dinonaktifkan.");

        [AccessAs(Acc.Edit)]
        public IActionResult AddStep(string flowKey)
        {
            TempData[KEY_FLOW_STEP] = flowKey;
            return Bagian(() => new PengaturanVM.AddStep(flowKey));
        }

        [AccessAs(Acc.Edit)]
        public IActionResult EditStep(string encKey)
        {
            TempData[KEY_STEP] = encKey;
            return Bagian(() => new PengaturanVM.EditStep(encKey));
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanStep([FromBody] PengaturanVM.NewStep model)
            => Aksi(user => PengaturanVM.Method.SimpanStep(EditKey(KEY_FLOW_STEP), model, user), "Langkah berhasil disimpan.", KEY_FLOW_STEP);

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateStep([FromBody] PengaturanVM.NewStep model)
            => Aksi(user => PengaturanVM.Method.UpdateStep(EditKey(KEY_STEP), model, user), "Langkah berhasil diubah.", KEY_STEP);

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult DeleteStep([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusStep(input.Key, user), "Langkah dihapus.");

        // ================================================================ Template pesan
        [AccessAs(Acc.Index)]
        public IActionResult ListTemplate() => Bagian(() => new PengaturanVM.ListTemplate());

        [AccessAs(Acc.Create)]
        public IActionResult AddTemplate() => Bagian(() => new PengaturanVM.AddTemplate());

        [AccessAs(Acc.Edit)]
        public IActionResult EditTemplate(string encKey)
        {
            TempData[KEY_TEMPLATE] = encKey;
            return Bagian(() => new PengaturanVM.EditTemplate(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanTemplate([FromBody] PengaturanVM.NewTemplate model)
            => Aksi(user => PengaturanVM.Method.SimpanTemplate(model, user), "Template berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateTemplate([FromBody] PengaturanVM.NewTemplate model)
            => Aksi(user => PengaturanVM.Method.UpdateTemplate(EditKey(KEY_TEMPLATE), model, user), "Template berhasil diubah.", KEY_TEMPLATE);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteTemplate([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusTemplate(input.Key, user), "Template dinonaktifkan.");

        // ================================================================ Antrian pesan
        [AccessAs(Acc.Index)]
        public IActionResult ListOutbox() => Bagian(() => new PengaturanVM.ListOutbox());

        [AccessAs(Acc.Index)]
        public IActionResult DetailOutbox(string encKey) => Bagian(() => new PengaturanVM.DetailOutbox(encKey));

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult RetryOutbox([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.Retry(input.Key, user), "Pesan dijadwalkan ulang.");

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult CancelOutbox([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.BatalPesan(input.Key, user), "Pesan dibatalkan.");

        // ================================================================ Log
        [AccessAs(Acc.Index)]
        public IActionResult ListJob() => Bagian(() => new PengaturanVM.ListJob());

        [AccessAs(Acc.Index)]
        public IActionResult ListLog() => Bagian(() => new PengaturanVM.ListLog());

        [AccessAs(Acc.Index)]
        public IActionResult DetailLog(string encKey) => Bagian(() => new PengaturanVM.DetailLog(encKey));

        // ================================================================ Sequence
        [AccessAs(Acc.Index)]
        public IActionResult ListSequence() => Bagian(() => new PengaturanVM.ListSequence());

        [AccessAs(Acc.Edit)]
        public IActionResult EditSequence(string encKey)
        {
            TempData[KEY_SEQUENCE] = encKey;
            return Bagian(() => new PengaturanVM.EditSequence(encKey));
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateSequence([FromBody] PengaturanVM.NewSequence model)
            => Aksi(user => PengaturanVM.Method.UpdateSequence(EditKey(KEY_SEQUENCE), model, user), "Sequence berhasil diubah.", KEY_SEQUENCE);
    }
}
