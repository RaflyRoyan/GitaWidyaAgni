using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_period_handover.status
    /// </summary>
    public enum HandoverStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Diajukan")] Diajukan = 2,
        [Description("Disetujui")] Disetujui = 3,
        [Description("Ditolak")] Ditolak = 4
    }
}
