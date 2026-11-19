using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Keuangan;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Inventaris;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Inventaris
{
    /// <summary>
    /// Menyusun invoice dan kuitansi peminjaman alat (PeminjamanReport) dari BorrowRequest, item, unit, dan transaksi
    /// Pemasukan Posted milik peminjaman. Penanda tangan = pemegang jabatan Bendahara di periode Aktif.
    /// </summary>
    public static class PeminjamanCetak
    {
        public static PeminjamanReport Invoice(string? key, string webRoot)
        {
            var obj = PeminjamanVM.Method.Load(key);
            var data = Data(obj);
            data.Stamp = obj.Status switch
            {
                BorrowRequestStatus.Rejected => "DITOLAK",
                BorrowRequestStatus.Cancelled => "DIBATALKAN",
                _ => obj.PaymentStatus == PaymentStatus.Lunas && obj.TotalDue > 0 ? "LUNAS" : null
            };
            return PeminjamanReport.Invoice(data, ReportBrand.Load(webRoot));
        }

        /// <summary>Kuitansi satu transaksi (trx = id transaksi) atau gabungan semua pembayaran (trx null).</summary>
        public static PeminjamanReport Kuitansi(string? key, int? trx, string webRoot)
        {
            var obj = PeminjamanVM.Method.Load(key);
            var data = Data(obj);
            if (data.Payments.Count == 0)
            {
                throw new ArgumentException("Peminjaman ini belum punya pembayaran; kuitansi belum bisa dicetak.");
            }
            PeminjamanReport.Payment? payment = null;
            if (trx.HasValue)
            {
                payment = data.Payments.FirstOrDefault(x => x.Id == trx.Value)
                    ?? throw new ArgumentException("Pembayaran tidak ditemukan pada peminjaman ini atau sudah dibatalkan.");
            }
            return PeminjamanReport.Kuitansi(data, payment, ReportBrand.Load(webRoot));
        }

        private static PeminjamanReport.Data Data(BorrowRequest obj)
        {
            var items = obj.GetItems(true).Select(x => new PeminjamanReport.Item(
                x.AssetName,
                x.TrackUnit == UnitTracking.PerUnit ? string.Join(", ", x.GetUnits(true).Select(u => u.UnitCode).OrderBy(c => c)) : null,
                x.Quantity, x.RentalDays, x.RentalFeePerDay, x.RentalFee, x.LateDays, x.LateFeePerDay, x.LateFee)).ToList();

            var payments = FinancialTransaction.GetDataList(transactionType: CashFlowType.Income, status: TransactionStatus.Posted)
                .Where(t => t.BorrowRequestId == obj.Id)
                .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
                .Select(t => new PeminjamanReport.Payment(t.Id, t.TransactionNumber, t.TransactionDate, t.CategoryName, t.AccountName, t.Amount))
                .ToList();

            var accounts = FinancialAccount.GetDataList(FilterAktif.Aktif)
                .Where(a => a.AccountType != FinancialAccountType.Cash && !string.IsNullOrWhiteSpace(a.AccountNumber))
                .Select(a => $"{(string.IsNullOrWhiteSpace(a.BankName) ? EnumHelper.Text(a.AccountType) : a.BankName)} {a.AccountNumber}"
                    + (string.IsNullOrWhiteSpace(a.AccountHolder) ? "" : " a.n. " + a.AccountHolder))
                .ToList();

            return new PeminjamanReport.Data
            {
                RequestNumber = obj.RequestNumber,
                BorrowerName = obj.BorrowerName ?? "-",
                BorrowerType = EnumHelper.Text(obj.BorrowerType),
                Institution = obj.BorrowerInstitution,
                Phone = obj.BorrowerPhone,
                Email = obj.BorrowerEmail,
                Purpose = obj.Purpose,
                Guarantee = obj.Guarantee,
                BorrowDate = obj.BorrowDate,
                ExpectedReturnDate = obj.ExpectedReturnDate,
                ActualReturnDate = obj.ActualReturnDate,
                Scheme = EnumHelper.Text(obj.RentalScheme),
                FixedRate = obj.RentalScheme == RentalScheme.TarifTetap,
                Voluntary = obj.RentalScheme == RentalScheme.Seikhlasnya,
                Status = EnumHelper.Text(obj.Status),
                Returned = obj.Status == BorrowRequestStatus.Returned,
                TotalRentalFee = obj.TotalRentalFee,
                TotalLateFee = obj.TotalLateFee,
                LateFeeWaived = obj.LateFeeWaived,
                WaiveNote = obj.WaiveNote,
                TotalDue = obj.TotalDue,
                PaidAmount = obj.PaidAmount,
                Items = items,
                Payments = payments,
                Accounts = accounts,
                Treasurer = Treasurer()
            };
        }

        // Bendahara yang sedang menjabat di periode Aktif; tanpa nama bila belum ada.
        private static DocumentReport.Signer Treasurer()
        {
            var period = ManagementPeriod.GetActive();
            var holder = period == null ? null : ManagementAssignment.GetDataList(period.Id, isCurrent: AssignmentPeriod.Berjalan)
                .Where(x => x.PositionName.StartsWith("Bendahara", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.PositionName.Length).ThenByDescending(x => x.StartDate)
                .FirstOrDefault();
            return new DocumentReport.Signer("Bendahara", holder?.MemberName, null);
        }
    }
}
