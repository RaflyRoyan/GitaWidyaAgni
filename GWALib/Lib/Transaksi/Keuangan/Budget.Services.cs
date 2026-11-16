using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Keuangan
{
    public partial class Budget
    {
        #region SYNC
        public List<BudgetItem> GetItems(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = BudgetItem.GetDataList(Id);
            }
            return ItemList;
        }
        public List<BudgetDocument> GetDocuments(bool refresh)
        {
            if (refresh || DocumentList == null)
            {
                DocumentList = BudgetDocument.GetDataList(Id);
            }
            return DocumentList;
        }
        public static List<Budget> GetDataList(int? periodId = null, BudgetStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Budget, EF.TBudget>();
        }
        public static Response<Budget> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Budget>(msg));
        }
        public Response<Budget> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        public Response<Budget> Submit(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetSubmit(context, row, userId);
                context.SaveChanges();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        public Response<Budget> Approve(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetDecision(context, row, BudgetStatus.Approved, userId);
                context.SaveChanges();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        public Response<Budget> Reject(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetDecision(context, row, BudgetStatus.Rejected, userId);
                context.SaveChanges();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        // Hanya Draft yang bisa dihapus (item dan dokumen ikut terhapus lewat ON DELETE CASCADE).
        public Response<Budget> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((BudgetStatus)row.Status, "menghapus anggaran", BudgetStatus.Draft);
                var data = new Budget(Reload(row.Id));
                context.TBudgets.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Budget>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<BudgetItem>> GetItemsAsync(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = await BudgetItem.GetDataListAsync(Id);
            }
            return ItemList;
        }
        public async Task<List<BudgetDocument>> GetDocumentsAsync(bool refresh)
        {
            if (refresh || DocumentList == null)
            {
                DocumentList = await BudgetDocument.GetDataListAsync(Id);
            }
            return DocumentList;
        }
        public static async Task<List<Budget>> GetDataListAsync(int? periodId = null, BudgetStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Budget, EF.TBudget>();
        }
        public static async Task<Response<Budget>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Budget>(msg));
        }
        public async Task<Response<Budget>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        public async Task<Response<Budget>> SubmitAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetSubmit(context, row, userId);
                await context.SaveChangesAsync();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        public async Task<Response<Budget>> ApproveAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetDecision(context, row, BudgetStatus.Approved, userId);
                await context.SaveChangesAsync();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        public async Task<Response<Budget>> RejectAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetDecision(context, row, BudgetStatus.Rejected, userId);
                await context.SaveChangesAsync();
                return new Budget(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Budget>(msg));
        }
        public async Task<Response<Budget>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((BudgetStatus)row.Status, "menghapus anggaran", BudgetStatus.Draft);
                var data = new Budget(Reload(row.Id));
                context.TBudgets.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Budget>(msg));
        }
        #endregion

        #region INTERNAL
        // Anggaran yang masih boleh diubah (Draft / Rejected); dipakai BudgetItem.
        // Local: anggaran yang baru di-Add dalam command yang sama (Save dengan Items) belum ada di DB.
        internal static EF.TBudget GetEditableBudget(EF.ModelContext context, int budgetId)
        {
            var row = context.TBudgets.Local.SingleOrDefault(x => x.Id == budgetId)
                ?? context.TBudgets.SingleOrDefault(x => x.Id == budgetId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggaran"));
            TransaksiHelper.ValidateStatus((BudgetStatus)row.Status, "mengubah anggaran", BudgetStatus.Draft, BudgetStatus.Rejected);
            return row;
        }
        // Total = jumlah amount item yang tersisa setelah perubahan di context (item yang dihapus tidak ada di Local).
        internal static void RecalculateTotal(EF.ModelContext context, EF.TBudget budget, int userId)
        {
            if (context.Entry(budget).State != EntityState.Added)
            {
                context.TBudgetItems.Where(x => x.BudgetId == budget.Id).Load();
            }
            budget.TotalAmount = context.TBudgetItems.Local.Where(x => x.BudgetId == budget.Id).Sum(x => x.Amount);
            budget.UpdDate = DateTime.Now;
            budget.UpdBy = userId;
        }
        #endregion

        #region HELPER
        private static EF.TBudget Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TBudget> FilterQuery(int? periodId, BudgetStatus? status)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId) && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.PeriodId).ThenBy(x => x.Name);
        }
        private static EF.TBudget GetRow(EF.ModelContext context, int id)
        {
            return context.TBudgets.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggaran"));
        }
        private static async Task<EF.TBudget> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TBudgets.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggaran"));
        }
        private static string ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Anggaran"));
            }
            return name.Trim();
        }
        private static EF.TBudget CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ManagementPeriod.ValidateOpenPeriod(context, newrow.PeriodId);
            var row = new EF.TBudget
            {
                Id = SequenceUtility.GetNextId<EF.TBudget>(context),
                PeriodId = newrow.PeriodId,
                Name = ValidateName(newrow.Name),
                TotalAmount = 0,
                Status = (int)BudgetStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TBudgets.Add(row);

            foreach (var item in newrow.Items ?? new())
            {
                item.BudgetId = row.Id;
                BudgetItem.CreateRow(context, item, userId);
            }
            RecalculateTotal(context, row, userId);
            row.UpdDate = null;
            row.UpdBy = null;
            return row;
        }
        // Periode tetap; hanya nama yang diubah di sini (item lewat BudgetItem).
        private void SetRow(EF.TBudget row, int userId)
        {
            TransaksiHelper.ValidateStatus((BudgetStatus)row.Status, "mengubah anggaran", BudgetStatus.Draft, BudgetStatus.Rejected);
            row.Name = ValidateName(Name);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetSubmit(EF.ModelContext context, EF.TBudget row, int userId)
        {
            TransaksiHelper.ValidateStatus((BudgetStatus)row.Status, "mengajukan anggaran", BudgetStatus.Draft, BudgetStatus.Rejected);
            ManagementPeriod.ValidateOpenPeriod(context, row.PeriodId);
            if (!context.TBudgetItems.Any(x => x.BudgetId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.BUDGET_ITEM_REQUIRED);
            }
            row.Status = (int)BudgetStatus.Submitted;
            row.ApprovedBy = null;
            row.ApprovedAt = null;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // ApprovedBy / ApprovedAt mencatat siapa yang memutuskan (disetujui maupun ditolak).
        private static void SetDecision(EF.ModelContext context, EF.TBudget row, BudgetStatus decision, int userId)
        {
            var action = decision == BudgetStatus.Approved ? "menyetujui anggaran" : "menolak anggaran";
            TransaksiHelper.ValidateStatus((BudgetStatus)row.Status, action, BudgetStatus.Submitted);
            if (decision == BudgetStatus.Approved)
            {
                ManagementPeriod.ValidateOpenPeriod(context, row.PeriodId);
            }
            var now = DateTime.Now;
            row.Status = (int)decision;
            row.ApprovedBy = userId;
            row.ApprovedAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
