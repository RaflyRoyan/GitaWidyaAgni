using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_external_party.status
    /// </summary>
    public enum ExternalPartyStatus
    {
        [Description("Aktif")] Aktif = 1,
        [Description("Tidak Aktif")] TidakAktif = 2,
        [Description("Blacklist")] Blacklist = 3
    }
}
