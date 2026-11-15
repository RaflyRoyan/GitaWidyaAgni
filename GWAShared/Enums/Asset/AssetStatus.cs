using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_asset.status
    /// </summary>
    public enum AssetStatus
    {
        [Description("Tersedia")] Available = 1,
        [Description("Dipinjam")] Borrowed = 2,
        [Description("Perawatan")] Maintenance = 3,
        [Description("Dihapuskan")] Disposed = 4
    }
}
