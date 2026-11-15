using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_financial_account.account_type
    /// </summary>
    public enum FinancialAccountType
    {
        [Description("Kas")] Cash = 1,
        [Description("Bank")] Bank = 2,
        [Description("E-Wallet")] EWallet = 3
    }
}
