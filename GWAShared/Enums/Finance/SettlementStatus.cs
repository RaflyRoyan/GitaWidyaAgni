using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_funding_request.settlement_status
    /// </summary>
    public enum SettlementStatus
    {
        [Description("Belum")] Belum = 1,
        [Description("Diajukan")] Diajukan = 2,
        [Description("Selesai")] Selesai = 3
    }
}
