using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_checklist.status, t_program_task.status
    /// </summary>
    public enum TaskProgressStatus
    {
        [Description("Belum")] Belum = 1,
        [Description("Proses")] Proses = 2,
        [Description("Selesai")] Selesai = 3,
        [Description("Batal")] Batal = 4
    }
}
