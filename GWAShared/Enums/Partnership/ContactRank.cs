using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_external_party_contact.is_primary | maksimal satu kontak utama aktif per pihak luar
    /// </summary>
    public enum ContactRank
    {
        [Description("Kontak Tambahan")] Tambahan = 0,
        [Description("Kontak Utama")] Utama = 1
    }
}
