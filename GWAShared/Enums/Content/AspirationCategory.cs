using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_aspiration.category
    /// </summary>
    public enum AspirationCategory
    {
        [Description("Aspirasi")] Aspirasi = 1,
        [Description("Pengaduan")] Pengaduan = 2,
        [Description("Saran")] Saran = 3
    }
}
