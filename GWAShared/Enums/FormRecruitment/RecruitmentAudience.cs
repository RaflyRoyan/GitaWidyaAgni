using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_recruitment.accept_public
    /// </summary>
    public enum RecruitmentAudience
    {
        [Description("Hanya Mahasiswa")] HanyaMahasiswa = 0,
        [Description("Juga Menerima Pendaftar Umum")] TermasukUmum = 1
    }
}
