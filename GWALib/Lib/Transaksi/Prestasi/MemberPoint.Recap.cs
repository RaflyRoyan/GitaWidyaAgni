using GWALib.General;
using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Prestasi
{
    /// <summary>
    /// Rekap poin keaktifan satu periode: per anggota yang punya poin, poin per sumber dan total, dengan peringkat kompetisi
    /// (nilai sama = peringkat sama) di antara semua anggota periode itu. Filter section (section saat ini) hanya menyaring baris, peringkat tetap.
    /// </summary>
    public partial class MemberPoint
    {
        public record PointRecapRow(int MemberId, string Name, string? MemberNumber, string? Section, int Rank,
            int Hadir, int Panitia, int Pemateri, int Pengurus, int Prestasi, int Lainnya)
        {
            public int Total => Hadir + Panitia + Pemateri + Pengurus + Prestasi + Lainnya;
        }
        public record PointRecap(string PeriodName, string? Section, int RankedMembers, List<PointRecapRow> Rows)
        {
            public int Total => Rows.Sum(x => x.Total);
        }

        public static PointRecap GetRecap(int periodId, int? sectionId = null)
        {
            using var context = DBClass.GetContext();
            var period = context.MManagementPeriods.Where(x => x.Id == periodId).Select(x => x.Name).SingleOrDefault()
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode"));
            string? section = null;
            if (sectionId > 0)
            {
                section = context.MSections.Where(x => x.Id == sectionId).Select(x => x.Name).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Section"));
            }

            var sums = context.TMemberPoints.AsNoTracking().Where(x => x.PeriodId == periodId)
                .GroupBy(x => new { x.MemberId, x.SourceType })
                .Select(g => new { g.Key.MemberId, g.Key.SourceType, Total = g.Sum(x => x.Points) }).ToList();
            var memberIds = sums.Select(x => x.MemberId).Distinct().ToList();
            var members = context.MMembers.AsNoTracking().Where(x => memberIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id, x.Fullname, x.MemberNumber,
                    SectionId = x.TSectionMembers.Where(s => s.EndDate == null).Select(s => (int?)s.SectionId).FirstOrDefault(),
                    Section = x.TSectionMembers.Where(s => s.EndDate == null).Select(s => s.Section.Name).FirstOrDefault()
                }).ToList();

            int Of(int memberId, PointSourceType t) => sums.Where(x => x.MemberId == memberId && x.SourceType == (int)t).Sum(x => x.Total);
            var rows = members.Select(m => new PointRecapRow(m.Id, m.Fullname, m.MemberNumber, m.Section, 0,
                    Of(m.Id, PointSourceType.HadirKegiatan), Of(m.Id, PointSourceType.Panitia), Of(m.Id, PointSourceType.Pemateri),
                    Of(m.Id, PointSourceType.Pengurus), Of(m.Id, PointSourceType.Prestasi), Of(m.Id, PointSourceType.Lainnya)))
                .ToList();
            rows = rows.Select(r => r with { Rank = rows.Count(x => x.Total > r.Total) + 1 })
                .OrderBy(x => x.Rank).ThenBy(x => x.Name).ToList();
            var ranked = rows.Count;
            if (sectionId > 0)
            {
                var inSection = members.Where(x => x.SectionId == sectionId).Select(x => x.Id).ToHashSet();
                rows = rows.Where(x => inSection.Contains(x.MemberId)).ToList();
            }
            return new PointRecap(period, section, ranked, rows);
        }
    }
}
