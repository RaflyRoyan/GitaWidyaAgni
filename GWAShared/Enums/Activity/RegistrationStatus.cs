using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_participant.registration_status
    /// </summary>
    public enum RegistrationStatus
    {
        [Description("Terdaftar")] Registered = 1,
        [Description("Terkonfirmasi")] Confirmed = 2,
        [Description("Dibatalkan")] Cancelled = 3
    }
}
