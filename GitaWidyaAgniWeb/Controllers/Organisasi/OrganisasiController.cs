using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models.Organisasi;
using GWALib.Lib.Master.Organization;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Organisasi
{
    /// <summary>Organisasi › Profil Organisasi (/Organisasi/Organisasi): form profil langsung di halaman + foto organisasi.</summary>
    [Area("Organisasi")]
    public class OrganisasiController : BaseController
    {
        private readonly AccessService _access;

        public OrganisasiController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex(new OrganisasiVM.Index());

        // FormData: profil + logo + SK pembina. Nama organisasi dipakai sidebar → cache dibersihkan.
        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> Simpan([FromForm] OrganisasiVM.NewRow model, IFormFile? logo, IFormFile? sk, bool hapusLogo = false, bool hapusSk = false)
        {
            var logoBytes = await BacaBerkasAsync(logo);
            var skBytes = await BacaBerkasAsync(sk);
            return Aksi(user =>
            {
                OrganisasiVM.Method.Simpan(model, logoBytes, hapusLogo, skBytes, sk?.FileName, hapusSk, user);
                _access.ClearAll();
            }, "Profil organisasi tersimpan.");
        }

        [AccessAs(Acc.Index)]
        public IActionResult SkPembina()
        {
            var file = OrganizationProfile.GetAdvisorSkFile();
            return Berkas(file?.Data, file?.FileName, file?.ContentType);
        }

        // ---------- Foto organisasi (dipakai kolase Tentang di landing page)
        [AccessAs(Acc.Index)]
        public IActionResult Foto() => Bagian(() => new OrganisasiVM.Foto());

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahFoto(IFormFile? foto)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(user => OrganisasiVM.Method.TambahFoto(bytes, user), "Foto ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult HapusFoto([FromBody] OrganisasiVM.FotoInput input)
            => Aksi(user => OrganisasiVM.Method.HapusFoto(input.SeqPhoto, user), "Foto dihapus.");
    }
}
