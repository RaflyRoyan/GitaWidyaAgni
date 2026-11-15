using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_product.is_preorder
    /// </summary>
    public enum SaleMode
    {
        [Description("Ready Stock")] ReadyStock = 0,
        [Description("Pre-Order")] PreOrder = 1
    }
}
