using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_asset_movement.movement_type
    /// </summary>
    public enum AssetMovementType
    {
        [Description("Masuk")] In = 1,
        [Description("Keluar")] Out = 2,
        [Description("Pinjam")] Borrow = 3,
        [Description("Kembali")] Return = 4,
        [Description("Penyesuaian")] Adjustment = 5,
        [Description("Penghapusan")] Disposal = 6
    }
}
