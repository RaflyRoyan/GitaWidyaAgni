using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Keuangan;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Keuangan
{
    /// <summary>
    /// Keuangan › Transaksi: tab Transaksi (t_financial_transaction: Draft → Diposting → Dibatalkan + lampiran),
    /// tab Saldo Periode (t_period_balance: buka / segarkan / tutup / buka lagi), tab Akun Kas dan Kategori (master).
    /// Transaksi dari dokumen lain (pengajuan dana, iuran, hutang, pengadaan, penjualan, peminjaman) hanya bisa dibatalkan dari dokumennya.
    /// </summary>
    public partial class TransaksiVM
    {
        public class Index
        {
        }

        // ================= Transaksi
        public class ListTransaksi
        {
            public List<TransaksiRow> Rows { get; set; } = Method.GetTransaksiList().ToList();
        }

        public class TransaksiRow
        {
            public string EncDecrKey { get; set; } = "";
            public string TransactionNumber { get; set; } = "";
            public DateTime TransactionDate { get; set; }
            public int Jenis { get; set; }
            public string JenisText { get; set; } = "";
            public string Akun { get; set; } = "";
            public string Kategori { get; set; } = "";
            public string? Periode { get; set; }
            public decimal Amount { get; set; }
            public decimal Masuk => Jenis == (int)CashFlowType.Income ? Amount : 0;
            public decimal Keluar => Jenis == (int)CashFlowType.Expense ? Amount : 0;
            public string? Description { get; set; }
            public string? Sumber { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int JumlahLampiran { get; set; }
            public bool Aktif => Status != (int)TransactionStatus.Void;
        }

        public class AddTransaksi
        {
            public NewTransaksi Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<CashFlowType>();
            public List<GwaVM.OpsiItem> AkunOpsi { get; set; } = Opsi.Akun();
            public List<GwaVM.OpsiItem> MasukOpsi { get; set; } = Opsi.Kategori(CashFlowType.Income);
            public List<GwaVM.OpsiItem> KeluarOpsi { get; set; } = Opsi.Kategori(CashFlowType.Expense);
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AnggaranOpsi { get; set; } = Opsi.MataAnggaran();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program();
            public List<GwaVM.OpsiItem> LombaOpsi { get; set; } = Opsi.Lomba();

            public AddTransaksi()
            {
                Row.TransactionDate = DateTime.Today;
                Row.PeriodId = Opsi.PeriodeAktifId();
            }
        }

        public class EditTransaksi : AddTransaksi
        {
            public string Nomor { get; set; } = "";
            public EditTransaksi(string encKey)
            {
                var obj = LibExtensions.Load<FinancialTransaction>(encKey, "Transaksi");
                if (obj.Status != TransactionStatus.Draft)
                {
                    throw new ArgumentException("Hanya transaksi Draft yang bisa diubah.");
                }
                Nomor = obj.TransactionNumber;
                Row = new NewTransaksi
                {
                    TransactionType = (int)obj.TransactionType,
                    AccountId = obj.AccountId,
                    CategoryId = obj.CategoryId,
                    PeriodId = obj.PeriodId,
                    Amount = obj.Amount,
                    TransactionDate = Opsi.KeDateTime(obj.TransactionDate),
                    Description = obj.Description,
                    BudgetItemId = obj.BudgetItemId,
                    ActivityId = obj.ActivityId,
                    ProgramId = obj.ProgramId,
                    CompetitionId = obj.CompetitionId
                };
            }
        }

        public class NewTransaksi
        {
            [Display(Name = "Jenis")]
            public int TransactionType { get; set; } = (int)CashFlowType.Expense;

            [Required(ErrorMessage = "Pilih akun kas")]
            [Display(Name = "Akun Kas")]
            public int? AccountId { get; set; }

            [Required(ErrorMessage = "Pilih kategori")]
            [Display(Name = "Kategori")]
            public int? CategoryId { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Range(1, 999999999999, ErrorMessage = "Nominal harus lebih dari 0")]
            [Display(Name = "Nominal (Rp)")]
            public decimal Amount { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal")]
            public DateTime? TransactionDate { get; set; }

            [StringLength(500, ErrorMessage = "Keterangan maksimal 500 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Mata Anggaran")]
            public int? BudgetItemId { get; set; }

            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Program")]
            public int? ProgramId { get; set; }

            [Display(Name = "Lomba")]
            public int? CompetitionId { get; set; }
        }

        // Lampiran transaksi (modal _Lampiran).
        public class Lampiran
        {
            public string Key { get; set; } = "";
            public string Nomor { get; set; } = "";
            public bool BisaUbah { get; set; }
            public List<LampiranRow> Rows { get; set; }

            public Lampiran(string encKey)
            {
                var obj = LibExtensions.Load<FinancialTransaction>(encKey, "Transaksi");
                Key = obj.EncDecrKey;
                Nomor = obj.TransactionNumber;
                BisaUbah = obj.Status != TransactionStatus.Void;
                Rows = obj.GetAttachments(true).Select(x => new LampiranRow { EncDecrKey = x.EncDecrKey, FileName = x.FileName, InsDate = x.InsDate }).ToList();
            }
        }

        public class LampiranRow
        {
            public string EncDecrKey { get; set; } = "";
            public string FileName { get; set; } = "";
            public DateTime InsDate { get; set; }
        }

        // ================= Saldo periode
        public class ListSaldo
        {
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode();
            public List<SaldoRow> Rows { get; set; } = PeriodBalance.GetDataList().Select(x => new SaldoRow
            {
                EncDecrKey = x.EncDecrKey,
                Periode = x.PeriodName,
                Akun = x.AccountName,
                OpeningBalance = x.OpeningBalance,
                TotalIncome = x.TotalIncome,
                TotalExpense = x.TotalExpense,
                ClosingBalance = x.ClosingBalance,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                ClosedAt = x.ClosedAt
            }).OrderByDescending(x => x.Periode).ThenBy(x => x.Akun).ToList();
        }

        public class SaldoRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Periode { get; set; } = "";
            public string Akun { get; set; } = "";
            public decimal OpeningBalance { get; set; }
            public decimal TotalIncome { get; set; }
            public decimal TotalExpense { get; set; }
            public decimal SaldoBerjalan => OpeningBalance + TotalIncome - TotalExpense;
            public decimal? ClosingBalance { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? ClosedAt { get; set; }
        }

        public class BukaSaldoInput
        {
            public int PeriodId { get; set; }
            public int? AccountId { get; set; }
        }

        // ================= Akun kas
        public class ListAkun
        {
            public List<AkunRow> Rows { get; set; } = FinancialAccount.GetDataList(FilterAktif.Semua).Select(x => new AkunRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Jenis = EnumHelper.Text(x.AccountType),
                BankName = x.BankName,
                AccountNumber = x.AccountNumber,
                AccountHolder = x.AccountHolder,
                OpeningBalance = x.OpeningBalance,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Name).ToList();
        }

        public class AkunRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string? BankName { get; set; }
            public string? AccountNumber { get; set; }
            public string? AccountHolder { get; set; }
            public decimal OpeningBalance { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddAkun
        {
            public NewAkun Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<FinancialAccountType>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class EditAkun : AddAkun
        {
            public EditAkun(string encKey)
            {
                var obj = LibExtensions.Load<FinancialAccount>(encKey, "Akun kas");
                Row = new NewAkun
                {
                    Name = obj.Name,
                    AccountType = (int)obj.AccountType,
                    BankName = obj.BankName,
                    AccountNumber = obj.AccountNumber,
                    AccountHolder = obj.AccountHolder,
                    OpeningBalance = obj.OpeningBalance,
                    IsActive = (int)obj.IsActive
                };
            }
        }

        public class NewAkun
        {
            [Required(ErrorMessage = "Nama akun harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Akun")]
            public string? Name { get; set; }

            [Display(Name = "Jenis")]
            public int AccountType { get; set; } = (int)FinancialAccountType.Cash;

            [StringLength(100, ErrorMessage = "Nama bank maksimal 100 karakter")]
            [Display(Name = "Bank / Penyedia")]
            public string? BankName { get; set; }

            [StringLength(50, ErrorMessage = "Nomor rekening maksimal 50 karakter")]
            [Display(Name = "Nomor Rekening / Akun")]
            public string? AccountNumber { get; set; }

            [StringLength(150, ErrorMessage = "Atas nama maksimal 150 karakter")]
            [Display(Name = "Atas Nama")]
            public string? AccountHolder { get; set; }

            [Display(Name = "Saldo Awal (Rp)")]
            public decimal OpeningBalance { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // ================= Kategori
        public class ListKategori
        {
            public List<KategoriRow> Rows { get; set; } = FinancialCategory.GetDataList(FilterAktif.Semua).Select(x => new KategoriRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Jenis = EnumHelper.Text(x.CategoryType),
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Jenis).ThenBy(x => x.Name).ToList();
        }

        public class KategoriRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string Jenis { get; set; } = "";
            public bool Aktif { get; set; }
        }

        public class AddKategori
        {
            public NewKategori Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<CashFlowType>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class EditKategori : AddKategori
        {
            public EditKategori(string encKey)
            {
                var obj = LibExtensions.Load<FinancialCategory>(encKey, "Kategori");
                Row = new NewKategori { Name = obj.Name, CategoryType = (int)obj.CategoryType, IsActive = (int)obj.IsActive };
            }
        }

        public class NewKategori
        {
            [Required(ErrorMessage = "Nama kategori harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Kategori")]
            public string? Name { get; set; }

            [Display(Name = "Jenis (tetap setelah dibuat)")]
            public int CategoryType { get; set; } = (int)CashFlowType.Expense;

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
    }
}
