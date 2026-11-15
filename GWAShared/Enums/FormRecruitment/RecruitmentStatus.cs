using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_recruitment.status
    /// </summary>
    public enum RecruitmentStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Dibuka")] Open = 2,
        [Description("Ditutup")] Closed = 3
    }
}
