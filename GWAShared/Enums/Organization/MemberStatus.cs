using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_member.status
    /// </summary>
    public enum MemberStatus
    {
        [Description("Aktif")] Active = 1,
        [Description("Alumni")] Alumni = 2,
        [Description("Tidak Aktif")] Inactive = 3
    }
}
