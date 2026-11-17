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

namespace GWALib.Lib.Transaksi.Merchandise
{
    public partial class SalesOrderItem
    {
        #region SYNC
        public static List<SalesOrderItem> GetDataList(int orderId)
        {
            return _Querys.Where(x => x.OrderId == orderId).OrderBy(x => x.Id).ToList().MappingData<SalesOrderItem, EF.TSalesOrderItem>();
        }
        #endregion

        #region ASYNC
        public static async Task<List<SalesOrderItem>> GetDataListAsync(int orderId)
        {
            return await (await _Querys.Where(x => x.OrderId == orderId).OrderBy(x => x.Id).ToListAsync())
                .MappingDataAsync<SalesOrderItem, EF.TSalesOrderItem>();
        }
        #endregion

        #region INTERNAL
        internal static EF.TSalesOrderItem CreateRow(EF.ModelContext context, EF.TSalesOrder order, NewRow newrow, int? userId)
        {
            if (newrow.Quantity <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.AMOUNT_MUST_POSITIVE, "Jumlah Pesanan"));
            }
            var (product, variant) = ProductStockMovement.GetSellable(context, newrow.ProductId, newrow.VariantId);
            var now = DateTime.Now;
            var isPreorder = product.IsPreorder == (int)SaleMode.PreOrder;
            if (isPreorder && product.PreorderCloseAt != null && product.PreorderCloseAt <= now)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.PREORDER_CLOSED, product.Name));
            }
            var unitPrice = variant?.Price ?? product.Price;
            var row = new EF.TSalesOrderItem
            {
                Id = SequenceUtility.GetNextId<EF.TSalesOrderItem>(context),
                OrderId = order.Id,
                ProductId = product.Id,
                VariantId = variant?.Id,
                Quantity = newrow.Quantity,
                UnitPrice = unitPrice,
                Subtotal = unitPrice * newrow.Quantity,
                Notes = TransaksiHelper.Truncate(newrow.Notes, 255),
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TSalesOrderItems.Add(row);
            if (!isPreorder && product.TrackStock == (int)StockTracking.Dilacak)
            {
                ProductStockMovement.Apply(context, product, variant, StockMovementType.Terjual, -newrow.Quantity, now, order.Id, null,
                    "Pesanan " + order.OrderNumber, userId);
            }
            return row;
        }
        #endregion
    }
}
