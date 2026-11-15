using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_sponsorship.sponsorship_form
    /// </summary>
    public enum SponsorshipForm
    {
        [Description("Dana")] Dana = 1,
        [Description("Barang")] Barang = 2,
        [Description("Jasa")] Jasa = 3,
        [Description("Media")] Media = 4
    }
}
