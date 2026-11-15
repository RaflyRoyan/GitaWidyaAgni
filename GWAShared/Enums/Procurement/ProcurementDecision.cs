using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_procurement_approval.decision
    /// </summary>
    public enum ProcurementDecision
    {
        [Description("Setuju")] Setuju = 1,
        [Description("Revisi")] Revisi = 2,
        [Description("Tolak")] Tolak = 3
    }
}
