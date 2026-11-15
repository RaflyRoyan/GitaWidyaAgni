using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_financial_transaction.status
    /// </summary>
    public enum TransactionStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Diposting")] Posted = 2,
        [Description("Dibatalkan")] Void = 3
    }
}
