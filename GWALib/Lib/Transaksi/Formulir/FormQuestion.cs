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
    /// Pertanyaan formulir (t_form_question), urut Seq (0 = di akhir). PilihanGanda / KotakCentang wajib punya minimal 2 pilihan berbeda
    /// (disimpan sebagai array JSON di kolom options); jenis lain mengosongkan pilihan. Tambah / ubah / hapus hanya selama formulir
    /// belum punya jawaban.
    /// </summary>
    public partial class FormQuestion : BaseLib
    {
        public class NewRow
        {
            // Diabaikan bila disimpan bersama Form.
            public int FormId { get; set; }
            public int Seq { get; set; }
            public string QuestionText { get; set; } = null!;
            public QuestionType QuestionType { get; set; } = QuestionType.TeksSingkat;
            public List<string>? Options { get; set; }
            public AnswerRequirement IsRequired { get; set; } = AnswerRequirement.Opsional;
        }

        public const int MAX_OPTIONS = 50;
        public const int OPTION_LENGTH = 200;

        public int Id { get; set; }
        public int FormId { get; private set; }
        public int Seq { get; set; }
        public string QuestionText { get; set; } = null!;
        public QuestionType QuestionType { get; set; }
        public List<string> Options { get; set; } = new();
        public AnswerRequirement IsRequired { get; set; }

        private static IQueryable<EF.TFormQuestion> _Querys
        {
            get
            {
                return DBClass.GetContext().TFormQuestions.AsQueryable();
            }
        }
        public FormQuestion()
        {
        }
        public FormQuestion(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FormQuestion>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FormQuestion(EF.TFormQuestion row)
        {
            SetData(row);
        }
        private void SetData(EF.TFormQuestion row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FormQuestion>();
            FormId = row.FormId;
            Seq = row.Seq;
            QuestionText = row.QuestionText;
            QuestionType = (QuestionType)row.QuestionType;
            Options = ParseOptions(row.Options);
            IsRequired = (AnswerRequirement)row.IsRequired;
        }
    }
}
