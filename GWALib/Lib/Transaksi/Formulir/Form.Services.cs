using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Formulir
{
    public partial class Form
    {
        #region SYNC
        public static List<Form> GetDataList(FormType? formType = null, int? activityId = null, int? recruitmentId = null, FormStatus? status = null)
        {
            return FilterQuery(formType, activityId, recruitmentId, status).ToList().MappingData<Form, EF.TForm>();
        }
        // Formulir yang sedang menerima jawaban (Open dan dalam rentang waktu).
        public static List<Form> GetOpenList(FormType? formType = null)
        {
            return OpenQuery(formType).ToList().MappingData<Form, EF.TForm>();
        }
        public static Response<Form> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Form(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Form>(msg));
        }
        public Response<Form> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<Form> Open(string user)
        {
            return Run(SetOpen, user);
        }
        public Response<Form> Close(string user)
        {
            return Run(SetClose, user);
        }
        public Response<Form> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                RemoveRow(context, row);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Form>(msg));
        }
        public List<QuestionSummary> GetSummary()
        {
            using var context = DBClass.GetContext();
            return BuildSummary(context, Id);
        }
        #endregion

        #region ASYNC
        public static async Task<List<Form>> GetDataListAsync(FormType? formType = null, int? activityId = null, int? recruitmentId = null, FormStatus? status = null)
        {
            return await (await FilterQuery(formType, activityId, recruitmentId, status).ToListAsync()).MappingDataAsync<Form, EF.TForm>();
        }
        public static async Task<List<Form>> GetOpenListAsync(FormType? formType = null)
        {
            return await (await OpenQuery(formType).ToListAsync()).MappingDataAsync<Form, EF.TForm>();
        }
        public static async Task<Response<Form>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Form(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Form>(msg));
        }
        public Task<Response<Form>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<Form>> OpenAsync(string user)
        {
            return RunAsync(SetOpen, user);
        }
        public Task<Response<Form>> CloseAsync(string user)
        {
            return RunAsync(SetClose, user);
        }
        public async Task<Response<Form>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                RemoveRow(context, row);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Form>(msg));
        }
        public async Task<List<QuestionSummary>> GetSummaryAsync()
        {
            using var context = DBClass.GetContext();
            return await Task.FromResult(BuildSummary(context, Id));
        }
        #endregion

        #region INTERNAL
        internal static EF.TForm GetForm(EF.ModelContext context, int formId)
        {
            return context.TForms.SingleOrDefault(x => x.Id == formId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Formulir"));
        }
        // Formulir yang sedang menerima jawaban: status Open dan sekarang di dalam rentang waktu buka / tutup.
        internal static EF.TForm GetOpenForm(EF.ModelContext context, int formId)
        {
            var row = GetForm(context, formId);
            var now = DateTime.Now;
            if (row.Status != (int)FormStatus.Open || (row.OpenAt != null && now < row.OpenAt) || (row.CloseAt != null && now > row.CloseAt))
            {
                throw new ArgumentException(ExceptionMessage.FORM_NOT_OPEN);
            }
            return row;
        }
        // Pertanyaan hanya boleh diubah selama formulir belum punya jawaban.
        internal static EF.TForm GetEditableForm(EF.ModelContext context, int formId)
        {
            var row = GetForm(context, formId);
            if (context.TFormResponses.Any(x => x.FormId == formId))
            {
                throw new ArgumentException(ExceptionMessage.FORM_HAS_RESPONSE);
            }
            return row;
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TForm row, int userId);

        private Response<Form> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Form(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Form>(msg));
        }
        private async Task<Response<Form>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Form(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Form>(msg));
        }
        private static EF.TForm Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TForm> FilterQuery(FormType? formType, int? activityId, int? recruitmentId, FormStatus? status)
        {
            return _Querys.Where(x => (formType == null || x.FormType == (int)formType)
                    && (activityId == null || x.ActivityId == activityId)
                    && (recruitmentId == null || x.RecruitmentId == recruitmentId)
                    && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TForm> OpenQuery(FormType? formType)
        {
            var now = DateTime.Now;
            return _Querys.Where(x => x.Status == (int)FormStatus.Open
                    && (x.OpenAt == null || x.OpenAt <= now)
                    && (x.CloseAt == null || x.CloseAt >= now)
                    && (formType == null || x.FormType == (int)formType))
                .OrderBy(x => x.CloseAt == null).ThenBy(x => x.CloseAt).ThenBy(x => x.Title);
        }
        private static EF.TForm GetRow(EF.ModelContext context, int id)
        {
            return GetForm(context, id);
        }
        private static async Task<EF.TForm> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TForms.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Formulir"));
        }
        // Mengisi referensi sesuai jenis formulir; referensi yang tidak berlaku dikosongkan.
        private static void SetReference(EF.ModelContext context, EF.TForm row, FormType formType, int? activityId, int? recruitmentId, IdentityMode anonymous)
        {
            if (!Enum.IsDefined(formType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Formulir"));
            }
            if (!Enum.IsDefined(anonymous))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Mode Identitas"));
            }
            activityId = activityId > 0 ? activityId : null;
            recruitmentId = recruitmentId > 0 ? recruitmentId : null;
            switch (formType)
            {
                case FormType.EvaluasiKegiatan:
                    if (activityId == null)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.FORM_REF_REQUIRED, formType.GetDescription(), "kegiatan"));
                    }
                    Activity.GetActivity(context, activityId.Value);
                    recruitmentId = null;
                    break;
                case FormType.PendaftaranRekrutmen:
                    if (anonymous == IdentityMode.Anonim)
                    {
                        throw new ArgumentException(ExceptionMessage.FORM_RECRUITMENT_NOT_ANONYMOUS);
                    }
                    if (recruitmentId != null && !context.TRecruitments.Any(x => x.Id == recruitmentId))
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rekrutmen"));
                    }
                    activityId = null;
                    break;
                default:
                    activityId = null;
                    recruitmentId = null;
                    break;
            }
            row.FormType = (int)formType;
            row.ActivityId = activityId;
            row.RecruitmentId = recruitmentId;
            row.IsAnonymous = (int)anonymous;
        }
        private static void SetSchedule(EF.TForm row, DateTime? openAt, DateTime? closeAt)
        {
            if (openAt != null && closeAt != null && closeAt <= openAt)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            row.OpenAt = openAt;
            row.CloseAt = closeAt;
        }
        private static EF.TForm CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var row = new EF.TForm
            {
                Title = newrow.Title?.Trim() ?? string.Empty,
                Description = TransaksiHelper.ToNullable(newrow.Description),
                Status = (int)FormStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            if (string.IsNullOrWhiteSpace(row.Title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Formulir"));
            }
            SetReference(context, row, newrow.FormType, newrow.ActivityId, newrow.RecruitmentId, newrow.IsAnonymous);
            SetSchedule(row, newrow.OpenAt, newrow.CloseAt);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);

            row.Id = SequenceUtility.GetNextId<EF.TForm>(context);
            context.TForms.Add(row);
            var questions = newrow.Questions ?? new List<FormQuestion.NewRow>();
            if (questions.Count > 0)
            {
                var firstId = SequenceUtility.ReserveIds<EF.TFormQuestion>(context, questions.Count);
                for (var i = 0; i < questions.Count; i++)
                {
                    context.TFormQuestions.Add(FormQuestion.CreateRow(context, row.Id, questions[i], firstId + i, i + 1, userId));
                }
            }
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TForm row, int userId)
        {
            var title = Title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Formulir"));
            }
            var hasResponse = context.TFormResponses.Any(x => x.FormId == row.Id);
            var activityId = ActivityId > 0 ? ActivityId : null;
            var recruitmentId = RecruitmentId > 0 ? RecruitmentId : null;
            var changed = (int)FormType != row.FormType || (int)IsAnonymous != row.IsAnonymous
                || (FormType == FormType.EvaluasiKegiatan && activityId != row.ActivityId)
                || (FormType == FormType.PendaftaranRekrutmen && recruitmentId != row.RecruitmentId);
            if (changed)
            {
                if (hasResponse)
                {
                    throw new ArgumentException(ExceptionMessage.FORM_HAS_RESPONSE);
                }
                // Formulir rekrutmen tim pelaksana harus tetap PendaftaranRekrutmen tanpa rekrutmen anggota.
                if ((FormType != FormType.PendaftaranRekrutmen || recruitmentId != null) && context.TCommitteeRecruitments.Any(x => x.FormId == row.Id))
                {
                    throw new ArgumentException(ExceptionMessage.FORM_IN_USE);
                }
                SetReference(context, row, FormType, activityId, recruitmentId, IsAnonymous);
            }
            SetSchedule(row, OpenAt, CloseAt);
            row.Title = title;
            row.Description = TransaksiHelper.ToNullable(Description);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetOpen(EF.ModelContext context, EF.TForm row, int userId)
        {
            TransaksiHelper.ValidateStatus((FormStatus)row.Status, "membuka formulir", FormStatus.Draft, FormStatus.Closed);
            if (!context.TFormQuestions.Any(x => x.FormId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.FORM_QUESTION_REQUIRED);
            }
            if (row.CloseAt != null && row.CloseAt <= DateTime.Now)
            {
                throw new ArgumentException(ExceptionMessage.FORM_CLOSE_TIME_PASSED);
            }
            row.Status = (int)FormStatus.Open;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetClose(EF.ModelContext context, EF.TForm row, int userId)
        {
            TransaksiHelper.ValidateStatus((FormStatus)row.Status, "menutup formulir", FormStatus.Open);
            row.Status = (int)FormStatus.Closed;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Hanya Draft tanpa jawaban dan tidak dipakai rekrutmen tim pelaksana. Pertanyaan ikut dihapus.
        private static void RemoveRow(EF.ModelContext context, EF.TForm row)
        {
            TransaksiHelper.ValidateStatus((FormStatus)row.Status, "menghapus formulir", FormStatus.Draft);
            if (context.TFormResponses.Any(x => x.FormId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.FORM_HAS_RESPONSE);
            }
            if (context.TCommitteeRecruitments.Any(x => x.FormId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.FORM_IN_USE);
            }
            context.TFormQuestions.RemoveRange(context.TFormQuestions.Where(x => x.FormId == row.Id).ToList());
            context.TForms.Remove(row);
        }
        private static List<QuestionSummary> BuildSummary(EF.ModelContext context, int formId)
        {
            GetForm(context, formId);
            var questions = context.TFormQuestions.Where(x => x.FormId == formId).OrderBy(x => x.Seq).ThenBy(x => x.Id).ToList();
            // Kolom file tidak ikut di-select.
            var answers = context.TFormAnswers.Where(x => x.Question.FormId == formId)
                .Select(x => new { x.QuestionId, x.AnswerText, HasFile = x.AnswerFileName != null })
                .ToList()
                .ToLookup(x => x.QuestionId);

            var result = new List<QuestionSummary>();
            foreach (var question in questions)
            {
                var type = (QuestionType)question.QuestionType;
                var rows = answers[question.Id].Where(x => !string.IsNullOrEmpty(x.AnswerText) || x.HasFile).ToList();
                var options = new List<OptionCount>();
                decimal? average = null;
                switch (type)
                {
                    case QuestionType.PilihanGanda:
                    case QuestionType.KotakCentang:
                        var picked = rows.SelectMany(x => type == QuestionType.KotakCentang
                            ? FormAnswer.ParseChoices(x.AnswerText)
                            : new List<string> { x.AnswerText! }).ToList();
                        options = FormQuestion.ParseOptions(question.Options)
                            .Select(o => new OptionCount(o, picked.Count(p => p == o))).ToList();
                        break;
                    case QuestionType.Skala1Sampai5:
                        var values = rows.Select(x => int.TryParse(x.AnswerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0)
                            .Where(x => x >= FormAnswer.SCALE_MIN && x <= FormAnswer.SCALE_MAX).ToList();
                        options = Enumerable.Range(FormAnswer.SCALE_MIN, FormAnswer.SCALE_MAX - FormAnswer.SCALE_MIN + 1)
                            .Select(v => new OptionCount(v.ToString(CultureInfo.InvariantCulture), values.Count(x => x == v))).ToList();
                        average = values.Count == 0 ? null : Math.Round((decimal)values.Average(), 2);
                        break;
                }
                result.Add(new QuestionSummary(question.Id, question.Seq, question.QuestionText, type, rows.Count, average, options));
            }
            return result;
        }
        #endregion
    }
}
