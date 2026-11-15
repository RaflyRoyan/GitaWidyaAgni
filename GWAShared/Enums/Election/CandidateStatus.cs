using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_election_candidate.status
    /// </summary>
    public enum CandidateStatus
    {
        [Description("Terdaftar")] Registered = 1,
        [Description("Terverifikasi")] Verified = 2,
        [Description("Ditolak")] Rejected = 3,
        [Description("Mengundurkan Diri")] Withdrawn = 4
    }
}
