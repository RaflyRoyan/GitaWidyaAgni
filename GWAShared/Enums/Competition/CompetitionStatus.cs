using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_competition.status
    /// </summary>
    public enum CompetitionStatus
    {
        [Description("Akan Datang")] Upcoming = 1,
        [Description("Pendaftaran")] Registration = 2,
        [Description("Berlangsung")] Ongoing = 3,
        [Description("Selesai")] Finished = 4,
        [Description("Dibatalkan")] Cancelled = 5
    }
}
