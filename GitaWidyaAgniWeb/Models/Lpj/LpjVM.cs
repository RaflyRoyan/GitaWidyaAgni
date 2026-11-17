using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Information;
using GWALib.Lib.Master.Organization;
using GWAShared.Enums;
using System.ComponentModel.DataAnnotations;
using LibLpj = GWALib.Lib.Transaksi.Program.Lpj;

namespace GitaWidyaAgniWeb.Models.Lpj
{
    /// <summary>
    /// LPJ (t_lpj): LPJ Kegiatan (kegiatan induk) dan LPJ Kepengurusan (periode). Halaman ini mengisi bagian naratif;
    /// data lain diambil dari modul lain saat dokumen dibuat (LpjReport). Draft → Final (PDF disimpan) → Buka lagi.
    /// </summary>
    public class LpjVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Method.GetList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public int Jenis { get; set; }
            public string JenisText { get; set; } = "";
            public string? Referensi { get; set; }
            public string Title { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime Diperbarui { get; set; }
            public DateTime? FinalizedAt { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = new();
            public bool Kegiatan => Row.Jenis == (int)LpjType.Kegiatan;

            public Add()
            {
            }

            public Add(int jenis)
            {
                Row.Jenis = jenis == (int)LpjType.Kepengurusan ? (int)LpjType.Kepengurusan : (int)LpjType.Kegiatan;
                var used = LibLpj.GetDataList();
                if (Kegiatan)
                {
                    var taken = used.Where(x => x.ActivityId != null).Select(x => x.ActivityId!.Value).ToHashSet();
                    KegiatanOpsi = GWALib.Lib.Transaksi.Program.Activity.GetDataList()
                        .Where(x => x.ParentActivityId == null && !taken.Contains(x.Id))
                        .OrderByDescending(x => x.StartAt)
                        .Select(x => new GwaVM.OpsiItem(x.Id, x.Title + " · " + x.StartAt.ToString("dd MMM yyyy")))
                        .ToList();
                }
                else
                {
                    var taken = used.Where(x => x.PeriodId != null).Select(x => x.PeriodId!.Value).ToHashSet();
                    PeriodeOpsi = Opsi.Periode().Where(x => !taken.Contains(x.Value)).ToList();
                    Row.PeriodId = PeriodeOpsi.Any(x => x.Value == Opsi.PeriodeAktifId()) ? Opsi.PeriodeAktifId() : null;
                }
            }
        }

        public class Edit : Add
        {
            public string Referensi { get; set; } = "";
            public string StatusText { get; set; } = "";

            public Edit(string encKey)
            {
                var obj = Method.Load(encKey);
                Referensi = obj.ActivityTitle ?? obj.PeriodName ?? "";
                StatusText = EnumHelper.Text(obj.Status);
                Row = new NewRow
                {
                    Jenis = (int)obj.LpjType,
                    ActivityId = obj.ActivityId,
                    PeriodId = obj.PeriodId,
                    Title = obj.Title,
                    Introduction = obj.Introduction,
                    Background = obj.Background,
                    ActivityKind = obj.ActivityKind,
                    Purpose = obj.Purpose,
                    ActivityForm = obj.ActivityForm,
                    VisionMission = obj.VisionMission,
                    Closing = obj.Closing
                };
            }
        }

        public class NewRow
        {
            public int Jenis { get; set; }

            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Judul LPJ")]
            public string? Title { get; set; }

            [Display(Name = "Pendahuluan")]
            public string? Introduction { get; set; }

            [Display(Name = "Latar Belakang")]
            public string? Background { get; set; }

            [Display(Name = "Macam Kegiatan")]
            public string? ActivityKind { get; set; }

            [Display(Name = "Maksud dan Tujuan")]
            public string? Purpose { get; set; }

            [Display(Name = "Bentuk Kegiatan")]
            public string? ActivityForm { get; set; }

            [Display(Name = "Visi dan Misi")]
            public string? VisionMission { get; set; }

            [Display(Name = "Penutup")]
            public string? Closing { get; set; }
        }

        public class Method
        {

            public static LibLpj Load(string? key) => LibExtensions.Load<LibLpj>(key, "LPJ");

            public static List<Row> GetList()
            {
                return LibLpj.GetDataList().Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Jenis = (int)x.LpjType,
                    JenisText = EnumHelper.Text(x.LpjType),
                    Referensi = x.ActivityTitle ?? x.PeriodName,
                    Title = x.Title,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    Diperbarui = x.UpdDate ?? x.InsDate,
                    FinalizedAt = x.FinalizedAt
                }).ToList();
            }

            public static void Simpan(NewRow input, string user)
            {
                LibLpj.Save(new LibLpj.NewRow
                {
                    LpjType = (LpjType)input.Jenis,
                    ActivityId = input.ActivityId,
                    PeriodId = input.PeriodId,
                    Title = input.Title,
                    Introduction = input.Introduction,
                    Background = input.Background,
                    ActivityKind = input.ActivityKind,
                    Purpose = input.Purpose,
                    ActivityForm = input.ActivityForm,
                    VisionMission = input.VisionMission,
                    Closing = input.Closing
                }, user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = Load(encKey);
                obj.Title = input.Title ?? "";
                obj.Introduction = input.Introduction;
                obj.Background = input.Background;
                obj.ActivityKind = input.ActivityKind;
                obj.Purpose = input.Purpose;
                obj.ActivityForm = input.ActivityForm;
                obj.VisionMission = input.VisionMission;
                obj.Closing = input.Closing;
                obj.Update(user).Ok();
            }

            /// <summary>Dokumen LPJ dari data terkini; logo dan kota tanda tangan dari ReportBrand.</summary>
            public static LpjReport BuildReport(LibLpj lpj, string webRoot)
            {
                var brand = ReportBrand.Load(webRoot);
                return lpj.LpjType == LpjType.Kegiatan
                    ? LpjReport.Kegiatan(lpj.GetKegiatanData(), brand)
                    : LpjReport.Kepengurusan(lpj.GetKepengurusanData(), brand);
            }

            public static byte[] ToPdf(LpjReport report)
            {
                using var ms = new MemoryStream();
                report.ExportToPdf(ms);
                return ms.ToArray();
            }
        }
    }
}
