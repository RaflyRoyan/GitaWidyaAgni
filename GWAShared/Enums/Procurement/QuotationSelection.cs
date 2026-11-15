using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_procurement_quotation.is_selected
    /// </summary>
    public enum QuotationSelection
    {
        [Description("Tidak Dipilih")] TidakDipilih = 0,
        [Description("Dipilih")] Dipilih = 1
    }
}
