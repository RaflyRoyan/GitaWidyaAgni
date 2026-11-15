using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_member.member_type | 3 tidak dipakai
    /// </summary>
    public enum MemberType
    {
        [Description("Anggota")] Anggota = 1,
        [Description("Pengurus")] Pengurus = 2,
        [Description("Alumni")] Alumni = 4
    }
}
