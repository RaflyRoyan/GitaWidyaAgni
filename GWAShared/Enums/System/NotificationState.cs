using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_user_notification_pref.is_enabled
    /// </summary>
    public enum NotificationState
    {
        [Description("Dimatikan")] Nonaktif = 0,
        [Description("Dinyalakan")] Aktif = 1
    }
}
