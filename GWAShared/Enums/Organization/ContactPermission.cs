using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_alumni_profile.is_contactable | alumni setuju dihubungi pengurus / adik tingkat
    /// </summary>
    public enum ContactPermission
    {
        [Description("Tidak Bersedia Dihubungi")] TidakBersedia = 0,
        [Description("Bersedia Dihubungi")] Bersedia = 1
    }
}
