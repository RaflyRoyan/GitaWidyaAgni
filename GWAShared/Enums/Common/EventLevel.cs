using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_achievement.level, t_competition.level | juga dipakai m_point_rule.achievement_level
    /// </summary>
    public enum EventLevel
    {
        [Description("Internal")] Internal = 1,
        [Description("Kampus")] Kampus = 2,
        [Description("Regional")] Regional = 3,
        [Description("Nasional")] Nasional = 4,
        [Description("Internasional")] Internasional = 5
    }
}
