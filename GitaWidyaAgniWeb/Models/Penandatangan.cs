using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models
{
    /// <summary>
    /// Penanda tangan laporan dari data kepengurusan: pemegang jabatan yang namanya diawali `prefix` (mis. "Bendahara", "Ketua Umum").
    /// Tanpa periode = periode Aktif, hanya yang sedang menjabat. Dengan periode = jabatan di periode itu (yang masih menjabat
    /// didahulukan, lalu yang mulai paling akhir), untuk laporan periode lama. Nama kosong bila tidak ada pemegangnya.
    /// </summary>
    public static class Penandatangan
    {
        public static DocumentReport.Signer Jabatan(string prefix, string? role = null, int? periodId = null)
        {
            var period = periodId ?? ManagementPeriod.GetActive()?.Id;
            var holder = period == null ? null : ManagementAssignment.GetDataList(period, isCurrent: periodId == null ? AssignmentPeriod.Berjalan : null)
                .Where(x => x.PositionName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.IsCurrent == AssignmentPeriod.Berjalan)
                .ThenBy(x => x.PositionName.Length).ThenByDescending(x => x.StartDate)
                .FirstOrDefault();
            return new DocumentReport.Signer(role ?? prefix, holder?.MemberName, null);
        }
    }
}
