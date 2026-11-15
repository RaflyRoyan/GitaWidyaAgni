using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_election.status
    /// </summary>
    public enum ElectionStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Pendaftaran")] Registration = 2,
        [Description("Pemungutan Suara Dibuka")] Open = 3,
        [Description("Ditutup")] Closed = 4,
        [Description("Final")] Finalized = 5
    }
}
