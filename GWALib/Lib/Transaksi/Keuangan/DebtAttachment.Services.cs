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
    public partial class DebtAttachment
    {
        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TDebtAttachments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.FileType }).SingleOrDefault();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.FileType);
        }
        public static List<DebtAttachment> GetDataList(int DebtId)
        {
            return FilterQuery(DebtId).ToList().MappingData<DebtAttachment, EF.TDebtAttachment>();
        }
        public static Response<DebtAttachment> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new DebtAttachment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DebtAttachment>(msg));
        }
        public Response<DebtAttachment> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDebt(context, row.DebtId);
                var data = new DebtAttachment(row);
                context.TDebtAttachments.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DebtAttachment>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TDebtAttachments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.FileType }).SingleOrDefaultAsync();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.FileType);
        }
        public static async Task<List<DebtAttachment>> GetDataListAsync(int DebtId)
        {
            return await (await FilterQuery(DebtId).ToListAsync()).MappingDataAsync<DebtAttachment, EF.TDebtAttachment>();
        }
        public static async Task<Response<DebtAttachment>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new DebtAttachment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DebtAttachment>(msg));
        }
        public async Task<Response<DebtAttachment>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDebt(context, row.DebtId);
                var data = new DebtAttachment(row);
                context.TDebtAttachments.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DebtAttachment>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TDebtAttachment> FilterQuery(int DebtId)
        {
            return _Querys.Where(x => x.DebtId == DebtId).OrderBy(x => x.Id);
        }
        private static EF.TDebtAttachment GetRow(EF.ModelContext context, int id)
        {
            return context.TDebtAttachments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lampiran Hutang"));
        }
        private static async Task<EF.TDebtAttachment> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TDebtAttachments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lampiran Hutang"));
        }
        // Lampiran boleh ditambah / dihapus pada status hutang apa pun.
        private static void ValidateDebt(EF.ModelContext context, int debtId)
        {
            if (!context.TDebts.Any(x => x.Id == debtId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Hutang"));
            }
        }
        private static EF.TDebtAttachment CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateDebt(context, newrow.DebtId);
            var fileName = TransaksiHelper.ValidateFile(newrow.FileData, newrow.FileName, "File Lampiran", MAX_FILE_BYTES);
            var row = new EF.TDebtAttachment
            {
                Id = SequenceUtility.GetNextId<EF.TDebtAttachment>(context),
                DebtId = newrow.DebtId,
                FileData = newrow.FileData,
                FileName = fileName,
                FileType = TransaksiHelper.ToNullable(newrow.FileType),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TDebtAttachments.Add(row);
            return row;
        }
        #endregion
    }
}
