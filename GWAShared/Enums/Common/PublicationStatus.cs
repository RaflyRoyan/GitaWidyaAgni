using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_announcement.status, t_gallery.status
    /// </summary>
    public enum PublicationStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Terbit")] Published = 2,
        [Description("Diarsipkan")] Archived = 3
    }
}
