using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_product_stock_movement.movement_type
    /// </summary>
    public enum StockMovementType
    {
        [Description("Masuk (Produksi/Beli)")] Masuk = 1,
        [Description("Terjual")] Terjual = 2,
        [Description("Penyesuaian")] Penyesuaian = 3,
        [Description("Retur")] Retur = 4,
        [Description("Rusak / Hilang")] RusakHilang = 5
    }
}
