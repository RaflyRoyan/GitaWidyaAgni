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
    public partial class ChecklistTemplateItem
    {
        private const int MAX_OFFSET_DAY = 365;

        #region SYNC
        public static List<ChecklistTemplateItem> GetDataList(int? templateId = null, string? sectionName = null, MilestoneFlag? isMilestone = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(templateId, sectionName, isMilestone);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ChecklistTemplateItem, EF.MChecklistTemplateItem>();
        }
        public static Response<ChecklistTemplateItem> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MChecklistTemplateItems.Add(row);
                context.SaveChanges();
                return new ChecklistTemplateItem(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ChecklistTemplateItem>(msg));
        }
        public Response<ChecklistTemplateItem> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ChecklistTemplateItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ChecklistTemplateItem>(msg));
        }
        public Response<ChecklistTemplateItem> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                DetachActivityChecklists(context, row, userId);
                context.MChecklistTemplateItems.Remove(row);
                context.SaveChanges();
                return new ChecklistTemplateItem(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ChecklistTemplateItem>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ChecklistTemplateItem>> GetDataListAsync(int? templateId = null, string? sectionName = null, MilestoneFlag? isMilestone = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(templateId, sectionName, isMilestone);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ChecklistTemplateItem, EF.MChecklistTemplateItem>();
        }
        public static async Task<Response<ChecklistTemplateItem>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MChecklistTemplateItems.Add(row);
                await context.SaveChangesAsync();
                return new ChecklistTemplateItem(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ChecklistTemplateItem>(msg));
        }
        public async Task<Response<ChecklistTemplateItem>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ChecklistTemplateItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ChecklistTemplateItem>(msg));
        }
        public async Task<Response<ChecklistTemplateItem>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                DetachActivityChecklists(context, row, userId);
                context.MChecklistTemplateItems.Remove(row);
                await context.SaveChangesAsync();
                return new ChecklistTemplateItem(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ChecklistTemplateItem>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MChecklistTemplateItem> FilterQuery(int? templateId, string? sectionName, MilestoneFlag? isMilestone)
        {
            return _Querys.Where(x => (templateId == null || x.TemplateId == templateId)
                    && (sectionName == null || x.SectionName == sectionName)
                    && (isMilestone == null || x.IsMilestone == (int)isMilestone))
                .OrderBy(x => x.TemplateId).ThenBy(x => x.SeqNo).ThenBy(x => x.OffsetDay);
        }
        private static EF.MChecklistTemplateItem GetRow(EF.ModelContext context, int id)
        {
            return context.MChecklistTemplateItems.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Checklist"));
        }
        private static async Task<EF.MChecklistTemplateItem> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MChecklistTemplateItems.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Checklist"));
        }
        private static void ValidateData(string title, int offsetDay, Priority priority, MilestoneFlag isMilestone)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Item"));
            }
            if (offsetDay < -MAX_OFFSET_DAY || offsetDay > MAX_OFFSET_DAY)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.CHECKLIST_OFFSET_DAY_INVALID, MAX_OFFSET_DAY));
            }
            if (!Enum.IsDefined(priority))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Prioritas"));
            }
            if (!Enum.IsDefined(isMilestone))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Milestone"));
            }
        }
        // Checklist kegiatan sudah menyalin isi item, jadi cukup dilepas tautannya (template_item_id = NULL) agar riwayat tetap ada.
        private static void DetachActivityChecklists(EF.ModelContext context, EF.MChecklistTemplateItem row, int userId)
        {
            foreach (var checklist in context.TActivityChecklists.Where(x => x.TemplateItemId == row.Id).ToList())
            {
                checklist.TemplateItemId = null;
                checklist.UpdDate = DateTime.Now;
                checklist.UpdBy = userId;
            }
        }
        // Tanpa SaveChanges; dipakai juga oleh ChecklistTemplate.Save untuk membuat item bersama template-nya.
        internal static EF.MChecklistTemplateItem CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            ChecklistTemplate.ValidateActiveTemplate(context, newrow.TemplateId);
            ValidateData(title, newrow.OffsetDay, newrow.Priority, newrow.IsMilestone);

            var row = new EF.MChecklistTemplateItem
            {
                Id = SequenceUtility.GetNextId<EF.MChecklistTemplateItem>(context),
                TemplateId = newrow.TemplateId,
                SectionName = string.IsNullOrWhiteSpace(newrow.SectionName) ? null : newrow.SectionName.Trim(),
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                OffsetDay = newrow.OffsetDay,
                Priority = (int)newrow.Priority,
                IsMilestone = (int)newrow.IsMilestone,
                SeqNo = newrow.SeqNo > 0 ? newrow.SeqNo
                    : ValueUtility.GetNextId<EF.MChecklistTemplateItem>(context, x => x.SeqNo, x => x.TemplateId == newrow.TemplateId),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Template tidak bisa dipindah.
        private void SetRow(EF.ModelContext context, EF.MChecklistTemplateItem row, int userId)
        {
            var title = Title?.Trim() ?? string.Empty;
            ValidateData(title, OffsetDay, Priority, IsMilestone);

            row.SectionName = string.IsNullOrWhiteSpace(SectionName) ? null : SectionName.Trim();
            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.OffsetDay = OffsetDay;
            row.Priority = (int)Priority;
            row.IsMilestone = (int)IsMilestone;
            row.SeqNo = SeqNo > 0 ? SeqNo
                : ValueUtility.GetNextId<EF.MChecklistTemplateItem>(context, x => x.SeqNo, x => x.TemplateId == row.TemplateId);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
