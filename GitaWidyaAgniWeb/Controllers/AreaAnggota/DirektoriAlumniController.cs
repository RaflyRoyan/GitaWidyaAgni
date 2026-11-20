using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Direktori Alumni (/AreaAnggota/DirektoriAlumni): daftar alumni untuk semua user login (kontak dan pekerjaan
    /// hanya bila alumni bersedia dihubungi), dan form "Profil alumni saya" bila user adalah anggota berstatus Alumni.
    /// </summary>
    public class DirektoriAlumniController : AreaAnggotaController
    {
        public class IndexVM
        {
            public string? Cari { get; set; }
            public short? Angkatan { get; set; }
            public List<AlumniProfile.DirectoryRow> Rows { get; set; } = new();
            public List<short> AngkatanOpsi { get; set; } = new();
            public int Total { get; set; }
            /// <summary>Profil alumni user login; null = bukan alumni (form tidak tampil).</summary>
            public AlumniProfile.MineRow? Saya { get; set; }
        }

        public class ProfilInput
        {
            public short? GraduationYear { get; set; }
            public string? CurrentJob { get; set; }
            public string? Company { get; set; }
            public string? Industry { get; set; }
            public string? City { get; set; }
            public string? LinkedinUrl { get; set; }
            public bool Bersedia { get; set; }
        }

        public IActionResult Index(string? cari, short? angkatan)
        {
            var semua = AlumniProfile.GetDirectory(LibUser);
            return HalamanIndex(new IndexVM
            {
                Cari = cari,
                Angkatan = angkatan,
                Rows = string.IsNullOrWhiteSpace(cari) && angkatan == null ? semua : AlumniProfile.GetDirectory(LibUser, cari, angkatan),
                AngkatanOpsi = semua.Where(x => x.Angkatan != null).Select(x => x.Angkatan!.Value).Distinct().OrderByDescending(x => x).ToList(),
                Total = semua.Count,
                Saya = AlumniProfile.GetMine(LibUser)
            });
        }

        // Alumni mengubah profilnya sendiri (anggota diambil dari user login di GWALib).
        [HttpPost]
        public IActionResult SimpanProfil([FromBody] ProfilInput m) => Aksi(u =>
        {
            AlumniProfile.SaveMine(new AlumniProfile.MineRow
            {
                GraduationYear = m.GraduationYear,
                CurrentJob = m.CurrentJob,
                Company = m.Company,
                Industry = m.Industry,
                City = m.City,
                LinkedinUrl = m.LinkedinUrl,
                IsContactable = m.Bersedia ? ContactPermission.Bersedia : ContactPermission.TidakBersedia
            }, u).Ok();
        }, "Profil alumni disimpan.");
    }
}
