using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Mitra;
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
    public partial class ProcurementQuotation
    {
        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;
        private const int NOTES_LENGTH = 500;
        private const int VENDOR_LENGTH = 200;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TProcurementQuotations.Where(x => x.Id == Id).Select(x => new { x.File, x.FileName, x.ContentType }).SingleOrDefault();
            return file?.File == null ? null : new FileContent(file.File, file.FileName ?? "penawaran", file.ContentType);
        }
        public static List<ProcurementQuotation> GetDataList(int requestId)
        {
            return FilterQuery(requestId).ToList().MappingData<ProcurementQuotation, EF.TProcurementQuotation>();
        }
        public static Response<ProcurementQuotation> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementQuotation>(msg));
        }
        public Response<ProcurementQuotation> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementQuotation>(msg));
        }
        // file null / kosong = file dihapus.
        public Response<ProcurementQuotation> UpdateFile(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateRequest(context, row.RequestId);
                SetFile(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementQuotation>(msg));
        }
        public Response<ProcurementQuotation> Select(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetSelected(context, row, userId);
                context.SaveChanges();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementQuotation>(msg));
        }
        public Response<ProcurementQuotation> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateRequest(context, row.RequestId);
                var data = new ProcurementQuotation(Reload(row.Id));
                context.TProcurementQuotations.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementQuotation>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TProcurementQuotations.Where(x => x.Id == Id).Select(x => new { x.File, x.FileName, x.ContentType }).SingleOrDefaultAsync();
            return file?.File == null ? null : new FileContent(file.File, file.FileName ?? "penawaran", file.ContentType);
        }
        public static async Task<List<ProcurementQuotation>> GetDataListAsync(int requestId)
        {
            return await (await FilterQuery(requestId).ToListAsync()).MappingDataAsync<ProcurementQuotation, EF.TProcurementQuotation>();
        }
        public static async Task<Response<ProcurementQuotation>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementQuotation>(msg));
        }
        public async Task<Response<ProcurementQuotation>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementQuotation>(msg));
        }
        public async Task<Response<ProcurementQuotation>> UpdateFileAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateRequest(context, row.RequestId);
                SetFile(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                await context.SaveChangesAsync();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementQuotation>(msg));
        }
        public async Task<Response<ProcurementQuotation>> SelectAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetSelected(context, row, userId);
                await context.SaveChangesAsync();
                return new ProcurementQuotation(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementQuotation>(msg));
        }
        public async Task<Response<ProcurementQuotation>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateRequest(context, row.RequestId);
                var data = new ProcurementQuotation(Reload(row.Id));
                context.TProcurementQuotations.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementQuotation>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TProcurementQuotation Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TProcurementQuotation> FilterQuery(int requestId)
        {
            return _Querys.Where(x => x.RequestId == requestId).OrderBy(x => x.ItemId).ThenBy(x => x.QuotedAmount);
        }
        private static EF.TProcurementQuotation GetRow(EF.ModelContext context, int id)
        {
            return context.TProcurementQuotations.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penawaran Vendor"));
        }
        private static async Task<EF.TProcurementQuotation> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TProcurementQuotations.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penawaran Vendor"));
        }
        private static EF.TProcurementRequest ValidateRequest(EF.ModelContext context, int requestId)
        {
            var request = ProcurementRequest.GetRequest(context, requestId);
            TransaksiHelper.ValidateStatus((ProcurementStatus)request.Status, "mengubah penawaran vendor",
                ProcurementStatus.Draft, ProcurementStatus.Diajukan, ProcurementStatus.Revisi, ProcurementStatus.Disetujui);
            return request;
        }
        private record Data(int? ItemId, int? ExternalPartyId, string? VendorName);

        private static Data ValidateData(EF.ModelContext context, int requestId, int? itemId, int? externalPartyId, string? vendorName, decimal quotedAmount)
        {
            itemId = itemId > 0 ? itemId : null;
            if (itemId != null)
            {
                ProcurementItem.GetItem(context, itemId.Value, requestId);
            }
            externalPartyId = externalPartyId > 0 ? externalPartyId : null;
            var vendor = TransaksiHelper.Truncate(vendorName, VENDOR_LENGTH);
            if (externalPartyId != null)
            {
                ExternalParty.GetActiveExternalParty(context, externalPartyId.Value);
                vendor = null;
            }
            else if (vendor == null)
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_VENDOR_REQUIRED);
            }
            TransaksiHelper.ValidatePositive(quotedAmount, "Nilai Penawaran");
            return new Data(itemId, externalPartyId, vendor);
        }
        private static void SetFile(EF.TProcurementQuotation row, byte[]? file, string? fileName, string? contentType)
        {
            if (file == null || file.Length == 0)
            {
                row.File = null;
                row.FileName = null;
                row.ContentType = null;
                return;
            }
            row.FileName = TransaksiHelper.ValidateFile(file, fileName, "File Penawaran", MAX_FILE_BYTES);
            row.File = file;
            row.ContentType = TransaksiHelper.ToNullable(contentType);
        }
        private static EF.TProcurementQuotation CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateRequest(context, newrow.RequestId);
            var data = ValidateData(context, newrow.RequestId, newrow.ItemId, newrow.ExternalPartyId, newrow.VendorName, newrow.QuotedAmount);
            var row = new EF.TProcurementQuotation
            {
                Id = SequenceUtility.GetNextId<EF.TProcurementQuotation>(context),
                RequestId = newrow.RequestId,
                ItemId = data.ItemId,
                ExternalPartyId = data.ExternalPartyId,
                VendorName = data.VendorName,
                QuotedAmount = newrow.QuotedAmount,
                IsSelected = (int)QuotationSelection.TidakDipilih,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetFile(row, newrow.File, newrow.FileName, newrow.ContentType);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TProcurementQuotations.Add(row);
            return row;
        }
        // Usulan induk tetap; file tidak diubah di sini (UpdateFile).
        private void SetRow(EF.ModelContext context, EF.TProcurementQuotation row, int userId)
        {
            ValidateRequest(context, row.RequestId);
            var data = ValidateData(context, row.RequestId, ItemId, ExternalPartyId, VendorName, QuotedAmount);
            if (data.ItemId != row.ItemId)
            {
                row.IsSelected = (int)QuotationSelection.TidakDipilih;
            }
            row.ItemId = data.ItemId;
            row.ExternalPartyId = data.ExternalPartyId;
            row.VendorName = data.VendorName;
            row.QuotedAmount = QuotedAmount;
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetSelected(EF.ModelContext context, EF.TProcurementQuotation row, int userId)
        {
            ValidateRequest(context, row.RequestId);
            var now = DateTime.Now;
            foreach (var other in context.TProcurementQuotations
                .Where(x => x.RequestId == row.RequestId && x.ItemId == row.ItemId && x.Id != row.Id && x.IsSelected == (int)QuotationSelection.Dipilih).ToList())
            {
                other.IsSelected = (int)QuotationSelection.TidakDipilih;
                other.UpdDate = now;
                other.UpdBy = userId;
            }
            row.IsSelected = (int)QuotationSelection.Dipilih;
            row.UpdDate = now;
            row.UpdBy = userId;
            if (row.ItemId != null && row.ExternalPartyId != null)
            {
                var item = context.TProcurementItems.Single(x => x.Id == row.ItemId);
                item.ExternalPartyId = row.ExternalPartyId;
                item.UpdDate = now;
                item.UpdBy = userId;
            }
        }
        #endregion
    }
}
