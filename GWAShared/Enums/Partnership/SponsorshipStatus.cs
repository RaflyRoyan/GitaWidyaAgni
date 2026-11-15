using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_sponsorship.status
    /// </summary>
    public enum SponsorshipStatus
    {
        [Description("Aktif")] Aktif = 1,
        [Description("Selesai")] Selesai = 2,
        [Description("Batal")] Batal = 3
    }
}
