using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Inventaris;
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
    public partial class ProcurementReceiptItem
    {
        #region SYNC
        public static List<ProcurementReceiptItem> GetDataList(int receiptId)
        {
            return _Querys.Where(x => x.ReceiptId == receiptId).OrderBy(x => x.Id).ToList().MappingData<ProcurementReceiptItem, EF.TProcurementReceiptItem>();
        }
        #endregion

        #region ASYNC
        public static async Task<List<ProcurementReceiptItem>> GetDataListAsync(int receiptId)
        {
            return await (await _Querys.Where(x => x.ReceiptId == receiptId).OrderBy(x => x.Id).ToListAsync())
                .MappingDataAsync<ProcurementReceiptItem, EF.TProcurementReceiptItem>();
        }
        #endregion

        #region INTERNAL
        // Menambah ReceivedQuantity item; aset dibuat bila diminta.
        internal static EF.TProcurementReceiptItem CreateRow(EF.ModelContext context, EF.TProcurementReceipt receipt, NewRow newrow, int userId)
        {
            var item = ProcurementItem.GetItem(context, newrow.ItemId, receipt.RequestId);
            if (newrow.Quantity <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.AMOUNT_MUST_POSITIVE, "Jumlah Diterima"));
            }
            if (!Enum.IsDefined(newrow.ItemCondition))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi Barang"));
            }
            var remaining = item.Quantity - item.ReceivedQuantity;
            if (newrow.Quantity > remaining)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.PROCUREMENT_RECEIVE_EXCEEDS, item.ItemName, remaining));
            }

            var now = DateTime.Now;
            var row = new EF.TProcurementReceiptItem
            {
                Id = SequenceUtility.GetNextId<EF.TProcurementReceiptItem>(context),
                ReceiptId = receipt.Id,
                ItemId = item.Id,
                Quantity = newrow.Quantity,
                ItemCondition = (int)newrow.ItemCondition,
                Notes = TransaksiHelper.Truncate(newrow.Notes, 500),
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TProcurementReceiptItems.Add(row);
            item.ReceivedQuantity += newrow.Quantity;
            item.UpdDate = now;
            item.UpdBy = userId;

            if (newrow.Asset != null)
            {
                if (item.IsAsset != (int)ProcurementItemKind.Aset || newrow.ItemCondition == ItemCondition.Hilang)
                {
                    throw new ArgumentException(ExceptionMessage.PROCUREMENT_ASSET_NOT_ASSET_ITEM);
                }
                var asset = newrow.Asset;
                asset.CategoryId = asset.CategoryId > 0 ? asset.CategoryId : item.AssetCategoryId!.Value;
                asset.Name = string.IsNullOrWhiteSpace(asset.Name) ? item.ItemName : asset.Name;
                asset.Specification ??= item.Specification;
                asset.Quantity = newrow.Quantity;
                asset.Unit = string.IsNullOrWhiteSpace(asset.Unit) ? item.Unit : asset.Unit;
                asset.AcquisitionSource = AcquisitionSource.Beli;
                asset.AcquisitionDate ??= receipt.ReceiptDate;
                asset.AcquisitionCost ??= (item.ActualUnitPrice ?? item.EstimatedUnitPrice) * newrow.Quantity;
                asset.AssetCondition = newrow.ItemCondition;
                GWALib.Lib.Master.Inventaris.Asset.CreateRow(context, asset, userId, item.Id);
            }
            return row;
        }
        #endregion
    }
}
