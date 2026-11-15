using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_report.status
    /// </summary>
    public enum ActivityReportStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Diajukan")] Submitted = 2,
        [Description("Revisi")] Revision = 3,
        [Description("Disetujui")] Approved = 4
    }
}
