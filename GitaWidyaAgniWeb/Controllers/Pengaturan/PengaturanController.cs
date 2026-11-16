using DevExtreme.AspNet.Mvc;
using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Pengaturan;
using GWALib.Lib.Master.Information;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Pengaturan
{
    /// <summary>
    /// Pengaturan Sistem. Tab: situs, format nomor, alur persetujuan, template pesan, antrian pesan (Process), log job,
    /// log aktivitas, sequence. Logika di Models/Pengaturan/PengaturanVM.Method.
    /// </summary>
    public class PengaturanController : BaseController
    {
        const string KEY_SETTING = "Pengaturan_Setting_EncryptKey";
        const string KEY_FORMAT = "Pengaturan_Format_EncryptKey";
        const string KEY_FLOW = "Pengaturan_Flow_EncryptKey";
        const string KEY_STEP = "Pengaturan_Step_EncryptKey";
        const string KEY_FLOW_STEP = "Pengaturan_FlowStep_EncryptKey";
        const string KEY_TEMPLATE = "Pengaturan_Template_EncryptKey";
        const string KEY_SEQUENCE = "Pengaturan_Sequence_EncryptKey";

        public IActionResult Index() => HalamanIndex();

        // ================================================================ Pengaturan situs
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetSetting(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetSettings, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult AddSetting() => Modal(() => new PengaturanVM.FormSetting(), "FormSetting");

        [AccessAs(Acc.Edit)]
        public IActionResult EditSetting(string encKey)
        {
            TempData[KEY_SETTING] = encKey;
            return Modal(() => new PengaturanVM.EditSetting(encKey, Url.Action("GambarSetting", new { encKey })!), "FormSetting");
        }

        // FormData (ada berkas gambar).
        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanSetting([FromForm] PengaturanVM.FormSetting input, IFormFile? image)
        {
            var bytes = await BacaBerkasAsync(image);
            return Aksi(user => PengaturanVM.Method.SimpanSetting(input, bytes, user), "Pengaturan tersimpan");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateSetting([FromForm] PengaturanVM.FormSetting input, IFormFile? image)
        {
            var bytes = await BacaBerkasAsync(image);
            return Aksi(user => PengaturanVM.Method.UpdateSetting(TempData[KEY_SETTING]?.ToString(), input, bytes, user), "Pengaturan berhasil diubah", KEY_SETTING);
        }

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteSetting([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusSetting(input.Key, user), "Pengaturan dihapus");

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
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetFormat(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetFormats, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult AddFormat() => Modal(() => new PengaturanVM.FormFormat(), "FormFormat");

        [AccessAs(Acc.Edit)]
        public IActionResult EditFormat(string encKey)
        {
            TempData[KEY_FORMAT] = encKey;
            return Modal(() => new PengaturanVM.EditFormat(encKey), "FormFormat");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanFormat([FromBody] PengaturanVM.FormFormat input)
            => Aksi(user => PengaturanVM.Method.SimpanFormat(input, user), "Format nomor tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateFormat([FromBody] PengaturanVM.FormFormat input)
            => Aksi(user => PengaturanVM.Method.UpdateFormat(TempData[KEY_FORMAT]?.ToString(), input, user), "Format nomor berhasil diubah", KEY_FORMAT);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteFormat([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusFormat(input.Key, user), "Format nomor dinonaktifkan");

        // ================================================================ Alur persetujuan + langkah
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetFlow(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetFlows, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult AddFlow() => Modal(() => new PengaturanVM.FormFlow(), "FormFlow");

        [AccessAs(Acc.Edit)]
        public IActionResult EditFlow(string encKey)
        {
            TempData[KEY_FLOW] = encKey;
            return Modal(() => new PengaturanVM.EditFlow(encKey), "FormFlow");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanFlow([FromBody] PengaturanVM.FormFlow input)
            => Aksi(user => PengaturanVM.Method.SimpanFlow(input, user), "Alur persetujuan tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateFlow([FromBody] PengaturanVM.FormFlow input)
            => Aksi(user => PengaturanVM.Method.UpdateFlow(TempData[KEY_FLOW]?.ToString(), input, user), "Alur persetujuan berhasil diubah", KEY_FLOW);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteFlow([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusFlow(input.Key, user), "Alur persetujuan dinonaktifkan");

        [AccessAs(Acc.Edit)]
        public IActionResult AddStep(string flowKey)
        {
            TempData[KEY_FLOW_STEP] = flowKey;
            return Modal(() => new PengaturanVM.AddStep(flowKey), "FormStep");
        }

        [AccessAs(Acc.Edit)]
        public IActionResult EditStep(string encKey)
        {
            TempData[KEY_STEP] = encKey;
            return Modal(() => new PengaturanVM.EditStep(encKey), "FormStep");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanStep([FromBody] PengaturanVM.FormStep input)
            => Aksi(user => PengaturanVM.Method.SimpanStep(TempData[KEY_FLOW_STEP]?.ToString(), input, user), "Langkah tersimpan", KEY_FLOW_STEP);

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateStep([FromBody] PengaturanVM.FormStep input)
            => Aksi(user => PengaturanVM.Method.UpdateStep(TempData[KEY_STEP]?.ToString(), input, user), "Langkah berhasil diubah", KEY_STEP);

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult DeleteStep([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusStep(input.Key, user), "Langkah dihapus");

        // ================================================================ Template pesan
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetTemplate(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetTemplates, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult AddTemplate() => Modal(() => new PengaturanVM.FormTemplate(), "FormTemplate");

        [AccessAs(Acc.Edit)]
        public IActionResult EditTemplate(string encKey)
        {
            TempData[KEY_TEMPLATE] = encKey;
            return Modal(() => new PengaturanVM.EditTemplate(encKey), "FormTemplate");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanTemplate([FromBody] PengaturanVM.FormTemplate input)
            => Aksi(user => PengaturanVM.Method.SimpanTemplate(input, user), "Template tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateTemplate([FromBody] PengaturanVM.FormTemplate input)
            => Aksi(user => PengaturanVM.Method.UpdateTemplate(TempData[KEY_TEMPLATE]?.ToString(), input, user), "Template berhasil diubah", KEY_TEMPLATE);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteTemplate([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.HapusTemplate(input.Key, user), "Template dinonaktifkan");

        // ================================================================ Antrian pesan
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetOutbox(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetOutbox, loadOptions);

        [AccessAs(Acc.Index)]
        public IActionResult DetailOutbox(string encKey) => Modal(() => new PengaturanVM.DetailOutbox(encKey));

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult RetryOutbox([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.Retry(input.Key, user), "Pesan dijadwalkan ulang");

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult CancelOutbox([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PengaturanVM.Method.BatalPesan(input.Key, user), "Pesan dibatalkan");

        // ================================================================ Log
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetJob(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetJobs, loadOptions);

        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetLog(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetLogs, loadOptions);

        [AccessAs(Acc.Index)]
        public IActionResult DetailLog(string encKey) => Modal(() => new PengaturanVM.DetailLog(encKey));

        // ================================================================ Sequence
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetSequence(DataSourceLoadOptions loadOptions) => Grid(PengaturanVM.Method.GetSequences, loadOptions);

        [AccessAs(Acc.Edit)]
        public IActionResult EditSequence(string encKey)
        {
            TempData[KEY_SEQUENCE] = encKey;
            return Modal(() => new PengaturanVM.FormSequence(encKey), "FormSequence");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateSequence([FromBody] PengaturanVM.FormSequence input)
            => Aksi(user => PengaturanVM.Method.UpdateSequence(TempData[KEY_SEQUENCE]?.ToString(), input, user), "Sequence berhasil diubah", KEY_SEQUENCE);
    }
}
