using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_dues_bill.status
    /// </summary>
    public enum DuesBillStatus
    {
        [Description("Belum Dibayar")] Unpaid = 1,
        [Description("Lunas")] Paid = 2,
        [Description("Dibebaskan")] Waived = 3
    }
}
