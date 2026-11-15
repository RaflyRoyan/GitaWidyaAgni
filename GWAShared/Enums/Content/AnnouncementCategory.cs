using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_announcement.category
    /// </summary>
    public enum AnnouncementCategory
    {
        [Description("Berita")] Berita = 1,
        [Description("Pengumuman")] Pengumuman = 2,
        [Description("Agenda")] Agenda = 3
    }
}
