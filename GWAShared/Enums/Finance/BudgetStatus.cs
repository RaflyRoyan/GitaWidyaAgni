using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_budget.status
    /// </summary>
    public enum BudgetStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Diajukan")] Submitted = 2,
        [Description("Disetujui")] Approved = 3,
        [Description("Ditolak")] Rejected = 4
    }
}
