using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_trip_member.trip_role
    /// </summary>
    public enum TripRole
    {
        [Description("Peserta")] Peserta = 1,
        [Description("Official")] Official = 2,
        [Description("Pelatih")] Pelatih = 3,
        [Description("Kru")] Kru = 4,
        [Description("Lainnya")] Lainnya = 5
    }
}
