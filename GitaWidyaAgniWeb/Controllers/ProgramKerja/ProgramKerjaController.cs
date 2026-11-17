using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.ProgramKerja;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.ProgramKerja.ProgramKerjaVM;

namespace GitaWidyaAgniWeb.Controllers.ProgramKerja
{
    /// <summary>Program Kerja (/ProgramKerja/ProgramKerja): tab Program, Tugas, Timeline.</summary>
    [Area("ProgramKerja")]
    public class ProgramKerjaController : BaseController
    {
        private const string KEY_PROGRAM = "Program_KEY_EDIT";
        private const string KEY_TUGAS = "Tugas_KEY_EDIT";
        private const string KEY_TIMELINE = "Timeline_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Program
        [AccessAs(Acc.Index)] public IActionResult ListProgram() => Bagian(() => new VM.ListProgram());
        [AccessAs(Acc.Create)] public IActionResult AddProgram() => Bagian(() => new VM.AddProgram());
        [AccessAs(Acc.Edit)]
        public IActionResult EditProgram(string encKey) { TempData[KEY_PROGRAM] = encKey; return Bagian(() => new VM.EditProgram(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanProgram([FromBody] VM.NewProgram model) => Aksi(user => VM.Method.SimpanProgram(model, user), "Program berhasil disimpan.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateProgram([FromBody] VM.NewProgram model) => Aksi(user => VM.Method.UpdateProgram(EditKey(KEY_PROGRAM), model, user), "Program berhasil diubah.", KEY_PROGRAM);
        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult StatusProgram([FromBody] GwaVM.StatusInput input) => Aksi(user => VM.Method.StatusProgram(input, user), "Status program diperbarui.");
        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusProgram([FromBody] GwaVM.KeyInput input) => Aksi(user => VM.Method.HapusProgram(input.Key, user), "Program dihapus.");

        // ---------- Tugas
        [AccessAs(Acc.Index)] public IActionResult ListTugas() => Bagian(() => new VM.ListTugas());
        [AccessAs(Acc.Create)] public IActionResult AddTugas() => Bagian(() => new VM.AddTugas());
        [AccessAs(Acc.Edit)]
        public IActionResult EditTugas(string encKey) { TempData[KEY_TUGAS] = encKey; return Bagian(() => new VM.EditTugas(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanTugas([FromBody] VM.NewTugas model) => Aksi(user => VM.Method.SimpanTugas(model, user), "Tugas berhasil disimpan.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateTugas([FromBody] VM.NewTugas model) => Aksi(user => VM.Method.UpdateTugas(EditKey(KEY_TUGAS), model, user), "Tugas berhasil diubah.", KEY_TUGAS);
        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult StatusTugas([FromBody] GwaVM.StatusInput input) => Aksi(user => VM.Method.StatusTugas(input, user), "Status tugas diperbarui.");
        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusTugas([FromBody] GwaVM.KeyInput input) => Aksi(user => VM.Method.HapusTugas(input.Key, user), "Tugas dihapus.");

        // ---------- Timeline
        [AccessAs(Acc.Index)] public IActionResult ListTimeline() => Bagian(() => new VM.ListTimeline());
        [AccessAs(Acc.Create)] public IActionResult AddTimeline() => Bagian(() => new VM.AddTimeline());
        [AccessAs(Acc.Edit)]
        public IActionResult EditTimeline(string encKey) { TempData[KEY_TIMELINE] = encKey; return Bagian(() => new VM.EditTimeline(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanTimeline([FromBody] VM.NewTimeline model) => Aksi(user => VM.Method.SimpanTimeline(model, user), "Rencana timeline disimpan.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateTimeline([FromBody] VM.NewTimeline model) => Aksi(user => VM.Method.UpdateTimeline(EditKey(KEY_TIMELINE), model, user), "Rencana timeline diubah.", KEY_TIMELINE);
        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult StatusTimeline([FromBody] GwaVM.StatusInput input) => Aksi(user => VM.Method.StatusTimeline(input, user), "Status timeline diperbarui.");
        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult TautKegiatan([FromBody] VM.TautInput input) => Aksi(user => VM.Method.TautKegiatan(input, user), "Kegiatan ditautkan; status menjadi Terjadwal.");
        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusTimeline([FromBody] GwaVM.KeyInput input) => Aksi(user => VM.Method.HapusTimeline(input.Key, user), "Rencana timeline dihapus.");

        // Pilihan kegiatan untuk dialog tautkan (JSON).
        [AccessAs(Acc.Index)]
        public IActionResult OpsiKegiatan() => Json(Opsi.Kegiatan(true));
    }
}
