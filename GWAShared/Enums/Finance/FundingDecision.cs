using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_funding_approval.decision
    /// </summary>
    public enum FundingDecision
    {
        [Description("Menunggu")] Pending = 1,
        [Description("Disetujui")] Approved = 2,
        [Description("Ditolak")] Rejected = 3,
        [Description("Revisi")] Revision = 4
    }
}
