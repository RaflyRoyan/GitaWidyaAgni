using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Keuangan;
using GWALib.Lib.Transaksi.Pengadaan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Pengadaan
{
    public partial class PengadaanVM
    {
        public class Method
        {
            public static ProcurementRequest Load(string? key) => LibExtensions.Load<ProcurementRequest>(key, "Pengadaan");

            public static void Simpan(NewRow input, string user)
                => ProcurementRequest.Save(new ProcurementRequest.NewRow
                {
                    PeriodId = input.PeriodId,
                    RequestedByMemberId = input.RequestedByMemberId,
                    RequestDate = Opsi.TanggalWajib(input.RequestDate, "Tanggal permohonan"),
                    Title = input.Title ?? "",
                    Reason = input.Reason ?? "",
                    ActivityId = input.ActivityId,
                    ProgramId = input.ProgramId,
                    BudgetItemId = input.BudgetItemId,
                    PaymentMethod = (ProcurementPaymentMethod)input.PaymentMethod,
                    NeededByDate = Opsi.Tanggal(input.NeededByDate)
                }, user).Ok();

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.PeriodId = input.PeriodId;
                if (input.RequestedByMemberId.HasValue)
                {
                    obj.RequestedByMemberId = input.RequestedByMemberId.Value;
                }
                obj.RequestDate = Opsi.TanggalWajib(input.RequestDate, "Tanggal permohonan");
                obj.Title = input.Title ?? "";
                obj.Reason = input.Reason ?? "";
                obj.ActivityId = input.ActivityId;
                obj.ProgramId = input.ProgramId;
                obj.BudgetItemId = input.BudgetItemId;
                obj.PaymentMethod = (ProcurementPaymentMethod)input.PaymentMethod;
                obj.NeededByDate = Opsi.Tanggal(input.NeededByDate);
                obj.Update(user).Ok();
            }

            // ---------- Item
            public static void TambahItem(ItemInput input, string user)
            {
                var obj = Load(input.Key);
                ProcurementItem.Save(new ProcurementItem.NewRow
                {
                    RequestId = obj.Id,
                    ItemName = input.ItemName ?? "",
                    Specification = input.Specification,
                    IsAsset = (ProcurementItemKind)input.IsAsset,
                    AssetCategoryId = input.AssetCategoryId,
                    Quantity = input.Quantity,
                    Unit = string.IsNullOrWhiteSpace(input.Unit) ? "pcs" : input.Unit,
                    EstimatedUnitPrice = input.EstimatedUnitPrice,
                    ExternalPartyId = input.ExternalPartyId
                }, user).Ok();
            }

            public static ProcurementItem LoadItem(string? key) => LibExtensions.Load<ProcurementItem>(key, "Item pengadaan");

            public static void HargaAktual(AktualInput input, string user)
            {
                var item = LoadItem(input.Key);
                item.SetActual(input.Price, item.ExternalPartyId, user).Ok();
            }

            // ---------- Penawaran
            public static void TambahPenawaran(PenawaranInput input, byte[]? file, IFormFile? meta, string user)
            {
                var obj = Load(input.Key);
                ProcurementQuotation.Save(new ProcurementQuotation.NewRow
                {
                    RequestId = obj.Id,
                    ItemId = input.ItemId,
                    ExternalPartyId = input.ExternalPartyId,
                    VendorName = input.VendorName,
                    QuotedAmount = input.QuotedAmount,
                    Notes = input.Notes,
                    File = file,
                    FileName = meta?.FileName,
                    ContentType = meta?.ContentType
                }, user).Ok();
            }

            public static ProcurementQuotation LoadPenawaran(string? key) => LibExtensions.Load<ProcurementQuotation>(key, "Penawaran");

            // ---------- Beli & bayar
            public static List<GwaVM.OpsiItem> OpsiPengajuanDana()
                => FundingRequest.GetDataList().Where(x => x.Status == FundingRequestStatus.Approved || x.Status == FundingRequestStatus.Disbursed)
                    .Select(x => new GwaVM.OpsiItem(x.Id, x.RequestNumber + " · " + x.Purpose + " · Rp " + x.Amount.ToString("#,##0"))).ToList();

            public static void Beli(BeliInput input, string user) => Load(input.Key).Purchase(input.FundingRequestId, user).Ok();

            public static GwaVM.BayarForm FormBayar(string encKey)
            {
                var obj = Load(encKey);
                return new GwaVM.BayarForm
                {
                    Judul = obj.RequestNumber + " · " + obj.Title,
                    Info = $"Estimasi Rp {obj.TotalEstimate:#,##0} · sudah dibayar Rp {obj.TotalActual:#,##0}. Dicatat sebagai transaksi Pengeluaran.",
                    OnSave = "Pengadaan.pay()",
                    Tombol = "Catat Pembayaran",
                    KategoriOpsi = Opsi.Kategori(CashFlowType.Expense),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey, Amount = Math.Max(0, obj.TotalEstimate - obj.TotalActual) }
                };
            }

            public static void Bayar(GwaVM.BayarInput input, string user)
                => Load(input.Key).RecordPayment(input.AccountId ?? 0, input.CategoryId ?? 0, input.Amount ?? 0, Opsi.TanggalWajib(input.Date, "Tanggal"), user).Ok();

            public static void BatalBayar(BatalBayarInput input, string user) => Load(input.Key).CancelPayment(input.TransactionId, input.Keterangan, user).Ok();

            // ---------- Penerimaan
            public static void SimpanTerima(TerimaInput input, string user)
            {
                var obj = Load(input.Key);
                var items = obj.GetItems(true).ToDictionary(x => x.Id);
                var lines = new List<ProcurementReceiptItem.NewRow>();
                foreach (var l in (input.Lines ?? new()).Where(x => x.Quantity > 0))
                {
                    var line = new ProcurementReceiptItem.NewRow { ItemId = l.ItemId, Quantity = l.Quantity, ItemCondition = (ItemCondition)l.Condition };
                    if (!string.IsNullOrWhiteSpace(l.AssetCode) && items.TryGetValue(l.ItemId, out var it) && it.IsAsset == ProcurementItemKind.Aset)
                    {
                        line.Asset = new Asset.NewRow { AssetCode = l.AssetCode.Trim().ToUpperInvariant(), Name = it.ItemName, Unit = it.Unit };
                    }
                    lines.Add(line);
                }
                if (lines.Count == 0)
                {
                    throw new ArgumentException("Isi jumlah diterima minimal untuk satu barang.");
                }
                ProcurementReceipt.Save(new ProcurementReceipt.NewRow
                {
                    RequestId = obj.Id,
                    ReceiptDate = Opsi.TanggalWajib(input.ReceiptDate, "Tanggal terima"),
                    ReceivedByMemberId = input.ReceivedByMemberId,
                    Notes = input.Notes,
                    Items = lines
                }, user).Ok();
            }

            public static ProcurementReceipt LoadTerima(string? key) => LibExtensions.Load<ProcurementReceipt>(key, "Penerimaan");
        }
    }
}
