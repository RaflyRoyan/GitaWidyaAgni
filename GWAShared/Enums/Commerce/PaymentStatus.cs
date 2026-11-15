using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_borrow_request.payment_status, t_sales_order.payment_status | TidakAdaTagihan hanya untuk t_borrow_request
    /// </summary>
    public enum PaymentStatus
    {
        [Description("Belum Bayar")] BelumBayar = 1,
        [Description("Sebagian")] Sebagian = 2,
        [Description("Lunas")] Lunas = 3,
        [Description("Tidak Ada Tagihan")] TidakAdaTagihan = 4
    }
}
