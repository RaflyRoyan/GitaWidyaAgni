using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_maintenance_log.status
    /// </summary>
    public enum MaintenanceStatus
    {
        [Description("Terjadwal")] Scheduled = 1,
        [Description("Dikerjakan")] InProgress = 2,
        [Description("Selesai")] Done = 3
    }
}
