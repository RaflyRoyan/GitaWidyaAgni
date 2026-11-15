using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_competition_participant.status
    /// </summary>
    public enum CompetitionParticipantStatus
    {
        [Description("Terdaftar")] Registered = 1,
        [Description("Mengikuti")] Participating = 2,
        [Description("Selesai")] Finished = 3,
        [Description("Mengundurkan Diri")] Withdrawn = 4
    }
}
