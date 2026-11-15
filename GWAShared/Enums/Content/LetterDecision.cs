using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_letter_approval.decision
    /// </summary>
    public enum LetterDecision
    {
        [Description("Menunggu")] Pending = 1,
        [Description("Disetujui")] Approved = 2,
        [Description("Ditolak")] Rejected = 3
    }
}
