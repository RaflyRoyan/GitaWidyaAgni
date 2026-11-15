using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_procurement_request.payment_method
    /// </summary>
    public enum ProcurementPaymentMethod
    {
        [Description("Dibayar Langsung")] DibayarLangsung = 1,
        [Description("Pengajuan Dana")] PengajuanDana = 2
    }
}
