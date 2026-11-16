using GWALib.General;
using GWALib.General.Utilities;
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
    public partial class FundingRealization
    {
        public const int MAX_RECEIPT_BYTES = 5 * 1024 * 1024;
        private const int TEXT_LENGTH = 255;

        #region SYNC
        public FileContent? GetReceipt()
        {
            using var context = DBClass.GetContext();
            var file = context.TFundingRealizations.Where(x => x.Id == Id).Select(x => new { x.Receipt, x.ReceiptFileName, x.ReceiptContentType }).SingleOrDefault();
            return file?.Receipt == null ? null : new FileContent(file.Receipt, file.ReceiptFileName ?? "nota", file.ReceiptContentType);
        }
        public static List<FundingRealization> GetDataList(int fundingRequestId)
        {
            return FilterQuery(fundingRequestId).ToList().MappingData<FundingRealization, EF.TFundingRealization>();
        }
        public static Response<FundingRealization> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new FundingRealization(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FundingRealization>(msg));
        }
        public Response<FundingRealization> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new FundingRealization(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRealization>(msg));
        }
        // file null / kosong = nota dihapus.
        public Response<FundingRealization> UpdateReceipt(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                FundingRequest.GetSettlingRequest(context, row.FundingRequestId);
                SetReceipt(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new FundingRealization(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRealization>(msg));
        }
        public Response<FundingRealization> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var request = FundingRequest.GetSettlingRequest(context, row.FundingRequestId);
                var data = new FundingRealization(Reload(row.Id));
                context.TFundingRealizations.Remove(row);
                FundingRequest.RecalculateRealized(context, request, userId);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FundingRealization>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetReceiptAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TFundingRealizations.Where(x => x.Id == Id).Select(x => new { x.Receipt, x.ReceiptFileName, x.ReceiptContentType }).SingleOrDefaultAsync();
            return file?.Receipt == null ? null : new FileContent(file.Receipt, file.ReceiptFileName ?? "nota", file.ReceiptContentType);
        }
        public static async Task<List<FundingRealization>> GetDataListAsync(int fundingRequestId)
        {
            return await (await FilterQuery(fundingRequestId).ToListAsync()).MappingDataAsync<FundingRealization, EF.TFundingRealization>();
        }
        public static async Task<Response<FundingRealization>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new FundingRealization(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FundingRealization>(msg));
        }
        public async Task<Response<FundingRealization>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new FundingRealization(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRealization>(msg));
        }
        public async Task<Response<FundingRealization>> UpdateReceiptAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                FundingRequest.GetSettlingRequest(context, row.FundingRequestId);
                SetReceipt(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                await context.SaveChangesAsync();
                return new FundingRealization(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRealization>(msg));
        }
        public async Task<Response<FundingRealization>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var request = FundingRequest.GetSettlingRequest(context, row.FundingRequestId);
                var data = new FundingRealization(Reload(row.Id));
                context.TFundingRealizations.Remove(row);
                FundingRequest.RecalculateRealized(context, request, userId);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FundingRealization>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TFundingRealization Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TFundingRealization> FilterQuery(int fundingRequestId)
        {
            return _Querys.Where(x => x.FundingRequestId == fundingRequestId).OrderBy(x => x.RealizationDate).ThenBy(x => x.Id);
        }
        private static EF.TFundingRealization GetRow(EF.ModelContext context, int id)
        {
            return context.TFundingRealizations.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Realisasi Dana"));
        }
        private static async Task<EF.TFundingRealization> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TFundingRealizations.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Realisasi Dana"));
        }
        private static string ValidateData(DateOnly realizationDate, string? description, decimal? quantity, decimal amount)
        {
            if (realizationDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Realisasi"));
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Uraian Realisasi"));
            }
            if (quantity != null)
            {
                TransaksiHelper.ValidatePositive(quantity.Value, "Jumlah");
            }
            TransaksiHelper.ValidatePositive(amount, "Nominal Realisasi");
            return TransaksiHelper.Truncate(description, TEXT_LENGTH)!;
        }
        private static void SetReceipt(EF.TFundingRealization row, byte[]? file, string? fileName, string? contentType)
        {
            if (file == null || file.Length == 0)
            {
                row.Receipt = null;
                row.ReceiptFileName = null;
                row.ReceiptContentType = null;
                return;
            }
            row.ReceiptFileName = TransaksiHelper.ValidateFile(file, fileName, "File Nota", MAX_RECEIPT_BYTES);
            row.Receipt = file;
            row.ReceiptContentType = TransaksiHelper.ToNullable(contentType);
        }
        private static EF.TFundingRealization CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var request = FundingRequest.GetSettlingRequest(context, newrow.FundingRequestId);
            var description = ValidateData(newrow.RealizationDate, newrow.Description, newrow.Quantity, newrow.Amount);
            var row = new EF.TFundingRealization
            {
                Id = SequenceUtility.GetNextId<EF.TFundingRealization>(context),
                FundingRequestId = request.Id,
                RealizationDate = newrow.RealizationDate,
                Description = description,
                Quantity = newrow.Quantity,
                Unit = TransaksiHelper.ToNullable(newrow.Unit),
                Amount = newrow.Amount,
                Notes = TransaksiHelper.Truncate(newrow.Notes, TEXT_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetReceipt(row, newrow.Receipt, newrow.ReceiptFileName, newrow.ReceiptContentType);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TFundingRealizations.Add(row);
            FundingRequest.RecalculateRealized(context, request, userId);
            return row;
        }
        // Nota tidak diubah di sini (UpdateReceipt).
        private void SetRow(EF.ModelContext context, EF.TFundingRealization row, int userId)
        {
            var request = FundingRequest.GetSettlingRequest(context, row.FundingRequestId);
            var description = ValidateData(RealizationDate, Description, Quantity, Amount);

            row.RealizationDate = RealizationDate;
            row.Description = description;
            row.Quantity = Quantity;
            row.Unit = TransaksiHelper.ToNullable(Unit);
            row.Amount = Amount;
            row.Notes = TransaksiHelper.Truncate(Notes, TEXT_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            FundingRequest.RecalculateRealized(context, request, userId);
        }
        #endregion
    }
}
