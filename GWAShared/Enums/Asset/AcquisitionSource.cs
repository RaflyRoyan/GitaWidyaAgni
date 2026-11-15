using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_asset.acquisition_source
    /// </summary>
    public enum AcquisitionSource
    {
        [Description("Beli")] Beli = 1,
        [Description("Hibah")] Hibah = 2,
        [Description("Sponsor")] Sponsor = 3,
        [Description("Lainnya")] Lainnya = 4
    }
}
