using GWALib.General;
using GWAShared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GWALib.Lib.Transaksi.Program
{
    /// <summary>
    /// Data LPJ yang diambil langsung dari modul lain (selalu data terkini; PDF final disimpan saat Finalize).
    /// </summary>
    public partial class Lpj
    {
        public const int MAX_PHOTO_KEGIATAN = 12;
        public const int MAX_PHOTO_KEPENGURUSAN = 24;
        private static readonly CultureInfo ID = new("id-ID");

        public record ScheduleLine(string Name, string Date, string Day, string Time, string? Location);
        public record PhotoLine(byte[] Data, string? Caption);
        /// <summary>Baris realisasi anggaran. Budget/Detail terisi bila berasal dari item RAB (qty × harga satuan).</summary>
        public record BudgetLine(CashFlowType Type, string Category, string Description, string? Detail, decimal Budget, decimal Realized, bool FromBudget);
        public record SignerLine(string Role, string? Name, string? Number);
        public record ProgramLine(string Division, int DivisionOrder, ProgramTerm? Term, string Title, ProgramStatus Status);
        public record ActivityLine(string Title, DateTime StartAt, ActivityStatus Status);
        public record AssetLine(string Category, string Name, int Quantity, string Unit, string Note);
        public record OfficerLine(string Position, string? Division, string Name, string? Number, string? Faculty, byte[]? Photo);

        public class OrgInfo
        {
            public string Name { get; set; } = "Gita Widya Agni";
            public string? Description { get; set; }
            public byte[]? Logo { get; set; }
            public string? AdvisorName { get; set; }
        }

        public class KegiatanData
        {
            public Lpj Lpj { get; set; } = null!;
            public OrgInfo Org { get; set; } = new();
            public string ActivityTitle { get; set; } = "";
            public string? ProgramTitle { get; set; }
            public int Year { get; set; }
            public List<ScheduleLine> Schedules { get; set; } = new();
            public List<string> Results { get; set; } = new();
            public List<PhotoLine> Photos { get; set; } = new();
            public List<BudgetLine> Budget { get; set; } = new();
            public List<SignerLine> Signers { get; set; } = new();
        }

        public class KepengurusanData
        {
            public Lpj Lpj { get; set; } = null!;
            public OrgInfo Org { get; set; } = new();
            public string PeriodName { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public List<ProgramLine> Programs { get; set; } = new();
            public List<ActivityLine> Activities { get; set; } = new();
            public List<AssetLine> Assets { get; set; } = new();
            public List<OfficerLine> Officers { get; set; } = new();
            public List<PhotoLine> Photos { get; set; } = new();
            public List<SignerLine> Signers { get; set; } = new();
        }

        // Jenis kegiatan yang dicantumkan di tabel kegiatan LPJ kepengurusan (rapat, latihan, piket tidak).
        private static readonly int[] ReportedActivityTypes = { (int)ActivityType.Event, (int)ActivityType.Lomba, (int)ActivityType.Lainnya };

        public KegiatanData GetKegiatanData()
        {
            using var context = DBClass.GetContext();
            var activityId = ActivityId ?? throw new ArgumentException(string.Format(GWAShared.Results.ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan"));
            var activity = context.TActivities.AsNoTracking().Include(x => x.Program).SingleOrDefault(x => x.Id == activityId)
                ?? throw new ArgumentException(string.Format(GWAShared.Results.ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan"));
            var children = context.TActivities.AsNoTracking().Where(x => x.ParentActivityId == activityId).OrderBy(x => x.StartAt).ToList();
            var ids = children.Select(x => x.Id).Prepend(activityId).ToList();

            var data = new KegiatanData
            {
                Lpj = this,
                Org = LoadOrg(context),
                ActivityTitle = activity.Title,
                ProgramTitle = activity.Program?.Title,
                Year = activity.StartAt.Year
            };

            // Waktu & tempat: kegiatan induk, sub-kegiatan, lalu jadwal rutin (latihan) yang aktif.
            foreach (var x in children.Prepend(activity))
            {
                data.Schedules.Add(new ScheduleLine(x.Title, DateRange(x.StartAt, x.EndAt), DayRange(x.StartAt, x.EndAt), TimeRange(x.StartAt.TimeOfDay, x.EndAt?.TimeOfDay), x.Location));
            }
            var schedules = context.TActivitySchedules.AsNoTracking().Include(x => x.Activity)
                .Where(x => ids.Contains(x.ActivityId) && x.IsActive == 1).OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToList();
            foreach (var g in schedules.GroupBy(x => new { x.ActivityId, x.StartTime, x.EndTime, x.Location, x.ValidFrom, x.ValidUntil }))
            {
                var first = g.First();
                var until = first.ValidUntil ?? DateOnly.FromDateTime(activity.EndAt ?? activity.StartAt);
                data.Schedules.Add(new ScheduleLine("Jadwal rutin " + first.Activity.Title,
                    DateRange(first.ValidFrom.ToDateTime(TimeOnly.MinValue), until.ToDateTime(TimeOnly.MinValue)),
                    string.Join(", ", g.Select(x => DayName(x.DayOfWeek)).Distinct()),
                    TimeRange(first.StartTime.ToTimeSpan(), first.EndTime.ToTimeSpan()), first.Location));
            }

            // Hasil: prestasi dari lomba yang terkait kegiatan / sub-kegiatan.
            var competitionIds = children.Prepend(activity).Where(x => x.CompetitionId != null).Select(x => x.CompetitionId!.Value).Distinct().ToList();
            if (competitionIds.Count > 0)
            {
                data.Results = context.TAchievements.AsNoTracking().Include(x => x.Member)
                    .Where(x => x.CompetitionId != null && competitionIds.Contains(x.CompetitionId.Value))
                    .OrderBy(x => x.AchievedDate).ThenBy(x => x.Id).ToList()
                    .Select(x => string.Join(" – ", new[]
                    {
                        string.Join(" ", new[] { x.RankTitle, x.Title }.Where(s => !string.IsNullOrWhiteSpace(s))),
                        x.Member?.Fullname ?? x.TeamName
                    }.Where(s => !string.IsNullOrWhiteSpace(s))))
                    .ToList();
            }

            data.Photos = LoadPhotos(context, context.TGalleries.Where(x => x.ActivityId != null && ids.Contains(x.ActivityId.Value)).Select(x => x.Id), MAX_PHOTO_KEGIATAN);
            data.Budget = LoadBudget(context, ids);

            // Tanda tangan: ketua pelaksana + bendahara kegiatan, mengetahui ketua umum.
            var committee = context.TCommitteeMembers.AsNoTracking().Include(x => x.Member)
                .Where(x => x.ActivityId == activityId && x.Status == (int)CommitteeMemberStatus.Aktif
                    && (x.CommitteeRole == (int)CommitteeRole.KetuaPelaksana || x.CommitteeRole == (int)CommitteeRole.Bendahara))
                .ToList();
            SignerLine Committee(CommitteeRole role)
            {
                var m = committee.FirstOrDefault(x => x.CommitteeRole == (int)role)?.Member;
                return new SignerLine(Text(role), m?.Fullname, m?.MemberNumber);
            }
            data.Signers.Add(Committee(CommitteeRole.KetuaPelaksana));
            data.Signers.Add(Committee(CommitteeRole.Bendahara));
            var periodId = activity.Program?.PeriodId ?? context.MManagementPeriods.Where(x => x.Status == (int)ManagementPeriodStatus.Active).Select(x => (int?)x.Id).FirstOrDefault();
            data.Signers.Add(PositionSigner(context, periodId, "Ketua Umum"));
            return data;
        }

        public KepengurusanData GetKepengurusanData()
        {
            using var context = DBClass.GetContext();
            var periodId = PeriodId ?? throw new ArgumentException(string.Format(GWAShared.Results.ExceptionMessage.DATA_X_NOT_FOUND, "Periode"));
            var period = context.MManagementPeriods.AsNoTracking().SingleOrDefault(x => x.Id == periodId)
                ?? throw new ArgumentException(string.Format(GWAShared.Results.ExceptionMessage.DATA_X_NOT_FOUND, "Periode"));
            var start = period.StartDate.ToDateTime(TimeOnly.MinValue);
            var end = period.EndDate.ToDateTime(TimeOnly.MaxValue);

            var data = new KepengurusanData
            {
                Lpj = this,
                Org = LoadOrg(context),
                PeriodName = period.Name,
                StartDate = period.StartDate,
                EndDate = period.EndDate
            };

            data.Programs = context.TPrograms.AsNoTracking().Include(x => x.Division).Where(x => x.PeriodId == periodId).ToList()
                .Select(x => new ProgramLine(x.Division?.Name ?? "Umum", x.Division?.SortOrder ?? int.MaxValue, (ProgramTerm?)x.Term, x.Title, (ProgramStatus)x.Status))
                .OrderBy(x => x.DivisionOrder).ThenBy(x => x.Division).ThenBy(x => x.Term == null).ThenBy(x => x.Term).ThenBy(x => x.Title)
                .ToList();

            var activityQuery = context.TActivities.AsNoTracking().Where(x => x.ParentActivityId == null && ReportedActivityTypes.Contains(x.ActivityType)
                && ((x.ProgramId != null && x.Program!.PeriodId == periodId) || (x.ProgramId == null && x.StartAt >= start && x.StartAt <= end)));
            data.Activities = activityQuery.OrderBy(x => x.StartAt).ToList()
                .Select(x => new ActivityLine(x.Title, x.StartAt, (ActivityStatus)x.Status)).ToList();

            // Inventaris: aset yang belum dihapuskan, kondisi per unit untuk aset per unit.
            var assets = context.MAssets.AsNoTracking().Include(x => x.Category).Include(x => x.MAssetUnits)
                .Where(x => x.Status != (int)AssetStatus.Disposed).ToList();
            data.Assets = assets.Select(x =>
                {
                    string note;
                    if (x.TrackUnit == 1)
                    {
                        var units = x.MAssetUnits.Where(u => u.Status != (int)AssetUnitStatus.Dihapus).ToList();
                        note = string.Join(", ", units
                            .GroupBy(u => u.Status == (int)AssetUnitStatus.Hilang ? (int)ItemCondition.Hilang : u.UnitCondition)
                            .OrderBy(g => g.Key)
                            .Select(g => $"{Text((ItemCondition)g.Key)} {g.Count()}"));
                    }
                    else
                    {
                        note = Text((ItemCondition)x.AssetCondition);
                    }
                    var name = string.IsNullOrWhiteSpace(x.Brand) ? x.Name : $"{x.Name} ({x.Brand})";
                    return new AssetLine(x.Category.Name, name, x.Quantity, x.Unit, note);
                })
                .OrderBy(x => x.Category).ThenBy(x => x.Name).ToList();

            // Susunan pengurus periode: inti dulu, lalu per divisi, lalu section leader.
            var assignments = context.TManagementAssignments.AsNoTracking().Include(x => x.Member).Include(x => x.Position).Include(x => x.Division).Include(x => x.Section)
                .Where(x => x.PeriodId == periodId).ToList()
                .OrderByDescending(x => x.Position.IsCore).ThenBy(x => x.SectionId != null).ThenBy(x => x.Division?.SortOrder ?? x.Section?.SortOrder ?? 0).ThenBy(x => x.Position.SortOrder).ThenBy(x => x.Member.Fullname)
                .ToList();
            var memberIds = assignments.Select(x => x.MemberId).Distinct().ToList();
            var photos = context.MMemberPhotos.AsNoTracking().Where(x => memberIds.Contains(x.MemberId))
                .GroupBy(x => x.MemberId).Select(g => g.OrderByDescending(x => x.Id).First()).ToList()
                .ToDictionary(x => x.MemberId, x => x.Photo);
            data.Officers = assignments.Select(x => new OfficerLine(
                    x.Position.Name,
                    x.Position.IsCore == 1 ? null : x.Division?.Name ?? x.Section?.Name,
                    x.Member.Fullname,
                    x.Member.MemberNumber,
                    string.Join(" / ", new[] { x.Member.Fakultas, x.Member.ProgramStudi }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    photos.TryGetValue(x.MemberId, out var p) ? p : null))
                .ToList();

            var activityIds = context.TActivities.Where(x => (x.ProgramId != null && x.Program!.PeriodId == periodId)
                || (x.ProgramId == null && x.StartAt >= start && x.StartAt <= end)).Select(x => x.Id);
            var galleryIds = context.TGalleries.Where(x => (x.ActivityId != null && activityIds.Contains(x.ActivityId.Value))
                || (x.ActivityId == null && x.EventDate != null && x.EventDate >= period.StartDate && x.EventDate <= period.EndDate)).Select(x => x.Id);
            data.Photos = LoadPhotos(context, galleryIds, MAX_PHOTO_KEPENGURUSAN);

            data.Signers.Add(PositionSigner(context, periodId, "Ketua Umum"));
            data.Signers.Add(PositionSigner(context, periodId, "Sekretaris"));
            data.Signers.Add(new SignerLine("Pembina", data.Org.AdvisorName, null));
            return data;
        }

        #region DATA HELPER
        private static string Text(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field == null ? null : (System.ComponentModel.DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }
        private static OrgInfo LoadOrg(EF.ModelContext context)
        {
            var org = context.MOrganizations.AsNoTracking().Select(x => new OrgInfo { Name = x.Name, Description = x.Description, Logo = x.Logo, AdvisorName = x.AdvisorName })
                .FirstOrDefault();
            return org ?? new OrgInfo();
        }
        private static List<PhotoLine> LoadPhotos(EF.ModelContext context, IQueryable<int> galleryIds, int max)
        {
            return context.TGalleryPhotos.AsNoTracking().Include(x => x.Gallery)
                .Where(x => galleryIds.Contains(x.GalleryId))
                .OrderBy(x => x.Gallery.EventDate).ThenBy(x => x.GalleryId).ThenByDescending(x => x.IsCover).ThenBy(x => x.Seq)
                .Take(max)
                .Select(x => new PhotoLine(x.Photo, x.Caption ?? x.Gallery.Title))
                .ToList();
        }
        // Pemegang jabatan (nama jabatan diawali text) pada periode; penugasan berjalan didahulukan.
        private static SignerLine PositionSigner(EF.ModelContext context, int? periodId, string position)
        {
            var member = periodId == null ? null : context.TManagementAssignments.AsNoTracking().Include(x => x.Member)
                .Where(x => x.PeriodId == periodId && x.Position.Name.StartsWith(position))
                .OrderByDescending(x => x.IsCurrent).ThenBy(x => x.Position.SortOrder).ThenByDescending(x => x.StartDate)
                .Select(x => x.Member).FirstOrDefault();
            return new SignerLine(position, member?.Fullname, member?.MemberNumber);
        }
        /// <summary>
        /// Realisasi anggaran kegiatan: transaksi Posted milik kegiatan / sub-kegiatan, sponsorship kegiatan, dan pengajuan dana kegiatan.
        /// Pengeluaran dengan mata anggaran tampil sebagai item RAB (qty × harga) berdampingan dengan realisasinya.
        /// </summary>
        private static List<BudgetLine> LoadBudget(EF.ModelContext context, List<int> activityIds)
        {
            var sponsorshipIds = context.TSponsorships.Where(x => x.ActivityId != null && activityIds.Contains(x.ActivityId.Value)).Select(x => x.Id).ToList();
            var fundings = context.TFundingRequests.AsNoTracking().Where(x => x.ActivityId != null && activityIds.Contains(x.ActivityId.Value)).ToList();
            var fundingIds = fundings.Select(x => x.Id).ToList();
            var transactions = context.TFinancialTransactions.AsNoTracking().Include(x => x.Category)
                .Where(x => x.Status == (int)TransactionStatus.Posted
                    && ((x.ActivityId != null && activityIds.Contains(x.ActivityId.Value))
                        || (x.SponsorshipId != null && sponsorshipIds.Contains(x.SponsorshipId.Value))
                        || (x.FundingRequestId != null && fundingIds.Contains(x.FundingRequestId.Value))))
                .OrderBy(x => x.TransactionDate).ThenBy(x => x.Id)
                .ToList();

            var lines = new List<BudgetLine>();
            foreach (var x in transactions.Where(x => x.TransactionType == (int)CashFlowType.Income))
            {
                lines.Add(new BudgetLine(CashFlowType.Income, x.Category.Name, x.Description ?? x.Category.Name, null, 0, x.Amount, false));
            }

            var expenses = transactions.Where(x => x.TransactionType == (int)CashFlowType.Expense).ToList();
            var budgetItemIds = expenses.Where(x => x.BudgetItemId != null).Select(x => x.BudgetItemId!.Value)
                .Concat(fundings.Where(x => x.BudgetItemId != null).Select(x => x.BudgetItemId!.Value))
                .Distinct().ToList();
            var budgetItems = context.TBudgetItems.AsNoTracking().Include(x => x.Category).Where(x => budgetItemIds.Contains(x.Id)).ToList();
            foreach (var b in budgetItems.OrderBy(x => x.Category.Name).ThenBy(x => x.Id))
            {
                var detail = $"{b.Quantity.ToString("0.##", ID)} {b.Unit} × Rp {b.UnitPrice.ToString("N0", ID)}".Replace("  ", " ");
                var realized = expenses.Where(x => x.BudgetItemId == b.Id).Sum(x => x.Amount);
                lines.Add(new BudgetLine(CashFlowType.Expense, b.Category.Name, b.Name, detail, b.Amount, realized, true));
            }
            foreach (var x in expenses.Where(x => x.BudgetItemId == null || !budgetItemIds.Contains(x.BudgetItemId.Value)))
            {
                lines.Add(new BudgetLine(CashFlowType.Expense, x.Category.Name, x.Description ?? x.Category.Name, null, 0, x.Amount, false));
            }
            return lines;
        }
        private static string DateRange(DateTime start, DateTime? end)
        {
            if (end == null || end.Value.Date == start.Date)
            {
                return start.ToString("d MMMM yyyy", ID);
            }
            if (end.Value.Year == start.Year && end.Value.Month == start.Month)
            {
                return $"{start:%d} – {end.Value.ToString("d MMMM yyyy", ID)}";
            }
            return $"{start.ToString(end.Value.Year == start.Year ? "d MMMM" : "d MMMM yyyy", ID)} – {end.Value.ToString("d MMMM yyyy", ID)}";
        }
        private static string DayRange(DateTime start, DateTime? end)
        {
            var first = start.ToString("dddd", ID);
            return end == null || end.Value.Date == start.Date ? first : $"{first} – {end.Value.ToString("dddd", ID)}";
        }
        private static string TimeRange(TimeSpan start, TimeSpan? end)
        {
            var first = start.ToString(@"hh\.mm");
            return (end == null || end == start ? first : $"{first} – {end.Value.ToString(@"hh\.mm")}") + " WIB";
        }
        private static string DayName(int scheduleDay)
        {
            return Enum.IsDefined(typeof(ScheduleDay), scheduleDay) ? Text((ScheduleDay)scheduleDay) : scheduleDay.ToString();
        }
        #endregion
    }
}
