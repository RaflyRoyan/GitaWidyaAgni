using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_aspiration.status
    /// </summary>
    public enum AspirationStatus
    {
        [Description("Baru")] Baru = 1,
        [Description("Diproses")] Diproses = 2,
        [Description("Selesai")] Selesai = 3,
        [Description("Ditolak")] Ditolak = 4
    }
}
