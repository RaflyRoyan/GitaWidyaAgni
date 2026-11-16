using GWALib.General;
using GWALib.General.Utilities;
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
    public partial class CommitteeRecruitment
    {
        #region SYNC
        public static CommitteeRecruitment? GetByActivity(int activityId)
        {
            var row = _Querys.SingleOrDefault(x => x.ActivityId == activityId);
            return row != null ? new CommitteeRecruitment(row) : null;
        }
        // Untuk anggota: recruitment yang sedang membuka pendaftaran.
        public static List<CommitteeRecruitment> GetOpenList()
        {
            return OpenQuery().ToList().MappingData<CommitteeRecruitment, EF.TCommitteeRecruitment>();
        }
        public static Response<CommitteeRecruitment> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TCommitteeRecruitments.Add(row);
                context.SaveChanges();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeRecruitment>(msg));
        }
        public Response<CommitteeRecruitment> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeRecruitment>(msg));
        }
        public Response<CommitteeRecruitment> Open(string user)
        {
            return SetStatus(CommitteeRecruitmentStatus.Buka, user);
        }
        public Response<CommitteeRecruitment> Close(string user)
        {
            return SetStatus(CommitteeRecruitmentStatus.Tutup, user);
        }
        public Response<CommitteeRecruitment> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                context.TCommitteeRecruitments.Remove(row);
                context.SaveChanges();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<CommitteeRecruitment>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<CommitteeRecruitment?> GetByActivityAsync(int activityId)
        {
            var row = await _Querys.SingleOrDefaultAsync(x => x.ActivityId == activityId);
            return row != null ? new CommitteeRecruitment(row) : null;
        }
        public static async Task<List<CommitteeRecruitment>> GetOpenListAsync()
        {
            return await (await OpenQuery().ToListAsync()).MappingDataAsync<CommitteeRecruitment, EF.TCommitteeRecruitment>();
        }
        public static async Task<Response<CommitteeRecruitment>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TCommitteeRecruitments.Add(row);
                await context.SaveChangesAsync();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeRecruitment>(msg));
        }
        public async Task<Response<CommitteeRecruitment>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeRecruitment>(msg));
        }
        public async Task<Response<CommitteeRecruitment>> OpenAsync(string user)
        {
            return await SetStatusAsync(CommitteeRecruitmentStatus.Buka, user);
        }
        public async Task<Response<CommitteeRecruitment>> CloseAsync(string user)
        {
            return await SetStatusAsync(CommitteeRecruitmentStatus.Tutup, user);
        }
        public async Task<Response<CommitteeRecruitment>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                context.TCommitteeRecruitments.Remove(row);
                await context.SaveChangesAsync();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<CommitteeRecruitment>(msg));
        }
        #endregion

        #region INTERNAL
        // Recruitment yang sedang menerima lamaran (Buka dan dalam rentang tanggal).
        internal static EF.TCommitteeRecruitment GetOpenRecruitment(EF.ModelContext context, int recruitmentId)
        {
            var row = context.TCommitteeRecruitments.SingleOrDefault(x => x.Id == recruitmentId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pendaftaran Tim Pelaksana"));
            var now = DateTime.Now;
            if (row.Status != (int)CommitteeRecruitmentStatus.Buka || now < row.OpenAt || now > row.CloseAt)
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_NOT_OPEN);
            }
            return row;
        }
        #endregion

        #region HELPER
        private Response<CommitteeRecruitment> SetStatus(CommitteeRecruitmentStatus target, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(context, row, target, userId);
                context.SaveChanges();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeRecruitment>(msg));
        }
        private async Task<Response<CommitteeRecruitment>> SetStatusAsync(CommitteeRecruitmentStatus target, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(context, row, target, userId);
                await context.SaveChangesAsync();
                return new CommitteeRecruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeRecruitment>(msg));
        }
        private static IQueryable<EF.TCommitteeRecruitment> OpenQuery()
        {
            var now = DateTime.Now;
            return _Querys.Where(x => x.Status == (int)CommitteeRecruitmentStatus.Buka && x.OpenAt <= now && x.CloseAt >= now)
                .OrderBy(x => x.CloseAt);
        }
        private static EF.TCommitteeRecruitment GetRow(EF.ModelContext context, int id)
        {
            return context.TCommitteeRecruitments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pendaftaran Tim Pelaksana"));
        }
        private static async Task<EF.TCommitteeRecruitment> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCommitteeRecruitments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pendaftaran Tim Pelaksana"));
        }
        private static void ValidateData(EF.ModelContext context, int id, DateTime openAt, DateTime closeAt, int? quota, int? formId)
        {
            if (openAt == default || closeAt == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Buka dan Tutup"));
            }
            if (closeAt <= openAt)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (quota != null && quota <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kuota"));
            }
            if (quota != null && id > 0 && context.TCommitteeApplications.Count(x => x.RecruitmentId == id && x.Status == (int)CommitteeApplicationStatus.Diterima) > quota)
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_QUOTA_BELOW_ACCEPTED);
            }
            if (formId != null)
            {
                var form = context.TForms.Where(x => x.Id == formId).Select(x => new { x.FormType, x.RecruitmentId }).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Formulir"));
                // Hanya formulir PendaftaranRekrutmen yang tidak terikat rekrutmen anggota.
                if (form.FormType != (int)FormType.PendaftaranRekrutmen || form.RecruitmentId != null)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Formulir"));
                }
            }
        }
        private static void ValidateDelete(EF.ModelContext context, EF.TCommitteeRecruitment row)
        {
            TransaksiHelper.ValidateStatus((CommitteeRecruitmentStatus)row.Status, "menghapus pendaftaran tim pelaksana", CommitteeRecruitmentStatus.Draft);
            if (context.TCommitteeApplications.Any(x => x.RecruitmentId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_IN_USE);
            }
        }
        private static EF.TCommitteeRecruitment CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            ValidateUtility.CheckDuplicateData<EF.TCommitteeRecruitment>(context, x => x.ActivityId == newrow.ActivityId,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Pendaftaran tim pelaksana untuk kegiatan"));
            var formId = newrow.FormId > 0 ? newrow.FormId : null;
            ValidateData(context, 0, newrow.OpenAt, newrow.CloseAt, newrow.Quota, formId);

            var row = new EF.TCommitteeRecruitment
            {
                Id = SequenceUtility.GetNextId<EF.TCommitteeRecruitment>(context),
                ActivityId = newrow.ActivityId,
                OpenAt = newrow.OpenAt,
                CloseAt = newrow.CloseAt,
                Quota = newrow.Quota,
                FormId = formId,
                Status = (int)CommitteeRecruitmentStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan tetap.
        private void SetRow(EF.ModelContext context, EF.TCommitteeRecruitment row, int userId)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            var formId = FormId > 0 ? FormId : null;
            ValidateData(context, row.Id, OpenAt, CloseAt, Quota, formId);

            row.OpenAt = OpenAt;
            row.CloseAt = CloseAt;
            row.Quota = Quota;
            row.FormId = formId;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void ApplyStatus(EF.ModelContext context, EF.TCommitteeRecruitment row, CommitteeRecruitmentStatus target, int userId)
        {
            var current = (CommitteeRecruitmentStatus)row.Status;
            if (target == CommitteeRecruitmentStatus.Buka)
            {
                TransaksiHelper.ValidateStatus(current, "membuka pendaftaran", CommitteeRecruitmentStatus.Draft, CommitteeRecruitmentStatus.Tutup);
                Activity.GetOpenActivity(context, row.ActivityId);
                if (row.CloseAt <= DateTime.Now)
                {
                    throw new ArgumentException(ExceptionMessage.RECRUITMENT_CLOSE_DATE_PASSED);
                }
            }
            else if (target == CommitteeRecruitmentStatus.Tutup)
            {
                TransaksiHelper.ValidateStatus(current, "menutup pendaftaran", CommitteeRecruitmentStatus.Buka);
            }
            else
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Pendaftaran"));
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
