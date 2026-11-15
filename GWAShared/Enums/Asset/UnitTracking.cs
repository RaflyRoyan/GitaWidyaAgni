using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_asset.track_unit
    /// </summary>
    public enum UnitTracking
    {
        [Description("Cukup Jumlah")] CukupJumlah = 0,
        [Description("Per Unit (Kode / QR)")] PerUnit = 1
    }
}
