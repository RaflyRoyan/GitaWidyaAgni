using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_management_period.status
    /// </summary>
    public enum ManagementPeriodStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Aktif")] Active = 2,
        [Description("Ditutup")] Closed = 3
    }
}
