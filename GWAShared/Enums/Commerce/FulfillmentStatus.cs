using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_sales_order.fulfillment_status
    /// </summary>
    public enum FulfillmentStatus
    {
        [Description("Dipesan")] Dipesan = 1,
        [Description("Siap Diambil")] SiapDiambil = 2,
        [Description("Selesai")] Selesai = 3,
        [Description("Batal")] Batal = 4
    }
}
