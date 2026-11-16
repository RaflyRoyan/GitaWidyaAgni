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

namespace GWALib.Lib.Transaksi.Prestasi
{
    public partial class CompetitionParticipant
    {
        private static readonly string[] Roles = { ROLE_KETUA_TIM, ROLE_ANGGOTA, ROLE_PEMBIMBING };

        #region SYNC
        public static List<CompetitionParticipant> GetDataList(int? competitionId = null, int? memberId = null, CompetitionParticipantStatus? status = null)
        {
            return FilterQuery(competitionId, memberId, status).ToList().MappingData<CompetitionParticipant, EF.TCompetitionParticipant>();
        }
        public static Response<CompetitionParticipant> Register(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = SetRegister(context, newrow, userId);
                context.SaveChanges();
                return new CompetitionParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CompetitionParticipant>(msg));
        }
        // Hanya nama tim dan peran.
        public Response<CompetitionParticipant> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<CompetitionParticipant> SetParticipating(string user)
        {
            return Run((context, row, userId) => SetStatus(row, CompetitionParticipantStatus.Participating, userId), user);
        }
        public Response<CompetitionParticipant> Withdraw(string user)
        {
            return Run((context, row, userId) => SetStatus(row, CompetitionParticipantStatus.Withdrawn, userId), user);
        }
        #endregion

        #region ASYNC
        public static async Task<List<CompetitionParticipant>> GetDataListAsync(int? competitionId = null, int? memberId = null, CompetitionParticipantStatus? status = null)
        {
            return await (await FilterQuery(competitionId, memberId, status).ToListAsync()).MappingDataAsync<CompetitionParticipant, EF.TCompetitionParticipant>();
        }
        public static async Task<Response<CompetitionParticipant>> RegisterAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = SetRegister(context, newrow, userId);
                await context.SaveChangesAsync();
                return new CompetitionParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CompetitionParticipant>(msg));
        }
        public Task<Response<CompetitionParticipant>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<CompetitionParticipant>> SetParticipatingAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, CompetitionParticipantStatus.Participating, userId), user);
        }
        public Task<Response<CompetitionParticipant>> WithdrawAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, CompetitionParticipantStatus.Withdrawn, userId), user);
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TCompetitionParticipant row, int userId);

        private Response<CompetitionParticipant> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new CompetitionParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CompetitionParticipant>(msg));
        }
        private async Task<Response<CompetitionParticipant>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new CompetitionParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CompetitionParticipant>(msg));
        }
        private static EF.TCompetitionParticipant Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TCompetitionParticipant> FilterQuery(int? competitionId, int? memberId, CompetitionParticipantStatus? status)
        {
            return _Querys.Where(x => (competitionId == null || x.CompetitionId == competitionId)
                    && (memberId == null || x.MemberId == memberId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.TeamName).ThenBy(x => x.Member.Fullname);
        }
        private static EF.TCompetitionParticipant GetRow(EF.ModelContext context, int id)
        {
            return context.TCompetitionParticipants.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Peserta Lomba"));
        }
        private static async Task<EF.TCompetitionParticipant> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCompetitionParticipants.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Peserta Lomba"));
        }
        private static string NormalizeRole(string? role)
        {
            var code = string.IsNullOrWhiteSpace(role) ? ROLE_ANGGOTA : role.Trim().ToUpperInvariant().Replace(' ', '_');
            if (!Roles.Contains(code))
            {
                throw new ArgumentException(ExceptionMessage.PARTICIPANT_ROLE_INVALID);
            }
            return code;
        }
        private static EF.TCompetitionParticipant SetRegister(EF.ModelContext context, NewRow newrow, int userId)
        {
            var competition = Competition.GetCompetition(context, newrow.CompetitionId);
            if (competition.Status != (int)CompetitionStatus.Registration
                || (competition.RegistrationDeadline != null && competition.RegistrationDeadline < DateTime.Now))
            {
                throw new ArgumentException(ExceptionMessage.COMPETITION_REGISTRATION_CLOSED);
            }
            Member.GetActiveMember(context, newrow.MemberId);
            var role = NormalizeRole(newrow.ParticipantRole);
            var now = DateTime.Now;
            var row = context.TCompetitionParticipants.SingleOrDefault(x => x.CompetitionId == competition.Id && x.MemberId == newrow.MemberId);
            if (row != null)
            {
                if (row.Status != (int)CompetitionParticipantStatus.Withdrawn)
                {
                    throw new ArgumentException(ExceptionMessage.COMPETITION_ALREADY_REGISTERED);
                }
                row.UpdDate = now;
                row.UpdBy = userId;
            }
            else
            {
                row = new EF.TCompetitionParticipant
                {
                    Id = SequenceUtility.GetNextId<EF.TCompetitionParticipant>(context),
                    CompetitionId = competition.Id,
                    MemberId = newrow.MemberId,
                    InsDate = now,
                    InsBy = userId
                };
                context.TCompetitionParticipants.Add(row);
            }
            row.TeamName = TransaksiHelper.Truncate(newrow.TeamName, 150);
            row.ParticipantRole = role;
            row.Status = (int)CompetitionParticipantStatus.Registered;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TCompetitionParticipant row, int userId)
        {
            TransaksiHelper.ValidateStatus((CompetitionParticipantStatus)row.Status, "mengubah peserta lomba",
                CompetitionParticipantStatus.Registered, CompetitionParticipantStatus.Participating);
            row.TeamName = TransaksiHelper.Truncate(TeamName, 150);
            row.ParticipantRole = NormalizeRole(ParticipantRole);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetStatus(EF.TCompetitionParticipant row, CompetitionParticipantStatus target, int userId)
        {
            if (target == CompetitionParticipantStatus.Participating)
            {
                TransaksiHelper.ValidateStatus((CompetitionParticipantStatus)row.Status, "menandai peserta mengikuti lomba", CompetitionParticipantStatus.Registered);
            }
            else
            {
                TransaksiHelper.ValidateStatus((CompetitionParticipantStatus)row.Status, "mengundurkan diri dari lomba",
                    CompetitionParticipantStatus.Registered, CompetitionParticipantStatus.Participating);
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
