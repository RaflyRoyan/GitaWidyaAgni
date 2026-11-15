using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_timeline_plan.plan_type
    /// </summary>
    public enum TimelinePlanType
    {
        [Description("Event")] Event = 1,
        [Description("Rapat")] Rapat = 2,
        [Description("Latihan")] Latihan = 3,
        [Description("Pertemuan")] Pertemuan = 4,
        [Description("Deadline")] Deadline = 5,
        [Description("Lainnya")] Lainnya = 6
    }
}
