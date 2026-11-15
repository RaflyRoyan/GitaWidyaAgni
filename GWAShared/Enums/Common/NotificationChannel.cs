using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_message_template.channel, m_user_notification_pref.channel, t_message_outbox.channel | t_message_outbox hanya 2 dan 3
    /// </summary>
    public enum NotificationChannel
    {
        [Description("In App")] InApp = 1,
        [Description("Email")] Email = 2,
        [Description("WhatsApp")] WhatsApp = 3
    }
}
