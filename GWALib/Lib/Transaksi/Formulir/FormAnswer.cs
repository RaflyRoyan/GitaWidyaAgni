using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Formulir
{
    /// <summary>
    /// Jawaban per pertanyaan (t_form_answer), unik per (respons, pertanyaan). Dibuat bersama FormResponse dan tidak diubah terpisah.
    /// answer_text: teks (TeksSingkat maks 255, Paragraf maks 2000), pilihan persis salah satu opsi (PilihanGanda), array JSON opsi
    /// (KotakCentang, diisi lewat Choices), angka 1-5 (Skala), atau yyyy-MM-dd (Tanggal). Jenis File menyimpan file (maks 5 MB) yang
    /// tidak dimuat di daftar (GetFile). Pertanyaan opsional yang tidak dijawab tidak membuat baris.
    /// </summary>
    public partial class FormAnswer : BaseLib
    {
        public class NewRow
        {
            public int QuestionId { get; set; }
            public string? AnswerText { get; set; }
            // KotakCentang: daftar opsi yang dicentang.
            public List<string>? Choices { get; set; }
            public byte[]? FileData { get; set; }
            public string? FileName { get; set; }
        }
        public record FileContent(byte[] Data, string FileName);

        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;
        public const int SHORT_TEXT_LENGTH = 255;
        public const int SCALE_MIN = 1;
        public const int SCALE_MAX = 5;

        public int Id { get; set; }
        public int ResponseId { get; private set; }
        public int QuestionId { get; private set; }
        public int QuestionSeq { get; private set; }
        public string QuestionText { get; private set; } = null!;
        public QuestionType QuestionType { get; private set; }
        public string? AnswerText { get; private set; }
        // Terisi untuk KotakCentang.
        public List<string> Choices { get; private set; } = new();
        public string? AnswerFileName { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TFormAnswer> _Querys
        {
            get
            {
                return DBClass.GetContext().TFormAnswers.Select(x => new EF.TFormAnswer
                {
                    Id = x.Id,
                    ResponseId = x.ResponseId,
                    QuestionId = x.QuestionId,
                    Question = new EF.TFormQuestion
                    {
                        Id = x.QuestionId,
                        Seq = x.Question.Seq,
                        QuestionText = x.Question.QuestionText,
                        QuestionType = x.Question.QuestionType
                    },
                    AnswerText = x.AnswerText,
                    AnswerFileName = x.AnswerFileName
                });
            }
        }
        public FormAnswer()
        {
        }
        public FormAnswer(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FormAnswer>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FormAnswer(EF.TFormAnswer row)
        {
            SetData(row);
        }
        private void SetData(EF.TFormAnswer row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FormAnswer>();
            ResponseId = row.ResponseId;
            QuestionId = row.QuestionId;
            QuestionSeq = row.Question?.Seq ?? 0;
            QuestionText = row.Question?.QuestionText ?? string.Empty;
            QuestionType = (QuestionType)(row.Question?.QuestionType ?? 0);
            AnswerText = row.AnswerText;
            Choices = QuestionType == QuestionType.KotakCentang ? ParseChoices(row.AnswerText) : new List<string>();
            AnswerFileName = row.AnswerFileName;
        }
    }
}
