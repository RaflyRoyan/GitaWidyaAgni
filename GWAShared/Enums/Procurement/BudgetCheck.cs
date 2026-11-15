using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_procurement_request.is_over_budget | hanya peringatan, tidak menolak
    /// </summary>
    public enum BudgetCheck
    {
        [Description("Sesuai Anggaran")] SesuaiAnggaran = 0,
        [Description("Melebihi Anggaran")] MelebihiAnggaran = 1
    }
}
