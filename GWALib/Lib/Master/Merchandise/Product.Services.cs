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
    public partial class Product
    {
        internal const string INITIAL_STOCK_NOTE = "Stok awal";

        #region SYNC
        public List<ProductVariant> GetVariants(bool refresh)
        {
            if (refresh || VariantList == null)
            {
                VariantList = ProductVariant.GetDataList(FilterAktif.Semua, productId: Id);
            }
            return VariantList;
        }
        public PhotoData? GetPhoto()
        {
            using var context = DBClass.GetContext();
            var photo = context.MProducts.Where(x => x.Id == Id).Select(x => new { x.Photo, x.PhotoFileName, x.PhotoContentType }).SingleOrDefault();
            return photo?.Photo == null ? null
                : new PhotoData(photo.Photo, photo.PhotoFileName ?? "produk", photo.PhotoContentType ?? ImageUtility.GetContentType(photo.Photo));
        }
        public static List<Product> GetDataList(FilterAktif aktif, ProductType? productType = null, int? activityId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, productType, activityId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Product, ListRow>();
        }
        public static Response<Product> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Product(row.Id);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Product>(msg));
        }
        public Response<Product> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Product(row.Id);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Product>(msg));
        }
        public Response<Product> UpdatePhoto(byte[]? photo, string? fileName, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowPhoto(row, photo, fileName, userId);
                context.SaveChanges();
                return new Product(row.Id);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Product>(msg));
        }
        public Response<Product> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new Product(row.Id);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Product>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<ProductVariant>> GetVariantsAsync(bool refresh)
        {
            if (refresh || VariantList == null)
            {
                VariantList = await ProductVariant.GetDataListAsync(FilterAktif.Semua, productId: Id);
            }
            return VariantList;
        }
        public async Task<PhotoData?> GetPhotoAsync()
        {
            using var context = DBClass.GetContext();
            var photo = await context.MProducts.Where(x => x.Id == Id).Select(x => new { x.Photo, x.PhotoFileName, x.PhotoContentType }).SingleOrDefaultAsync();
            return photo?.Photo == null ? null
                : new PhotoData(photo.Photo, photo.PhotoFileName ?? "produk", photo.PhotoContentType ?? ImageUtility.GetContentType(photo.Photo));
        }
        public static async Task<List<Product>> GetDataListAsync(FilterAktif aktif, ProductType? productType = null, int? activityId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, productType, activityId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Product, ListRow>();
        }
        public static async Task<Response<Product>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Product(row.Id);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Product>(msg));
        }
        public async Task<Response<Product>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Product(row.Id);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Product>(msg));
        }
        public async Task<Response<Product>> UpdatePhotoAsync(byte[]? photo, string? fileName, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowPhoto(row, photo, fileName, userId);
                await context.SaveChangesAsync();
                return new Product(row.Id);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Product>(msg));
        }
        public async Task<Response<Product>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new Product(row.Id);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Product>(msg));
        }
        #endregion

        #region HELPER
        // Local: produk yang baru di-Add dalam command yang sama (Save dengan Variants) belum ada di DB.
        internal static EF.MProduct GetActiveProduct(EF.ModelContext context, int productId)
        {
            var row = context.MProducts.Local.SingleOrDefault(x => x.Id == productId)
                ?? context.MProducts.SingleOrDefault(x => x.Id == productId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Produk"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Produk"));
            }
            return row;
        }
        // Stok awal dicatat sebagai mutasi Penyesuaian supaya riwayat stok lengkap sejak awal.
        internal static EF.TProductStockMovement CreateInitialStockMovement(EF.ModelContext context, int productId, int? variantId, int quantity, int userId)
        {
            return new EF.TProductStockMovement
            {
                Id = SequenceUtility.GetNextId<EF.TProductStockMovement>(context),
                ProductId = productId,
                VariantId = variantId,
                MovementType = (int)StockMovementType.Penyesuaian,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                Notes = INITIAL_STOCK_NOTE,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private static IQueryable<ListRow> FilterQuery(FilterAktif aktif, ProductType? productType, int? activityId)
        {
            return _Querys.Where(x => (x.Product.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (productType == null || x.Product.ProductType == (int)productType)
                    && (activityId == null || x.Product.ActivityId == activityId))
                .OrderBy(x => x.Product.Name);
        }
        private static EF.MProduct GetRow(EF.ModelContext context, int id)
        {
            return context.MProducts.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Produk"));
        }
        private static async Task<EF.MProduct> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MProducts.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Produk"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, ProductType productType, decimal price,
            decimal? costPrice, string unit, StockTracking trackStock, SaleMode isPreorder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Produk"));
            }
            if (string.IsNullOrWhiteSpace(unit))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Satuan"));
            }
            if (!Enum.IsDefined(productType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Produk"));
            }
            if (!Enum.IsDefined(trackStock))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Pelacakan Stok"));
            }
            if (!Enum.IsDefined(isPreorder))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Mode Penjualan"));
            }
            if (price < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Harga Jual"));
            }
            if (costPrice != null && costPrice < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Harga Modal"));
            }
            ValidateUtility.CheckDuplicateData<EF.MProduct>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Produk"));
        }
        // Kegiatan (danus) cukup harus ada.
        private static void ValidateActivity(EF.ModelContext context, int activityId)
        {
            if (!context.TActivities.Any(x => x.Id == activityId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan"));
            }
        }
        private static EF.MProduct CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var unit = newrow.Unit?.Trim() ?? string.Empty;
            var activityId = newrow.ActivityId > 0 ? newrow.ActivityId : null;
            var preorderCloseAt = newrow.IsPreorder == SaleMode.PreOrder ? newrow.PreorderCloseAt : null;
            var hasVariants = newrow.Variants?.Count > 0;
            var stock = newrow.TrackStock == StockTracking.Dilacak ? newrow.Stock : 0;
            ValidateData(context, 0, name, newrow.ProductType, newrow.Price, newrow.CostPrice, unit, newrow.TrackStock, newrow.IsPreorder);
            if (stock < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Stok Awal"));
            }
            if (hasVariants && stock != 0)
            {
                throw new ArgumentException(ExceptionMessage.PRODUCT_STOCK_USES_VARIANT);
            }
            if (preorderCloseAt != null && preorderCloseAt <= DateTime.Now)
            {
                throw new ArgumentException(ExceptionMessage.PREORDER_CLOSE_INVALID);
            }
            if (activityId != null)
            {
                ValidateActivity(context, activityId.Value);
            }

            var row = new EF.MProduct
            {
                Id = SequenceUtility.GetNextId<EF.MProduct>(context),
                Name = name,
                ProductType = (int)newrow.ProductType,
                Description = ToNullable(newrow.Description),
                Price = newrow.Price,
                CostPrice = newrow.CostPrice,
                Unit = unit,
                TrackStock = (int)newrow.TrackStock,
                Stock = stock,
                IsPreorder = (int)newrow.IsPreorder,
                PreorderCloseAt = preorderCloseAt,
                ActivityId = activityId,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            if (newrow.Photo != null)
            {
                ApplyPhoto(row, newrow.Photo, newrow.PhotoFileName);
            }
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MProducts.Add(row);
            if (stock > 0)
            {
                context.TProductStockMovements.Add(CreateInitialStockMovement(context, row.Id, null, stock, userId));
            }

            foreach (var variant in newrow.Variants ?? new())
            {
                variant.ProductId = row.Id;
                ProductVariant.AddRow(context, variant, userId);
            }
            return row;
        }
        // Stok tidak diubah di sini (lewat mutasi stok); foto lewat UpdatePhoto.
        // Batas pre-order dan kegiatan hanya divalidasi bila diganti.
        private void SetRow(EF.ModelContext context, EF.MProduct row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            var unit = Unit?.Trim() ?? string.Empty;
            var activityId = ActivityId > 0 ? ActivityId : null;
            var preorderCloseAt = IsPreorder == SaleMode.PreOrder ? PreorderCloseAt : null;
            ValidateData(context, row.Id, name, ProductType, Price, CostPrice, unit, TrackStock, IsPreorder);
            if (preorderCloseAt != null && preorderCloseAt != row.PreorderCloseAt && preorderCloseAt <= DateTime.Now)
            {
                throw new ArgumentException(ExceptionMessage.PREORDER_CLOSE_INVALID);
            }
            if (activityId != null && activityId != row.ActivityId)
            {
                ValidateActivity(context, activityId.Value);
            }

            row.Name = name;
            row.ProductType = (int)ProductType;
            row.Description = ToNullable(Description);
            row.Price = Price;
            row.CostPrice = CostPrice;
            row.Unit = unit;
            row.TrackStock = (int)TrackStock;
            row.IsPreorder = (int)IsPreorder;
            row.PreorderCloseAt = preorderCloseAt;
            row.ActivityId = activityId;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void ApplyPhoto(EF.MProduct row, byte[] photo, string? fileName)
        {
            var image = ImageUtility.Normalize(photo);
            row.Photo = image.Data;
            row.PhotoContentType = image.ContentType;
            row.PhotoFileName = ToNullable(fileName);
        }
        // null = hapus foto.
        private static void SetRowPhoto(EF.MProduct row, byte[]? photo, string? fileName, int userId)
        {
            if (photo != null)
            {
                ApplyPhoto(row, photo, fileName);
            }
            else
            {
                row.Photo = null;
                row.PhotoContentType = null;
                row.PhotoFileName = null;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MProduct row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
