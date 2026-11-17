using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Transaksi.Surat;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>Area Anggota › Disposisi (/AreaAnggota/DisposisiSaya): disposisi surat yang ditujukan ke anggota login; tandai selesai.</summary>
    public class DisposisiSayaController : AreaAnggotaController
    {
        public IActionResult Index()
        {
            var saya = SayaAtauNull();
            var rows = saya == null ? null : LetterDisposition.GetDataList(toMemberId: saya.Id)
                .OrderBy(x => x.Status).ThenBy(x => x.DueDate ?? DateOnly.MaxValue).ToList();
            return HalamanIndex(rows);
        }

        [HttpPost]
        public IActionResult Selesai([FromBody] GwaVM.KeyInput i) => Aksi(u =>
        {
            var d = LibExtensions.Load<LetterDisposition>(i.Key, "Disposisi");
            if (d.ToMemberId != Saya().Id)
            {
                throw new ArgumentException(ACCESS_DENIED_MESSAGE);
            }
            d.MarkDone(u).Ok();
        }, "Disposisi ditandai selesai.");
    }
}
