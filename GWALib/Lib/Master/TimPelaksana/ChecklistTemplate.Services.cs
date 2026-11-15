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

namespace GWALib.Lib.Master.TimPelaksana
{
    public partial class ChecklistTemplate
    {
        #region SYNC
        public List<ChecklistTemplateItem> GetItems(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = ChecklistTemplateItem.GetDataList(templateId: Id);
            }
            return ItemList;
        }
        public static List<ChecklistTemplate> GetDataList(FilterAktif aktif, ActivityType? activityType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, activityType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ChecklistTemplate, EF.MChecklistTemplate>();
        }
        public static List<ChecklistTemplate> GetAvailableList(ActivityType? activityType = null)
        {
            return AvailableQuery(activityType).ToList().MappingData<ChecklistTemplate, EF.MChecklistTemplate>();
        }
        public static Response<ChecklistTemplate> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new ChecklistTemplate(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ChecklistTemplate>(msg));
        }
        public Response<ChecklistTemplate> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ChecklistTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ChecklistTemplate>(msg));
        }
        public Response<ChecklistTemplate> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new ChecklistTemplate(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ChecklistTemplate>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<ChecklistTemplateItem>> GetItemsAsync(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = await ChecklistTemplateItem.GetDataListAsync(templateId: Id);
            }
            return ItemList;
        }
        public static async Task<List<ChecklistTemplate>> GetDataListAsync(FilterAktif aktif, ActivityType? activityType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, activityType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ChecklistTemplate, EF.MChecklistTemplate>();
        }
        public static async Task<List<ChecklistTemplate>> GetAvailableListAsync(ActivityType? activityType = null)
        {
            return await (await AvailableQuery(activityType).ToListAsync()).MappingDataAsync<ChecklistTemplate, EF.MChecklistTemplate>();
        }
        public static async Task<Response<ChecklistTemplate>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ChecklistTemplate(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ChecklistTemplate>(msg));
        }
        public async Task<Response<ChecklistTemplate>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ChecklistTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ChecklistTemplate>(msg));
        }
        public async Task<Response<ChecklistTemplate>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new ChecklistTemplate(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ChecklistTemplate>(msg));
        }
        #endregion

        #region HELPER
        // Local: template yang baru di-Add dalam command yang sama (Save dengan Items) belum ada di DB.
        internal static void ValidateActiveTemplate(EF.ModelContext context, int templateId)
        {
            var row = context.MChecklistTemplates.Local.SingleOrDefault(x => x.Id == templateId)
                ?? context.MChecklistTemplates.SingleOrDefault(x => x.Id == templateId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Checklist"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Template Checklist"));
            }
        }
        private static IQueryable<EF.MChecklistTemplate> FilterQuery(FilterAktif aktif, ActivityType? activityType)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (activityType == null || x.ActivityType == (int)activityType))
                .OrderBy(x => x.Name);
        }
        // Template aktif yang cocok untuk jenis kegiatan; template tanpa ActivityType berlaku untuk semua jenis kegiatan.
        // Yang khusus jenis kegiatan itu ditaruh lebih dulu.
        private static IQueryable<EF.MChecklistTemplate> AvailableQuery(ActivityType? activityType)
        {
            return _Querys.Where(x => x.IsActive == (int)ActiveStatus.Aktif
                    && (activityType == null || x.ActivityType == null || x.ActivityType == (int)activityType))
                .OrderBy(x => x.ActivityType == null).ThenBy(x => x.Name);
        }
        private static EF.MChecklistTemplate GetRow(EF.ModelContext context, int id)
        {
            return context.MChecklistTemplates.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Checklist"));
        }
        private static async Task<EF.MChecklistTemplate> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MChecklistTemplates.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Checklist"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, ActivityType? activityType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Template"));
            }
            if (activityType != null && !Enum.IsDefined(activityType.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Kegiatan"));
            }
            ValidateUtility.CheckDuplicateData<EF.MChecklistTemplate>(context,
                x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Template"));
        }
        private static EF.MChecklistTemplate CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name, newrow.ActivityType);

            var row = new EF.MChecklistTemplate
            {
                Id = SequenceUtility.GetNextId<EF.MChecklistTemplate>(context),
                Name = name,
                ActivityType = newrow.ActivityType != null ? (int)newrow.ActivityType : null,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MChecklistTemplates.Add(row);

            foreach (var item in newrow.Items ?? new())
            {
                item.TemplateId = row.Id;
                context.MChecklistTemplateItems.Add(ChecklistTemplateItem.CreateRow(context, item, userId));
            }
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MChecklistTemplate row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, ActivityType);

            row.Name = name;
            row.ActivityType = ActivityType != null ? (int)ActivityType : null;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MChecklistTemplate row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
