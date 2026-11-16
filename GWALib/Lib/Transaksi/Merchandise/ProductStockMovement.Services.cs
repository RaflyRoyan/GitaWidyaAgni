using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Merchandise;
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
    public partial class ProductStockMovement
    {
        private const int NOTES_LENGTH = 255;

        #region SYNC
        public static List<ProductStockMovement> GetDataList(int? productId = null, int? variantId = null, StockMovementType? movementType = null,
            DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(productId, variantId, movementType, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ProductStockMovement, EF.TProductStockMovement>();
        }
        public static Response<ProductStockMovement> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                context.SaveChanges();
                return new ProductStockMovement(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProductStockMovement>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ProductStockMovement>> GetDataListAsync(int? productId = null, int? variantId = null, StockMovementType? movementType = null,
            DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(productId, variantId, movementType, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ProductStockMovement, EF.TProductStockMovement>();
        }
        public static async Task<Response<ProductStockMovement>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ProductStockMovement(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProductStockMovement>(msg));
        }
        #endregion

        #region INTERNAL
        // Produk / varian yang dijual: produk aktif, varian wajib bila produk punya varian dan harus aktif. Mengembalikan (produk, varian).
        internal static (EF.MProduct Product, EF.MProductVariant? Variant) GetSellable(EF.ModelContext context, int productId, int? variantId)
        {
            var product = Product.GetActiveProduct(context, productId);
            var hasVariants = context.MProductVariants.Any(x => x.ProductId == productId);
            variantId = variantId > 0 ? variantId : null;
            if (hasVariants && variantId == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.PRODUCT_VARIANT_REQUIRED, product.Name));
            }
            if (!hasVariants && variantId != null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.PRODUCT_NO_VARIANT, product.Name));
            }
            EF.MProductVariant? variant = null;
            if (variantId != null)
            {
                ProductVariant.ValidateActiveVariant(context, variantId.Value, productId);
                variant = context.MProductVariants.Single(x => x.Id == variantId);
            }
            return (product, variant);
        }
        // Mencatat mutasi bertanda (positif / negatif) dan mengubah stok produk / varian; stok tidak boleh minus.
        internal static EF.TProductStockMovement Apply(EF.ModelContext context, EF.MProduct product, EF.MProductVariant? variant, StockMovementType movementType,
            int quantity, DateTime movementDate, int? orderId, int? procurementId, string? notes, int userId)
        {
            if (product.TrackStock != (int)StockTracking.Dilacak)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.STOCK_NOT_TRACKED, product.Name));
            }
            var current = variant?.Stock ?? product.Stock;
            if (current + quantity < 0)
            {
                var name = variant == null ? product.Name : product.Name + " " + variant.Name;
                throw new ArgumentException(string.Format(ExceptionMessage.STOCK_INSUFFICIENT, name, current));
            }
            var now = DateTime.Now;
            if (variant != null)
            {
                variant.Stock = current + quantity;
                variant.UpdDate = now;
                variant.UpdBy = userId;
            }
            else
            {
                product.Stock = current + quantity;
                product.UpdDate = now;
                product.UpdBy = userId;
            }
            var row = new EF.TProductStockMovement
            {
                Id = SequenceUtility.GetNextId<EF.TProductStockMovement>(context),
                ProductId = product.Id,
                VariantId = variant?.Id,
                MovementType = (int)movementType,
                Quantity = quantity,
                MovementDate = movementDate,
                OrderId = orderId,
                ProcurementId = procurementId,
                Notes = TransaksiHelper.Truncate(notes, NOTES_LENGTH),
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TProductStockMovements.Add(row);
            return row;
        }
        #endregion

        #region HELPER
        private static EF.TProductStockMovement Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TProductStockMovement> FilterQuery(int? productId, int? variantId, StockMovementType? movementType, DateTime? from, DateTime? to)
        {
            return _Querys.Where(x => (productId == null || x.ProductId == productId)
                    && (variantId == null || x.VariantId == variantId)
                    && (movementType == null || x.MovementType == (int)movementType)
                    && (from == null || x.MovementDate >= from)
                    && (to == null || x.MovementDate <= to))
                .OrderByDescending(x => x.MovementDate).ThenByDescending(x => x.Id);
        }
        private static EF.TProductStockMovement CreateManualRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (!Enum.IsDefined(newrow.MovementType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Mutasi"));
            }
            if (newrow.MovementType == StockMovementType.Terjual || newrow.MovementType == StockMovementType.Retur)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.STOCK_MOVEMENT_AUTO, newrow.MovementType.GetDescription()));
            }
            var quantity = newrow.MovementType switch
            {
                StockMovementType.Masuk => newrow.Quantity,
                StockMovementType.RusakHilang => -Math.Abs(newrow.Quantity),
                _ => newrow.Quantity
            };
            if (quantity == 0 || (newrow.MovementType == StockMovementType.Masuk && quantity < 0))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jumlah Mutasi"));
            }
            var procurementId = newrow.ProcurementId > 0 ? newrow.ProcurementId : null;
            if (procurementId != null)
            {
                if (newrow.MovementType != StockMovementType.Masuk)
                {
                    throw new ArgumentException(ExceptionMessage.STOCK_PROCUREMENT_ONLY_IN);
                }
                if (!context.TProcurementRequests.Any(x => x.Id == procurementId))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengadaan"));
                }
            }
            var (product, variant) = GetSellable(context, newrow.ProductId, newrow.VariantId);
            return Apply(context, product, variant, newrow.MovementType, quantity, newrow.MovementDate ?? DateTime.Now, null, procurementId, newrow.Notes, userId);
        }
        #endregion
    }
}
