using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Organisasi;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Organisasi
{
    /// <summary>Organisasi › Periode (/Organisasi/Periode): tab Periode dan tab Serah Terima (submit / approve).</summary>
    [Area("Organisasi")]
    public class PeriodeController : BaseController
    {
        private const string KEY_PERIODE = "Periode_KEY_EDIT";
        private const string KEY_SERAH = "SerahTerima_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new PeriodeVM.Index());

        // ---------- Periode
        [AccessAs(Acc.Index)]
        public IActionResult ListPeriode() => Bagian(() => new PeriodeVM.ListPeriode());

        [AccessAs(Acc.Create)]
        public IActionResult AddPeriode() => Bagian(() => new PeriodeVM.AddPeriode());

        [AccessAs(Acc.Edit)]
        public IActionResult EditPeriode(string encKey)
        {
            TempData[KEY_PERIODE] = encKey;
            return Bagian(() => new PeriodeVM.EditPeriode(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanPeriode([FromBody] PeriodeVM.NewPeriode model)
            => Aksi(user => PeriodeVM.Method.SimpanPeriode(model, user), "Periode berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdatePeriode([FromBody] PeriodeVM.NewPeriode model)
            => Aksi(user => PeriodeVM.Method.UpdatePeriode(EditKey(KEY_PERIODE), model, user), "Periode berhasil diubah.", KEY_PERIODE);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusPeriode([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PeriodeVM.Method.HapusPeriode(input.Key, user), "Periode dihapus.");

        // ---------- Serah terima
        [AccessAs(Acc.Index)]
        public IActionResult ListSerahTerima() => Bagian(() => new PeriodeVM.ListSerahTerima());

        [AccessAs(Acc.Create)]
        public IActionResult AddSerahTerima() => Bagian(() => new PeriodeVM.AddSerahTerima());

        [AccessAs(Acc.Edit)]
        public IActionResult EditSerahTerima(string encKey)
        {
            TempData[KEY_SERAH] = encKey;
            return Bagian(() => new PeriodeVM.EditSerahTerima(encKey));
        }

        // FormData (ada berkas LPJ).
        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanSerahTerima([FromForm] PeriodeVM.NewSerahTerima model, IFormFile? lpj)
        {
            var bytes = await BacaBerkasAsync(lpj);
            return Aksi(user => PeriodeVM.Method.SimpanSerahTerima(model, bytes, lpj?.FileName, user), "Serah terima berhasil disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateSerahTerima([FromForm] PeriodeVM.NewSerahTerima model, IFormFile? lpj, bool hapusLpj = false)
        {
            var bytes = await BacaBerkasAsync(lpj);
            return Aksi(user => PeriodeVM.Method.UpdateSerahTerima(EditKey(KEY_SERAH), model, bytes, lpj?.FileName, hapusLpj, user), "Serah terima berhasil diubah.", KEY_SERAH);
        }

        [AccessAs(Acc.Index)]
        public IActionResult Lpj(string encKey)
        {
            try
            {
                var file = PeriodeVM.Method.Load(encKey).GetLpj();
                return Berkas(file?.Data, file?.FileName);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost, AccessAs(Acc.Submit)]
        public IActionResult AjukanSerahTerima([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PeriodeVM.Method.Load(input.Key).Submit(user).Ok(), "Serah terima diajukan.");

        [HttpPost, AccessAs(Acc.Approve)]
        public IActionResult SetujuiSerahTerima([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PeriodeVM.Method.Load(input.Key).Approve(user).Ok(), "Serah terima disetujui. Periode baru sudah aktif.");

        [HttpPost, AccessAs(Acc.Approve)]
        public IActionResult TolakSerahTerima([FromBody] GwaVM.AlasanInput input)
            => Aksi(user => PeriodeVM.Method.Load(input.Key).Reject(input.Keterangan, user).Ok(), "Serah terima ditolak.");

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusSerahTerima([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PeriodeVM.Method.Load(input.Key).Delete(user).Ok(), "Serah terima dihapus.");

        // ---------- Item serah terima
        [AccessAs(Acc.Index)]
        public IActionResult Item(string encKey) => Bagian(() => new PeriodeVM.ItemIndex(encKey));

        [AccessAs(Acc.Index)]
        public IActionResult ItemList(string encKey) => Bagian(() => new PeriodeVM.ItemList(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult TambahItem([FromBody] PeriodeVM.NewItem model)
            => Aksi(user => PeriodeVM.Method.TambahItem(model, user), "Item ditambahkan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult HapusItem([FromBody] GwaVM.KeyInput input)
            => Aksi(user => PeriodeVM.Method.HapusItem(input.Key, user), "Item dihapus.");
    }
}
