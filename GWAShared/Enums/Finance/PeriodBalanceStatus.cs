using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_period_balance.status
    /// </summary>
    public enum PeriodBalanceStatus
    {
        [Description("Dibuka")] Open = 1,
        [Description("Ditutup")] Closed = 2
    }
}
