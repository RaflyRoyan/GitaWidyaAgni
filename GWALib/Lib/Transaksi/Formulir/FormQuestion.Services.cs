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
using System.Text.Json;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Formulir
{
    public partial class FormQuestion
    {
        #region SYNC
        public static List<FormQuestion> GetDataList(int formId)
        {
            return FilterQuery(formId).ToList().MappingData<FormQuestion, EF.TFormQuestion>();
        }
        public static Response<FormQuestion> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var form = Form.GetEditableForm(context, newrow.FormId);
                var row = CreateRow(context, form.Id, newrow, SequenceUtility.GetNextId<EF.TFormQuestion>(context), NextSeq(context, form.Id, newrow.Seq), userId);
                context.TFormQuestions.Add(row);
                context.SaveChanges();
                return new FormQuestion(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FormQuestion>(msg));
        }
        public Response<FormQuestion> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new FormQuestion(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FormQuestion>(msg));
        }
        public Response<FormQuestion> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                Form.GetEditableForm(context, row.FormId);
                context.TFormQuestions.Remove(row);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FormQuestion>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<FormQuestion>> GetDataListAsync(int formId)
        {
            return await (await FilterQuery(formId).ToListAsync()).MappingDataAsync<FormQuestion, EF.TFormQuestion>();
        }
        public static async Task<Response<FormQuestion>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var form = Form.GetEditableForm(context, newrow.FormId);
                var id = await SequenceUtility.GetNextIdAsync<EF.TFormQuestion>(context);
                var row = CreateRow(context, form.Id, newrow, id, NextSeq(context, form.Id, newrow.Seq), userId);
                context.TFormQuestions.Add(row);
                await context.SaveChangesAsync();
                return new FormQuestion(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FormQuestion>(msg));
        }
        public async Task<Response<FormQuestion>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new FormQuestion(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FormQuestion>(msg));
        }
        public async Task<Response<FormQuestion>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                Form.GetEditableForm(context, row.FormId);
                context.TFormQuestions.Remove(row);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FormQuestion>(msg));
        }
        #endregion

        #region INTERNAL
        // Tanpa SaveChanges / Add; dipakai juga oleh Form.Save. Id dan Seq disiapkan pemanggil.
        internal static EF.TFormQuestion CreateRow(EF.ModelContext context, int formId, NewRow newrow, int id, int seq, int userId)
        {
            var row = new EF.TFormQuestion
            {
                Id = id,
                FormId = formId,
                Seq = seq,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetValues(row, newrow.QuestionText, newrow.QuestionType, newrow.Options, newrow.IsRequired);
            return row;
        }
        internal static List<string> ParseOptions(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TFormQuestion> FilterQuery(int formId)
        {
            return _Querys.Where(x => x.FormId == formId).OrderBy(x => x.Seq).ThenBy(x => x.Id);
        }
        private static EF.TFormQuestion GetRow(EF.ModelContext context, int id)
        {
            return context.TFormQuestions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pertanyaan Formulir"));
        }
        private static async Task<EF.TFormQuestion> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TFormQuestions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pertanyaan Formulir"));
        }
        private static int NextSeq(EF.ModelContext context, int formId, int seq)
        {
            if (seq > 0)
            {
                return seq;
            }
            return (context.TFormQuestions.Where(x => x.FormId == formId).Max(x => (int?)x.Seq) ?? 0) + 1;
        }
        private static void SetValues(EF.TFormQuestion row, string? questionText, QuestionType questionType, List<string>? options, AnswerRequirement isRequired)
        {
            if (string.IsNullOrWhiteSpace(questionText))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Pertanyaan"));
            }
            if (!Enum.IsDefined(questionType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Pertanyaan"));
            }
            if (!Enum.IsDefined(isRequired))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Wajib Diisi"));
            }
            string? json = null;
            if (questionType == QuestionType.PilihanGanda || questionType == QuestionType.KotakCentang)
            {
                var list = (options ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                if (list.Count < 2 || list.Distinct(StringComparer.OrdinalIgnoreCase).Count() != list.Count)
                {
                    throw new ArgumentException(ExceptionMessage.FORM_OPTIONS_REQUIRED);
                }
                if (list.Count > MAX_OPTIONS || list.Any(x => x.Length > OPTION_LENGTH))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Pilihan Jawaban"));
                }
                json = JsonSerializer.Serialize(list);
            }
            row.QuestionText = questionText.Trim();
            row.QuestionType = (int)questionType;
            row.Options = json;
            row.IsRequired = (int)isRequired;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private void SetRow(EF.ModelContext context, EF.TFormQuestion row, int userId)
        {
            Form.GetEditableForm(context, row.FormId);
            SetValues(row, QuestionText, QuestionType, Options, IsRequired);
            row.Seq = NextSeq(context, row.FormId, Seq);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
