using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_dues_setting.frequency
    /// </summary>
    public enum DuesFrequency
    {
        [Description("Bulanan")] Monthly = 1,
        [Description("Per Semester")] Semester = 2,
        [Description("Tahunan")] Yearly = 3,
        [Description("Sekali")] Once = 4
    }
}
