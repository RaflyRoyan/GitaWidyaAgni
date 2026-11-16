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
    public partial class FinancialAttachment
    {
        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TFinancialAttachments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.FileType }).SingleOrDefault();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.FileType);
        }
        public static List<FinancialAttachment> GetDataList(int transactionId)
        {
            return FilterQuery(transactionId).ToList().MappingData<FinancialAttachment, EF.TFinancialAttachment>();
        }
        public static Response<FinancialAttachment> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new FinancialAttachment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialAttachment>(msg));
        }
        public Response<FinancialAttachment> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateTransaction(context, row.TransactionId);
                var data = new FinancialAttachment(row);
                context.TFinancialAttachments.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialAttachment>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TFinancialAttachments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.FileType }).SingleOrDefaultAsync();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.FileType);
        }
        public static async Task<List<FinancialAttachment>> GetDataListAsync(int transactionId)
        {
            return await (await FilterQuery(transactionId).ToListAsync()).MappingDataAsync<FinancialAttachment, EF.TFinancialAttachment>();
        }
        public static async Task<Response<FinancialAttachment>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new FinancialAttachment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialAttachment>(msg));
        }
        public async Task<Response<FinancialAttachment>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateTransaction(context, row.TransactionId);
                var data = new FinancialAttachment(row);
                context.TFinancialAttachments.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialAttachment>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TFinancialAttachment> FilterQuery(int transactionId)
        {
            return _Querys.Where(x => x.TransactionId == transactionId).OrderBy(x => x.Id);
        }
        private static EF.TFinancialAttachment GetRow(EF.ModelContext context, int id)
        {
            return context.TFinancialAttachments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Bukti Transaksi"));
        }
        private static async Task<EF.TFinancialAttachment> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TFinancialAttachments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Bukti Transaksi"));
        }
        private static void ValidateTransaction(EF.ModelContext context, int transactionId)
        {
            var status = context.TFinancialTransactions.Where(x => x.Id == transactionId).Select(x => (int?)x.Status).SingleOrDefault()
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Transaksi Keuangan"));
            TransaksiHelper.ValidateStatus((TransactionStatus)status, "mengubah bukti transaksi", TransactionStatus.Draft, TransactionStatus.Posted);
        }
        private static EF.TFinancialAttachment CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateTransaction(context, newrow.TransactionId);
            var fileName = TransaksiHelper.ValidateFile(newrow.FileData, newrow.FileName, "File Bukti", MAX_FILE_BYTES);
            var row = new EF.TFinancialAttachment
            {
                Id = SequenceUtility.GetNextId<EF.TFinancialAttachment>(context),
                TransactionId = newrow.TransactionId,
                FileData = newrow.FileData,
                FileName = fileName,
                FileType = TransaksiHelper.ToNullable(newrow.FileType),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TFinancialAttachments.Add(row);
            return row;
        }
        #endregion
    }
}
