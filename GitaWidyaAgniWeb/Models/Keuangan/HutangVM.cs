using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Keuangan
{
    /// <summary>
    /// Keuangan › Hutang (t_debt + pembayaran + lampiran). Pinjaman tunai (akun + kategori diisi) membuat transaksi Pemasukan.
    /// Pembayaran membuat transaksi Pengeluaran; hapus pembayaran mem-void transaksinya. Dihapuskan = final.
    /// </summary>
    public class HutangVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Debt.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                DebtNumber = x.DebtNumber,
                CreditorName = x.CreditorName,
                CreditorContact = x.CreditorContact,
                Description = x.Description,
                Periode = x.PeriodName,
                Amount = x.Amount,
                PaidAmount = x.PaidAmount,
                DebtDate = x.DebtDate.ToDateTime(TimeOnly.MinValue),
                DueDate = Opsi.KeDateTime(x.DueDate),
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                Tunai = x.TransactionId.HasValue
            }).OrderByDescending(x => x.DebtDate).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string DebtNumber { get; set; } = "";
            public string CreditorName { get; set; } = "";
            public string? CreditorContact { get; set; }
            public string? Description { get; set; }
            public string Periode { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal Sisa => Amount - PaidAmount;
            public DateTime DebtDate { get; set; }
            public DateTime? DueDate { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public bool Tunai { get; set; }
            public bool Terlambat => DueDate.HasValue && DueDate.Value.Date < DateTime.Today && Status <= (int)DebtStatus.Sebagian;
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AkunOpsi { get; set; } = Opsi.Akun();
            public List<GwaVM.OpsiItem> KategoriOpsi { get; set; } = Opsi.Kategori(CashFlowType.Income);
            public Add()
            {
                Row.PeriodId = Opsi.PeriodeAktifId();
                Row.DebtDate = DateTime.Today;
            }
        }

        public class Edit : Add
        {
            public string Nomor { get; set; } = "";
            public bool Tunai { get; set; }
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Debt>(encKey, "Hutang");
                if (obj.Status != DebtStatus.BelumBayar && obj.Status != DebtStatus.Sebagian)
                {
                    throw new ArgumentException("Hutang Lunas / Dihapuskan tidak bisa diubah.");
                }
                Nomor = obj.DebtNumber;
                Tunai = obj.TransactionId.HasValue;
                Row = new NewRow
                {
                    PeriodId = obj.PeriodId,
                    CreditorName = obj.CreditorName,
                    CreditorContact = obj.CreditorContact,
                    Description = obj.Description,
                    Amount = obj.Amount,
                    DebtDate = Opsi.KeDateTime(obj.DebtDate),
                    DueDate = Opsi.KeDateTime(obj.DueDate)
                };
            }
        }

        public class NewRow
        {
            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Nama pemberi pinjaman harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Pemberi Pinjaman")]
            public string? CreditorName { get; set; }

            [StringLength(100, ErrorMessage = "Kontak maksimal 100 karakter")]
            [Display(Name = "Kontak")]
            public string? CreditorContact { get; set; }

            [StringLength(500, ErrorMessage = "Keterangan maksimal 500 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Range(1, 999999999999, ErrorMessage = "Nominal harus lebih dari 0")]
            [Display(Name = "Nominal (Rp)")]
            public decimal Amount { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal Hutang")]
            public DateTime? DebtDate { get; set; }

            [Display(Name = "Jatuh Tempo")]
            public DateTime? DueDate { get; set; }

            [Display(Name = "Masuk ke Akun (pinjaman tunai)")]
            public int? LoanAccountId { get; set; }

            [Display(Name = "Kategori Pemasukan")]
            public int? LoanCategoryId { get; set; }
        }

        // Riwayat: pembayaran + lampiran.
        public class Riwayat
        {
            public string Key { get; set; } = "";
            public string Nomor { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public bool BisaUbah { get; set; }
            public List<BayarRow> Bayar { get; set; }
            public List<LampiranRow> Lampiran { get; set; }

            public Riwayat(string encKey)
            {
                var obj = LibExtensions.Load<Debt>(encKey, "Hutang");
                Key = obj.EncDecrKey;
                Nomor = obj.DebtNumber + " · " + obj.CreditorName;
                Ringkasan = "Rp " + obj.PaidAmount.ToString("#,##0") + " dari Rp " + obj.Amount.ToString("#,##0") + " · " + EnumHelper.Text(obj.Status);
                BisaUbah = obj.Status != DebtStatus.Dihapuskan;
                Bayar = obj.GetPayments(true).Select(x => new BayarRow
                {
                    EncDecrKey = x.EncDecrKey,
                    TransactionNumber = x.TransactionNumber,
                    Akun = x.AccountName,
                    Amount = x.Amount,
                    PaidDate = x.PaidDate.ToDateTime(TimeOnly.MinValue),
                    Notes = x.Notes
                }).OrderBy(x => x.PaidDate).ToList();
                Lampiran = obj.GetAttachments(true).Select(x => new LampiranRow { EncDecrKey = x.EncDecrKey, FileName = x.FileName }).ToList();
            }
        }

        public class BayarRow
        {
            public string EncDecrKey { get; set; } = "";
            public string TransactionNumber { get; set; } = "";
            public string Akun { get; set; } = "";
            public decimal Amount { get; set; }
            public DateTime PaidDate { get; set; }
            public string? Notes { get; set; }
        }

        public class LampiranRow
        {
            public string EncDecrKey { get; set; } = "";
            public string FileName { get; set; } = "";
        }

        public class Method
        {
            public static Debt Load(string? key) => LibExtensions.Load<Debt>(key, "Hutang");

            public static void Simpan(NewRow input, string user)
                => Debt.Save(new Debt.NewRow
                {
                    PeriodId = input.PeriodId,
                    CreditorName = input.CreditorName ?? "",
                    CreditorContact = input.CreditorContact,
                    Description = input.Description,
                    Amount = input.Amount,
                    DebtDate = Opsi.TanggalWajib(input.DebtDate, "Tanggal hutang"),
                    DueDate = Opsi.Tanggal(input.DueDate),
                    LoanAccountId = input.LoanAccountId,
                    LoanCategoryId = input.LoanCategoryId
                }, user).Ok();

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.CreditorName = input.CreditorName ?? "";
                obj.CreditorContact = input.CreditorContact;
                obj.Description = input.Description;
                obj.Amount = input.Amount;
                obj.DebtDate = Opsi.TanggalWajib(input.DebtDate, "Tanggal hutang");
                obj.DueDate = Opsi.Tanggal(input.DueDate);
                obj.Update(user).Ok();
            }

            public static GwaVM.BayarForm FormBayar(string encKey)
            {
                var obj = Load(encKey);
                return new GwaVM.BayarForm
                {
                    Judul = obj.DebtNumber + " · " + obj.CreditorName,
                    Info = "Sisa Rp " + (obj.Amount - obj.PaidAmount).ToString("#,##0") + ". Pembayaran dicatat sebagai transaksi Pengeluaran (Diposting).",
                    OnSave = "Hutang.pay()",
                    Tombol = "Catat Pembayaran",
                    ShowNotes = true,
                    KategoriOpsi = Opsi.Kategori(CashFlowType.Expense),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey, Amount = obj.Amount - obj.PaidAmount }
                };
            }

            public static void Bayar(GwaVM.BayarInput input, string user)
            {
                var obj = Load(input.Key);
                DebtPayment.Save(new DebtPayment.NewRow
                {
                    DebtId = obj.Id,
                    AccountId = input.AccountId ?? 0,
                    CategoryId = input.CategoryId ?? 0,
                    Amount = input.Amount ?? 0,
                    PaidDate = Opsi.TanggalWajib(input.Date, "Tanggal"),
                    Notes = input.Notes
                }, user).Ok();
            }

            public static void HapusBayar(GwaVM.AlasanInput input, string user)
                => LibExtensions.Load<DebtPayment>(input.Key, "Pembayaran").Delete(input.Keterangan, user).Ok();

            public static void TambahLampiran(string? key, byte[]? data, IFormFile? meta, string user)
            {
                var obj = Load(key);
                if (data == null)
                {
                    throw new ArgumentException("Pilih berkas terlebih dahulu.");
                }
                DebtAttachment.Save(new DebtAttachment.NewRow { DebtId = obj.Id, FileData = data, FileName = meta!.FileName, FileType = meta.ContentType }, user).Ok();
            }

            public static DebtAttachment LoadLampiran(string? key) => LibExtensions.Load<DebtAttachment>(key, "Lampiran");
        }
    }
}
