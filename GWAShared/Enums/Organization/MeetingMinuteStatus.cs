using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_meeting_minutes.status
    /// </summary>
    public enum MeetingMinuteStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Final")] Final = 2
    }
}
