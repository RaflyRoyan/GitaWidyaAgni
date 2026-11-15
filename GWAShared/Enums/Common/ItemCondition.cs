using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_asset.asset_condition, m_asset_unit.unit_condition, t_asset_inspection.item_condition, t_borrow_item.condition_on_borrow, t_borrow_item.condition_on_return, t_borrow_item_unit.condition_out, t_borrow_item_unit.condition_in, t_period_handover_item.item_condition, t_procurement_receipt_item.item_condition | m_asset_unit dan t_borrow_item_unit hanya memakai 1-3
    /// </summary>
    public enum ItemCondition
    {
        [Description("Baik")] Baik = 1,
        [Description("Rusak Ringan")] RusakRingan = 2,
        [Description("Rusak Berat")] RusakBerat = 3,
        [Description("Hilang")] Hilang = 4
    }
}
