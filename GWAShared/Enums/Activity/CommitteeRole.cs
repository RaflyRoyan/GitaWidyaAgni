using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_committee_member.committee_role
    /// </summary>
    public enum CommitteeRole
    {
        [Description("Ketua Pelaksana")] KetuaPelaksana = 1,
        [Description("Wakil Ketua")] WakilKetua = 2,
        [Description("Sekretaris")] Sekretaris = 3,
        [Description("Bendahara")] Bendahara = 4,
        [Description("Koordinator Sie")] KoordinatorSie = 5,
        [Description("Staf")] Staf = 6,
        [Description("Official")] Official = 7,
        [Description("Pelatih")] Pelatih = 8
    }
}
