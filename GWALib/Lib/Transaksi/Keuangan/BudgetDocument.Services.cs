using GWALib.General;
using GWALib.General.Utilities;
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
    public partial class BudgetDocument
    {
        public const int MAX_FILE_BYTES = 10 * 1024 * 1024;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TBudgetDocuments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.FileType }).SingleOrDefault();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.FileType);
        }
        public static List<BudgetDocument> GetDataList(int BudgetId)
        {
            return FilterQuery(BudgetId).ToList().MappingData<BudgetDocument, EF.TBudgetDocument>();
        }
        public static Response<BudgetDocument> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new BudgetDocument(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BudgetDocument>(msg));
        }
        public Response<BudgetDocument> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateBudget(context, row.BudgetId);
                var data = new BudgetDocument(row);
                context.TBudgetDocuments.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BudgetDocument>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TBudgetDocuments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.FileType }).SingleOrDefaultAsync();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.FileType);
        }
        public static async Task<List<BudgetDocument>> GetDataListAsync(int BudgetId)
        {
            return await (await FilterQuery(BudgetId).ToListAsync()).MappingDataAsync<BudgetDocument, EF.TBudgetDocument>();
        }
        public static async Task<Response<BudgetDocument>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new BudgetDocument(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BudgetDocument>(msg));
        }
        public async Task<Response<BudgetDocument>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateBudget(context, row.BudgetId);
                var data = new BudgetDocument(row);
                context.TBudgetDocuments.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BudgetDocument>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TBudgetDocument> FilterQuery(int BudgetId)
        {
            return _Querys.Where(x => x.BudgetId == BudgetId).OrderBy(x => x.Id);
        }
        private static EF.TBudgetDocument GetRow(EF.ModelContext context, int id)
        {
            return context.TBudgetDocuments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Dokumen Anggaran"));
        }
        private static async Task<EF.TBudgetDocument> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TBudgetDocuments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Dokumen Anggaran"));
        }
        // Dokumen pendukung boleh ditambah / dihapus pada status anggaran apa pun.
        private static void ValidateBudget(EF.ModelContext context, int budgetId)
        {
            if (!context.TBudgets.Any(x => x.Id == budgetId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggaran"));
            }
        }
        private static EF.TBudgetDocument CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateBudget(context, newrow.BudgetId);
            var fileName = TransaksiHelper.ValidateFile(newrow.FileData, newrow.FileName, "File Dokumen", MAX_FILE_BYTES);
            var row = new EF.TBudgetDocument
            {
                Id = SequenceUtility.GetNextId<EF.TBudgetDocument>(context),
                BudgetId = newrow.BudgetId,
                FileData = newrow.FileData,
                FileName = fileName,
                FileType = TransaksiHelper.ToNullable(newrow.FileType),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TBudgetDocuments.Add(row);
            return row;
        }
        #endregion
    }
}
