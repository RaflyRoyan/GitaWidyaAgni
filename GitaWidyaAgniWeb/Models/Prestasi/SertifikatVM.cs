using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Prestasi;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Prestasi
{
    /// <summary>
    /// Prestasi › Sertifikat: sertifikat anggota dengan nomor (format CERTIFICATE) dan kode verifikasi publik.
    /// Peserta / Panitia / Pemateri wajib kegiatan, Pengurus wajib jabatan anggota itu, Prestasi wajib prestasi anggota itu. Terbit ↔ Dicabut.
    /// </summary>
    public class SertifikatVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Certificate.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                CertificateNumber = x.CertificateNumber,
                MemberName = x.MemberName,
                Jenis = EnumHelper.Text(x.CertificateType),
                Title = x.Title,
                IssuedDate = x.IssuedDate.ToDateTime(TimeOnly.MinValue),
                VerificationCode = x.VerificationCode,
                FileName = x.FileName,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderByDescending(x => x.IssuedDate).ThenByDescending(x => x.CertificateNumber).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string CertificateNumber { get; set; } = "";
            public string MemberName { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string Title { get; set; } = "";
            public DateTime IssuedDate { get; set; }
            public string VerificationCode { get; set; } = "";
            public string? FileName { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class Add
        {
            public NewRow Row { get; set; } = new() { IssuedDate = DateTime.Today };
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota(null);
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<CertificateType>();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
        }

        public class Edit
        {
            public NewRow Row { get; set; }
            public string Info { get; set; }
            public string? FileName { get; set; }
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Certificate>(encKey, "Sertifikat");
                Info = obj.CertificateNumber + " · " + obj.MemberName + " · " + EnumHelper.Text(obj.CertificateType) + " · kode " + obj.VerificationCode;
                FileName = obj.FileName;
                Row = new NewRow { MemberId = obj.MemberId, CertificateType = (int)obj.CertificateType, Title = obj.Title, IssuedDate = obj.IssuedDate.ToDateTime(TimeOnly.MinValue) };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Pilih anggota")]
            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Display(Name = "Jenis")]
            public int CertificateType { get; set; } = (int)GWAShared.Enums.CertificateType.Peserta;

            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Jabatan Pengurus")]
            public int? AssignmentId { get; set; }

            [Display(Name = "Prestasi")]
            public int? AchievementId { get; set; }

            [StringLength(100, ErrorMessage = "Nomor maksimal 100 karakter")]
            [Display(Name = "Nomor (kosong = otomatis)")]
            public string? CertificateNumber { get; set; }

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(255, ErrorMessage = "Judul maksimal 255 karakter")]
            [Display(Name = "Judul Sertifikat")]
            public string? Title { get; set; }

            [Required(ErrorMessage = "Tanggal terbit harus diisi")]
            [Display(Name = "Tanggal Terbit")]
            public DateTime? IssuedDate { get; set; }
        }

        public class Method
        {
            public static Certificate Load(string? key) => LibExtensions.Load<Certificate>(key, "Sertifikat");

            // Jabatan & prestasi milik anggota, untuk referensi sertifikat Pengurus / Prestasi.
            public static object OpsiReferensi(int memberId) => new
            {
                Pengurus = ManagementAssignment.GetDataList(memberId: memberId)
                    .Select(x => new GwaVM.OpsiItem(x.Id, x.PositionName + (x.DivisionName != null ? " " + x.DivisionName : "") + " · " + x.PeriodName)).ToList(),
                Prestasi = Achievement.GetDataList(memberId: memberId, jmldata: -1)
                    .Select(x => new GwaVM.OpsiItem(x.Id, x.Title + (x.RankTitle != null ? " · " + x.RankTitle : "") + " · " + x.AchievedDate.ToString("dd MMM yyyy"))).ToList()
            };

            public static void Simpan(NewRow input, byte[]? file, string? fileName, string user)
                => Certificate.Save(new Certificate.NewRow
                {
                    MemberId = input.MemberId ?? 0,
                    CertificateType = (CertificateType)input.CertificateType,
                    ActivityId = input.ActivityId,
                    AssignmentId = input.AssignmentId,
                    AchievementId = input.AchievementId,
                    CertificateNumber = input.CertificateNumber,
                    Title = input.Title ?? "",
                    IssuedDate = Opsi.TanggalWajib(input.IssuedDate, "Tanggal terbit"),
                    FileData = file,
                    FileName = fileName
                }, user).Ok();

            public static void Update(string key, NewRow input, byte[]? file, string? fileName, bool hapusFile, string user)
            {
                var obj = Load(key);
                obj.Title = input.Title ?? "";
                obj.IssuedDate = Opsi.TanggalWajib(input.IssuedDate, "Tanggal terbit");
                obj.Update(user).Ok();
                if (file != null || hapusFile)
                {
                    obj.UpdateFile(file, fileName, user).Ok();
                }
            }
        }
    }
}
