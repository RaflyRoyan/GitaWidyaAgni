using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_external_party.party_type
    /// </summary>
    public enum ExternalPartyType
    {
        [Description("Perusahaan / Brand")] Perusahaan = 1,
        [Description("Instansi Pemerintah")] InstansiPemerintah = 2,
        [Description("Media Partner")] MediaPartner = 3,
        [Description("Komunitas / Organisasi")] Komunitas = 4,
        [Description("Vendor / Toko")] Vendor = 5,
        [Description("Lembaga Pendidikan")] LembagaPendidikan = 6,
        [Description("Lainnya")] Lainnya = 7
    }
}
