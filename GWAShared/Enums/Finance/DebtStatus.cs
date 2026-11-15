using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_debt.status
    /// </summary>
    public enum DebtStatus
    {
        [Description("Belum Bayar")] BelumBayar = 1,
        [Description("Sebagian")] Sebagian = 2,
        [Description("Lunas")] Lunas = 3,
        [Description("Dihapuskan")] Dihapuskan = 4
    }
}
