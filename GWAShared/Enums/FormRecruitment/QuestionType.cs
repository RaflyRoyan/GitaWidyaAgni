using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_form_question.question_type
    /// </summary>
    public enum QuestionType
    {
        [Description("Teks Singkat")] TeksSingkat = 1,
        [Description("Paragraf")] Paragraf = 2,
        [Description("Pilihan Ganda")] PilihanGanda = 3,
        [Description("Kotak Centang")] KotakCentang = 4,
        [Description("Skala 1-5")] Skala1Sampai5 = 5,
        [Description("Tanggal")] Tanggal = 6,
        [Description("File")] File = 7
    }
}
