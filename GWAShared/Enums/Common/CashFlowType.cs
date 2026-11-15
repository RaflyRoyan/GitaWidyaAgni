using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_financial_category.category_type, t_financial_transaction.transaction_type
    /// </summary>
    public enum CashFlowType
    {
        [Description("Pemasukan")] Income = 1,
        [Description("Pengeluaran")] Expense = 2
    }
}
