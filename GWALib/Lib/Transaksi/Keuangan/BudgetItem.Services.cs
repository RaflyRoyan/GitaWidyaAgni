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
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Keuangan
{
    public partial class BudgetItem
    {
        #region SYNC
        public static List<BudgetItem> GetDataList(int budgetId)
        {
            var list = FilterQuery(budgetId).ToList().MappingData<BudgetItem, EF.TBudgetItem>();
            SetRealized(list, RealizedQuery(budgetId).ToList());
            return list;
        }
        public static Response<BudgetItem> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                Budget.RecalculateTotal(context, Budget.GetEditableBudget(context, row.BudgetId), userId);
                context.SaveChanges();
                return new BudgetItem(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BudgetItem>(msg));
        }
        public Response<BudgetItem> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var budget = SetRow(context, row, userId);
                Budget.RecalculateTotal(context, budget, userId);
                context.SaveChanges();
                return new BudgetItem(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BudgetItem>(msg));
        }
        public Response<BudgetItem> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var budget = Budget.GetEditableBudget(context, row.BudgetId);
                var data = new BudgetItem(Reload(row.Id));
                context.TBudgetItems.Remove(row);
                Budget.RecalculateTotal(context, budget, userId);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BudgetItem>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<BudgetItem>> GetDataListAsync(int budgetId)
        {
            var list = await (await FilterQuery(budgetId).ToListAsync()).MappingDataAsync<BudgetItem, EF.TBudgetItem>();
            SetRealized(list, await RealizedQuery(budgetId).ToListAsync());
            return list;
        }
        public static async Task<Response<BudgetItem>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                Budget.RecalculateTotal(context, Budget.GetEditableBudget(context, row.BudgetId), userId);
                await context.SaveChangesAsync();
                return new BudgetItem(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BudgetItem>(msg));
        }
        public async Task<Response<BudgetItem>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var budget = SetRow(context, row, userId);
                Budget.RecalculateTotal(context, budget, userId);
                await context.SaveChangesAsync();
                return new BudgetItem(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BudgetItem>(msg));
        }
        public async Task<Response<BudgetItem>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var budget = Budget.GetEditableBudget(context, row.BudgetId);
                var data = new BudgetItem(Reload(row.Id));
                context.TBudgetItems.Remove(row);
                Budget.RecalculateTotal(context, budget, userId);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BudgetItem>(msg));
        }
        #endregion

        #region INTERNAL
        // Item dari anggaran yang sudah Approved; dipakai transaksi keuangan dan pengajuan dana.
        internal static EF.TBudgetItem GetApprovedItem(EF.ModelContext context, int budgetItemId)
        {
            var row = context.TBudgetItems.Include(x => x.Budget).SingleOrDefault(x => x.Id == budgetItemId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Anggaran"));
            if (row.Budget.Status != (int)BudgetStatus.Approved)
            {
                throw new ArgumentException(ExceptionMessage.BUDGET_NOT_APPROVED);
            }
            return row;
        }
        // Tidak menghitung ulang total; pemanggil (Save / Budget.CreateRow) yang menghitung.
        internal static EF.TBudgetItem CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var budget = Budget.GetEditableBudget(context, newrow.BudgetId);
            var programId = newrow.ProgramId > 0 ? newrow.ProgramId : null;
            var name = ValidateData(context, budget, newrow.CategoryId, programId, newrow.Name, newrow.Quantity, newrow.UnitPrice);

            var row = new EF.TBudgetItem
            {
                Id = SequenceUtility.GetNextId<EF.TBudgetItem>(context),
                BudgetId = budget.Id,
                CategoryId = newrow.CategoryId,
                ProgramId = programId,
                Name = name,
                Quantity = newrow.Quantity,
                Unit = TransaksiHelper.ToNullable(newrow.Unit),
                UnitPrice = newrow.UnitPrice,
                Amount = Math.Round(newrow.Quantity * newrow.UnitPrice, 2),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TBudgetItems.Add(row);
            return row;
        }
        #endregion

        #region HELPER
        private record Realized(int BudgetItemId, decimal Amount);

        private static EF.TBudgetItem Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TBudgetItem> FilterQuery(int budgetId)
        {
            return _Querys.Where(x => x.BudgetId == budgetId).OrderBy(x => x.Id);
        }
        private static IQueryable<Realized> RealizedQuery(int budgetId)
        {
            return DBClass.GetContext().TFinancialTransactions
                .Where(x => x.BudgetItem!.BudgetId == budgetId && x.Status == (int)TransactionStatus.Posted)
                .GroupBy(x => x.BudgetItemId!.Value)
                .Select(g => new Realized(g.Key, g.Sum(x => x.TransactionType == (int)CashFlowType.Expense ? x.Amount : -x.Amount)));
        }
        // Realisasi = pengeluaran dikurangi pemasukan (mis. pengembalian sisa dana) yang memakai item.
        private static void SetRealized(List<BudgetItem> list, List<Realized> realized)
        {
            var map = realized.ToDictionary(x => x.BudgetItemId, x => x.Amount);
            foreach (var item in list)
            {
                item.RealizedAmount = map.GetValueOrDefault(item.Id);
            }
        }
        private static EF.TBudgetItem GetRow(EF.ModelContext context, int id)
        {
            return context.TBudgetItems.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Anggaran"));
        }
        private static async Task<EF.TBudgetItem> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TBudgetItems.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Anggaran"));
        }
        private static string ValidateData(EF.ModelContext context, EF.TBudget budget, int categoryId, int? programId, string? name, decimal quantity, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Item Anggaran"));
            }
            TransaksiHelper.ValidatePositive(quantity, "Jumlah");
            TransaksiHelper.ValidatePositive(unitPrice, "Harga Satuan");
            FinancialCategory.ValidateActiveCategory(context, categoryId, null);
            if (programId != null)
            {
                var programPeriod = context.TPrograms.Where(x => x.Id == programId).Select(x => (int?)x.PeriodId).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
                if (programPeriod != budget.PeriodId)
                {
                    throw new ArgumentException(ExceptionMessage.BUDGET_PROGRAM_PERIOD_MISMATCH);
                }
            }
            return name.Trim();
        }
        // Anggaran induk tetap.
        private EF.TBudget SetRow(EF.ModelContext context, EF.TBudgetItem row, int userId)
        {
            var budget = Budget.GetEditableBudget(context, row.BudgetId);
            var programId = ProgramId > 0 ? ProgramId : null;
            var name = ValidateData(context, budget, CategoryId, programId, Name, Quantity, UnitPrice);

            row.CategoryId = CategoryId;
            row.ProgramId = programId;
            row.Name = name;
            row.Quantity = Quantity;
            row.Unit = TransaksiHelper.ToNullable(Unit);
            row.UnitPrice = UnitPrice;
            row.Amount = Math.Round(Quantity * UnitPrice, 2);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return budget;
        }
        #endregion
    }
}
