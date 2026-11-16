using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Inventaris;
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
    public partial class ProcurementItem
    {
        private const int NOTES_LENGTH = 500;

        #region SYNC
        public static List<ProcurementItem> GetDataList(int requestId)
        {
            return FilterQuery(requestId).ToList().MappingData<ProcurementItem, EF.TProcurementItem>();
        }
        public static Response<ProcurementItem> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                ProcurementRequest.RecalculateEstimate(context, ProcurementRequest.GetEditableRequest(context, row.RequestId), userId);
                context.SaveChanges();
                return new ProcurementItem(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementItem>(msg));
        }
        public Response<ProcurementItem> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var request = SetRow(context, row, userId);
                ProcurementRequest.RecalculateEstimate(context, request, userId);
                context.SaveChanges();
                return new ProcurementItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementItem>(msg));
        }
        // Harga aktual per unit dan vendor terpilih, setelah usulan disetujui.
        public Response<ProcurementItem> SetActual(decimal actualUnitPrice, int? externalPartyId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetActualRow(context, row, actualUnitPrice, externalPartyId, userId);
                context.SaveChanges();
                return new ProcurementItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementItem>(msg));
        }
        // Penawaran yang menunjuk item ini ikut dihapus.
        public Response<ProcurementItem> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var request = ProcurementRequest.GetEditableRequest(context, row.RequestId);
                var data = new ProcurementItem(row);
                context.TProcurementQuotations.RemoveRange(context.TProcurementQuotations.Where(x => x.ItemId == row.Id));
                context.TProcurementItems.Remove(row);
                ProcurementRequest.RecalculateEstimate(context, request, userId);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementItem>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ProcurementItem>> GetDataListAsync(int requestId)
        {
            return await (await FilterQuery(requestId).ToListAsync()).MappingDataAsync<ProcurementItem, EF.TProcurementItem>();
        }
        public static async Task<Response<ProcurementItem>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                ProcurementRequest.RecalculateEstimate(context, ProcurementRequest.GetEditableRequest(context, row.RequestId), userId);
                await context.SaveChangesAsync();
                return new ProcurementItem(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementItem>(msg));
        }
        public async Task<Response<ProcurementItem>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var request = SetRow(context, row, userId);
                ProcurementRequest.RecalculateEstimate(context, request, userId);
                await context.SaveChangesAsync();
                return new ProcurementItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementItem>(msg));
        }
        public async Task<Response<ProcurementItem>> SetActualAsync(decimal actualUnitPrice, int? externalPartyId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetActualRow(context, row, actualUnitPrice, externalPartyId, userId);
                await context.SaveChangesAsync();
                return new ProcurementItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementItem>(msg));
        }
        public async Task<Response<ProcurementItem>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var request = ProcurementRequest.GetEditableRequest(context, row.RequestId);
                var data = new ProcurementItem(row);
                context.TProcurementQuotations.RemoveRange(context.TProcurementQuotations.Where(x => x.ItemId == row.Id));
                context.TProcurementItems.Remove(row);
                ProcurementRequest.RecalculateEstimate(context, request, userId);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementItem>(msg));
        }
        #endregion

        #region INTERNAL
        // Item harus milik usulan yang disebut; dipakai penawaran dan penerimaan.
        internal static EF.TProcurementItem GetItem(EF.ModelContext context, int itemId, int requestId)
        {
            var row = context.TProcurementItems.SingleOrDefault(x => x.Id == itemId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Pengadaan"));
            if (row.RequestId != requestId)
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_ITEM_MISMATCH);
            }
            return row;
        }
        // Tidak menghitung ulang TotalEstimate; pemanggil (Save / ProcurementRequest.CreateRow) yang menghitung.
        internal static EF.TProcurementItem CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var request = ProcurementRequest.GetEditableRequest(context, newrow.RequestId);
            var data = ValidateData(context, newrow.ItemName, newrow.IsAsset, newrow.AssetCategoryId, newrow.Quantity, newrow.Unit, newrow.EstimatedUnitPrice, newrow.ExternalPartyId);
            var row = new EF.TProcurementItem
            {
                Id = SequenceUtility.GetNextId<EF.TProcurementItem>(context),
                RequestId = request.Id,
                ItemName = data.ItemName,
                Specification = TransaksiHelper.ToNullable(newrow.Specification),
                IsAsset = (int)newrow.IsAsset,
                AssetCategoryId = data.AssetCategoryId,
                Quantity = newrow.Quantity,
                Unit = data.Unit,
                EstimatedUnitPrice = newrow.EstimatedUnitPrice,
                ExternalPartyId = data.ExternalPartyId,
                ReceivedQuantity = 0,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TProcurementItems.Add(row);
            return row;
        }
        #endregion

        #region HELPER
        private record Data(string ItemName, int? AssetCategoryId, string Unit, int? ExternalPartyId);

        private static IQueryable<EF.TProcurementItem> FilterQuery(int requestId)
        {
            return _Querys.Where(x => x.RequestId == requestId).OrderBy(x => x.Id);
        }
        private static EF.TProcurementItem GetRow(EF.ModelContext context, int id)
        {
            return context.TProcurementItems.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Pengadaan"));
        }
        private static async Task<EF.TProcurementItem> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TProcurementItems.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Pengadaan"));
        }
        private static int? ValidateParty(EF.ModelContext context, int? externalPartyId)
        {
            if (externalPartyId > 0)
            {
                ExternalParty.GetActiveExternalParty(context, externalPartyId.Value);
                return externalPartyId;
            }
            return null;
        }
        private static Data ValidateData(EF.ModelContext context, string? itemName, ProcurementItemKind isAsset, int? assetCategoryId, int quantity,
            string? unit, decimal estimatedUnitPrice, int? externalPartyId)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Barang"));
            }
            if (!Enum.IsDefined(isAsset))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Barang"));
            }
            if (quantity <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.AMOUNT_MUST_POSITIVE, "Jumlah"));
            }
            if (estimatedUnitPrice < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Harga Estimasi"));
            }
            int? categoryId = null;
            if (isAsset == ProcurementItemKind.Aset)
            {
                if (!(assetCategoryId > 0))
                {
                    throw new ArgumentException(ExceptionMessage.PROCUREMENT_ASSET_CATEGORY_REQUIRED);
                }
                AssetCategory.ValidateActiveAssetCategory(context, assetCategoryId!.Value);
                categoryId = assetCategoryId;
            }
            var unitText = string.IsNullOrWhiteSpace(unit) ? "pcs" : unit.Trim();
            return new Data(itemName.Trim(), categoryId, unitText, ValidateParty(context, externalPartyId));
        }
        // Usulan induk tetap.
        private EF.TProcurementRequest SetRow(EF.ModelContext context, EF.TProcurementItem row, int userId)
        {
            var request = ProcurementRequest.GetEditableRequest(context, row.RequestId);
            var data = ValidateData(context, ItemName, IsAsset, AssetCategoryId, Quantity, Unit, EstimatedUnitPrice, ExternalPartyId);

            row.ItemName = data.ItemName;
            row.Specification = TransaksiHelper.ToNullable(Specification);
            row.IsAsset = (int)IsAsset;
            row.AssetCategoryId = data.AssetCategoryId;
            row.Quantity = Quantity;
            row.Unit = data.Unit;
            row.EstimatedUnitPrice = EstimatedUnitPrice;
            row.ExternalPartyId = data.ExternalPartyId;
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return request;
        }
        private static void SetActualRow(EF.ModelContext context, EF.TProcurementItem row, decimal actualUnitPrice, int? externalPartyId, int userId)
        {
            var request = ProcurementRequest.GetRequest(context, row.RequestId);
            TransaksiHelper.ValidateStatus((ProcurementStatus)request.Status, "mengisi harga aktual",
                ProcurementStatus.Disetujui, ProcurementStatus.Dibeli, ProcurementStatus.DiterimaSebagian);
            if (actualUnitPrice < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Harga Aktual"));
            }
            row.ActualUnitPrice = actualUnitPrice;
            row.ExternalPartyId = ValidateParty(context, externalPartyId) ?? row.ExternalPartyId;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
