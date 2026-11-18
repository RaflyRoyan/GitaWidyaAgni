using GWALib.General;
using GWALib.Lib.Transaksi.Information;
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
    public partial class ManagementAssignment
    {
        #region SYNC
        public static List<ManagementAssignment> GetDataList(int? periodId = null, int? memberId = null, AssignmentPeriod? isCurrent = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, memberId, isCurrent);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ManagementAssignment, EF.TManagementAssignment>();
        }
        public static Response<ManagementAssignment> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TManagementAssignments.Add(row);
                context.SaveChanges();
                return new ManagementAssignment(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ManagementAssignment>(msg));
        }
        public Response<ManagementAssignment> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ManagementAssignment(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ManagementAssignment>(msg));
        }
        // Selesai menjabat (mis. diganti di tengah periode); endDate null = hari ini.
        public Response<ManagementAssignment> End(DateOnly? endDate, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetEnd(row, endDate ?? DateOnly.FromDateTime(DateTime.Today), userId);
                context.SaveChanges();
                return new ManagementAssignment(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ManagementAssignment>(msg));
        }
        // Hapus permanen (mis. salah input); ditolak bila penugasan sudah dipakai sertifikat.
        public Response<ManagementAssignment> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = context.TManagementAssignments.Include(x => x.Period).Include(x => x.Member).Include(x => x.Position).Include(x => x.Division)
                    .SingleOrDefault(x => x.Id == Id) ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penugasan Pengurus"));
                ValidateDelete(context, row);
                context.TManagementAssignments.Remove(row);
                context.SaveChanges();
                return new ManagementAssignment(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ManagementAssignment>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ManagementAssignment>> GetDataListAsync(int? periodId = null, int? memberId = null, AssignmentPeriod? isCurrent = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, memberId, isCurrent);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ManagementAssignment, EF.TManagementAssignment>();
        }
        public static async Task<Response<ManagementAssignment>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TManagementAssignments.Add(row);
                await context.SaveChangesAsync();
                return new ManagementAssignment(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ManagementAssignment>(msg));
        }
        public async Task<Response<ManagementAssignment>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ManagementAssignment(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ManagementAssignment>(msg));
        }
        public async Task<Response<ManagementAssignment>> EndAsync(DateOnly? endDate, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetEnd(row, endDate ?? DateOnly.FromDateTime(DateTime.Today), userId);
                await context.SaveChangesAsync();
                return new ManagementAssignment(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ManagementAssignment>(msg));
        }
        public async Task<Response<ManagementAssignment>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await context.TManagementAssignments.Include(x => x.Period).Include(x => x.Member).Include(x => x.Position).Include(x => x.Division)
                    .SingleOrDefaultAsync(x => x.Id == Id) ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penugasan Pengurus"));
                ValidateDelete(context, row);
                context.TManagementAssignments.Remove(row);
                await context.SaveChangesAsync();
                return new ManagementAssignment(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ManagementAssignment>(msg));
        }
        #endregion

        #region INTERNAL
        // Dipakai serah terima periode: semua penugasan berjalan periode itu diakhiri pada tanggal serah terima.
        internal static void EndAllInPeriod(EF.ModelContext context, int periodId, DateOnly endDate, int userId)
        {
            var rows = context.TManagementAssignments.Where(x => x.PeriodId == periodId && x.IsCurrent == (int)AssignmentPeriod.Berjalan).ToList();
            foreach (var row in rows)
            {
                row.IsCurrent = (int)AssignmentPeriod.Lampau;
                row.EndDate ??= endDate < row.StartDate ? row.StartDate : endDate;
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
            }
        }
        #endregion

        #region HELPER
        private static EF.TManagementAssignment Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TManagementAssignment> FilterQuery(int? periodId, int? memberId, AssignmentPeriod? isCurrent)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (memberId == null || x.MemberId == memberId)
                    && (isCurrent == null || x.IsCurrent == (int)isCurrent))
                .OrderByDescending(x => x.Period.StartDate).ThenByDescending(x => x.Position.IsCore).ThenBy(x => x.Position.SortOrder).ThenBy(x => x.Member.Fullname);
        }
        private static EF.TManagementAssignment GetRow(EF.ModelContext context, int id)
        {
            return context.TManagementAssignments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penugasan Pengurus"));
        }
        private static async Task<EF.TManagementAssignment> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TManagementAssignments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penugasan Pengurus"));
        }
        private static void ValidateDates(DateOnly startDate, DateOnly? endDate)
        {
            if (startDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Mulai"));
            }
            if (endDate != null && endDate < startDate)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
        }
        // Jabatan inti hanya satu pemegang berjalan per (periode, jabatan, divisi, section); jabatan biasa boleh banyak.
        private static void ValidateHolder(EF.ModelContext context, int id, int periodId, int memberId, int positionId, int? divisionId, int? sectionId)
        {
            var position = context.MPositions.Single(x => x.Id == positionId);
            ValidateUtility.CheckDuplicateData<EF.TManagementAssignment>(context,
                x => x.PeriodId == periodId && x.MemberId == memberId && x.PositionId == positionId && x.DivisionId == divisionId && x.SectionId == sectionId
                    && x.IsCurrent == (int)AssignmentPeriod.Berjalan && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Anggota, Jabatan dan Periode"));
            if (position.IsCore == (int)PositionLevel.Inti)
            {
                ValidateUtility.CheckDuplicateData<EF.TManagementAssignment>(context,
                    x => x.PeriodId == periodId && x.PositionId == positionId && x.DivisionId == divisionId && x.SectionId == sectionId
                        && x.IsCurrent == (int)AssignmentPeriod.Berjalan && x.Id != id,
                    string.Format(ExceptionMessage.POSITION_HOLDER_EXISTS, position.Name));
            }
        }
        // Penugasan memimpin divisi ATAU section (Section Leader), tidak keduanya. Section divalidasi aktif bila baru / diganti.
        private static void ValidateSection(EF.ModelContext context, int? divisionId, int? sectionId, int? oldSectionId)
        {
            if (divisionId != null && sectionId != null)
            {
                throw new ArgumentException("Penugasan hanya untuk satu divisi atau satu section, tidak keduanya.");
            }
            if (sectionId != null && sectionId != oldSectionId)
            {
                Section.ValidateActiveSection(context, sectionId.Value);
            }
        }
        private static void ValidateDelete(EF.ModelContext context, EF.TManagementAssignment row)
        {
            if (context.TCertificates.Any(x => x.AssignmentId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.ASSIGNMENT_HAS_CERTIFICATE);
            }
        }
        private static EF.TManagementAssignment CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ManagementPeriod.ValidateOpenPeriod(context, newrow.PeriodId);
            var member = Member.GetActiveMember(context, newrow.MemberId);
            Position.ValidateActivePosition(context, newrow.PositionId);
            var divisionId = newrow.DivisionId > 0 ? newrow.DivisionId : null;
            if (divisionId != null)
            {
                Division.ValidateActiveDivision(context, divisionId.Value);
            }
            ValidateDates(newrow.StartDate, newrow.EndDate);
            var sectionId = newrow.SectionId > 0 ? newrow.SectionId : null;
            ValidateSection(context, divisionId, sectionId, null);
            ValidateHolder(context, 0, newrow.PeriodId, newrow.MemberId, newrow.PositionId, divisionId, sectionId);

            var row = new EF.TManagementAssignment
            {
                Id = SequenceUtility.GetNextId<EF.TManagementAssignment>(context),
                PeriodId = newrow.PeriodId,
                MemberId = newrow.MemberId,
                PositionId = newrow.PositionId,
                DivisionId = divisionId,
                SectionId = sectionId,
                SkNumber = string.IsNullOrWhiteSpace(newrow.SkNumber) ? null : newrow.SkNumber.Trim(),
                StartDate = newrow.StartDate,
                EndDate = newrow.EndDate,
                IsCurrent = (int)AssignmentPeriod.Berjalan,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            if (member.MemberType == (int)MemberType.Anggota)
            {
                member.MemberType = (int)MemberType.Pengurus;
                member.UpdDate = DateTime.Now;
                member.UpdBy = userId;
            }
            var position = context.MPositions.Where(x => x.Id == row.PositionId).Select(x => x.Name).FirstOrDefault();
            NotifyHelper.ToMember(context, row.MemberId, NotifyHelper.TYPE_ASSIGNMENT, "Anda ditetapkan sebagai " + position,
                row.SkNumber != null ? "SK " + row.SkNumber : null, NotifyHelper.LINK_MY_PROFILE, "t_management_assignment", row.Id, userId);
            return row;
        }
        // Periode, anggota dan jabatan tetap (salah pilih = hapus lalu buat ulang); penugasan yang sudah berakhir dikunci.
        private void SetRow(EF.ModelContext context, EF.TManagementAssignment row, int userId)
        {
            TransaksiHelper.ValidateStatus((AssignmentPeriod)row.IsCurrent, "mengubah penugasan", AssignmentPeriod.Berjalan);
            var divisionId = DivisionId > 0 ? DivisionId : null;
            if (divisionId != null && divisionId != row.DivisionId)
            {
                Division.ValidateActiveDivision(context, divisionId.Value);
            }
            ValidateDates(StartDate, EndDate);
            var sectionId = SectionId > 0 ? SectionId : null;
            ValidateSection(context, divisionId, sectionId, row.SectionId);
            ValidateHolder(context, row.Id, row.PeriodId, row.MemberId, row.PositionId, divisionId, sectionId);

            row.DivisionId = divisionId;
            row.SectionId = sectionId;
            row.SkNumber = string.IsNullOrWhiteSpace(SkNumber) ? null : SkNumber.Trim();
            row.StartDate = StartDate;
            row.EndDate = EndDate;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetEnd(EF.TManagementAssignment row, DateOnly endDate, int userId)
        {
            TransaksiHelper.ValidateStatus((AssignmentPeriod)row.IsCurrent, "mengakhiri penugasan", AssignmentPeriod.Berjalan);
            ValidateDates(row.StartDate, endDate);
            row.IsCurrent = (int)AssignmentPeriod.Lampau;
            row.EndDate = endDate;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
