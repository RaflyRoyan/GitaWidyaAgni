using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kegiatan
{
    /// <summary>
    /// Menyusun rekap kehadiran (RekapPresensiReport) dari AttendanceSession.GetRecap. Penanda tangan = Sekretaris periode Aktif.
    /// </summary>
    public static class PresensiCetak
    {
        public static RekapPresensiReport Rekap(DateTime? dari, DateTime? sampai, int? kegiatan, int? section, string webRoot)
        {
            var to = sampai.HasValue ? DateOnly.FromDateTime(sampai.Value) : DateOnly.FromDateTime(DateTime.Today);
            var from = dari.HasValue ? DateOnly.FromDateTime(dari.Value) : new DateOnly(to.Year, to.Month, 1);
            if (to < from)
            {
                throw new ArgumentException("Tanggal awal harus sebelum tanggal akhir.");
            }
            var activityId = kegiatan > 0 ? kegiatan : null;
            var sectionId = section > 0 ? section : null;
            var sessions = AttendanceSession.GetDataList(activityId, AttendanceSessionStatus.Closed, from.ToDateTime(TimeOnly.MinValue), to.AddDays(1).ToDateTime(TimeOnly.MinValue));
            var data = new RekapPresensiReport.Data
            {
                From = from,
                To = to,
                Activity = activityId == null ? "Semua kegiatan" : Opsi.Kegiatan().FirstOrDefault(x => x.Value == activityId)?.Text ?? "-",
                Section = sectionId == null ? "Semua section" : Opsi.Section().FirstOrDefault(x => x.Value == sectionId)?.Text ?? "-",
                Sessions = sessions.Count,
                Rows = AttendanceSession.GetRecap(from, to, activityId, sectionId),
                Signer = Sekretaris()
            };
            return RekapPresensiReport.Create(data, ReportBrand.Load(webRoot));
        }

        // Sekretaris yang sedang menjabat di periode Aktif; tanpa nama bila belum ada.
        private static DocumentReport.Signer Sekretaris()
        {
            var period = ManagementPeriod.GetActive();
            var holder = period == null ? null : ManagementAssignment.GetDataList(period.Id, isCurrent: AssignmentPeriod.Berjalan)
                .Where(x => x.PositionName.StartsWith("Sekretaris", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.PositionName.Length).ThenByDescending(x => x.StartDate)
                .FirstOrDefault();
            return new DocumentReport.Signer("Sekretaris", holder?.MemberName, null);
        }
    }
}
