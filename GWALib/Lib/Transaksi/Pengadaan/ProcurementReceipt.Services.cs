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

namespace GWALib.Lib.Transaksi.Pengadaan
{
    public partial class ProcurementReceipt
    {
        public const int MAX_PHOTO_BYTES = 5 * 1024 * 1024;
        private const int NOTES_LENGTH = 500;

        #region SYNC
        public List<ProcurementReceiptItem> GetItems(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = ProcurementReceiptItem.GetDataList(Id);
            }
            return ItemList;
        }
        public FileContent? GetPhoto()
        {
            using var context = DBClass.GetContext();
            var file = context.TProcurementReceipts.Where(x => x.Id == Id).Select(x => new { x.Photo, x.PhotoFileName, x.PhotoContentType }).SingleOrDefault();
            return file?.Photo == null ? null : new FileContent(file.Photo, file.PhotoFileName ?? "foto", file.PhotoContentType);
        }
        public static List<ProcurementReceipt> GetDataList(int requestId)
        {
            return FilterQuery(requestId).ToList().MappingData<ProcurementReceipt, EF.TProcurementReceipt>();
        }
        // Penerimaan, baris, aset (bila diminta) dan status usulan disimpan dalam satu transaksi DB.
        public static Response<ProcurementReceipt> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new ProcurementReceipt(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementReceipt>(msg));
        }
        // Hanya tanggal, penerima dan catatan; baris tidak diubah.
        public Response<ProcurementReceipt> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ProcurementReceipt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementReceipt>(msg));
        }
        // file null / kosong = foto dihapus.
        public Response<ProcurementReceipt> UpdatePhoto(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetPhoto(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new ProcurementReceipt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementReceipt>(msg));
        }
        public Response<ProcurementReceipt> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new ProcurementReceipt(Reload(row.Id));
                SetDelete(context, row, userId);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementReceipt>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<ProcurementReceiptItem>> GetItemsAsync(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = await ProcurementReceiptItem.GetDataListAsync(Id);
            }
            return ItemList;
        }
        public async Task<FileContent?> GetPhotoAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TProcurementReceipts.Where(x => x.Id == Id).Select(x => new { x.Photo, x.PhotoFileName, x.PhotoContentType }).SingleOrDefaultAsync();
            return file?.Photo == null ? null : new FileContent(file.Photo, file.PhotoFileName ?? "foto", file.PhotoContentType);
        }
        public static async Task<List<ProcurementReceipt>> GetDataListAsync(int requestId)
        {
            return await (await FilterQuery(requestId).ToListAsync()).MappingDataAsync<ProcurementReceipt, EF.TProcurementReceipt>();
        }
        public static async Task<Response<ProcurementReceipt>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ProcurementReceipt(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementReceipt>(msg));
        }
        public async Task<Response<ProcurementReceipt>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ProcurementReceipt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementReceipt>(msg));
        }
        public async Task<Response<ProcurementReceipt>> UpdatePhotoAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetPhoto(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                await context.SaveChangesAsync();
                return new ProcurementReceipt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementReceipt>(msg));
        }
        public async Task<Response<ProcurementReceipt>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new ProcurementReceipt(Reload(row.Id));
                SetDelete(context, row, userId);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementReceipt>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TProcurementReceipt Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TProcurementReceipt> FilterQuery(int requestId)
        {
            return _Querys.Where(x => x.RequestId == requestId).OrderBy(x => x.ReceiptDate).ThenBy(x => x.Id);
        }
        private static EF.TProcurementReceipt GetRow(EF.ModelContext context, int id)
        {
            return context.TProcurementReceipts.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penerimaan Barang"));
        }
        private static async Task<EF.TProcurementReceipt> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TProcurementReceipts.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penerimaan Barang"));
        }
        private static int ResolveMember(EF.ModelContext context, int? memberId, int userId)
        {
            var id = memberId > 0 ? memberId!.Value
                : context.MMembers.Where(x => x.UserId == userId).Select(x => (int?)x.Id).FirstOrDefault()
                    ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
            Member.GetActiveMember(context, id);
            return id;
        }
        private static void SetPhoto(EF.TProcurementReceipt row, byte[]? file, string? fileName, string? contentType)
        {
            if (file == null || file.Length == 0)
            {
                row.Photo = null;
                row.PhotoFileName = null;
                row.PhotoContentType = null;
                return;
            }
            row.PhotoFileName = TransaksiHelper.ValidateFile(file, fileName, "Foto Penerimaan", MAX_PHOTO_BYTES);
            row.Photo = file;
            row.PhotoContentType = TransaksiHelper.ToNullable(contentType);
        }
        private static EF.TProcurementReceipt CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var request = ProcurementRequest.GetRequest(context, newrow.RequestId);
            TransaksiHelper.ValidateStatus((ProcurementStatus)request.Status, "mencatat penerimaan barang", ProcurementStatus.Dibeli, ProcurementStatus.DiterimaSebagian);
            if (newrow.ReceiptDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Terima"));
            }
            var lines = newrow.Items ?? new();
            if (lines.Count == 0)
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_RECEIPT_ITEM_REQUIRED);
            }
            if (lines.Select(x => x.ItemId).Distinct().Count() != lines.Count)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_DUPLICATE, "item yang sama dalam satu penerimaan"));
            }

            var row = new EF.TProcurementReceipt
            {
                Id = SequenceUtility.GetNextId<EF.TProcurementReceipt>(context),
                RequestId = request.Id,
                ReceiptDate = newrow.ReceiptDate,
                ReceivedByMemberId = ResolveMember(context, newrow.ReceivedByMemberId, userId),
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetPhoto(row, newrow.Photo, newrow.PhotoFileName, newrow.PhotoContentType);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TProcurementReceipts.Add(row);
            foreach (var line in lines)
            {
                ProcurementReceiptItem.CreateRow(context, row, line, userId);
            }
            ProcurementRequest.RecalculateReceiveStatus(context, request, userId);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TProcurementReceipt row, int userId)
        {
            if (ReceiptDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Terima"));
            }
            row.ReceiptDate = ReceiptDate;
            row.ReceivedByMemberId = ResolveMember(context, ReceivedByMemberId, userId);
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetDelete(EF.ModelContext context, EF.TProcurementReceipt row, int userId)
        {
            var request = ProcurementRequest.GetRequest(context, row.RequestId);
            TransaksiHelper.ValidateStatus((ProcurementStatus)request.Status, "menghapus penerimaan barang", ProcurementStatus.DiterimaSebagian, ProcurementStatus.Selesai);
            var lines = context.TProcurementReceiptItems.Where(x => x.ReceiptId == row.Id).ToList();
            var itemIds = lines.Select(x => x.ItemId).ToList();
            if (context.MAssets.Any(x => x.ProcurementItemId != null && itemIds.Contains(x.ProcurementItemId.Value)))
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_RECEIPT_HAS_ASSET);
            }
            var now = DateTime.Now;
            foreach (var line in lines)
            {
                var item = context.TProcurementItems.Single(x => x.Id == line.ItemId);
                item.ReceivedQuantity -= line.Quantity;
                item.UpdDate = now;
                item.UpdBy = userId;
            }
            context.TProcurementReceiptItems.RemoveRange(lines);
            context.TProcurementReceipts.Remove(row);
            ProcurementRequest.RecalculateReceiveStatus(context, request, userId);
        }
        #endregion
    }
}
