using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_procurement_request.status
    /// </summary>
    public enum ProcurementStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Diajukan")] Diajukan = 2,
        [Description("Revisi")] Revisi = 3,
        [Description("Disetujui")] Disetujui = 4,
        [Description("Ditolak")] Ditolak = 5,
        [Description("Dibeli")] Dibeli = 6,
        [Description("Diterima Sebagian")] DiterimaSebagian = 7,
        [Description("Selesai")] Selesai = 8,
        [Description("Batal")] Batal = 9
    }
}
