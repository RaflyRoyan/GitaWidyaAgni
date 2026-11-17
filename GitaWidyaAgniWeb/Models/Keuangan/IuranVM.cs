using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Keuangan;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Keuangan
{
    /// <summary>
    /// Keuangan › Iuran: tab Tagihan (t_dues_bill: buat tagihan massal, bayar → transaksi Pemasukan, batal bayar, bebaskan)
    /// dan tab Pengaturan Iuran (m_dues_setting). Format periode tagihan mengikuti frekuensi: yyyy-MM, yyyy-GANJIL/GENAP, yyyy, atau bebas.
    /// </summary>
    public class IuranVM
    {
        public class Index
        {
        }

        // ================= Tagihan
        public class ListTagihan
        {
            public List<TagihanRow> Rows { get; set; } = DuesBill.GetDataList().Select(x => new TagihanRow
            {
                EncDecrKey = x.EncDecrKey,
                Iuran = x.DuesSettingName,
                Anggota = x.MemberName,
                BillPeriod = x.BillPeriod,
                Amount = x.Amount,
                DueDate = Opsi.KeDateTime(x.DueDate),
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                PaidAt = x.PaidAt
            }).OrderByDescending(x => x.BillPeriod).ThenBy(x => x.Anggota).ToList();
        }

        public class TagihanRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Iuran { get; set; } = "";
            public string Anggota { get; set; } = "";
            public string BillPeriod { get; set; } = "";
            public decimal Amount { get; set; }
            public DateTime? DueDate { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? PaidAt { get; set; }
            public bool Terlambat => Status == (int)DuesBillStatus.Unpaid && DueDate.HasValue && DueDate.Value.Date < DateTime.Today;
        }

        public class Generate
        {
            public GenerateInput Row { get; set; } = new();
            public List<GwaVM.OpsiItem> IuranOpsi { get; set; } = DuesSetting.GetDataList(FilterAktif.Aktif)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name + " · " + EnumHelper.Text(x.Frequency) + " · Rp " + x.Amount.ToString("#,##0"))).ToList();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public Generate()
            {
                Row.BillPeriod = DateTime.Today.ToString("yyyy-MM");
                Row.DueDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
            }
        }

        public class GenerateInput
        {
            [Required(ErrorMessage = "Pilih iuran")]
            [Display(Name = "Iuran")]
            public int? DuesSettingId { get; set; }

            [StringLength(20, ErrorMessage = "Periode tagihan maksimal 20 karakter")]
            [Display(Name = "Periode Tagihan (mis. 2026-11, 2026-GANJIL, 2026)")]
            public string? BillPeriod { get; set; }

            [Display(Name = "Jatuh Tempo")]
            public DateTime? DueDate { get; set; }

            [Display(Name = "Anggota (kosong = semua anggota aktif)")]
            public List<int>? MemberIds { get; set; }
        }

        public class EditTagihan
        {
            public string Nama { get; set; } = "";
            public TagihanInput Row { get; set; }
            public EditTagihan(string encKey)
            {
                var obj = LibExtensions.Load<DuesBill>(encKey, "Tagihan");
                if (obj.Status != DuesBillStatus.Unpaid)
                {
                    throw new ArgumentException("Hanya tagihan Belum Dibayar yang bisa diubah.");
                }
                Nama = obj.MemberName + " · " + obj.DuesSettingName + " " + obj.BillPeriod;
                Row = new TagihanInput { Amount = obj.Amount, DueDate = Opsi.KeDateTime(obj.DueDate) };
            }
        }

        public class TagihanInput
        {
            [Range(0, 999999999999, ErrorMessage = "Nominal tidak boleh negatif")]
            [Display(Name = "Nominal (Rp)")]
            public decimal Amount { get; set; }

            [Display(Name = "Jatuh Tempo")]
            public DateTime? DueDate { get; set; }
        }

        // ================= Pengaturan iuran
        public class ListPengaturan
        {
            public List<PengaturanRow> Rows { get; set; } = DuesSetting.GetDataList(FilterAktif.Semua).Select(x => new PengaturanRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Periode = x.Period?.Name,
                Amount = x.Amount,
                Frekuensi = EnumHelper.Text(x.Frequency),
                Kategori = x.Category?.Name,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Name).ToList();
        }

        public class PengaturanRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Periode { get; set; }
            public decimal Amount { get; set; }
            public string Frekuensi { get; set; } = "";
            public string? Kategori { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddPengaturan
        {
            public NewPengaturan Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> FrekuensiOpsi { get; set; } = GwaVM.Opsi<DuesFrequency>();
            public List<GwaVM.OpsiItem> KategoriOpsi { get; set; } = Opsi.Kategori(CashFlowType.Income);
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
            public AddPengaturan() { Row.PeriodId = Opsi.PeriodeAktifId(); }
        }

        public class EditPengaturan : AddPengaturan
        {
            public EditPengaturan(string encKey)
            {
                var obj = LibExtensions.Load<DuesSetting>(encKey, "Pengaturan iuran");
                Row = new NewPengaturan
                {
                    PeriodId = obj.Period?.Id,
                    Name = obj.Name,
                    Amount = obj.Amount,
                    Frequency = (int)obj.Frequency,
                    CategoryId = obj.Category?.Id,
                    IsActive = (int)obj.IsActive
                };
            }
        }

        public class NewPengaturan
        {
            [Required(ErrorMessage = "Nama iuran harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Iuran")]
            public string? Name { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Range(1, 999999999, ErrorMessage = "Nominal harus lebih dari 0")]
            [Display(Name = "Nominal (Rp)")]
            public decimal Amount { get; set; }

            [Display(Name = "Frekuensi")]
            public int Frequency { get; set; } = (int)DuesFrequency.Monthly;

            [Display(Name = "Kategori Pemasukan")]
            public int? CategoryId { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        public class Method
        {
            public static DuesBill LoadTagihan(string? key) => LibExtensions.Load<DuesBill>(key, "Tagihan");

            public static int Buat(GenerateInput input, string user)
                => DuesBill.Generate(input.DuesSettingId ?? 0, string.IsNullOrWhiteSpace(input.BillPeriod) ? null : input.BillPeriod.Trim().ToUpperInvariant(),
                    Opsi.Tanggal(input.DueDate), input.MemberIds is { Count: > 0 } ? input.MemberIds : null, user).Ok();

            public static void UpdateTagihan(string key, TagihanInput input, string user)
            {
                var obj = LoadTagihan(key);
                obj.Amount = input.Amount;
                obj.DueDate = Opsi.Tanggal(input.DueDate);
                obj.Update(user).Ok();
            }

            public static GwaVM.BayarForm FormBayar(string encKey)
            {
                var obj = LoadTagihan(encKey);
                return new GwaVM.BayarForm
                {
                    Judul = obj.MemberName + " · " + obj.DuesSettingName + " " + obj.BillPeriod,
                    Info = "Rp " + obj.Amount.ToString("#,##0") + ". Pembayaran dicatat sebagai transaksi Pemasukan (Diposting).",
                    OnSave = "Iuran.pay()",
                    Tombol = "Catat Bayar",
                    ShowAmount = false,
                    CategoryRequired = false,
                    KategoriOpsi = Opsi.Kategori(CashFlowType.Income),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey }
                };
            }

            public static void Bayar(GwaVM.BayarInput input, string user)
                => LoadTagihan(input.Key).Pay(input.AccountId ?? 0, Opsi.TanggalWajib(input.Date, "Tanggal"), input.CategoryId, user).Ok();

            public static void SimpanPengaturan(NewPengaturan input, string user)
                => DuesSetting.Save(new DuesSetting.NewRow
                {
                    PeriodId = input.PeriodId,
                    Name = input.Name ?? "",
                    Amount = input.Amount,
                    Frequency = (DuesFrequency)input.Frequency,
                    CategoryId = input.CategoryId
                }, user).Ok();

            public static void UpdatePengaturan(string key, NewPengaturan input, string user)
            {
                var obj = LibExtensions.Load<DuesSetting>(key, "Pengaturan iuran");
                obj.Period = input.PeriodId.HasValue ? new ManagementPeriod { Id = input.PeriodId.Value } : null;
                obj.Name = input.Name ?? "";
                obj.Amount = input.Amount;
                obj.Frequency = (DuesFrequency)input.Frequency;
                obj.Category = input.CategoryId.HasValue ? new FinancialCategory { Id = input.CategoryId.Value } : null;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void HapusPengaturan(string? key, string user) => LibExtensions.Load<DuesSetting>(key, "Pengaturan iuran").Delete(user).Ok();
        }
    }
}
