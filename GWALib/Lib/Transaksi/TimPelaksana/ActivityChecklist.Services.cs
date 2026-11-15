using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    public partial class ActivityChecklist
    {
        #region SYNC
        public static List<ActivityChecklist> GetDataList(int activityId, int? sectionId = null, TaskProgressStatus? status = null, int? picMemberId = null)
        {
            return FilterQuery(activityId, sectionId, status, picMemberId).ToList().MappingData<ActivityChecklist, EF.TActivityChecklist>();
        }
        public static Response<ActivityChecklist> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityChecklists.Add(row);
                context.SaveChanges();
                return new ActivityChecklist(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityChecklist>(msg));
        }
        // Membuat checklist dari template; hasil = jumlah item yang dibuat.
        public static Response<int> GenerateFromTemplate(int activityId, int templateId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = Generate(context, activityId, templateId, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public Response<ActivityChecklist> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ActivityChecklist(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityChecklist>(msg));
        }
        public Response<ActivityChecklist> SetStatus(TaskProgressStatus status, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(row, status, userId);
                context.SaveChanges();
                return new ActivityChecklist(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityChecklist>(msg));
        }
        public Response<ActivityChecklist> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityChecklist(Reload(row.Id));
                context.TActivityChecklists.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityChecklist>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ActivityChecklist>> GetDataListAsync(int activityId, int? sectionId = null, TaskProgressStatus? status = null, int? picMemberId = null)
        {
            return await (await FilterQuery(activityId, sectionId, status, picMemberId).ToListAsync()).MappingDataAsync<ActivityChecklist, EF.TActivityChecklist>();
        }
        public static async Task<Response<ActivityChecklist>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityChecklists.Add(row);
                await context.SaveChangesAsync();
                return new ActivityChecklist(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityChecklist>(msg));
        }
        public static async Task<Response<int>> GenerateFromTemplateAsync(int activityId, int templateId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = Generate(context, activityId, templateId, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public async Task<Response<ActivityChecklist>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ActivityChecklist(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityChecklist>(msg));
        }
        public async Task<Response<ActivityChecklist>> SetStatusAsync(TaskProgressStatus status, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(row, status, userId);
                await context.SaveChangesAsync();
                return new ActivityChecklist(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityChecklist>(msg));
        }
        public async Task<Response<ActivityChecklist>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityChecklist(Reload(row.Id));
                context.TActivityChecklists.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityChecklist>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TActivityChecklist Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TActivityChecklist> FilterQuery(int activityId, int? sectionId, TaskProgressStatus? status, int? picMemberId)
        {
            return _Querys.Where(x => x.ActivityId == activityId
                    && (sectionId == null || x.SectionId == sectionId)
                    && (status == null || x.Status == (int)status)
                    && (picMemberId == null || x.PicMemberId == picMemberId))
                .OrderBy(x => x.DueDate == null).ThenBy(x => x.DueDate).ThenByDescending(x => x.Priority).ThenBy(x => x.Title);
        }
        private static EF.TActivityChecklist GetRow(EF.ModelContext context, int id)
        {
            return context.TActivityChecklists.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Checklist"));
        }
        private static async Task<EF.TActivityChecklist> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivityChecklists.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Checklist"));
        }
        private static void ValidateData(EF.ModelContext context, int activityId, string title, int? sectionId, int? picMemberId, Priority priority, MilestoneFlag isMilestone)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Checklist"));
            }
            if (!Enum.IsDefined(priority) || !Enum.IsDefined(isMilestone))
            {
                throw new ArgumentException(ExceptionMessage.DATA_INVALID);
            }
            if (sectionId != null)
            {
                CommitteeSection.GetSection(context, sectionId.Value, activityId);
            }
            if (picMemberId != null)
            {
                Member.GetActiveMember(context, picMemberId.Value);
            }
        }
        private static EF.TActivityChecklist CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            var title = newrow.Title?.Trim() ?? string.Empty;
            var sectionId = newrow.SectionId > 0 ? newrow.SectionId : null;
            var picMemberId = newrow.PicMemberId > 0 ? newrow.PicMemberId : null;
            ValidateData(context, newrow.ActivityId, title, sectionId, picMemberId, newrow.Priority, newrow.IsMilestone);

            var row = new EF.TActivityChecklist
            {
                Id = SequenceUtility.GetNextId<EF.TActivityChecklist>(context),
                ActivityId = newrow.ActivityId,
                SectionId = sectionId,
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                DueDate = newrow.DueDate,
                PicMemberId = picMemberId,
                Priority = (int)newrow.Priority,
                IsMilestone = (int)newrow.IsMilestone,
                Status = (int)TaskProgressStatus.Belum,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Tenggat = tanggal mulai kegiatan + OffsetDay; sie dicocokkan dengan nama (tanpa beda huruf besar/kecil), kosong bila tidak ada.
        private static int Generate(EF.ModelContext context, int activityId, int templateId, int userId)
        {
            var activity = Activity.GetOpenActivity(context, activityId);
            var template = context.MChecklistTemplates.SingleOrDefault(x => x.Id == templateId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Checklist"));
            if (template.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Template Checklist"));
            }
            var existing = context.TActivityChecklists.Where(x => x.ActivityId == activityId && x.TemplateItemId != null).Select(x => x.TemplateItemId!.Value).ToHashSet();
            var items = context.MChecklistTemplateItems.Where(x => x.TemplateId == templateId).OrderBy(x => x.SeqNo).ToList()
                .Where(x => !existing.Contains(x.Id)).ToList();
            if (items.Count == 0)
            {
                return 0;
            }
            var sections = context.TCommitteeSections.Where(x => x.ActivityId == activityId).ToList()
                .GroupBy(x => x.Name.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.First().Id);
            var hDay = DateOnly.FromDateTime(activity.StartAt);
            var firstId = SequenceUtility.ReserveIds<EF.TActivityChecklist>(context, items.Count);
            var now = DateTime.Now;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                int? sectionId = item.SectionName != null && sections.TryGetValue(item.SectionName.Trim().ToUpperInvariant(), out var id) ? id : null;
                context.TActivityChecklists.Add(new EF.TActivityChecklist
                {
                    Id = firstId + i,
                    ActivityId = activityId,
                    SectionId = sectionId,
                    TemplateItemId = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    DueDate = hDay.AddDays(item.OffsetDay),
                    Priority = item.Priority,
                    IsMilestone = item.IsMilestone,
                    Status = (int)TaskProgressStatus.Belum,
                    InsDate = now,
                    InsBy = userId
                });
            }
            return items.Count;
        }
        // Kegiatan dan asal template tetap; item Selesai / Batal dikunci.
        private void SetRow(EF.ModelContext context, EF.TActivityChecklist row, int userId)
        {
            TransaksiHelper.ValidateStatus((TaskProgressStatus)row.Status, "mengubah checklist", TaskProgressStatus.Belum, TaskProgressStatus.Proses);
            Activity.GetOpenActivity(context, row.ActivityId);
            var title = Title?.Trim() ?? string.Empty;
            var sectionId = SectionId > 0 ? SectionId : null;
            var picMemberId = PicMemberId > 0 ? PicMemberId : null;
            ValidateData(context, row.ActivityId, title, sectionId, picMemberId != row.PicMemberId ? picMemberId : null, Priority, IsMilestone);

            row.SectionId = sectionId;
            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.DueDate = DueDate;
            row.PicMemberId = picMemberId;
            row.Priority = (int)Priority;
            row.IsMilestone = (int)IsMilestone;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void ApplyStatus(EF.TActivityChecklist row, TaskProgressStatus target, int userId)
        {
            var current = (TaskProgressStatus)row.Status;
            switch (target)
            {
                case TaskProgressStatus.Proses:
                    TransaksiHelper.ValidateStatus(current, "memproses checklist", TaskProgressStatus.Belum, TaskProgressStatus.Selesai);
                    row.DoneAt = null;
                    row.DoneBy = null;
                    break;
                case TaskProgressStatus.Selesai:
                    TransaksiHelper.ValidateStatus(current, "menyelesaikan checklist", TaskProgressStatus.Belum, TaskProgressStatus.Proses);
                    row.DoneAt = DateTime.Now;
                    row.DoneBy = userId;
                    break;
                case TaskProgressStatus.Batal:
                    TransaksiHelper.ValidateStatus(current, "membatalkan checklist", TaskProgressStatus.Belum, TaskProgressStatus.Proses);
                    break;
                default:
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Checklist"));
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
