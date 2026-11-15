using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_letter.status
    /// </summary>
    public enum LetterStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Menunggu Persetujuan")] WaitingApproval = 2,
        [Description("Disetujui")] Approved = 3,
        [Description("Ditolak")] Rejected = 4,
        [Description("Terkirim")] Sent = 5,
        [Description("Diterima")] Received = 6,
        [Description("Diarsipkan")] Archived = 7
    }
}
