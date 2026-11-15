using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_achievement.is_featured, t_activity.is_featured, t_announcement.is_featured, t_competition.is_featured, t_gallery.is_featured | disorot di landing page
    /// </summary>
    public enum FeaturedStatus
    {
        [Description("Biasa")] Biasa = 0,
        [Description("Disorot di Landing Page")] Disorot = 1
    }
}
