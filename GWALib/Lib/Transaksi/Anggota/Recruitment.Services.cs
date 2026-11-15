using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    public partial class Recruitment
    {
        #region SYNC
        public List<RecruitmentApplicant> GetApplicants(bool refresh, ApplicantStatus? status = null)
        {
            if (refresh || ApplicantList == null || status != null)
            {
                ApplicantList = RecruitmentApplicant.GetDataList(recruitmentId: Id, status: status);
            }
            return ApplicantList;
        }
        public static List<Recruitment> GetDataList(RecruitmentStatus? status = null, int? periodId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Recruitment, EF.TRecruitment>();
        }
        // Untuk halaman publik: rekrutmen berstatus Open yang sedang dalam rentang tanggal pendaftaran.
        public static List<Recruitment> GetOpenList()
        {
            return OpenQuery().ToList().MappingData<Recruitment, EF.TRecruitment>();
        }
        public static Response<Recruitment> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TRecruitments.Add(row);
                context.SaveChanges();
                return new Recruitment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Recruitment>(msg));
        }
        public Response<Recruitment> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Recruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Recruitment>(msg));
        }
        public Response<Recruitment> Open(string user)
        {
            return SetStatus(RecruitmentStatus.Open, user);
        }
        public Response<Recruitment> Close(string user)
        {
            return SetStatus(RecruitmentStatus.Closed, user);
        }
        // Hanya rekrutmen Draft yang belum punya pendaftar yang bisa dihapus.
        public Response<Recruitment> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                context.TRecruitments.Remove(row);
                context.SaveChanges();
                return new Recruitment(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Recruitment>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<RecruitmentApplicant>> GetApplicantsAsync(bool refresh, ApplicantStatus? status = null)
        {
            if (refresh || ApplicantList == null || status != null)
            {
                ApplicantList = await RecruitmentApplicant.GetDataListAsync(recruitmentId: Id, status: status);
            }
            return ApplicantList;
        }
        public static async Task<List<Recruitment>> GetDataListAsync(RecruitmentStatus? status = null, int? periodId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Recruitment, EF.TRecruitment>();
        }
        public static async Task<List<Recruitment>> GetOpenListAsync()
        {
            return await (await OpenQuery().ToListAsync()).MappingDataAsync<Recruitment, EF.TRecruitment>();
        }
        public static async Task<Response<Recruitment>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TRecruitments.Add(row);
                await context.SaveChangesAsync();
                return new Recruitment(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Recruitment>(msg));
        }
        public async Task<Response<Recruitment>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Recruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Recruitment>(msg));
        }
        public async Task<Response<Recruitment>> OpenAsync(string user)
        {
            return await SetStatusAsync(RecruitmentStatus.Open, user);
        }
        public async Task<Response<Recruitment>> CloseAsync(string user)
        {
            return await SetStatusAsync(RecruitmentStatus.Closed, user);
        }
        public async Task<Response<Recruitment>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                context.TRecruitments.Remove(row);
                await context.SaveChangesAsync();
                return new Recruitment(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Recruitment>(msg));
        }
        #endregion

        #region INTERNAL
        // Rekrutmen yang sedang menerima pendaftaran (status Open dan dalam rentang tanggal).
        internal static EF.TRecruitment GetOpenRecruitment(EF.ModelContext context, int recruitmentId)
        {
            var row = context.TRecruitments.SingleOrDefault(x => x.Id == recruitmentId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rekrutmen"));
            var now = DateTime.Now;
            if (row.Status != (int)RecruitmentStatus.Open || now < row.OpenDate || now > row.CloseDate)
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_NOT_OPEN);
            }
            return row;
        }
        #endregion

        #region HELPER
        private Response<Recruitment> SetStatus(RecruitmentStatus target, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(row, target, userId);
                context.SaveChanges();
                return new Recruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Recruitment>(msg));
        }
        private async Task<Response<Recruitment>> SetStatusAsync(RecruitmentStatus target, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(row, target, userId);
                await context.SaveChangesAsync();
                return new Recruitment(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Recruitment>(msg));
        }
        // Draft → Open → Closed. Rekrutmen Closed bisa dibuka lagi (mis. perpanjangan) bila tanggal tutup diperbarui dulu.
        private static void ApplyStatus(EF.TRecruitment row, RecruitmentStatus target, int userId)
        {
            var current = (RecruitmentStatus)row.Status;
            if (target == RecruitmentStatus.Open)
            {
                TransaksiHelper.ValidateStatus(current, "membuka rekrutmen", RecruitmentStatus.Draft, RecruitmentStatus.Closed);
                if (row.CloseDate <= DateTime.Now)
                {
                    throw new ArgumentException(ExceptionMessage.RECRUITMENT_CLOSE_DATE_PASSED);
                }
            }
            else
            {
                TransaksiHelper.ValidateStatus(current, "menutup rekrutmen", RecruitmentStatus.Open);
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static IQueryable<EF.TRecruitment> FilterQuery(RecruitmentStatus? status, int? periodId)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status)
                    && (periodId == null || x.PeriodId == periodId))
                .OrderByDescending(x => x.OpenDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TRecruitment> OpenQuery()
        {
            var now = DateTime.Now;
            return _Querys.Where(x => x.Status == (int)RecruitmentStatus.Open && x.OpenDate <= now && x.CloseDate >= now)
                .OrderBy(x => x.CloseDate);
        }
        private static EF.TRecruitment GetRow(EF.ModelContext context, int id)
        {
            return context.TRecruitments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rekrutmen"));
        }
        private static async Task<EF.TRecruitment> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TRecruitments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rekrutmen"));
        }
        private static void ValidateData(string title, DateTime openDate, DateTime closeDate, int? quota, RecruitmentAudience acceptPublic)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Rekrutmen"));
            }
            if (openDate == default || closeDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Buka dan Tutup"));
            }
            if (closeDate <= openDate)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (quota != null && quota <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kuota"));
            }
            if (!Enum.IsDefined(acceptPublic))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Sasaran Pendaftar"));
            }
        }
        private static void ValidateDelete(EF.ModelContext context, EF.TRecruitment row)
        {
            TransaksiHelper.ValidateStatus((RecruitmentStatus)row.Status, "menghapus rekrutmen", RecruitmentStatus.Draft);
            if (context.TRecruitmentApplicants.Any(x => x.RecruitmentId == row.Id) || context.TForms.Any(x => x.RecruitmentId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_IN_USE);
            }
        }
        private static EF.TRecruitment CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            var periodId = newrow.PeriodId > 0 ? newrow.PeriodId : null;
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            ValidateData(title, newrow.OpenDate, newrow.CloseDate, newrow.Quota, newrow.AcceptPublic);

            var row = new EF.TRecruitment
            {
                Id = SequenceUtility.GetNextId<EF.TRecruitment>(context),
                PeriodId = periodId,
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                OpenDate = newrow.OpenDate,
                CloseDate = newrow.CloseDate,
                Quota = newrow.Quota,
                AcceptPublic = (int)newrow.AcceptPublic,
                Status = (int)RecruitmentStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kuota tidak boleh di bawah jumlah pendaftar yang sudah diterima.
        private void SetRow(EF.ModelContext context, EF.TRecruitment row, int userId)
        {
            var title = Title?.Trim() ?? string.Empty;
            var periodId = PeriodId > 0 ? PeriodId : null;
            if (periodId != null && periodId != row.PeriodId)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            ValidateData(title, OpenDate, CloseDate, Quota, AcceptPublic);
            if (Quota != null && context.TRecruitmentApplicants.Count(x => x.RecruitmentId == row.Id && x.Status == (int)ApplicantStatus.Accepted) > Quota)
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_QUOTA_BELOW_ACCEPTED);
            }

            row.PeriodId = periodId;
            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.OpenDate = OpenDate;
            row.CloseDate = CloseDate;
            row.Quota = Quota;
            row.AcceptPublic = (int)AcceptPublic;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
