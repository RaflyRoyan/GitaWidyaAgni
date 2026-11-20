using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Transaksi.Information;
using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Master.Anggota
{
    /// <summary>
    /// Portal alumni:
    /// - Direktori (GetDirectory) untuk anggota yang login: semua anggota berstatus Alumni dengan nama, NIA, angkatan, tahun lulus,
    ///   section terakhir, kota, dan bidang industri. Pekerjaan, perusahaan, LinkedIn, WhatsApp, dan email hanya tampil bila alumni
    ///   memilih Bersedia Dihubungi (is_contactable).
    /// - Alumni mengubah profilnya sendiri (GetMine / SaveMine); profil dibuat bila belum ada.
    /// - Undangan (Invite, mis. reuni): notifikasi in-app ke alumni yang punya akun, plus WhatsApp lewat template TEMPLATE_INVITE
    ///   (placeholder {name} {title} {message} {link}) hanya ke alumni yang Bersedia Dihubungi dan punya nomor.
    /// </summary>
    public partial class AlumniProfile
    {
        public const string TEMPLATE_INVITE = "ALUMNI_INVITE";

        public record DirectoryRow(int MemberId, string Fullname, string? MemberNumber, short? Angkatan, short? GraduationYear, string? LastSection,
            string? City, string? Industry, bool Contactable, string? CurrentJob, string? Company, string? LinkedinUrl, string? Phone, string? Email);

        // Profil alumni milik user login. HasProfile = false: belum pernah diisi (nilai bawaan kosong).
        public class MineRow
        {
            public short? GraduationYear { get; set; }
            public string? CurrentJob { get; set; }
            public string? Company { get; set; }
            public string? Industry { get; set; }
            public string? City { get; set; }
            public string? LinkedinUrl { get; set; }
            public ContactPermission IsContactable { get; set; } = ContactPermission.TidakBersedia;
            public bool HasProfile { get; set; }
        }

        public class InviteRow
        {
            public string Title { get; set; } = null!;
            public string Message { get; set; } = null!;
            /// <summary>Kegiatan tujuan (mis. reuni yang sudah dibuat di Daftar Kegiatan); kosong = undangan umum.</summary>
            public int? ActivityId { get; set; }
            /// <summary>Hanya alumni angkatan ini; kosong = semua alumni.</summary>
            public short? Angkatan { get; set; }
            public bool SendWhatsApp { get; set; } = true;
            /// <summary>Alamat publik aplikasi (diisi web) untuk link di pesan WhatsApp, mis. https://gwa.example.org.</summary>
            public string? BaseUrl { get; set; }
        }
        public record InviteResult(int Alumni, int InApp, int WhatsApp);

        private const int INVITE_TITLE_LENGTH = 150;
        private const int INVITE_MESSAGE_LENGTH = 500;

        #region SYNC
        /// <summary>Direktori alumni untuk anggota login. search mencocokkan nama / NIA / kota / industri / perusahaan (yang boleh tampil).</summary>
        public static List<DirectoryRow> GetDirectory(string user, string? search = null, short? angkatan = null)
        {
            ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            int alumni = (int)MemberStatus.Alumni, bersedia = (int)ContactPermission.Bersedia;
            var rows = context.MMembers.AsNoTracking()
                .Where(x => x.Status == alumni && (angkatan == null || x.Angkatan == angkatan))
                .Select(x => new
                {
                    x.Id,
                    x.Fullname,
                    x.MemberNumber,
                    x.Angkatan,
                    x.Phone,
                    x.Email,
                    Profile = x.MAlumniProfile,
                    LastSection = x.TSectionMembers.OrderByDescending(s => s.StartDate).Select(s => s.Section.Name).FirstOrDefault()
                }).ToList()
                .Select(x =>
                {
                    var p = x.Profile;
                    var open = p != null && p.IsContactable == bersedia;
                    return new DirectoryRow(x.Id, x.Fullname, x.MemberNumber, x.Angkatan, p?.GraduationYear, x.LastSection, p?.City, p?.Industry, open,
                        open ? p!.CurrentJob : null, open ? p!.Company : null, open ? p!.LinkedinUrl : null, open ? x.Phone : null, open ? x.Email : null);
                });
            var text = search?.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                bool Has(string? v) => v != null && v.Contains(text, StringComparison.OrdinalIgnoreCase);
                rows = rows.Where(x => Has(x.Fullname) || Has(x.MemberNumber) || Has(x.City) || Has(x.Industry) || Has(x.Company) || Has(x.CurrentJob));
            }
            return rows.OrderByDescending(x => x.Angkatan).ThenBy(x => x.Fullname).ToList();
        }

        /// <summary>Profil alumni user login; null bila user bukan anggota berstatus Alumni.</summary>
        public static MineRow? GetMine(string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            var member = context.MMembers.AsNoTracking().Include(x => x.MAlumniProfile)
                .SingleOrDefault(x => x.UserId == userId && x.Status == (int)MemberStatus.Alumni);
            if (member == null)
            {
                return null;
            }
            var p = member.MAlumniProfile;
            return p == null ? new MineRow() : new MineRow
            {
                GraduationYear = p.GraduationYear,
                CurrentJob = p.CurrentJob,
                Company = p.Company,
                Industry = p.Industry,
                City = p.City,
                LinkedinUrl = p.LinkedinUrl,
                IsContactable = (ContactPermission)p.IsContactable,
                HasProfile = true
            };
        }

        /// <summary>Alumni menyimpan profilnya sendiri (dibuat bila belum ada). Catatan pengurus (notes) tidak disentuh.</summary>
        public static Response<MineRow> SaveMine(MineRow input, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var member = context.MMembers.SingleOrDefault(x => x.UserId == userId)
                    ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
                if (member.Status != (int)MemberStatus.Alumni)
                {
                    throw new ArgumentException(ExceptionMessage.MEMBER_NOT_ALUMNI);
                }
                ValidateData(input.GraduationYear, input.IsContactable);
                var linkedin = ToNullable(input.LinkedinUrl);
                if (linkedin != null && !(Uri.TryCreate(linkedin, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "URL LinkedIn"));
                }
                var row = context.MAlumniProfiles.SingleOrDefault(x => x.MemberId == member.Id);
                if (row == null)
                {
                    row = new EF.MAlumniProfile
                    {
                        Id = SequenceUtility.GetNextId<EF.MAlumniProfile>(context),
                        MemberId = member.Id,
                        InsDate = DateTime.Now,
                        InsBy = userId
                    };
                    context.MAlumniProfiles.Add(row);
                }
                else
                {
                    row.UpdDate = DateTime.Now;
                    row.UpdBy = userId;
                }
                row.GraduationYear = input.GraduationYear;
                row.CurrentJob = ToNullable(input.CurrentJob);
                row.Company = ToNullable(input.Company);
                row.Industry = ToNullable(input.Industry);
                row.City = ToNullable(input.City);
                row.LinkedinUrl = linkedin;
                row.IsContactable = (int)input.IsContactable;
                GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
                context.SaveChanges();
                input.HasProfile = true;
                return input;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MineRow>(msg));
        }

        /// <summary>Kirim undangan ke alumni (opsional satu angkatan). Kegiatan, bila diisi, harus masih Direncanakan / Berjalan.</summary>
        public static Response<InviteResult> Invite(InviteRow input, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var title = ToNullable(input.Title) ?? throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul"));
                var message = ToNullable(input.Message) ?? throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Pesan"));
                if (title.Length > INVITE_TITLE_LENGTH || message.Length > INVITE_MESSAGE_LENGTH)
                {
                    throw new ArgumentException($"Judul maksimal {INVITE_TITLE_LENGTH} dan pesan maksimal {INVITE_MESSAGE_LENGTH} karakter.");
                }
                var activityId = input.ActivityId > 0 ? input.ActivityId : null;
                var link = NotifyHelper.LINK_ALUMNI_DIRECTORY;
                if (activityId != null)
                {
                    var activity = Transaksi.Program.Activity.GetOpenActivity(context, activityId.Value);
                    link = NotifyHelper.LINK_MY_ACTIVITY;
                    message += $" Kegiatan: {activity.Title}, {activity.StartAt:dd MMM yyyy HH:mm}" + (string.IsNullOrWhiteSpace(activity.Location) ? "." : $" di {activity.Location}.");
                }
                int alumni = (int)MemberStatus.Alumni, bersedia = (int)ContactPermission.Bersedia;
                var targets = context.MMembers
                    .Where(x => x.Status == alumni && (input.Angkatan == null || x.Angkatan == input.Angkatan))
                    .Select(x => new { x.Id, x.UserId, x.Fullname, x.Phone, Open = x.MAlumniProfile != null && x.MAlumniProfile.IsContactable == bersedia })
                    .ToList();
                if (targets.Count == 0)
                {
                    throw new ArgumentException("Tidak ada alumni yang cocok dengan saringan ini.");
                }
                var inApp = NotifyHelper.ToUsers(context, targets.Select(x => x.UserId), NotifyHelper.TYPE_ALUMNI, title, message, link,
                    activityId != null ? "t_activity" : "m_alumni_profile", activityId ?? 0, userId);
                var wa = 0;
                if (input.SendWhatsApp && Approval.MessageTemplate.GetTemplate(TEMPLATE_INVITE, NotificationChannel.WhatsApp) != null)
                {
                    foreach (var t in targets.Where(x => x.Open && !string.IsNullOrWhiteSpace(x.Phone)))
                    {
                        try
                        {
                            Transaksi.Approval.MessageOutbox.CreateFromTemplate(context, new Transaksi.Approval.MessageOutbox.TemplateRow
                            {
                                UserId = t.UserId,
                                Recipient = t.Phone!,
                                Channel = NotificationChannel.WhatsApp,
                                TemplateCode = TEMPLATE_INVITE,
                                NotifType = NotifyHelper.TYPE_ALUMNI,
                                Values = new() { ["name"] = t.Fullname, ["title"] = title, ["message"] = message, ["link"] = (input.BaseUrl?.TrimEnd('/') ?? "") + link },
                                RefType = activityId != null ? "t_activity" : "m_member",
                                RefId = activityId ?? t.Id
                            }, userId);
                            wa++;
                        }
                        catch (ArgumentException)
                        {
                            // Nomor tidak valid: lewati.
                        }
                    }
                }
                context.SaveChanges();
                return new InviteResult(targets.Count, inApp, wa);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<InviteResult>(msg));
        }
        #endregion
    }
}
