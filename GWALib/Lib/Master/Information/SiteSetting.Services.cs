using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Information
{
    public partial class SiteSetting
    {
        #region SYNC
        public ImageContent? GetImage()
        {
            using var context = DBClass.GetContext();
            var image = context.MSiteSettings.Where(x => x.Id == Id).Select(x => x.SettingImage).SingleOrDefault();
            return image == null ? null : new ImageContent(image, ImageUtility.GetContentType(image));
        }
        // Untuk landing page: satu pengaturan berdasarkan key (mis. "contact_email"); null = belum diatur.
        public static SiteSetting? GetByKey(string settingKey)
        {
            var key = ToKey(settingKey);
            var row = _Querys.SingleOrDefault(x => x.Setting.SettingKey == key);
            return row != null ? new SiteSetting(row) : null;
        }
        public static List<SiteSetting> GetDataList(string? settingGroup = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(settingGroup);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<SiteSetting, ListRow>();
        }
        public static Response<SiteSetting> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MSiteSettings.Add(row);
                context.SaveChanges();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SiteSetting>(msg));
        }
        public Response<SiteSetting> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SiteSetting>(msg));
        }
        public Response<SiteSetting> UpdateImage(byte[]? image, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowImage(row, image, userId);
                context.SaveChanges();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SiteSetting>(msg));
        }
        public Response<SiteSetting> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MSiteSettings.Remove(row);
                context.SaveChanges();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SiteSetting>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<ImageContent?> GetImageAsync()
        {
            using var context = DBClass.GetContext();
            var image = await context.MSiteSettings.Where(x => x.Id == Id).Select(x => x.SettingImage).SingleOrDefaultAsync();
            return image == null ? null : new ImageContent(image, ImageUtility.GetContentType(image));
        }
        public static async Task<SiteSetting?> GetByKeyAsync(string settingKey)
        {
            var key = ToKey(settingKey);
            var row = await _Querys.SingleOrDefaultAsync(x => x.Setting.SettingKey == key);
            return row != null ? new SiteSetting(row) : null;
        }
        public static async Task<List<SiteSetting>> GetDataListAsync(string? settingGroup = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(settingGroup);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<SiteSetting, ListRow>();
        }
        public static async Task<Response<SiteSetting>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MSiteSettings.Add(row);
                await context.SaveChangesAsync();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SiteSetting>(msg));
        }
        public async Task<Response<SiteSetting>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SiteSetting>(msg));
        }
        public async Task<Response<SiteSetting>> UpdateImageAsync(byte[]? image, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowImage(row, image, userId);
                await context.SaveChangesAsync();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SiteSetting>(msg));
        }
        public async Task<Response<SiteSetting>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MSiteSettings.Remove(row);
                await context.SaveChangesAsync();
                return new SiteSetting(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SiteSetting>(msg));
        }
        #endregion

        #region HELPER
        // Grup: huruf besar (TENTANG, KONTAK, SOSMED); key: huruf kecil snake_case (about_text, contact_email).
        internal static string ToGroupCode(string? value)
        {
            return Regex.Replace(value?.Trim().ToUpperInvariant() ?? string.Empty, "[^A-Z0-9]+", "_").Trim('_');
        }
        internal static string ToKey(string? value)
        {
            return Regex.Replace(value?.Trim().ToLowerInvariant() ?? string.Empty, "[^a-z0-9]+", "_").Trim('_');
        }
        private static IQueryable<ListRow> FilterQuery(string? settingGroup)
        {
            var group = string.IsNullOrWhiteSpace(settingGroup) ? null : ToGroupCode(settingGroup);
            return _Querys.Where(x => group == null || x.Setting.SettingGroup == group)
                .OrderBy(x => x.Setting.SettingGroup).ThenBy(x => x.Setting.SettingKey);
        }
        private static EF.MSiteSetting GetRow(EF.ModelContext context, int id)
        {
            return context.MSiteSettings.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengaturan Situs"));
        }
        private static async Task<EF.MSiteSetting> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MSiteSettings.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengaturan Situs"));
        }
        private static EF.MSiteSetting CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var group = ToGroupCode(newrow.SettingGroup);
            var key = ToKey(newrow.SettingKey);
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Grup Pengaturan"));
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Key Pengaturan"));
            }
            ValidateUtility.CheckDuplicateData<EF.MSiteSetting>(context,
                x => x.SettingKey == key,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Key Pengaturan"));

            var row = new EF.MSiteSetting
            {
                Id = SequenceUtility.GetNextId<EF.MSiteSetting>(context),
                SettingGroup = group,
                SettingKey = key,
                SettingValue = string.IsNullOrWhiteSpace(newrow.SettingValue) ? null : newrow.SettingValue.Trim(),
                SettingImage = newrow.SettingImage != null ? ImageUtility.Normalize(newrow.SettingImage).Data : null,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Key tetap (key dipakai kode / tampilan); gambar diubah lewat UpdateImage.
        private void SetRow(EF.MSiteSetting row, int userId)
        {
            var group = ToGroupCode(SettingGroup);
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Grup Pengaturan"));
            }

            row.SettingGroup = group;
            row.SettingValue = string.IsNullOrWhiteSpace(SettingValue) ? null : SettingValue.Trim();
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // null = hapus gambar.
        private static void SetRowImage(EF.MSiteSetting row, byte[]? image, int userId)
        {
            row.SettingImage = image != null ? ImageUtility.Normalize(image).Data : null;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
