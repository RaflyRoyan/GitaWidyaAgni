using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_product.product_type
    /// </summary>
    public enum ProductType
    {
        [Description("Merchandise")] Merchandise = 1,
        [Description("Makanan & Minuman")] MakananMinuman = 2,
        [Description("Jasa")] Jasa = 3,
        [Description("Lainnya")] Lainnya = 4
    }
}
