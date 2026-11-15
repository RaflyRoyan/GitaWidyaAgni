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

namespace GWALib.Lib.Master.Merchandise
{
    public partial class ProductVariant
    {
        #region SYNC
        public Product GetProduct(bool refresh = false)
        {
            if (refresh || ProductData == null)
            {
                ProductData = new Product(ProductId);
            }
            return ProductData;
        }
        public static List<ProductVariant> GetDataList(FilterAktif aktif, int? productId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, productId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ProductVariant, EF.MProductVariant>();
        }
        public static Response<ProductVariant> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, userId);
                context.SaveChanges();
                return new ProductVariant(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProductVariant>(msg));
        }
        public Response<ProductVariant> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ProductVariant(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProductVariant>(msg));
        }
        public Response<ProductVariant> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new ProductVariant(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProductVariant>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ProductVariant>> GetDataListAsync(FilterAktif aktif, int? productId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, productId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ProductVariant, EF.MProductVariant>();
        }
        public static async Task<Response<ProductVariant>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ProductVariant(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProductVariant>(msg));
        }
        public async Task<Response<ProductVariant>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ProductVariant(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProductVariant>(msg));
        }
        public async Task<Response<ProductVariant>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new ProductVariant(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProductVariant>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActiveVariant(EF.ModelContext context, int variantId, int productId)
        {
            var row = context.MProductVariants.SingleOrDefault(x => x.Id == variantId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Varian Produk"));
            if (row.ProductId != productId)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Varian Produk"));
            }
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Varian Produk"));
            }
        }
        private static IQueryable<EF.MProductVariant> FilterQuery(FilterAktif aktif, int? productId)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (productId == null || x.ProductId == productId))
                .OrderBy(x => x.ProductId).ThenBy(x => x.SeqNo).ThenBy(x => x.Name);
        }
        private static EF.MProductVariant GetRow(EF.ModelContext context, int id)
        {
            return context.MProductVariants.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Varian Produk"));
        }
        private static async Task<EF.MProductVariant> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MProductVariants.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Varian Produk"));
        }
        // Local ikut dicek: beberapa varian bisa di-Add sekaligus lewat Product.Save.
        private static void ValidateData(EF.ModelContext context, int id, int productId, string name, decimal? price, string? sku)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Varian"));
            }
            if (price != null && price < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Harga Varian"));
            }
            if (context.MProductVariants.Local.Any(x => x.ProductId == productId && x.Name == name && x.Id != id))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Varian"));
            }
            ValidateUtility.CheckDuplicateData<EF.MProductVariant>(context, x => x.ProductId == productId && x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Varian"));
            if (sku != null)
            {
                if (context.MProductVariants.Local.Any(x => x.Sku == sku && x.Id != id))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.DATA_DUPLICATE, "SKU"));
                }
                ValidateUtility.CheckDuplicateData<EF.MProductVariant>(context, x => x.Sku == sku && x.Id != id,
                    string.Format(ExceptionMessage.DATA_DUPLICATE, "SKU"));
            }
        }
        // Tanpa SaveChanges: menambahkan varian (+ mutasi stok awal bila ada) ke context; dipakai juga oleh Product.Save.
        internal static EF.MProductVariant AddRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var sku = string.IsNullOrWhiteSpace(newrow.Sku) ? null : newrow.Sku.Trim();
            var product = Product.GetActiveProduct(context, newrow.ProductId);
            if (product.Stock != 0)
            {
                throw new ArgumentException(ExceptionMessage.PRODUCT_HAS_OWN_STOCK);
            }
            var stock = product.TrackStock == (int)StockTracking.Dilacak ? newrow.Stock : 0;
            if (stock < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Stok Awal"));
            }
            ValidateData(context, 0, product.Id, name, newrow.Price, sku);

            var row = new EF.MProductVariant
            {
                Id = SequenceUtility.GetNextId<EF.MProductVariant>(context),
                ProductId = product.Id,
                Name = name,
                Price = newrow.Price,
                Stock = stock,
                Sku = sku,
                SeqNo = newrow.SeqNo > 0 ? newrow.SeqNo
                    : ValueUtility.GetNextId<EF.MProductVariant>(context, x => x.SeqNo, x => x.ProductId == product.Id),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MProductVariants.Add(row);
            if (stock > 0)
            {
                context.TProductStockMovements.Add(Product.CreateInitialStockMovement(context, product.Id, row.Id, stock, userId));
            }
            return row;
        }
        // Produk tetap; stok tidak diubah di sini (lewat mutasi stok).
        private void SetRow(EF.ModelContext context, EF.MProductVariant row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            var sku = string.IsNullOrWhiteSpace(Sku) ? null : Sku.Trim();
            ValidateData(context, row.Id, row.ProductId, name, Price, sku);

            row.Name = name;
            row.Price = Price;
            row.Sku = sku;
            row.SeqNo = SeqNo > 0 ? SeqNo
                : ValueUtility.GetNextId<EF.MProductVariant>(context, x => x.SeqNo, x => x.ProductId == row.ProductId);
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MProductVariant row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
