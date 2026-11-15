using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_committee_recruitment.status
    /// </summary>
    public enum CommitteeRecruitmentStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Buka")] Buka = 2,
        [Description("Tutup")] Tutup = 3
    }
}
