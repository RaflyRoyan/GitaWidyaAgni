using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Keuangan
{
    /// <summary>
    /// Keuangan › Pengajuan Dana (t_funding_request + persetujuan + realisasi).
    /// Draft → Diajukan → Disetujui (alur FUNDING_REQUEST) → Dicairkan (transaksi Pengeluaran); Diajukan → Ditolak / kembali Draft (revisi).
    /// Setelah dicair: realisasi + bukti, lalu LPJ: Belum → Diajukan → Selesai (sisa dana dikembalikan / kekurangan dibayar).
    /// </summary>
    public class PengajuanDanaVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = FundingRequest.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                RequestNumber = x.RequestNumber,
                Purpose = x.Purpose,
                Pengaju = x.RequestedByName,
                Periode = x.PeriodName,
                Amount = x.Amount,
                RealizedAmount = x.RealizedAmount,
                ReturnedAmount = x.ReturnedAmount,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status),
                Lpj = (int)x.SettlementStatus,
                LpjText = EnumHelper.Text(x.SettlementStatus),
                InsDate = x.InsDate
            }).OrderByDescending(x => x.InsDate).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string RequestNumber { get; set; } = "";
            public string Purpose { get; set; } = "";
            public string Pengaju { get; set; } = "";
            public string? Periode { get; set; }
            public decimal Amount { get; set; }
            public decimal RealizedAmount { get; set; }
            public decimal ReturnedAmount { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int Lpj { get; set; }
            public string LpjText { get; set; } = "";
            public DateTime InsDate { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AnggaranOpsi { get; set; } = Opsi.MataAnggaran();
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program(hanyaTerbuka: true);
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan(true);
            public List<GwaVM.OpsiItem> LombaOpsi { get; set; } = Opsi.Lomba();
        }

        public class Edit : Add
        {
            public string Nomor { get; set; } = "";
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<FundingRequest>(encKey, "Pengajuan dana");
                if (obj.Status != FundingRequestStatus.Draft)
                {
                    throw new ArgumentException("Hanya pengajuan Draft yang bisa diubah.");
                }
                Nomor = obj.RequestNumber;
                Row = new NewRow
                {
                    PeriodId = obj.PeriodId,
                    BudgetItemId = obj.BudgetItemId,
                    ProgramId = obj.ProgramId,
                    ActivityId = obj.ActivityId,
                    CompetitionId = obj.CompetitionId,
                    Purpose = obj.Purpose,
                    Amount = obj.Amount
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Keperluan harus diisi")]
            [StringLength(500, ErrorMessage = "Keperluan maksimal 500 karakter")]
            [Display(Name = "Keperluan")]
            public string? Purpose { get; set; }

            [Range(1, 999999999999, ErrorMessage = "Nominal harus lebih dari 0")]
            [Display(Name = "Nominal (Rp)")]
            public decimal Amount { get; set; }

            [Display(Name = "Mata Anggaran")]
            public int? BudgetItemId { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Display(Name = "Program")]
            public int? ProgramId { get; set; }

            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Lomba")]
            public int? CompetitionId { get; set; }
        }

        // ---------- Detail: persetujuan + realisasi
        public class Detail
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public bool BisaRealisasi { get; set; }
            public RealisasiInput Input { get; set; } = new() { RealizationDate = DateTime.Today };
            public List<ApprovalRow> Persetujuan { get; set; }
            public List<RealisasiRow> Realisasi { get; set; }

            public Detail(string encKey)
            {
                var obj = LibExtensions.Load<FundingRequest>(encKey, "Pengajuan dana");
                Key = obj.EncDecrKey;
                Judul = obj.RequestNumber + " · " + obj.Purpose;
                Ringkasan = $"Dana Rp {obj.Amount:#,##0} · realisasi Rp {obj.RealizedAmount:#,##0} · LPJ {EnumHelper.Text(obj.SettlementStatus)}";
                BisaRealisasi = obj.Status == FundingRequestStatus.Disbursed && obj.SettlementStatus == SettlementStatus.Belum;
                Persetujuan = obj.GetApprovals(true).Select(x => new ApprovalRow
                {
                    Level = x.ApprovalLevel,
                    Langkah = x.StepName,
                    Approver = x.ApproverName,
                    Keputusan = EnumHelper.Text(x.Decision),
                    Note = x.Note,
                    DecidedAt = x.DecidedAt
                }).OrderBy(x => x.Level).ToList();
                Realisasi = obj.GetRealizations(true).Select(x => new RealisasiRow
                {
                    EncDecrKey = x.EncDecrKey,
                    RealizationDate = x.RealizationDate.ToDateTime(TimeOnly.MinValue),
                    Description = x.Description,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    Amount = x.Amount,
                    Notes = x.Notes,
                    ReceiptFileName = x.ReceiptFileName
                }).OrderBy(x => x.RealizationDate).ToList();
            }
        }

        public class ApprovalRow
        {
            public int Level { get; set; }
            public string? Langkah { get; set; }
            public string Approver { get; set; } = "";
            public string Keputusan { get; set; } = "";
            public string? Note { get; set; }
            public DateTime? DecidedAt { get; set; }
        }

        public class RealisasiRow
        {
            public string EncDecrKey { get; set; } = "";
            public DateTime RealizationDate { get; set; }
            public string Description { get; set; } = "";
            public decimal? Quantity { get; set; }
            public string? Unit { get; set; }
            public decimal Amount { get; set; }
            public string? Notes { get; set; }
            public string? ReceiptFileName { get; set; }
        }

        public class RealisasiInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal")]
            public DateTime? RealizationDate { get; set; }

            [Required(ErrorMessage = "Uraian harus diisi")]
            [StringLength(255, ErrorMessage = "Uraian maksimal 255 karakter")]
            [Display(Name = "Uraian")]
            public string? Description { get; set; }

            [Display(Name = "Jumlah")]
            public decimal? Quantity { get; set; }

            [StringLength(30, ErrorMessage = "Satuan maksimal 30 karakter")]
            [Display(Name = "Satuan")]
            public string? Unit { get; set; }

            [Range(1, 999999999999, ErrorMessage = "Nominal harus lebih dari 0")]
            [Display(Name = "Nominal (Rp)")]
            public decimal Amount { get; set; }

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        public class Method
        {
            public static FundingRequest Load(string? key) => LibExtensions.Load<FundingRequest>(key, "Pengajuan dana");

            public static void Simpan(NewRow input, string user)
                => FundingRequest.Save(new FundingRequest.NewRow
                {
                    PeriodId = input.PeriodId,
                    BudgetItemId = input.BudgetItemId,
                    ProgramId = input.ProgramId,
                    ActivityId = input.ActivityId,
                    CompetitionId = input.CompetitionId,
                    Purpose = input.Purpose ?? "",
                    Amount = input.Amount
                }, user).Ok();

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.PeriodId = input.PeriodId;
                obj.BudgetItemId = input.BudgetItemId;
                obj.ProgramId = input.ProgramId;
                obj.ActivityId = input.ActivityId;
                obj.CompetitionId = input.CompetitionId;
                obj.Purpose = input.Purpose ?? "";
                obj.Amount = input.Amount;
                obj.Update(user).Ok();
            }

            public static GwaVM.BayarForm FormCair(string encKey)
            {
                var obj = Load(encKey);
                return new GwaVM.BayarForm
                {
                    Judul = obj.RequestNumber + " · " + obj.Purpose,
                    Info = "Rp " + obj.Amount.ToString("#,##0") + " dicairkan sebagai transaksi Pengeluaran (Diposting).",
                    OnSave = "PengajuanDana.disburse()",
                    Tombol = "Cairkan",
                    ShowAmount = false,
                    KategoriOpsi = Opsi.Kategori(CashFlowType.Expense),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey }
                };
            }

            public static void Cair(GwaVM.BayarInput input, string user)
                => Load(input.Key).Disburse(input.AccountId ?? 0, input.CategoryId ?? 0, Opsi.TanggalWajib(input.Date, "Tanggal"), user).Ok();

            public static GwaVM.BayarForm FormSelesai(string encKey)
            {
                var obj = Load(encKey);
                var selisih = obj.Amount - obj.RealizedAmount;
                return new GwaVM.BayarForm
                {
                    Judul = obj.RequestNumber + " · LPJ",
                    Info = selisih == 0 ? "Realisasi pas dengan dana; akun dan kategori boleh dikosongkan."
                        : selisih > 0 ? $"Sisa dana Rp {selisih:#,##0} dikembalikan (transaksi Pemasukan): pilih akun dan kategori pemasukan."
                        : $"Kelebihan pemakaian Rp {-selisih:#,##0} dibayarkan (transaksi Pengeluaran): pilih akun dan kategori pengeluaran.",
                    OnSave = "PengajuanDana.settle()",
                    Tombol = "Selesaikan LPJ",
                    ShowAmount = false,
                    AccountRequired = selisih != 0,
                    CategoryRequired = selisih != 0,
                    KategoriOpsi = Opsi.Kategori(selisih >= 0 ? CashFlowType.Income : CashFlowType.Expense),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey }
                };
            }

            public static void Selesai(GwaVM.BayarInput input, string user)
                => Load(input.Key).Settle(input.AccountId, input.CategoryId, Opsi.TanggalWajib(input.Date, "Tanggal"), user).Ok();

            public static void TambahRealisasi(RealisasiInput input, byte[]? bukti, IFormFile? meta, string user)
            {
                var obj = Load(input.Key);
                var real = FundingRealization.Save(new FundingRealization.NewRow
                {
                    FundingRequestId = obj.Id,
                    RealizationDate = Opsi.TanggalWajib(input.RealizationDate, "Tanggal"),
                    Description = input.Description ?? "",
                    Quantity = input.Quantity,
                    Unit = input.Unit,
                    Amount = input.Amount,
                    Notes = input.Notes
                }, user).Ok();
                if (bukti != null)
                {
                    real.UpdateReceipt(bukti, meta!.FileName, meta.ContentType, user).Ok();
                }
            }

            public static FundingRealization LoadRealisasi(string? key) => LibExtensions.Load<FundingRealization>(key, "Realisasi");
        }
    }
}
