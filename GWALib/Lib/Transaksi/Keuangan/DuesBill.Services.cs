using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Keuangan;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Keuangan
{
    public partial class DuesBill
    {
        public const string ONCE_BILL_PERIOD = "SEKALI";

        #region SYNC
        public static List<DuesBill> GetDataList(int? duesSettingId = null, int? memberId = null, string? billPeriod = null, DuesBillStatus? status = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(duesSettingId, memberId, billPeriod, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<DuesBill, EF.TDuesBill>();
        }
        // memberIds null / kosong = semua anggota aktif. Anggota yang sudah punya tagihan periode ini dilewati; mengembalikan jumlah tagihan baru.
        public static Response<int> Generate(int duesSettingId, string? billPeriod, DateOnly? dueDate, List<int>? memberIds, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = CreateRows(context, duesSettingId, billPeriod, dueDate, memberIds, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public Response<DuesBill> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        // categoryId null = kategori dari pengaturan iuran. Transaksi dan status tagihan disimpan dalam satu transaksi DB.
        public Response<DuesBill> Pay(int accountId, DateOnly paidDate, int? categoryId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetPay(context, row, accountId, paidDate, categoryId, userId);
                context.SaveChanges();
                transaction.Commit();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public Response<DuesBill> CancelPayment(string? reason, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetCancelPayment(context, row, reason, userId);
                context.SaveChanges();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public Response<DuesBill> Waive(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetStatus(row, DuesBillStatus.Waived, userId);
                context.SaveChanges();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public Response<DuesBill> Unwaive(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetStatus(row, DuesBillStatus.Unpaid, userId);
                context.SaveChanges();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public Response<DuesBill> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((DuesBillStatus)row.Status, "menghapus tagihan iuran", DuesBillStatus.Unpaid);
                var data = new DuesBill(Reload(row.Id));
                context.TDuesBills.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DuesBill>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<DuesBill>> GetDataListAsync(int? duesSettingId = null, int? memberId = null, string? billPeriod = null, DuesBillStatus? status = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(duesSettingId, memberId, billPeriod, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<DuesBill, EF.TDuesBill>();
        }
        public static async Task<Response<int>> GenerateAsync(int duesSettingId, string? billPeriod, DateOnly? dueDate, List<int>? memberIds, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = CreateRows(context, duesSettingId, billPeriod, dueDate, memberIds, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public async Task<Response<DuesBill>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public async Task<Response<DuesBill>> PayAsync(int accountId, DateOnly paidDate, int? categoryId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetPay(context, row, accountId, paidDate, categoryId, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public async Task<Response<DuesBill>> CancelPaymentAsync(string? reason, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetCancelPayment(context, row, reason, userId);
                await context.SaveChangesAsync();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public async Task<Response<DuesBill>> WaiveAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetStatus(row, DuesBillStatus.Waived, userId);
                await context.SaveChangesAsync();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public async Task<Response<DuesBill>> UnwaiveAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetStatus(row, DuesBillStatus.Unpaid, userId);
                await context.SaveChangesAsync();
                return new DuesBill(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesBill>(msg));
        }
        public async Task<Response<DuesBill>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((DuesBillStatus)row.Status, "menghapus tagihan iuran", DuesBillStatus.Unpaid);
                var data = new DuesBill(Reload(row.Id));
                context.TDuesBills.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DuesBill>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TDuesBill Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TDuesBill> FilterQuery(int? duesSettingId, int? memberId, string? billPeriod, DuesBillStatus? status)
        {
            var period = string.IsNullOrWhiteSpace(billPeriod) ? null : billPeriod.Trim().ToUpperInvariant();
            return _Querys.Where(x => (duesSettingId == null || x.DuesSettingId == duesSettingId)
                    && (memberId == null || x.MemberId == memberId)
                    && (period == null || x.BillPeriod == period)
                    && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.BillPeriod).ThenBy(x => x.Member.Fullname);
        }
        private static EF.TDuesBill GetRow(EF.ModelContext context, int id)
        {
            return context.TDuesBills.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Tagihan Iuran"));
        }
        private static async Task<EF.TDuesBill> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TDuesBills.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Tagihan Iuran"));
        }
        // Periode tagihan dinormalisasi huruf besar dan dicek sesuai frekuensi.
        private static string NormalizeBillPeriod(DuesFrequency frequency, string? billPeriod)
        {
            var period = billPeriod?.Trim().ToUpperInvariant() ?? string.Empty;
            var (pattern, format) = frequency switch
            {
                DuesFrequency.Monthly => (@"^\d{4}-(0[1-9]|1[0-2])$", "yyyy-MM, mis. 2026-10"),
                DuesFrequency.Semester => (@"^\d{4}-(GANJIL|GENAP)$", "yyyy-GANJIL atau yyyy-GENAP"),
                DuesFrequency.Yearly => (@"^\d{4}$", "yyyy, mis. 2026"),
                _ => (null, null)
            };
            if (pattern == null)
            {
                return period.Length == 0 ? ONCE_BILL_PERIOD : period;
            }
            if (!Regex.IsMatch(period, pattern))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DUES_BILL_PERIOD_INVALID, frequency.GetDescription(), format));
            }
            return period;
        }
        private static int CreateRows(EF.ModelContext context, int duesSettingId, string? billPeriod, DateOnly? dueDate, List<int>? memberIds, int userId)
        {
            var setting = DuesSetting.GetActiveDuesSetting(context, duesSettingId);
            var period = NormalizeBillPeriod((DuesFrequency)setting.Frequency, billPeriod);
            TransaksiHelper.ValidatePositive(setting.Amount, "Nominal Iuran");

            var ids = memberIds?.Where(x => x > 0).Distinct().ToList();
            if (ids != null && ids.Count > 0)
            {
                var activeIds = context.MMembers.Where(x => ids.Contains(x.Id) && x.Status == (int)MemberStatus.Active).Select(x => x.Id).ToList();
                if (activeIds.Count != ids.Count)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Sebagian anggota"));
                }
            }
            var members = context.MMembers.Where(x => x.Status == (int)MemberStatus.Active);
            if (ids != null && ids.Count > 0)
            {
                members = members.Where(x => ids.Contains(x.Id));
            }
            var targets = members
                .Where(x => !context.TDuesBills.Any(b => b.DuesSettingId == setting.Id && b.MemberId == x.Id && b.BillPeriod == period))
                .Select(x => x.Id)
                .ToList();
            if (targets.Count == 0)
            {
                return 0;
            }

            var nextId = SequenceUtility.ReserveIds<EF.TDuesBill>(context, targets.Count);
            var now = DateTime.Now;
            foreach (var memberId in targets)
            {
                var row = new EF.TDuesBill
                {
                    Id = nextId++,
                    DuesSettingId = setting.Id,
                    MemberId = memberId,
                    BillPeriod = period,
                    Amount = setting.Amount,
                    DueDate = dueDate,
                    Status = (int)DuesBillStatus.Unpaid,
                    InsDate = now,
                    InsBy = userId
                };
                GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
                context.TDuesBills.Add(row);
            }
            return targets.Count;
        }
        private void SetRow(EF.TDuesBill row, int userId)
        {
            TransaksiHelper.ValidateStatus((DuesBillStatus)row.Status, "mengubah tagihan iuran", DuesBillStatus.Unpaid);
            TransaksiHelper.ValidatePositive(Amount, "Nominal Tagihan");
            row.Amount = Amount;
            row.DueDate = DueDate;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetPay(EF.ModelContext context, EF.TDuesBill row, int accountId, DateOnly paidDate, int? categoryId, int userId)
        {
            TransaksiHelper.ValidateStatus((DuesBillStatus)row.Status, "membayar tagihan iuran", DuesBillStatus.Unpaid);
            var setting = context.MDuesSettings.Single(x => x.Id == row.DuesSettingId);
            var category = (categoryId > 0 ? categoryId : setting.CategoryId)
                ?? throw new ArgumentException(ExceptionMessage.DUES_CATEGORY_REQUIRED);
            var memberName = context.MMembers.Where(x => x.Id == row.MemberId).Select(x => x.Fullname).Single();

            var trx = FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
            {
                AccountId = accountId,
                CategoryId = category,
                PeriodId = FinancialTransaction.OpenPeriodOrDefault(context, setting.PeriodId),
                TransactionType = CashFlowType.Income,
                Amount = row.Amount,
                TransactionDate = paidDate,
                Description = $"Iuran {setting.Name} {row.BillPeriod} - {memberName}"
            }, null, true, userId);

            var now = DateTime.Now;
            row.Status = (int)DuesBillStatus.Paid;
            row.PaidAt = paidDate.ToDateTime(TimeOnly.FromDateTime(now));
            row.TransactionId = trx.Id;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static void SetCancelPayment(EF.ModelContext context, EF.TDuesBill row, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((DuesBillStatus)row.Status, "membatalkan pembayaran iuran", DuesBillStatus.Paid);
            if (row.TransactionId != null)
            {
                FinancialTransaction.VoidRow(context, row.TransactionId.Value, reason ?? "Pembayaran iuran dibatalkan", userId);
            }
            row.Status = (int)DuesBillStatus.Unpaid;
            row.PaidAt = null;
            row.TransactionId = null;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Waived dari Unpaid, Unpaid (unwaive) dari Waived.
        private static void SetStatus(EF.TDuesBill row, DuesBillStatus target, int userId)
        {
            if (target == DuesBillStatus.Waived)
            {
                TransaksiHelper.ValidateStatus((DuesBillStatus)row.Status, "membebaskan tagihan iuran", DuesBillStatus.Unpaid);
            }
            else
            {
                TransaksiHelper.ValidateStatus((DuesBillStatus)row.Status, "membatalkan pembebasan iuran", DuesBillStatus.Waived);
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
