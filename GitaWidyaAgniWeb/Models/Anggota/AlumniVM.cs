using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Anggota
{
    /// <summary>Anggota › Alumni: profil alumni (m_alumni_profile), hanya untuk anggota berstatus Alumni (satu profil per anggota).</summary>
    public class AlumniVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Method.GetDataList().ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public short? Angkatan { get; set; }
            public short? GraduationYear { get; set; }
            public string? CurrentJob { get; set; }
            public string? Company { get; set; }
            public string? Industry { get; set; }
            public string? City { get; set; }
            public string? LinkedinUrl { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public bool BisaDihubungi { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; }
            public List<GwaVM.OpsiItem> KontakOpsi { get; set; } = GwaVM.Opsi<ContactPermission>();

            public Add()
            {
                // Anggota berstatus Alumni yang belum punya profil.
                var sudah = AlumniProfile.GetDataList().Select(x => x.Member.Id).ToHashSet();
                AnggotaOpsi = Opsi.Anggota(MemberStatus.Alumni).Where(x => !sudah.Contains(x.Value)).ToList();
            }
        }

        public class Edit : Add
        {
            public string Nama { get; set; } = "";

            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<AlumniProfile>(encKey, "Profil alumni");
                Nama = obj.Member.Fullname;
                Row = new NewRow
                {
                    MemberId = obj.Member.Id,
                    GraduationYear = obj.GraduationYear,
                    CurrentJob = obj.CurrentJob,
                    Company = obj.Company,
                    Industry = obj.Industry,
                    City = obj.City,
                    LinkedinUrl = obj.LinkedinUrl,
                    IsContactable = (int)obj.IsContactable,
                    Notes = obj.Notes
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Pilih anggota alumni")]
            [Display(Name = "Anggota (status Alumni)")]
            public int? MemberId { get; set; }

            [Range(1990, 2100, ErrorMessage = "Tahun lulus tidak valid")]
            [Display(Name = "Tahun Lulus")]
            public short? GraduationYear { get; set; }

            [StringLength(100, ErrorMessage = "Pekerjaan maksimal 100 karakter")]
            [Display(Name = "Pekerjaan Saat Ini")]
            public string? CurrentJob { get; set; }

            [StringLength(150, ErrorMessage = "Perusahaan maksimal 150 karakter")]
            [Display(Name = "Perusahaan / Instansi")]
            public string? Company { get; set; }

            [StringLength(100, ErrorMessage = "Bidang maksimal 100 karakter")]
            [Display(Name = "Bidang Industri")]
            public string? Industry { get; set; }

            [StringLength(100, ErrorMessage = "Kota maksimal 100 karakter")]
            [Display(Name = "Kota")]
            public string? City { get; set; }

            [StringLength(255, ErrorMessage = "URL maksimal 255 karakter")]
            [Display(Name = "LinkedIn")]
            public string? LinkedinUrl { get; set; }

            [Display(Name = "Bersedia Dihubungi")]
            public int IsContactable { get; set; } = (int)ContactPermission.TidakBersedia;

            [StringLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                return AlumniProfile.GetDataList().Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Nama = x.Member.Fullname,
                    Angkatan = x.Member.Angkatan,
                    GraduationYear = x.GraduationYear,
                    CurrentJob = x.CurrentJob,
                    Company = x.Company,
                    Industry = x.Industry,
                    City = x.City,
                    LinkedinUrl = x.LinkedinUrl,
                    Phone = x.Member.Phone,
                    Email = x.Member.Email,
                    BisaDihubungi = x.IsContactable == ContactPermission.Bersedia
                }).OrderBy(x => x.Nama);
            }

            public static void Simpan(NewRow input, string user)
            {
                AlumniProfile.Save(new AlumniProfile.NewRow
                {
                    MemberId = input.MemberId ?? 0,
                    GraduationYear = input.GraduationYear,
                    CurrentJob = input.CurrentJob,
                    Company = input.Company,
                    Industry = input.Industry,
                    City = input.City,
                    LinkedinUrl = input.LinkedinUrl,
                    IsContactable = (ContactPermission)input.IsContactable,
                    Notes = input.Notes
                }, user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = LibExtensions.Load<AlumniProfile>(encKey, "Profil alumni");
                obj.GraduationYear = input.GraduationYear;
                obj.CurrentJob = input.CurrentJob;
                obj.Company = input.Company;
                obj.Industry = input.Industry;
                obj.City = input.City;
                obj.LinkedinUrl = input.LinkedinUrl;
                obj.IsContactable = (ContactPermission)input.IsContactable;
                obj.Notes = input.Notes;
                obj.Update(user).Ok();
            }

            public static void Hapus(string? encKey, string user)
            {
                LibExtensions.Load<AlumniProfile>(encKey, "Profil alumni").Delete(user).Ok();
            }
        }
    }
}
