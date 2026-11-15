using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_borrow_item_unit.return_status
    /// </summary>
    public enum UnitReturnStatus
    {
        [Description("Belum Kembali")] BelumKembali = 1,
        [Description("Kembali")] Kembali = 2,
        [Description("Kembali Rusak")] KembaliRusak = 3,
        [Description("Hilang")] Hilang = 4
    }
}
