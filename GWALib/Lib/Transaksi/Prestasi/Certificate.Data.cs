using GWALib.General;
using GWAShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Prestasi
{
    /// <summary>
    /// Data cetak sertifikat (laporan DevExpress di web): penerima, keterangan peran sesuai jenis dan referensinya,
    /// serta penanda tangan. Kegiatan (Peserta / Panitia / Pemateri) ditandatangani Ketua Pelaksana + Ketua Umum;
    /// jenis lain Ketua Umum + Pembina. Ketua Umum diambil dari periode yang memuat tanggal terbit, else periode Active.
    /// </summary>
    public partial class Certificate
    {
        public record SignerLine(string Role, string? Name, string? Number);

        public class PrintData
        {
            public string Number { get; set; } = "";
            public CertificateType Type { get; set; }
            public CertificateStatus Status { get; set; }
            public string Title { get; set; } = "";
            public DateOnly IssuedDate { get; set; }
            public string VerificationCode { get; set; } = "";
            public string MemberName { get; set; } = "";
            public string? MemberNumber { get; set; }
            /// <summary>Nama kegiatan / jabatan + periode / prestasi + lomba; null untuk Lainnya.</summary>
            public string? Reference { get; set; }
            /// <summary>Tanggal + tempat kegiatan, tingkat prestasi, atau masa jabatan.</summary>
            public string? ReferenceDetail { get; set; }
            public List<SignerLine> Signers { get; set; } = new();
        }

        public PrintData GetPrintData()
        {
            var context = DBClass.GetContext();
            var row = context.TCertificates.AsNoTracking().Include(x => x.Member).SingleOrDefault(x => x.Id == Id)
                ?? throw new ArgumentException("Sertifikat tidak ditemukan.");
            var data = new PrintData
            {
                Number = row.CertificateNumber,
                Type = (CertificateType)row.CertificateType,
                Status = (CertificateStatus)row.Status,
                Title = row.Title,
                IssuedDate = row.IssuedDate,
                VerificationCode = row.VerificationCode,
                MemberName = row.Member.Fullname,
                MemberNumber = row.Member.MemberNumber
            };
            var id = new System.Globalization.CultureInfo("id-ID");
            int? periodId = null;

            if (row.ActivityId != null)
            {
                var activity = context.TActivities.AsNoTracking().Include(x => x.Program).Single(x => x.Id == row.ActivityId);
                data.Reference = activity.Title;
                var date = activity.StartAt.ToString("d MMMM yyyy", id);
                if (activity.EndAt != null && activity.EndAt.Value.Date != activity.StartAt.Date)
                {
                    date += " – " + activity.EndAt.Value.ToString("d MMMM yyyy", id);
                }
                data.ReferenceDetail = string.IsNullOrWhiteSpace(activity.Location) ? date : date + " · " + activity.Location;
                periodId = activity.Program?.PeriodId;

                var ketua = context.TCommitteeMembers.AsNoTracking().Include(x => x.Member)
                    .Where(x => x.ActivityId == activity.Id && x.Status == (int)CommitteeMemberStatus.Aktif && x.CommitteeRole == (int)CommitteeRole.KetuaPelaksana)
                    .Select(x => x.Member).FirstOrDefault();
                data.Signers.Add(new SignerLine("Ketua Pelaksana", ketua?.Fullname, ketua?.MemberNumber));
            }
            else if (row.AssignmentId != null)
            {
                var a = context.TManagementAssignments.AsNoTracking().Include(x => x.Position).Include(x => x.Division).Include(x => x.Section).Include(x => x.Period)
                    .Single(x => x.Id == row.AssignmentId);
                data.Reference = a.Position.Name + (a.Division != null ? " " + a.Division.Name : a.Section != null ? " " + a.Section.Name : "");
                data.ReferenceDetail = "Periode " + a.Period.Name.Replace("Periode ", "", StringComparison.OrdinalIgnoreCase);
                periodId = a.PeriodId;
            }
            else if (row.AchievementId != null)
            {
                var a = context.TAchievements.AsNoTracking().Include(x => x.Competition).Single(x => x.Id == row.AchievementId);
                data.Reference = (a.RankTitle != null ? a.RankTitle + " " : "") + (a.Competition?.Name ?? a.Title);
                data.ReferenceDetail = "Tingkat " + Text((EventLevel)a.Level) + " · " + a.AchievedDate.ToString("d MMMM yyyy", id);
            }

            periodId ??= context.MManagementPeriods.AsNoTracking()
                .Where(x => x.StartDate <= row.IssuedDate && x.EndDate >= row.IssuedDate)
                .OrderByDescending(x => x.StartDate).Select(x => (int?)x.Id).FirstOrDefault()
                ?? context.MManagementPeriods.Where(x => x.Status == (int)ManagementPeriodStatus.Active).Select(x => (int?)x.Id).FirstOrDefault();
            var chair = periodId == null ? null : context.TManagementAssignments.AsNoTracking().Include(x => x.Member)
                .Where(x => x.PeriodId == periodId && x.Position.Name.StartsWith("Ketua Umum"))
                .OrderByDescending(x => x.IsCurrent).ThenByDescending(x => x.StartDate)
                .Select(x => x.Member).FirstOrDefault();
            data.Signers.Add(new SignerLine("Ketua Umum", chair?.Fullname, chair?.MemberNumber));

            if (row.ActivityId == null)
            {
                var advisor = context.MOrganizations.AsNoTracking().Select(x => x.AdvisorName).FirstOrDefault();
                data.Signers.Add(new SignerLine("Pembina", advisor, null));
            }
            return data;
        }

        private static string Text(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field == null ? null : (System.ComponentModel.DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }
    }
}
