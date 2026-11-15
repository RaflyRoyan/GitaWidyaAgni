using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_schedule.day_of_week | 1=Senin .. 7=Minggu, berbeda dengan System.DayOfWeek
    /// </summary>
    public enum ScheduleDay
    {
        [Description("Senin")] Senin = 1,
        [Description("Selasa")] Selasa = 2,
        [Description("Rabu")] Rabu = 3,
        [Description("Kamis")] Kamis = 4,
        [Description("Jumat")] Jumat = 5,
        [Description("Sabtu")] Sabtu = 6,
        [Description("Minggu")] Minggu = 7
    }
}
