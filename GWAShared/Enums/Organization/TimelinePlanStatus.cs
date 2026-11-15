using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_timeline_plan.status
    /// </summary>
    public enum TimelinePlanStatus
    {
        [Description("Rencana")] Rencana = 1,
        [Description("Terjadwal")] Terjadwal = 2,
        [Description("Selesai")] Selesai = 3,
        [Description("Batal")] Batal = 4
    }
}
