using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_program.status
    /// </summary>
    public enum ProgramStatus
    {
        [Description("Direncanakan")] Planned = 1,
        [Description("Berjalan")] Ongoing = 2,
        [Description("Selesai")] Done = 3,
        [Description("Dibatalkan")] Cancelled = 4
    }
}
