using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_funding_request.status
    /// </summary>
    public enum FundingRequestStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Diajukan")] Submitted = 2,
        [Description("Disetujui")] Approved = 3,
        [Description("Ditolak")] Rejected = 4,
        [Description("Dicairkan")] Disbursed = 5
    }
}
