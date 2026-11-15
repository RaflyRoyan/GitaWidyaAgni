using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_asset_unit.status
    /// </summary>
    public enum AssetUnitStatus
    {
        [Description("Tersedia")] Tersedia = 1,
        [Description("Dipinjam")] Dipinjam = 2,
        [Description("Perawatan")] Perawatan = 3,
        [Description("Hilang")] Hilang = 4,
        [Description("Dihapus")] Dihapus = 5
    }
}
