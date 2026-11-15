using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_sponsorship_benefit.is_done
    /// </summary>
    public enum SponsorshipBenefitStatus
    {
        [Description("Belum Diberikan")] Belum = 0,
        [Description("Sudah Diberikan")] Sudah = 1
    }
}
