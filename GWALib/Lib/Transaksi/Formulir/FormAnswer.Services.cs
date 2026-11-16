using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Formulir
{
    public partial class FormAnswer
    {
        #region SYNC
        public static List<FormAnswer> GetDataList(int responseId)
        {
            return FilterQuery(responseId).ToList().MappingData<FormAnswer, EF.TFormAnswer>();
        }
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            return context.TFormAnswers.Where(x => x.Id == Id && x.AnswerFile != null)
                .Select(x => new FileContent(x.AnswerFile!, x.AnswerFileName ?? "file"))
                .SingleOrDefault();
        }
        #endregion

        #region ASYNC
        public static async Task<List<FormAnswer>> GetDataListAsync(int responseId)
        {
            return await (await FilterQuery(responseId).ToListAsync()).MappingDataAsync<FormAnswer, EF.TFormAnswer>();
        }
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            return await context.TFormAnswers.Where(x => x.Id == Id && x.AnswerFile != null)
                .Select(x => new FileContent(x.AnswerFile!, x.AnswerFileName ?? "file"))
                .SingleOrDefaultAsync();
        }
        #endregion

        #region INTERNAL
        internal static List<string> ParseChoices(string? json)
        {
            return FormQuestion.ParseOptions(json);
        }
        // Memvalidasi semua jawaban terhadap pertanyaan formulir dan membuat barisnya (tanpa Id / ResponseId; diisi pemanggil).
        // Pertanyaan wajib harus dijawab; jawaban kosong untuk pertanyaan opsional dilewati.
        internal static List<EF.TFormAnswer> BuildRows(EF.ModelContext context, int formId, List<NewRow>? answers, int? userId)
        {
            var questions = context.TFormQuestions.Where(x => x.FormId == formId).ToDictionary(x => x.Id);
            var input = answers ?? new List<NewRow>();
            var seen = new HashSet<int>();
            var result = new List<EF.TFormAnswer>();
            var now = DateTime.Now;
            foreach (var answer in input)
            {
                if (!questions.TryGetValue(answer.QuestionId, out var question))
                {
                    throw new ArgumentException(ExceptionMessage.FORM_QUESTION_NOT_IN_FORM);
                }
                if (!seen.Add(question.Id))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FORM_ANSWER_DUPLICATE, question.QuestionText));
                }
                var row = BuildRow(question, answer);
                if (row == null)
                {
                    continue;
                }
                row.QuestionId = question.Id;
                row.InsDate = now;
                row.InsBy = userId;
                result.Add(row);
            }
            var answered = result.Select(x => x.QuestionId).ToHashSet();
            var missing = questions.Values.Where(x => x.IsRequired == (int)AnswerRequirement.Wajib && !answered.Contains(x.Id))
                .OrderBy(x => x.Seq).ThenBy(x => x.Id).FirstOrDefault();
            if (missing != null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FORM_ANSWER_REQUIRED, missing.QuestionText));
            }
            return result;
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TFormAnswer> FilterQuery(int responseId)
        {
            return _Querys.Where(x => x.ResponseId == responseId).OrderBy(x => x.Question.Seq).ThenBy(x => x.QuestionId);
        }
        // Null = tidak dijawab.
        private static EF.TFormAnswer? BuildRow(EF.TFormQuestion question, NewRow answer)
        {
            var type = (QuestionType)question.QuestionType;
            if (type == QuestionType.File)
            {
                if (answer.FileData == null || answer.FileData.Length == 0)
                {
                    return null;
                }
                var fileName = TransaksiHelper.ValidateFile(answer.FileData, answer.FileName, "File Jawaban", MAX_FILE_BYTES);
                return new EF.TFormAnswer { AnswerFile = answer.FileData, AnswerFileName = TransaksiHelper.Truncate(fileName, 255) };
            }

            var text = TransaksiHelper.ToNullable(answer.AnswerText);
            string? value;
            switch (type)
            {
                case QuestionType.KotakCentang:
                    var picked = (answer.Choices ?? (text == null ? new List<string>() : new List<string> { text }))
                        .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                    if (picked.Count == 0)
                    {
                        return null;
                    }
                    var options = FormQuestion.ParseOptions(question.Options);
                    var matched = picked.Select(p => options.FirstOrDefault(o => string.Equals(o, p, StringComparison.OrdinalIgnoreCase))).ToList();
                    if (matched.Any(x => x == null))
                    {
                        throw Invalid(question);
                    }
                    // Disimpan sesuai urutan opsi, tanpa duplikat.
                    value = JsonSerializer.Serialize(options.Where(o => matched.Contains(o)).ToList());
                    break;
                default:
                    if (text == null)
                    {
                        return null;
                    }
                    value = NormalizeText(question, type, text);
                    break;
            }
            if (value.Length > 2000)
            {
                throw Invalid(question);
            }
            return new EF.TFormAnswer { AnswerText = value };
        }
        private static string NormalizeText(EF.TFormQuestion question, QuestionType type, string text)
        {
            switch (type)
            {
                case QuestionType.TeksSingkat:
                    return text.Length <= SHORT_TEXT_LENGTH ? text : throw Invalid(question);
                case QuestionType.PilihanGanda:
                    return FormQuestion.ParseOptions(question.Options).FirstOrDefault(o => string.Equals(o, text, StringComparison.OrdinalIgnoreCase))
                        ?? throw Invalid(question);
                case QuestionType.Skala1Sampai5:
                    if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var scale) || scale < SCALE_MIN || scale > SCALE_MAX)
                    {
                        throw Invalid(question);
                    }
                    return scale.ToString(CultureInfo.InvariantCulture);
                case QuestionType.Tanggal:
                    if (!DateOnly.TryParseExact(text, new[] { "yyyy-MM-dd", "dd-MM-yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    {
                        throw Invalid(question);
                    }
                    return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                default:
                    return text;
            }
        }
        private static ArgumentException Invalid(EF.TFormQuestion question)
        {
            return new ArgumentException(string.Format(ExceptionMessage.FORM_ANSWER_INVALID, question.QuestionText));
        }
        #endregion
    }
}
