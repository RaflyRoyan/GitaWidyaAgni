using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_trip_member.permission_status | izin orang tua/instansi
    /// </summary>
    public enum TripPermissionStatus
    {
        [Description("Belum Ada")] Belum = 1,
        [Description("Ada")] Ada = 2,
        [Description("Tidak Perlu")] TidakPerlu = 3
    }
}
