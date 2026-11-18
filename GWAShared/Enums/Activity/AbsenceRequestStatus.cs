using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_absence_request.status
    /// </summary>
    public enum AbsenceRequestStatus
    {
        [Description("Menunggu")] Menunggu = 1,
        [Description("Disetujui")] Disetujui = 2,
        [Description("Ditolak")] Ditolak = 3,
        [Description("Ditarik")] Ditarik = 4
    }
}
