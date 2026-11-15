using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_form_question.is_required
    /// </summary>
    public enum AnswerRequirement
    {
        [Description("Opsional")] Opsional = 0,
        [Description("Wajib Diisi")] Wajib = 1
    }
}
