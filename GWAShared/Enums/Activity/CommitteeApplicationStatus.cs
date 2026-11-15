using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_committee_application.status
    /// </summary>
    public enum CommitteeApplicationStatus
    {
        [Description("Daftar")] Daftar = 1,
        [Description("Diterima")] Diterima = 2,
        [Description("Ditolak")] Ditolak = 3,
        [Description("Mundur")] Mundur = 4
    }
}
