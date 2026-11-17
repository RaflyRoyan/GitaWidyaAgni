using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Transaksi.Information;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>Area Anggota › Notifikasi (/AreaAnggota/Notifikasi): notifikasi in-app milik user login. Lonceng di header menautkan ke sini.</summary>
    public class NotifikasiController : AreaAnggotaController
    {
        public IActionResult Index() => HalamanIndex();

        public IActionResult List(bool belumDibaca = false) => Bagian(() => Notification.GetMyList(LibUser, belumDibaca, 0, 100));

        [HttpPost]
        public IActionResult Baca([FromBody] GwaVM.KeyInput i)
            => Aksi(u => { LibExtensions.Load<Notification>(i.Key, "Notifikasi").MarkRead(u).Ok(); }, "Ditandai sudah dibaca.");

        [HttpPost]
        public IActionResult BacaSemua() => Aksi(u => Notification.MarkAllRead(u).Ok(), "Semua notifikasi ditandai sudah dibaca.");

        [HttpPost]
        public IActionResult Hapus([FromBody] GwaVM.KeyInput i)
            => Aksi(u => { LibExtensions.Load<Notification>(i.Key, "Notifikasi").Delete(u).Ok(); }, "Notifikasi dihapus.");
    }
}
