using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Inventaris;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Inventaris
{
    /// <summary>
    /// Inventaris › Peminjaman (t_borrow_request + item + unit). Menunggu → Disetujui / Ditolak → Dipinjam (checkout, scan kode unit)
    /// → Dikembalikan (semua item kembali); Dipinjam lewat tanggal = Terlambat. Sewa (Tarif Tetap) dan denda telat dibayar
    /// lewat transaksi Pemasukan.
    /// </summary>
    public class PeminjamanVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = BorrowRequest.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                RequestNumber = x.RequestNumber,
                Peminjam = x.BorrowerName ?? "",
                Jenis = EnumHelper.Text(x.BorrowerType),
                Purpose = x.Purpose,
                BorrowDate = x.BorrowDate,
                ExpectedReturnDate = x.ExpectedReturnDate,
                ActualReturnDate = x.ActualReturnDate,
                Skema = EnumHelper.Text(x.RentalScheme),
                Tagihan = x.TotalRentalFee + x.TotalLateFee - x.LateFeeWaived,
                PaidAmount = x.PaidAmount,
                Bayar = (int)x.PaymentStatus,
                BayarText = EnumHelper.Text(x.PaymentStatus),
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                Seikhlasnya = x.RentalScheme == RentalScheme.Seikhlasnya
            }).OrderByDescending(x => x.BorrowDate).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string RequestNumber { get; set; } = "";
            public string Peminjam { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string Purpose { get; set; } = "";
            public DateTime BorrowDate { get; set; }
            public DateTime ExpectedReturnDate { get; set; }
            public DateTime? ActualReturnDate { get; set; }
            public string Skema { get; set; } = "";
            public decimal Tagihan { get; set; }
            public decimal PaidAmount { get; set; }
            public int Bayar { get; set; }
            public string BayarText { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public bool Seikhlasnya { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<BorrowerType>();
            public List<GwaVM.OpsiItem> SkemaOpsi { get; set; } = GwaVM.Opsi<RentalScheme>();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> MitraOpsi { get; set; } = Opsi.Mitra();
            public List<GwaVM.OpsiItem> AsetOpsi { get; set; } = AsetVM.OpsiAset();

            public Add()
            {
                Row.BorrowDate = DateTime.Today.AddHours(16);
                Row.ExpectedReturnDate = DateTime.Today.AddDays(3).AddHours(16);
            }
        }

        public class Edit : Add
        {
            public string Nomor { get; set; } = "";
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<BorrowRequest>(encKey, "Peminjaman");
                if (obj.Status != BorrowRequestStatus.Pending)
                {
                    throw new ArgumentException("Hanya peminjaman Menunggu yang bisa diubah.");
                }
                Nomor = obj.RequestNumber;
                Row = new NewRow
                {
                    BorrowerType = (int)obj.BorrowerType,
                    MemberId = obj.MemberId,
                    ExternalPartyId = obj.ExternalPartyId,
                    BorrowerName = obj.BorrowerName,
                    BorrowerInstitution = obj.BorrowerInstitution,
                    BorrowerPhone = obj.BorrowerPhone,
                    BorrowerEmail = obj.BorrowerEmail,
                    Guarantee = obj.Guarantee,
                    Purpose = obj.Purpose,
                    BorrowDate = obj.BorrowDate,
                    ExpectedReturnDate = obj.ExpectedReturnDate,
                    RentalScheme = (int)obj.RentalScheme,
                    Notes = obj.Notes
                };
            }
        }

        public class NewRow
        {
            [Display(Name = "Jenis Peminjam")]
            public int BorrowerType { get; set; } = (int)GWAShared.Enums.BorrowerType.Anggota;

            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Display(Name = "Organisasi / Instansi")]
            public int? ExternalPartyId { get; set; }

            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Peminjam")]
            public string? BorrowerName { get; set; }

            [StringLength(150, ErrorMessage = "Instansi maksimal 150 karakter")]
            [Display(Name = "Instansi")]
            public string? BorrowerInstitution { get; set; }

            [StringLength(30, ErrorMessage = "Telepon maksimal 30 karakter")]
            [Display(Name = "Telepon")]
            public string? BorrowerPhone { get; set; }

            [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
            [Display(Name = "Email")]
            public string? BorrowerEmail { get; set; }

            [StringLength(255, ErrorMessage = "Jaminan maksimal 255 karakter")]
            [Display(Name = "Jaminan")]
            public string? Guarantee { get; set; }

            [Required(ErrorMessage = "Keperluan harus diisi")]
            [StringLength(255, ErrorMessage = "Keperluan maksimal 255 karakter")]
            [Display(Name = "Keperluan")]
            public string? Purpose { get; set; }

            [Required(ErrorMessage = "Waktu pinjam harus diisi")]
            [Display(Name = "Dipinjam")]
            public DateTime? BorrowDate { get; set; }

            [Required(ErrorMessage = "Rencana kembali harus diisi")]
            [Display(Name = "Rencana Kembali")]
            public DateTime? ExpectedReturnDate { get; set; }

            [Display(Name = "Skema Biaya")]
            public int RentalScheme { get; set; } = (int)GWAShared.Enums.RentalScheme.TarifTetap;

            [StringLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }

            // Item pertama (Add); item lain ditambah dari Detail selama Menunggu.
            [Display(Name = "Aset")]
            public int? AssetId { get; set; }

            [Range(1, 10000, ErrorMessage = "Jumlah minimal 1")]
            [Display(Name = "Jumlah")]
            public int Quantity { get; set; } = 1;
        }

        // ---------- Detail: item, unit, pembayaran
        public class Detail
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public int Status { get; set; }
            public ItemInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> AsetOpsi { get; set; } = new();
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>(ItemCondition.Hilang);
            public List<ItemRow> Items { get; set; } = new();
            public List<BayarRow> Bayar { get; set; } = new();

            public bool BisaUbahItem => Status == (int)BorrowRequestStatus.Pending;
            public bool BisaKembali => Status == (int)BorrowRequestStatus.Borrowed || Status == (int)BorrowRequestStatus.Overdue;

            public Detail(string encKey)
            {
                var obj = LibExtensions.Load<BorrowRequest>(encKey, "Peminjaman");
                Key = obj.EncDecrKey;
                Status = (int)obj.Status;
                Judul = obj.RequestNumber + " · " + obj.BorrowerName;
                Ringkasan = $"Sewa Rp {obj.TotalRentalFee:#,##0} · denda Rp {obj.TotalLateFee:#,##0}" + (obj.LateFeeWaived > 0 ? $" (dibebaskan Rp {obj.LateFeeWaived:#,##0})" : "")
                    + $" · dibayar Rp {obj.PaidAmount:#,##0} · {EnumHelper.Text(obj.PaymentStatus)}";
                if (BisaUbahItem)
                {
                    AsetOpsi = AsetVM.OpsiAset();
                }
                Items = obj.GetItems(true).Select(x => new ItemRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Aset = x.AssetName,
                    PerUnit = x.TrackUnit == UnitTracking.PerUnit,
                    Quantity = x.Quantity,
                    ReturnedQuantity = x.ReturnedQuantity,
                    RentalFee = x.RentalFee,
                    LateDays = x.LateDays,
                    LateFee = x.LateFee,
                    Units = x.TrackUnit == UnitTracking.PerUnit
                        ? x.GetUnits(true).Select(u => new UnitRow { UnitCode = u.UnitCode, Status = EnumHelper.Text(u.ReturnStatus), Kembali = u.ReturnStatus != UnitReturnStatus.BelumKembali }).ToList()
                        : new()
                }).ToList();
                Bayar = FinancialTransaction.GetDataList(transactionType: CashFlowType.Income, status: TransactionStatus.Posted)
                    .Where(t => t.BorrowRequestId == obj.Id)
                    .Select(t => new BayarRow { TransactionId = t.Id, Nomor = t.TransactionNumber, Tanggal = t.TransactionDate.ToDateTime(TimeOnly.MinValue), Kategori = t.CategoryName, Amount = t.Amount })
                    .OrderBy(t => t.Tanggal).ToList();
            }
        }

        public class ItemRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Aset { get; set; } = "";
            public bool PerUnit { get; set; }
            public int Quantity { get; set; }
            public int ReturnedQuantity { get; set; }
            public decimal RentalFee { get; set; }
            public int LateDays { get; set; }
            public decimal LateFee { get; set; }
            public List<UnitRow> Units { get; set; } = new();
        }

        public class UnitRow
        {
            public string UnitCode { get; set; } = "";
            public string Status { get; set; } = "";
            public bool Kembali { get; set; }
        }

        public class BayarRow
        {
            public int TransactionId { get; set; }
            public string Nomor { get; set; } = "";
            public DateTime Tanggal { get; set; }
            public string Kategori { get; set; } = "";
            public decimal Amount { get; set; }
        }

        public class ItemInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Pilih aset")]
            [Display(Name = "Aset")]
            public int? AssetId { get; set; }

            [Range(1, 10000, ErrorMessage = "Jumlah minimal 1")]
            [Display(Name = "Jumlah")]
            public int Quantity { get; set; } = 1;
        }

        // ---------- Checkout (modal _Checkout)
        public class Checkout
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>(ItemCondition.Hilang);
            public List<CheckoutItem> Items { get; set; }

            public Checkout(string encKey)
            {
                var obj = LibExtensions.Load<BorrowRequest>(encKey, "Peminjaman");
                if (obj.Status != BorrowRequestStatus.Approved)
                {
                    throw new ArgumentException("Checkout hanya untuk peminjaman Disetujui.");
                }
                Key = obj.EncDecrKey;
                Judul = obj.RequestNumber + " · " + obj.BorrowerName;
                Items = obj.GetItems(true).Select(x => new CheckoutItem
                {
                    Id = x.Id,
                    Aset = x.AssetName,
                    Quantity = x.Quantity,
                    PerUnit = x.TrackUnit == UnitTracking.PerUnit,
                    UnitTersedia = x.TrackUnit == UnitTracking.PerUnit
                        ? AssetUnit.GetDataList(x.AssetId, AssetUnitStatus.Tersedia).Select(u => u.UnitCode).OrderBy(c => c).ToList()
                        : new()
                }).ToList();
            }
        }

        public class CheckoutItem
        {
            public int Id { get; set; }
            public string Aset { get; set; } = "";
            public int Quantity { get; set; }
            public bool PerUnit { get; set; }
            public List<string> UnitTersedia { get; set; } = new();
        }

        public class CheckoutInput
        {
            public string Key { get; set; } = "";
            public List<BorrowRequest.CheckoutLine> Lines { get; set; } = new();
        }

        public class KembaliJumlahInput
        {
            public string Key { get; set; } = "";
            public int Returned { get; set; }
            public int Lost { get; set; }
            public int Condition { get; set; } = (int)ItemCondition.Baik;
            public string? Notes { get; set; }
        }

        public class KembaliUnitInput
        {
            public string Key { get; set; } = "";
            public string UnitCode { get; set; } = "";
            public int Condition { get; set; } = (int)ItemCondition.Baik;
            public bool Lost { get; set; }
            public string? Notes { get; set; }
        }

        public class BebasDendaInput
        {
            public string Key { get; set; } = "";
            public decimal Amount { get; set; }
            public string? Keterangan { get; set; }
        }

        public class BatalBayarInput
        {
            public string Key { get; set; } = "";
            public int TransactionId { get; set; }
            public string? Keterangan { get; set; }
        }

        public class Method
        {
            public static BorrowRequest Load(string? key) => LibExtensions.Load<BorrowRequest>(key, "Peminjaman");

            public static void Simpan(NewRow input, string user)
            {
                if (!input.AssetId.HasValue)
                {
                    throw new ArgumentException("Pilih minimal satu aset.");
                }
                BorrowRequest.Save(new BorrowRequest.NewRow
                {
                    BorrowerType = (BorrowerType)input.BorrowerType,
                    MemberId = input.MemberId,
                    ExternalPartyId = input.ExternalPartyId,
                    BorrowerName = input.BorrowerName,
                    BorrowerInstitution = input.BorrowerInstitution,
                    BorrowerPhone = input.BorrowerPhone,
                    BorrowerEmail = input.BorrowerEmail,
                    Guarantee = input.Guarantee,
                    Purpose = input.Purpose ?? "",
                    BorrowDate = input.BorrowDate ?? DateTime.Now,
                    ExpectedReturnDate = input.ExpectedReturnDate ?? DateTime.Now,
                    RentalScheme = (RentalScheme)input.RentalScheme,
                    Notes = input.Notes,
                    Items = new() { new BorrowItem.NewRow { AssetId = input.AssetId.Value, Quantity = input.Quantity } }
                }, user).Ok();
            }

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.BorrowerType = (BorrowerType)input.BorrowerType;
                obj.MemberId = input.MemberId;
                obj.ExternalPartyId = input.ExternalPartyId;
                obj.BorrowerName = input.BorrowerName;
                obj.BorrowerInstitution = input.BorrowerInstitution;
                obj.BorrowerPhone = input.BorrowerPhone;
                obj.BorrowerEmail = input.BorrowerEmail;
                obj.Guarantee = input.Guarantee;
                obj.Purpose = input.Purpose ?? "";
                obj.BorrowDate = input.BorrowDate ?? obj.BorrowDate;
                obj.ExpectedReturnDate = input.ExpectedReturnDate ?? obj.ExpectedReturnDate;
                obj.RentalScheme = (RentalScheme)input.RentalScheme;
                obj.Notes = input.Notes;
                obj.Update(user).Ok();
            }

            public static void TambahItem(ItemInput input, string user)
            {
                var obj = Load(input.Key);
                BorrowItem.Save(new BorrowItem.NewRow { BorrowRequestId = obj.Id, AssetId = input.AssetId ?? 0, Quantity = input.Quantity }, user).Ok();
            }

            public static void HapusItem(string? key, string user) => LibExtensions.Load<BorrowItem>(key, "Item").Delete(user).Ok();

            // Hasil scan label bisa berupa URL publik /aset/{kode}; diambil kode unitnya.
            public static void SimpanCheckout(CheckoutInput input, string user)
            {
                var lines = input.Lines ?? new();
                foreach (var line in lines.Where(x => x.UnitCodes != null))
                {
                    line.UnitCodes = line.UnitCodes!.Select(AsetLabel.ResolveKode).ToList();
                }
                Load(input.Key).Checkout(lines, user).Ok();
            }

            public static void KembaliJumlah(KembaliJumlahInput input, string user)
                => LibExtensions.Load<BorrowItem>(input.Key, "Item").ReturnQuantity(input.Returned, input.Lost, (ItemCondition)input.Condition, input.Notes, user).Ok();

            public static void KembaliUnit(KembaliUnitInput input, string user)
                => BorrowItemUnit.ReturnUnit(Load(input.Key).Id, AsetLabel.ResolveKode(input.UnitCode), (ItemCondition)input.Condition, input.Lost, input.Notes, user).Ok();

            public static GwaVM.BayarForm FormBayar(string encKey)
            {
                var obj = Load(encKey);
                var tagihan = obj.TotalRentalFee + obj.TotalLateFee - obj.LateFeeWaived - obj.PaidAmount;
                return new GwaVM.BayarForm
                {
                    Judul = obj.RequestNumber + " · " + obj.BorrowerName,
                    Info = obj.RentalScheme == RentalScheme.Seikhlasnya ? "Skema seikhlasnya: nominal bebas." : $"Sisa tagihan Rp {Math.Max(0, tagihan):#,##0}. Pilih kategori sewa / donasi / denda.",
                    OnSave = "Peminjaman.pay()",
                    Tombol = "Catat Pembayaran",
                    KategoriOpsi = Opsi.Kategori(CashFlowType.Income),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey, Amount = Math.Max(0, tagihan) }
                };
            }

            public static void Bayar(GwaVM.BayarInput input, string user)
                => Load(input.Key).ReceivePayment(input.AccountId ?? 0, input.CategoryId ?? 0, input.Amount ?? 0, Opsi.TanggalWajib(input.Date, "Tanggal"), user).Ok();

            public static void BatalBayar(BatalBayarInput input, string user) => Load(input.Key).CancelPayment(input.TransactionId, input.Keterangan, user).Ok();

            public static void BebasDenda(BebasDendaInput input, string user) => Load(input.Key).WaiveLateFee(input.Amount, input.Keterangan, user).Ok();
        }
    }
}
