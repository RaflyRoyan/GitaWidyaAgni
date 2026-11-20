using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Transaksi.Information;
using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Master.Anggota
{
    /// <summary>
    /// Ulang tahun anggota: daftar ulang tahun mendatang (widget dashboard) dan ucapan otomatis (job harian BIRTHDAY_GREETINGS):
    /// notifikasi in-app ke yang berulang tahun, plus WhatsApp bila template TEMPLATE_BIRTHDAY ada dan anggota punya nomor.
    /// Anggota Aktif dan Alumni; lahir 29 Februari dirayakan 28 Februari pada tahun bukan kabisat.
    /// </summary>
    public partial class Member
    {
        public const string TEMPLATE_BIRTHDAY = "BIRTHDAY";

        // NextBirthday = tanggal ulang tahun berikutnya (hari ini termasuk); Age = umur pada tanggal itu.
        public record BirthdayRow(int MemberId, string Fullname, string? MemberNumber, MemberStatus Status, string? Section, DateOnly BirthDate,
            DateOnly NextBirthday, int Age, int DaysLeft);

        #region SYNC
        /// <summary>Ulang tahun dalam [from, from + days) untuk anggota Aktif (opsional juga Alumni), urut tanggal lalu nama.</summary>
        public static List<BirthdayRow> GetBirthdays(DateOnly from, int days, bool includeAlumni = false)
        {
            using var context = DBClass.GetContext();
            return Birthdays(context, from, Math.Clamp(days, 1, 366), includeAlumni);
        }

        /// <summary>
        /// Untuk job harian: kirim ucapan ke anggota yang berulang tahun hari ini. Anggota yang sudah menerima ucapan hari ini
        /// (notifikasi ULANG_TAHUN dengan ref anggotanya) dilewati, jadi aman dijalankan ulang. Data = jumlah anggota yang diberi ucapan.
        /// </summary>
        public static Response<int> SendBirthdayGreetings(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var today = DateOnly.FromDateTime(DateTime.Today);
                var rows = Birthdays(context, today, 1, true);
                if (rows.Count == 0)
                {
                    return 0;
                }
                var start = DateTime.Today;
                var ids = rows.Select(x => x.MemberId).ToList();
                var greeted = context.TNotifications
                    .Where(x => x.NotifType == NotifyHelper.TYPE_BIRTHDAY && x.RefType == "m_member" && x.InsDate >= start && x.RefId != null && ids.Contains(x.RefId.Value))
                    .Select(x => x.RefId!.Value).Distinct().ToHashSet();
                var org = context.MOrganizations.Select(x => x.Name).FirstOrDefault() ?? "Gita Widya Agni";
                var hasTemplate = Approval.MessageTemplate.GetTemplate(TEMPLATE_BIRTHDAY, NotificationChannel.WhatsApp) != null;
                var members = context.MMembers.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.UserId, x.Phone }).ToDictionary(x => x.Id);
                var count = 0;
                foreach (var row in rows.Where(x => !greeted.Contains(x.MemberId)))
                {
                    var member = members[row.MemberId];
                    var first = row.Fullname.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? row.Fullname;
                    var sent = NotifyHelper.ToMember(context, row.MemberId, NotifyHelper.TYPE_BIRTHDAY, $"Selamat ulang tahun ke-{row.Age}, {first}! 🎉",
                        $"Keluarga besar {org} mengucapkan selamat ulang tahun. Semoga sehat, sukses, dan terus berkarya bersama kami.",
                        NotifyHelper.LINK_MY_PROFILE, "m_member", row.MemberId, null);
                    if (hasTemplate && !string.IsNullOrWhiteSpace(member.Phone))
                    {
                        try
                        {
                            Transaksi.Approval.MessageOutbox.CreateFromTemplate(context, new Transaksi.Approval.MessageOutbox.TemplateRow
                            {
                                UserId = member.UserId,
                                Recipient = member.Phone!,
                                Channel = NotificationChannel.WhatsApp,
                                TemplateCode = TEMPLATE_BIRTHDAY,
                                NotifType = NotifyHelper.TYPE_BIRTHDAY,
                                Values = new() { ["name"] = row.Fullname, ["age"] = row.Age.ToString(), ["org"] = org },
                                RefType = "m_member",
                                RefId = row.MemberId
                            }, userId);
                            sent++;
                        }
                        catch (ArgumentException)
                        {
                            // Nomor tidak valid: lewati WhatsApp, ucapan in-app tetap terkirim.
                        }
                    }
                    if (sent > 0)
                    {
                        count++;
                    }
                }
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        #endregion

        #region HELPER
        private static List<BirthdayRow> Birthdays(EF.ModelContext context, DateOnly from, int days, bool includeAlumni)
        {
            int active = (int)MemberStatus.Active, alumni = (int)MemberStatus.Alumni;
            var rows = context.MMembers.AsNoTracking()
                .Where(x => x.BirthDate != null && (x.Status == active || (includeAlumni && x.Status == alumni)))
                .Select(x => new
                {
                    x.Id,
                    x.Fullname,
                    x.MemberNumber,
                    x.Status,
                    BirthDate = x.BirthDate!.Value,
                    Section = x.TSectionMembers.Where(s => s.EndDate == null).Select(s => s.Section.Name).FirstOrDefault()
                }).ToList();
            var until = from.AddDays(days);
            return rows.Select(x =>
            {
                var next = BirthdayIn(x.BirthDate, from.Year);
                if (next < from)
                {
                    next = BirthdayIn(x.BirthDate, from.Year + 1);
                }
                return new BirthdayRow(x.Id, x.Fullname, x.MemberNumber, (MemberStatus)x.Status, x.Section, x.BirthDate, next,
                    next.Year - x.BirthDate.Year, next.DayNumber - from.DayNumber);
            }).Where(x => x.NextBirthday < until).OrderBy(x => x.NextBirthday).ThenBy(x => x.Fullname).ToList();
        }
        private static DateOnly BirthdayIn(DateOnly birth, int year)
        {
            var day = Math.Min(birth.Day, DateTime.DaysInMonth(year, birth.Month));
            return new DateOnly(year, birth.Month, day);
        }
        #endregion
    }
}
