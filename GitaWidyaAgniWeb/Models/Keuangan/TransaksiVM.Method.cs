using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Keuangan;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Keuangan
{
    public partial class TransaksiVM
    {
        public class Method
        {
            // ---------- Transaksi
            public static IEnumerable<TransaksiRow> GetTransaksiList()
            {
                return FinancialTransaction.GetDataList().Select(x => new TransaksiRow
                {
                    EncDecrKey = x.EncDecrKey,
                    TransactionNumber = x.TransactionNumber,
                    TransactionDate = x.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    Jenis = (int)x.TransactionType,
                    JenisText = EnumHelper.Text(x.TransactionType),
                    Akun = x.AccountName,
                    Kategori = x.CategoryName,
                    Periode = x.PeriodName,
                    Amount = x.Amount,
                    Description = x.Description,
                    Sumber = x.FundingRequestId.HasValue ? "Pengajuan Dana"
                        : x.ProcurementId.HasValue ? "Pengadaan"
                        : x.SalesOrderId.HasValue ? "Penjualan"
                        : x.BorrowRequestId.HasValue ? "Peminjaman"
                        : x.SponsorshipId.HasValue ? "Sponsorship" : null,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    JumlahLampiran = x.GetAttachments(true).Count
                }).OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.TransactionNumber);
            }

            public static FinancialTransaction Load(string? key) => LibExtensions.Load<FinancialTransaction>(key, "Transaksi");

            public static void SimpanTransaksi(NewTransaksi input, string user)
                => FinancialTransaction.Save(new FinancialTransaction.NewRow
                {
                    TransactionType = (CashFlowType)input.TransactionType,
                    AccountId = input.AccountId ?? 0,
                    CategoryId = input.CategoryId ?? 0,
                    PeriodId = input.PeriodId,
                    Amount = input.Amount,
                    TransactionDate = Opsi.TanggalWajib(input.TransactionDate, "Tanggal"),
                    Description = input.Description,
                    BudgetItemId = input.BudgetItemId,
                    ActivityId = input.ActivityId,
                    ProgramId = input.ProgramId,
                    CompetitionId = input.CompetitionId
                }, user).Ok();

            public static void UpdateTransaksi(string key, NewTransaksi input, string user)
            {
                var obj = Load(key);
                obj.TransactionType = (CashFlowType)input.TransactionType;
                obj.AccountId = input.AccountId ?? 0;
                obj.CategoryId = input.CategoryId ?? 0;
                obj.PeriodId = input.PeriodId;
                obj.Amount = input.Amount;
                obj.TransactionDate = Opsi.TanggalWajib(input.TransactionDate, "Tanggal");
                obj.Description = input.Description;
                obj.BudgetItemId = input.BudgetItemId;
                obj.ActivityId = input.ActivityId;
                obj.ProgramId = input.ProgramId;
                obj.CompetitionId = input.CompetitionId;
                obj.Update(user).Ok();
            }

            public static void TambahLampiran(string? key, byte[]? data, IFormFile? meta, string user)
            {
                var obj = Load(key);
                if (data == null)
                {
                    throw new ArgumentException("Pilih berkas terlebih dahulu.");
                }
                FinancialAttachment.Save(new FinancialAttachment.NewRow { TransactionId = obj.Id, FileData = data, FileName = meta!.FileName, FileType = meta.ContentType }, user).Ok();
            }

            public static FinancialAttachment LoadLampiran(string? key) => LibExtensions.Load<FinancialAttachment>(key, "Lampiran");

            // ---------- Saldo periode
            public static int BukaSaldo(BukaSaldoInput input, string user) => PeriodBalance.Open(input.PeriodId, input.AccountId, user).Ok();
            public static PeriodBalance LoadSaldo(string? key) => LibExtensions.Load<PeriodBalance>(key, "Saldo periode");

            // ---------- Akun
            public static void SimpanAkun(NewAkun input, string user)
                => FinancialAccount.Save(new FinancialAccount.NewRow
                {
                    Name = input.Name ?? "",
                    AccountType = (FinancialAccountType)input.AccountType,
                    BankName = input.BankName,
                    AccountNumber = input.AccountNumber,
                    AccountHolder = input.AccountHolder,
                    OpeningBalance = input.OpeningBalance
                }, user).Ok();

            public static void UpdateAkun(string key, NewAkun input, string user)
            {
                var obj = LibExtensions.Load<FinancialAccount>(key, "Akun kas");
                obj.Name = input.Name ?? "";
                obj.AccountType = (FinancialAccountType)input.AccountType;
                obj.BankName = input.BankName;
                obj.AccountNumber = input.AccountNumber;
                obj.AccountHolder = input.AccountHolder;
                obj.OpeningBalance = input.OpeningBalance;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void HapusAkun(string? key, string user) => LibExtensions.Load<FinancialAccount>(key, "Akun kas").Delete(user).Ok();

            // ---------- Kategori
            public static void SimpanKategori(NewKategori input, string user)
                => FinancialCategory.Save(new FinancialCategory.NewRow { Name = input.Name ?? "", CategoryType = (CashFlowType)input.CategoryType }, user).Ok();

            public static void UpdateKategori(string key, NewKategori input, string user)
            {
                var obj = LibExtensions.Load<FinancialCategory>(key, "Kategori");
                obj.Name = input.Name ?? "";
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void HapusKategori(string? key, string user) => LibExtensions.Load<FinancialCategory>(key, "Kategori").Delete(user).Ok();
        }
    }
}
