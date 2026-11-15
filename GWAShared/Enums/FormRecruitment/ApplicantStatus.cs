using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_recruitment_applicant.status
    /// </summary>
    public enum ApplicantStatus
    {
        [Description("Terkirim")] Submitted = 1,
        [Description("Seleksi Berkas")] Screening = 2,
        [Description("Wawancara")] Interview = 3,
        [Description("Diterima")] Accepted = 4,
        [Description("Ditolak")] Rejected = 5
    }
}
