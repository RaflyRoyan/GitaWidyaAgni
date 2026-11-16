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

namespace GWALib.Lib.Transaksi.Formulir
{
    public partial class FormResponse
    {
        #region SYNC
        public static List<FormResponse> GetDataList(int formId, int startdata = -1, int jmldata = 50)
        {
            return Page(FilterQuery(formId), startdata, jmldata).ToList().MappingData<FormResponse, EF.TFormResponse>();
        }
        // Respons milik user login pada formulir tidak anonim; null bila belum mengisi.
        public static FormResponse? GetMyResponse(int formId, string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            var row = _Querys.FirstOrDefault(x => x.FormId == formId && x.InsBy == userId);
            return row == null ? null : new FormResponse(row);
        }
        public static Response<FormResponse> Submit(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new FormResponse(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FormResponse>(msg));
        }
        // Jawaban formulir rekrutmen dari pendaftar, tanpa login (ins_by kosong).
        public static Response<FormResponse> SubmitApplicant(ApplicantRow newrow)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var row = CreateApplicantRow(context, newrow);
                context.SaveChanges();
                return new FormResponse(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FormResponse>(msg));
        }
        public Response<FormResponse> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                RemoveRow(context, row);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FormResponse>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<FormResponse>> GetDataListAsync(int formId, int startdata = -1, int jmldata = 50)
        {
            return await (await Page(FilterQuery(formId), startdata, jmldata).ToListAsync()).MappingDataAsync<FormResponse, EF.TFormResponse>();
        }
        public static async Task<FormResponse?> GetMyResponseAsync(int formId, string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            var row = await _Querys.FirstOrDefaultAsync(x => x.FormId == formId && x.InsBy == userId);
            return row == null ? null : new FormResponse(row);
        }
        public static async Task<Response<FormResponse>> SubmitAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new FormResponse(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FormResponse>(msg));
        }
        public static async Task<Response<FormResponse>> SubmitApplicantAsync(ApplicantRow newrow)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var row = CreateApplicantRow(context, newrow);
                await context.SaveChangesAsync();
                return new FormResponse(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FormResponse>(msg));
        }
        public async Task<Response<FormResponse>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                RemoveRow(context, row);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FormResponse>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TFormResponse Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TFormResponse> FilterQuery(int formId)
        {
            return _Querys.Where(x => x.FormId == formId).OrderByDescending(x => x.SubmittedAt).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TFormResponse> Page(IQueryable<EF.TFormResponse> query, int startdata, int jmldata)
        {
            return startdata > -1 ? query.Skip(startdata).Take(jmldata) : query;
        }
        private static EF.TFormResponse GetRow(EF.ModelContext context, int id)
        {
            return context.TFormResponses.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jawaban Formulir"));
        }
        private static async Task<EF.TFormResponse> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TFormResponses.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jawaban Formulir"));
        }
        private static EF.TFormResponse CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var form = Form.GetOpenForm(context, newrow.FormId);
            if (form.FormType == (int)FormType.PendaftaranRekrutmen && form.RecruitmentId != null)
            {
                // Formulir rekrutmen anggota diisi pendaftar lewat SubmitApplicant.
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Formulir"));
            }
            int? memberId = null;
            int? insBy = null;
            if (form.IsAnonymous != (int)IdentityMode.Anonim)
            {
                if (context.TFormResponses.Any(x => x.FormId == form.Id && x.InsBy == userId))
                {
                    throw new ArgumentException(ExceptionMessage.FORM_ALREADY_RESPONDED);
                }
                var member = context.MMembers.Where(x => x.UserId == userId).Select(x => new { x.Id, x.Status }).SingleOrDefault();
                if (form.FormType == (int)FormType.PendaftaranRekrutmen)
                {
                    if (member == null)
                    {
                        throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
                    }
                    if (member.Status != (int)MemberStatus.Active)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Anggota"));
                    }
                }
                memberId = member?.Id;
                insBy = userId;
            }
            return AddRow(context, form.Id, newrow.Answers, memberId, null, insBy);
        }
        private static EF.TFormResponse CreateApplicantRow(EF.ModelContext context, ApplicantRow newrow)
        {
            var form = Form.GetOpenForm(context, newrow.FormId);
            var applicant = context.TRecruitmentApplicants.SingleOrDefault(x => x.Id == newrow.ApplicantId);
            var email = newrow.ApplicantEmail?.Trim().ToLowerInvariant();
            if (form.FormType != (int)FormType.PendaftaranRekrutmen || form.RecruitmentId == null
                || applicant == null || applicant.RecruitmentId != form.RecruitmentId || applicant.Email != email)
            {
                throw new ArgumentException(ExceptionMessage.FORM_APPLICANT_MISMATCH);
            }
            TransaksiHelper.ValidateStatus((ApplicantStatus)applicant.Status, "mengisi formulir rekrutmen",
                ApplicantStatus.Submitted, ApplicantStatus.Screening, ApplicantStatus.Interview);
            if (context.TFormResponses.Any(x => x.FormId == form.Id && x.ApplicantId == applicant.Id))
            {
                throw new ArgumentException(ExceptionMessage.FORM_ALREADY_RESPONDED);
            }
            return AddRow(context, form.Id, newrow.Answers, applicant.MemberId, applicant.Id, null);
        }
        // Jawaban divalidasi dulu supaya id tidak terpakai bila gagal.
        private static EF.TFormResponse AddRow(EF.ModelContext context, int formId, List<FormAnswer.NewRow>? answers, int? memberId, int? applicantId, int? insBy)
        {
            var rows = FormAnswer.BuildRows(context, formId, answers, insBy);
            var now = DateTime.Now;
            var row = new EF.TFormResponse
            {
                Id = SequenceUtility.GetNextId<EF.TFormResponse>(context),
                FormId = formId,
                MemberId = memberId,
                ApplicantId = applicantId,
                SubmittedAt = now,
                InsDate = now,
                InsBy = insBy
            };
            context.TFormResponses.Add(row);
            if (rows.Count > 0)
            {
                var firstId = SequenceUtility.ReserveIds<EF.TFormAnswer>(context, rows.Count);
                for (var i = 0; i < rows.Count; i++)
                {
                    rows[i].Id = firstId + i;
                    rows[i].ResponseId = row.Id;
                    context.TFormAnswers.Add(rows[i]);
                }
            }
            return row;
        }
        // Jawaban dihapus lewat stub (tanpa memuat file) yang membawa ResponseId supaya EF menghapusnya lebih dulu.
        private static void RemoveRow(EF.ModelContext context, EF.TFormResponse row)
        {
            if (context.TCommitteeApplications.Any(x => x.FormResponseId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.FORM_RESPONSE_IN_USE);
            }
            var answerIds = context.TFormAnswers.Where(x => x.ResponseId == row.Id).Select(x => x.Id).ToList();
            foreach (var answerId in answerIds)
            {
                context.TFormAnswers.Remove(new EF.TFormAnswer { Id = answerId, ResponseId = row.Id });
            }
            context.TFormResponses.Remove(row);
        }
        #endregion
    }
}
