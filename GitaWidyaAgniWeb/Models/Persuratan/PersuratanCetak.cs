using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Surat;
using GWALib.Lib.Transaksi.Surat;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Persuratan
{
    /// <summary>
    /// Menyusun laporan DevExpress Persuratan (PersuratanReport) dari data GWALib: cetak surat keluar, pratinjau template,
    /// lembar disposisi, agenda surat, dan daftar arsip dokumen.
    /// </summary>
    public static class PersuratanCetak
    {
        // Surat keluar yang belum sah diberi tanda air.
        private static readonly LetterStatus[] Sah = { LetterStatus.Approved, LetterStatus.Sent, LetterStatus.Archived };

        public static PersuratanReport Surat(string? key, string webRoot)
        {
            var letter = LibExtensions.Load<Letter>(key, "Surat");
            if (letter.Direction != LetterDirection.Out)
            {
                throw new ArgumentException("Cetak surat hanya untuk surat keluar; untuk surat masuk pakai lembar disposisi.");
            }
            var approvals = letter.GetApprovals(true).OrderBy(x => x.ApprovalLevel).ToList();
            var body = letter.Body;
            if (!string.IsNullOrWhiteSpace(body) && !body.Contains('<'))
            {
                body = PersuratanReport.PlainToHtml(body);
            }
            return PersuratanReport.Surat(new PersuratanReport.SuratData
            {
                Number = letter.LetterNumber,
                Date = letter.LetterDate,
                Subject = letter.Subject,
                Recipient = letter.Recipient,
                Attachment = "-",
                Body = body,
                Summary = letter.Summary,
                Signers = approvals.Select(Signer).ToList(),
                Stamp = Sah.Contains(letter.Status) ? null : (letter.Status == LetterStatus.Rejected ? "DITOLAK" : "DRAFT")
            }, ReportBrand.Load(webRoot));
        }

        public static PersuratanReport Template(string? key, string webRoot)
        {
            var template = LibExtensions.Load<LetterTemplate>(key, "Template");
            var body = template.Body;
            if (!string.IsNullOrWhiteSpace(body) && !body.Contains('<'))
            {
                body = PersuratanReport.PlainToHtml(body);
            }
            return PersuratanReport.Template(template.Name, template.LetterType, body, ReportBrand.Load(webRoot));
        }

        public static PersuratanReport Disposisi(string? key, string webRoot)
        {
            var letter = LibExtensions.Load<Letter>(key, "Surat");
            var names = new Dictionary<int, string>();
            var lines = letter.GetDispositions(true).OrderBy(x => x.InsDate).Select(x => new PersuratanReport.DisposisiLine(
                UserName(x.FromUserId, names), x.ToMemberName, x.Instruction, x.DueDate, EnumHelper.Text(x.Status), x.InsDate)).ToList();
            return PersuratanReport.Disposisi(letter, lines, ReportBrand.Load(webRoot));
        }

        /// <summary>Agenda surat (arah 1 = masuk, 2 = keluar) dalam rentang tanggal; tanggal = diterima (masuk) / tanggal surat (keluar), else tanggal input.</summary>
        public static PersuratanReport Agenda(int arah, DateTime? dari, DateTime? sampai, string webRoot)
        {
            var incoming = arah == (int)LetterDirection.In;
            var from = dari.HasValue ? DateOnly.FromDateTime(dari.Value) : (DateOnly?)null;
            var to = sampai.HasValue ? DateOnly.FromDateTime(sampai.Value) : (DateOnly?)null;
            if (from != null && to != null && to < from)
            {
                (from, to) = (to, from);
            }
            DateOnly Tanggal(Letter x) => (incoming ? x.ReceivedDate : x.LetterDate) ?? x.LetterDate ?? DateOnly.FromDateTime(x.InsDate);
            var letters = Letter.GetDataList(incoming ? LetterDirection.In : LetterDirection.Out)
                .Where(x => (from == null || Tanggal(x) >= from) && (to == null || Tanggal(x) <= to))
                .OrderBy(Tanggal).ThenBy(x => x.Id)
                .ToList();
            var dispositions = new Dictionary<int, string>();
            if (incoming)
            {
                foreach (var letter in letters)
                {
                    var targets = letter.GetDispositions(true).Select(x => x.ToMemberName).Distinct().ToList();
                    if (targets.Count > 0)
                    {
                        dispositions[letter.Id] = string.Join(", ", targets);
                    }
                }
            }
            var id = new System.Globalization.CultureInfo("id-ID");
            var range = from == null && to == null ? "Semua tanggal"
                : $"Tanggal {(from == null ? "awal" : from.Value.ToString("d MMMM yyyy", id))} s.d. {(to == null ? "sekarang" : to.Value.ToString("d MMMM yyyy", id))}";
            return PersuratanReport.Agenda(incoming, letters, dispositions, range, ReportBrand.Load(webRoot));
        }

        public static PersuratanReport Arsip(string webRoot)
        {
            return PersuratanReport.Arsip(Document.GetDataList(), ReportBrand.Load(webRoot));
        }

        // Penanda tangan dari persetujuan: nama + NIA anggota pemilik user; yang belum memutuskan tanpa nama.
        // Jabatan: jabatan di persetujuan, else nama role aktif approver selain Anggota.
        private static DocumentReport.Signer Signer(LetterApproval x)
        {
            var profile = UserAccess.GetProfile(x.ApproverUserId);
            var role = !string.IsNullOrWhiteSpace(x.PositionName) ? x.PositionName
                : profile?.RoleNames.FirstOrDefault(r => !r.Equals("Anggota", StringComparison.OrdinalIgnoreCase)) ?? "Pengesah";
            if (x.Decision != LetterDecision.Approved)
            {
                return new DocumentReport.Signer(role, null, null);
            }
            var member = Member.GetByUser(x.ApproverUserId.ToString());
            var name = member?.Fullname ?? profile?.DisplayName ?? x.ApproverName;
            return new DocumentReport.Signer(role, name, member?.MemberNumber, x.GetSignature());
        }

        private static string UserName(int userId, Dictionary<int, string> cache)
        {
            if (!cache.TryGetValue(userId, out var name))
            {
                name = UserAccess.GetProfile(userId)?.DisplayName ?? "-";
                cache[userId] = name;
            }
            return name;
        }
    }
}
