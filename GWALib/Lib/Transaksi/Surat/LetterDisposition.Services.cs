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

namespace GWALib.Lib.Transaksi.Surat
{
    public partial class LetterDisposition
    {
        #region SYNC
        public static List<LetterDisposition> GetDataList(int? letterId = null, int? toMemberId = null, DispositionStatus? status = null)
        {
            return FilterQuery(letterId, toMemberId, status).ToList().MappingData<LetterDisposition, EF.TLetterDisposition>();
        }
        public static Response<LetterDisposition> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new LetterDisposition(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<LetterDisposition>(msg));
        }
        public Response<LetterDisposition> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<LetterDisposition> MarkDone(string user)
        {
            return Run((context, row, userId) => SetStatus(row, DispositionStatus.Done, userId), user);
        }
        public Response<LetterDisposition> Reopen(string user)
        {
            return Run((context, row, userId) => SetStatus(row, DispositionStatus.Open, userId), user);
        }
        public Response<LetterDisposition> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((DispositionStatus)row.Status, "menghapus disposisi", DispositionStatus.Open);
                var data = new LetterDisposition(Reload(row.Id));
                context.TLetterDispositions.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<LetterDisposition>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<LetterDisposition>> GetDataListAsync(int? letterId = null, int? toMemberId = null, DispositionStatus? status = null)
        {
            return await (await FilterQuery(letterId, toMemberId, status).ToListAsync()).MappingDataAsync<LetterDisposition, EF.TLetterDisposition>();
        }
        public static async Task<Response<LetterDisposition>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new LetterDisposition(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<LetterDisposition>(msg));
        }
        public Task<Response<LetterDisposition>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<LetterDisposition>> MarkDoneAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, DispositionStatus.Done, userId), user);
        }
        public Task<Response<LetterDisposition>> ReopenAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, DispositionStatus.Open, userId), user);
        }
        public async Task<Response<LetterDisposition>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((DispositionStatus)row.Status, "menghapus disposisi", DispositionStatus.Open);
                var data = new LetterDisposition(Reload(row.Id));
                context.TLetterDispositions.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<LetterDisposition>(msg));
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TLetterDisposition row, int userId);

        private Response<LetterDisposition> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new LetterDisposition(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<LetterDisposition>(msg));
        }
        private async Task<Response<LetterDisposition>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new LetterDisposition(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<LetterDisposition>(msg));
        }
        private static EF.TLetterDisposition Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TLetterDisposition> FilterQuery(int? letterId, int? toMemberId, DispositionStatus? status)
        {
            return _Querys.Where(x => (letterId == null || x.LetterId == letterId)
                    && (toMemberId == null || x.ToMemberId == toMemberId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.Status).ThenBy(x => x.DueDate).ThenByDescending(x => x.Id);
        }
        private static EF.TLetterDisposition GetRow(EF.ModelContext context, int id)
        {
            return context.TLetterDispositions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Disposisi Surat"));
        }
        private static async Task<EF.TLetterDisposition> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TLetterDispositions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Disposisi Surat"));
        }
        private static string ValidateData(EF.ModelContext context, int toMemberId, string? instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Instruksi"));
            }
            Member.GetActiveMember(context, toMemberId);
            return TransaksiHelper.Truncate(instruction, 500)!;
        }
        private static EF.TLetterDisposition CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Letter.GetDispositionLetter(context, newrow.LetterId);
            var row = new EF.TLetterDisposition
            {
                Id = SequenceUtility.GetNextId<EF.TLetterDisposition>(context),
                LetterId = newrow.LetterId,
                FromUserId = userId,
                ToMemberId = newrow.ToMemberId,
                Instruction = ValidateData(context, newrow.ToMemberId, newrow.Instruction),
                DueDate = newrow.DueDate,
                Status = (int)DispositionStatus.Open,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TLetterDispositions.Add(row);
            var subject = context.TLetters.Where(x => x.Id == row.LetterId).Select(x => x.Subject).FirstOrDefault();
            NotifyHelper.ToMember(context, row.ToMemberId, NotifyHelper.TYPE_DISPOSITION, "Disposisi surat: " + subject,
                row.Instruction, NotifyHelper.LINK_MY_DISPOSITION, "t_letter_disposition", row.Id, userId);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TLetterDisposition row, int userId)
        {
            TransaksiHelper.ValidateStatus((DispositionStatus)row.Status, "mengubah disposisi", DispositionStatus.Open);
            row.Instruction = ValidateData(context, ToMemberId, Instruction);
            row.ToMemberId = ToMemberId;
            row.DueDate = DueDate;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetStatus(EF.TLetterDisposition row, DispositionStatus target, int userId)
        {
            var from = target == DispositionStatus.Done ? DispositionStatus.Open : DispositionStatus.Done;
            TransaksiHelper.ValidateStatus((DispositionStatus)row.Status, target == DispositionStatus.Done ? "menyelesaikan disposisi" : "membuka kembali disposisi", from);
            var now = DateTime.Now;
            row.Status = (int)target;
            row.DoneAt = target == DispositionStatus.Done ? now : null;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
