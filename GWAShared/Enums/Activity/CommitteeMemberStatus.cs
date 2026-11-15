using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_committee_member.status
    /// </summary>
    public enum CommitteeMemberStatus
    {
        [Description("Aktif")] Aktif = 1,
        [Description("Mundur")] Mundur = 2,
        [Description("Dikeluarkan")] Dikeluarkan = 3
    }
}
