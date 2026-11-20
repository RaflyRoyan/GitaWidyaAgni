using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Anggota;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Anggota
{
    /// <summary>Anggota › Alumni (/Anggota/Alumni): profil alumni.</summary>
    [Area("Anggota")]
    public class AlumniController : BaseController
    {
        private const string KEY_EDIT = "Alumni_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new AlumniVM.Index());

        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new AlumniVM.List());

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new AlumniVM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new AlumniVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] AlumniVM.NewRow model)
            => Aksi(user => AlumniVM.Method.Simpan(model, user), "Profil alumni berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] AlumniVM.NewRow model)
            => Aksi(user => AlumniVM.Method.Update(EditKey(KEY_EDIT), model, user), "Profil alumni berhasil diubah.", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => AlumniVM.Method.Hapus(input.Key, user), "Profil alumni dihapus.");

        // ---------- Undangan alumni (mis. reuni)
        [AccessAs(Acc.Process)]
        public IActionResult Undang() => Bagian(() => new AlumniVM.Undang());

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult KirimUndangan([FromBody] AlumniVM.UndangInput m) => Aksi(user =>
        {
            var r = GWALib.Lib.Master.Anggota.AlumniProfile.Invite(new GWALib.Lib.Master.Anggota.AlumniProfile.InviteRow
            {
                Title = m.Judul ?? "",
                Message = m.Pesan ?? "",
                ActivityId = m.Kegiatan,
                Angkatan = m.Angkatan,
                SendWhatsApp = m.KirimWa,
                BaseUrl = AsetLabel.BaseUrl(Request, HttpContext.RequestServices.GetRequiredService<IConfiguration>())
            }, user).Ok();
            return $"Undangan untuk {r.Alumni} alumni: {r.InApp} notifikasi aplikasi, {r.WhatsApp} WhatsApp dijadwalkan.";
        }, "Undangan terkirim.");
    }
}
