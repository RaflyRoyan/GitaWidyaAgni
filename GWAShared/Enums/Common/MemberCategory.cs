using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_member.member_category, t_recruitment_applicant.applicant_category
    /// </summary>
    public enum MemberCategory
    {
        [Description("Mahasiswa")] Mahasiswa = 1,
        [Description("Umum")] Umum = 2
    }
}
