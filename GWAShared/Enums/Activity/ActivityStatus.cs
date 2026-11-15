using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity.status
    /// </summary>
    public enum ActivityStatus
    {
        [Description("Direncanakan")] Planned = 1,
        [Description("Berlangsung")] Ongoing = 2,
        [Description("Selesai")] Done = 3,
        [Description("Dibatalkan")] Cancelled = 4
    }
}
