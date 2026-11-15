using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_product.track_stock
    /// </summary>
    public enum StockTracking
    {
        [Description("Tidak Dilacak")] TidakDilacak = 0,
        [Description("Dilacak")] Dilacak = 1
    }
}
