using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_procurement_item.is_asset
    /// </summary>
    public enum ProcurementItemKind
    {
        [Description("Habis Pakai")] HabisPakai = 0,
        [Description("Aset Inventaris")] Aset = 1
    }
}
