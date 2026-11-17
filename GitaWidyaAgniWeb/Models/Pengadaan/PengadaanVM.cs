using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Keuangan;
using GWALib.Lib.Transaksi.Pengadaan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Pengadaan
{
    /// <summary>
    /// Pengadaan (t_procurement_request + item, penawaran, persetujuan, penerimaan, pembayaran).
    /// Draft → Diajukan → Disetujui / Revisi / Ditolak (alur PROCUREMENT) → Dibeli → Diterima Sebagian → Selesai (mengikuti penerimaan).
    /// Item aset yang diterima bisa langsung dibuatkan data aset (isi kode aset baru).
    /// </summary>
    public partial class PengadaanVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = ProcurementRequest.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                RequestNumber = x.RequestNumber,
                Title = x.Title,
                Pemohon = x.RequestedByName,
                RequestDate = x.RequestDate.ToDateTime(TimeOnly.MinValue),
                NeededByDate = Opsi.KeDateTime(x.NeededByDate),
                Metode = (int)x.PaymentMethod,
                MetodeText = EnumHelper.Text(x.PaymentMethod),
                TotalEstimate = x.TotalEstimate,
                TotalActual = x.TotalActual,
                LebihAnggaran = x.IsOverBudget == BudgetCheck.MelebihiAnggaran,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderByDescending(x => x.RequestDate).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string RequestNumber { get; set; } = "";
            public string Title { get; set; } = "";
            public string Pemohon { get; set; } = "";
            public DateTime RequestDate { get; set; }
            public DateTime? NeededByDate { get; set; }
            public int Metode { get; set; }
            public string MetodeText { get; set; } = "";
            public decimal TotalEstimate { get; set; }
            public decimal TotalActual { get; set; }
            public bool LebihAnggaran { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class Add
        {
            public NewRow Row { get; set; } = new() { RequestDate = DateTime.Today };
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AnggaranOpsi { get; set; } = Opsi.MataAnggaran();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan(true);
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program(hanyaTerbuka: true);
            public List<GwaVM.OpsiItem> MetodeOpsi { get; set; } = GwaVM.Opsi<ProcurementPaymentMethod>();
        }

        public class Edit : Add
        {
            public string Nomor { get; set; } = "";
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<ProcurementRequest>(encKey, "Pengadaan");
                if (obj.Status != ProcurementStatus.Draft && obj.Status != ProcurementStatus.Revisi)
                {
                    throw new ArgumentException("Pengadaan hanya bisa diubah saat Draft / Revisi.");
                }
                Nomor = obj.RequestNumber;
                Row = new NewRow
                {
                    PeriodId = obj.PeriodId,
                    RequestedByMemberId = obj.RequestedByMemberId,
                    RequestDate = obj.RequestDate.ToDateTime(TimeOnly.MinValue),
                    Title = obj.Title,
                    Reason = obj.Reason,
                    ActivityId = obj.ActivityId,
                    ProgramId = obj.ProgramId,
                    BudgetItemId = obj.BudgetItemId,
                    PaymentMethod = (int)obj.PaymentMethod,
                    NeededByDate = Opsi.KeDateTime(obj.NeededByDate)
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Judul Pengadaan")]
            public string? Title { get; set; }

            [Required(ErrorMessage = "Alasan harus diisi")]
            [Display(Name = "Alasan / Kebutuhan")]
            public string? Reason { get; set; }

            [Display(Name = "Pemohon (kosong = Anda)")]
            public int? RequestedByMemberId { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal Permohonan")]
            public DateTime? RequestDate { get; set; }

            [Display(Name = "Dibutuhkan Tanggal")]
            public DateTime? NeededByDate { get; set; }

            [Display(Name = "Metode Pembayaran")]
            public int PaymentMethod { get; set; } = (int)ProcurementPaymentMethod.DibayarLangsung;

            [Display(Name = "Mata Anggaran")]
            public int? BudgetItemId { get; set; }

            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Program")]
            public int? ProgramId { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }
        }

        // ---------- Detail
        public class Detail
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public int Status { get; set; }
            public ItemInput ItemInput { get; set; } = new();
            public PenawaranInput PenawaranInput { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<ProcurementItemKind>();
            public List<GwaVM.OpsiItem> KategoriAsetOpsi { get; set; } = AssetCategory.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
            public List<GwaVM.OpsiItem> MitraOpsi { get; set; } = Opsi.Mitra();
            public List<GwaVM.OpsiItem> ItemOpsi { get; set; } = new();
            public List<ItemRow> Items { get; set; } = new();
            public List<PenawaranRow> Penawaran { get; set; } = new();
            public List<ApprovalRow> Persetujuan { get; set; } = new();
            public List<PenerimaanRow> Penerimaan { get; set; } = new();
            public List<BayarRow> Bayar { get; set; } = new();

            public bool BisaUbahItem => Status == (int)ProcurementStatus.Draft || Status == (int)ProcurementStatus.Revisi;
            public bool BisaPenawaran => Status >= (int)ProcurementStatus.Draft && Status <= (int)ProcurementStatus.Disetujui && Status != (int)ProcurementStatus.Ditolak;
            public bool BisaHargaAktual => Status >= (int)ProcurementStatus.Disetujui && Status <= (int)ProcurementStatus.DiterimaSebagian && Status != (int)ProcurementStatus.Ditolak;
            public bool BisaTerima => Status == (int)ProcurementStatus.Dibeli || Status == (int)ProcurementStatus.DiterimaSebagian;

            public Detail(string encKey)
            {
                var obj = LibExtensions.Load<ProcurementRequest>(encKey, "Pengadaan");
                Key = obj.EncDecrKey;
                Status = (int)obj.Status;
                Judul = obj.RequestNumber + " · " + obj.Title;
                Ringkasan = $"Estimasi Rp {obj.TotalEstimate:#,##0} · aktual Rp {obj.TotalActual:#,##0}"
                    + (obj.BudgetRemaining.HasValue ? $" · sisa anggaran Rp {obj.BudgetRemaining:#,##0}" : "")
                    + (obj.IsOverBudget == BudgetCheck.MelebihiAnggaran ? " · MELEBIHI ANGGARAN" : "")
                    + (string.IsNullOrEmpty(obj.RejectReason) ? "" : " · catatan: " + obj.RejectReason);
                var mitra = MitraOpsi.ToDictionary(x => x.Value, x => x.Text);
                var items = obj.GetItems(true);
                Items = items.Select(x => new ItemRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Id = x.Id,
                    ItemName = x.ItemName,
                    Specification = x.Specification,
                    Aset = x.IsAsset == ProcurementItemKind.Aset,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    EstimatedUnitPrice = x.EstimatedUnitPrice,
                    ActualUnitPrice = x.ActualUnitPrice,
                    Vendor = x.ExternalPartyId.HasValue && mitra.TryGetValue(x.ExternalPartyId.Value, out var m) ? m : null,
                    ReceivedQuantity = x.ReceivedQuantity
                }).ToList();
                ItemOpsi = items.Select(x => new GwaVM.OpsiItem(x.Id, x.ItemName)).ToList();
                Penawaran = obj.GetQuotations(true).Select(x => new PenawaranRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Item = x.ItemId.HasValue ? items.FirstOrDefault(i => i.Id == x.ItemId)?.ItemName : "Seluruh pengadaan",
                    Vendor = x.ExternalPartyName ?? x.VendorName ?? "",
                    QuotedAmount = x.QuotedAmount,
                    Dipilih = x.IsSelected == QuotationSelection.Dipilih,
                    FileName = x.FileName,
                    Notes = x.Notes
                }).ToList();
                Persetujuan = obj.GetApprovals(true).Select(x => new ApprovalRow
                {
                    Level = x.ApprovalLevel,
                    Langkah = x.StepName,
                    Approver = x.ApproverName,
                    Keputusan = EnumHelper.Text(x.Decision),
                    Note = x.Note,
                    DecidedAt = x.DecidedAt
                }).OrderBy(x => x.DecidedAt).ToList();
                Penerimaan = obj.GetReceipts(true).Select(x => new PenerimaanRow
                {
                    EncDecrKey = x.EncDecrKey,
                    ReceiptDate = x.ReceiptDate.ToDateTime(TimeOnly.MinValue),
                    Penerima = x.ReceivedByName,
                    Notes = x.Notes,
                    Rincian = string.Join(", ", x.GetItems(true).Select(i => i.ItemName + " × " + i.Quantity + " (" + EnumHelper.Text(i.ItemCondition) + ")"))
                }).OrderBy(x => x.ReceiptDate).ToList();
                Bayar = FinancialTransaction.GetDataList(transactionType: CashFlowType.Expense, status: TransactionStatus.Posted)
                    .Where(t => t.ProcurementId == obj.Id)
                    .Select(t => new BayarRow { TransactionId = t.Id, Nomor = t.TransactionNumber, Tanggal = t.TransactionDate.ToDateTime(TimeOnly.MinValue), Akun = t.AccountName, Amount = t.Amount })
                    .ToList();
            }
        }

        public class ItemRow
        {
            public string EncDecrKey { get; set; } = "";
            public int Id { get; set; }
            public string ItemName { get; set; } = "";
            public string? Specification { get; set; }
            public bool Aset { get; set; }
            public int Quantity { get; set; }
            public string Unit { get; set; } = "";
            public decimal EstimatedUnitPrice { get; set; }
            public decimal? ActualUnitPrice { get; set; }
            public string? Vendor { get; set; }
            public int ReceivedQuantity { get; set; }
        }

        public class PenawaranRow
        {
            public string EncDecrKey { get; set; } = "";
            public string? Item { get; set; }
            public string Vendor { get; set; } = "";
            public decimal QuotedAmount { get; set; }
            public bool Dipilih { get; set; }
            public string? FileName { get; set; }
            public string? Notes { get; set; }
        }

        public class ApprovalRow
        {
            public int Level { get; set; }
            public string? Langkah { get; set; }
            public string Approver { get; set; } = "";
            public string Keputusan { get; set; } = "";
            public string? Note { get; set; }
            public DateTime DecidedAt { get; set; }
        }

        public class PenerimaanRow
        {
            public string EncDecrKey { get; set; } = "";
            public DateTime ReceiptDate { get; set; }
            public string Penerima { get; set; } = "";
            public string? Notes { get; set; }
            public string Rincian { get; set; } = "";
        }

        public class BayarRow
        {
            public int TransactionId { get; set; }
            public string Nomor { get; set; } = "";
            public DateTime Tanggal { get; set; }
            public string Akun { get; set; } = "";
            public decimal Amount { get; set; }
        }

        public class ItemInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Nama barang harus diisi")]
            [StringLength(200, ErrorMessage = "Nama maksimal 200 karakter")]
            [Display(Name = "Barang")]
            public string? ItemName { get; set; }

            [Display(Name = "Jenis")]
            public int IsAsset { get; set; } = (int)ProcurementItemKind.HabisPakai;

            [Display(Name = "Kategori Aset")]
            public int? AssetCategoryId { get; set; }

            [Range(1, 100000, ErrorMessage = "Jumlah minimal 1")]
            [Display(Name = "Jumlah")]
            public int Quantity { get; set; } = 1;

            [StringLength(30, ErrorMessage = "Satuan maksimal 30 karakter")]
            [Display(Name = "Satuan")]
            public string? Unit { get; set; } = "pcs";

            [Range(0, 999999999999, ErrorMessage = "Harga tidak boleh negatif")]
            [Display(Name = "Estimasi Harga Satuan")]
            public decimal EstimatedUnitPrice { get; set; }

            [Display(Name = "Vendor")]
            public int? ExternalPartyId { get; set; }

            [StringLength(255, ErrorMessage = "Spesifikasi maksimal 255 karakter")]
            [Display(Name = "Spesifikasi")]
            public string? Specification { get; set; }
        }

        public class PenawaranInput
        {
            public string Key { get; set; } = "";

            [Display(Name = "Untuk Barang (kosong = seluruh)")]
            public int? ItemId { get; set; }

            [Display(Name = "Vendor (mitra)")]
            public int? ExternalPartyId { get; set; }

            [StringLength(150, ErrorMessage = "Nama vendor maksimal 150 karakter")]
            [Display(Name = "Nama Vendor (bila bukan mitra)")]
            public string? VendorName { get; set; }

            [Range(0, 999999999999, ErrorMessage = "Nominal tidak boleh negatif")]
            [Display(Name = "Nominal Penawaran")]
            public decimal QuotedAmount { get; set; }

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        public class AktualInput
        {
            public string Key { get; set; } = "";
            public decimal Price { get; set; }
        }

        public class BatalBayarInput
        {
            public string Key { get; set; } = "";
            public int TransactionId { get; set; }
            public string? Keterangan { get; set; }
        }

        public class BeliInput
        {
            public string Key { get; set; } = "";
            public int? FundingRequestId { get; set; }
        }

        // ---------- Penerimaan (modal _Terima)
        public class Terima
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> KondisiOpsi { get; set; } = GwaVM.Opsi<ItemCondition>();
            public List<ItemRow> Items { get; set; }

            public Terima(string encKey)
            {
                var obj = LibExtensions.Load<ProcurementRequest>(encKey, "Pengadaan");
                if (obj.Status != ProcurementStatus.Dibeli && obj.Status != ProcurementStatus.DiterimaSebagian)
                {
                    throw new ArgumentException("Penerimaan hanya saat Dibeli / Diterima Sebagian.");
                }
                Key = obj.EncDecrKey;
                Judul = obj.RequestNumber + " · " + obj.Title;
                Items = obj.GetItems(true).Where(x => x.ReceivedQuantity < x.Quantity).Select(x => new ItemRow
                {
                    Id = x.Id,
                    ItemName = x.ItemName,
                    Aset = x.IsAsset == ProcurementItemKind.Aset,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    ReceivedQuantity = x.ReceivedQuantity
                }).ToList();
            }
        }

        public class TerimaInput
        {
            public string Key { get; set; } = "";
            public DateTime? ReceiptDate { get; set; }
            public int? ReceivedByMemberId { get; set; }
            public string? Notes { get; set; }
            public List<TerimaLine> Lines { get; set; } = new();
        }

        public class TerimaLine
        {
            public int ItemId { get; set; }
            public int Quantity { get; set; }
            public int Condition { get; set; } = (int)ItemCondition.Baik;
            public string? AssetCode { get; set; }
        }
    }
}
