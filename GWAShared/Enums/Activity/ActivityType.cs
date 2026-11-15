using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity.activity_type | juga dipakai m_checklist_template.activity_type, m_point_rule.activity_type
    /// </summary>
    public enum ActivityType
    {
        [Description("Event")] Event = 1,
        [Description("Rapat")] Rapat = 2,
        [Description("Latihan")] Latihan = 3,
        [Description("Pertemuan")] Pertemuan = 4,
        [Description("Lainnya")] Lainnya = 5,
        [Description("Lomba")] Lomba = 6,
        [Description("Piket")] Piket = 7
    }
}
