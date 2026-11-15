using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_achievement.is_public, t_activity.is_public, t_announcement.is_public, t_competition.is_public, t_gallery.is_public
    /// </summary>
    public enum PublicVisibility
    {
        [Description("Private")] Private = 0,
        [Description("Public")] Public = 1
    }
}
