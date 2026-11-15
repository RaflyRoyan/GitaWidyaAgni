using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_scheduled_job_log.status
    /// </summary>
    public enum ScheduledJobStatus
    {
        [Description("Berjalan")] Berjalan = 1,
        [Description("Sukses")] Sukses = 2,
        [Description("Gagal")] Gagal = 3
    }
}
