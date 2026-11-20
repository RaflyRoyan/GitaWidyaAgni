using GWALib.General;
using GWALib.General.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Tanda tangan elektronik (QR) pada laporan cetak (t_document_signature). Setiap penanda tangan bernama di laporan yang memakai QR
    /// mendapat token acak; QR berisi {PublicUrl}/ttd/{token}. Cetak ulang dokumen yang sama (jenis + judul + referensi + jabatan + nama +
    /// tanggal tanda tangan) memakai token yang sama dan hanya menambah print_count. Verify(token) untuk halaman publik.
    /// </summary>
    public static class DocumentSignature
    {
        public record IssueRow(string DocType, string Title, string? Reference, string SignerRole, string SignerName, string? SignerNumber, DateOnly SignDate);
        public record Check(string DocType, string Title, string? Reference, string SignerRole, string SignerName, string? SignerNumber,
            DateOnly SignDate, DateTime IssuedAt, string? IssuedBy, DateTime LastPrintedAt, int PrintCount);

        private const int TOKEN_LENGTH = 32;
        private static readonly Regex TokenPattern = new("^[0-9A-F]{32}$", RegexOptions.Compiled);

        /// <summary>Token untuk setiap baris (urutan sama). Baris tanpa nama penanda tangan tidak boleh dikirim.</summary>
        public static List<string> Issue(IReadOnlyList<IssueRow> rows, string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            var now = DateTime.Now;
            var tokens = new List<string>();
            foreach (var r in rows)
            {
                var row = Clean(r);
                var fingerprint = Fingerprint(row);
                var existing = context.TDocumentSignatures.SingleOrDefault(x => x.Fingerprint == fingerprint)
                    ?? context.TDocumentSignatures.Local.SingleOrDefault(x => x.Fingerprint == fingerprint);
                if (existing != null)
                {
                    existing.PrintCount += 1;
                    existing.LastPrintedAt = now;
                    existing.LastPrintedBy = userId;
                    existing.UpdDate = now;
                    existing.UpdBy = userId;
                    tokens.Add(existing.Token);
                    continue;
                }
                var entity = new EF.TDocumentSignature
                {
                    Id = SequenceUtility.GetNextId<EF.TDocumentSignature>(context),
                    Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(TOKEN_LENGTH / 2)),
                    Fingerprint = fingerprint,
                    DocType = row.DocType,
                    DocTitle = row.Title,
                    DocRef = row.Reference,
                    SignerRole = row.SignerRole,
                    SignerName = row.SignerName,
                    SignerNumber = row.SignerNumber,
                    SignDate = row.SignDate,
                    PrintCount = 1,
                    LastPrintedAt = now,
                    LastPrintedBy = userId,
                    InsDate = now,
                    InsBy = userId
                };
                context.TDocumentSignatures.Add(entity);
                tokens.Add(entity.Token);
            }
            context.SaveChanges();
            return tokens;
        }

        /// <summary>Data tanda tangan untuk halaman publik /ttd/{token}; null bila token tidak dikenal. Penerbit = nama anggota user, else username.</summary>
        public static Check? Verify(string? token)
        {
            var t = (token ?? "").Trim().ToUpperInvariant();
            if (!TokenPattern.IsMatch(t))
            {
                return null;
            }
            using var context = DBClass.GetContext();
            var row = context.TDocumentSignatures.AsNoTracking().SingleOrDefault(x => x.Token == t);
            if (row == null)
            {
                return null;
            }
            var issuer = row.InsBy == null ? null
                : context.MMembers.Where(x => x.UserId == row.InsBy).Select(x => x.Fullname).FirstOrDefault()
                    ?? context.MUsers.Where(x => x.Id == row.InsBy).Select(x => x.Username).FirstOrDefault();
            return new Check(row.DocType, row.DocTitle, row.DocRef, row.SignerRole, row.SignerName, row.SignerNumber, row.SignDate,
                row.InsDate, issuer, row.LastPrintedAt, row.PrintCount);
        }

        private static IssueRow Clean(IssueRow r)
        {
            static string Cut(string? s, int max) => (s ?? "").Trim().Length > max ? s!.Trim()[..max] : (s ?? "").Trim();
            if (string.IsNullOrWhiteSpace(r.SignerName) || string.IsNullOrWhiteSpace(r.SignerRole))
            {
                throw new ArgumentException("Penanda tangan tanpa nama atau jabatan tidak bisa diberi QR.");
            }
            return new IssueRow(Cut(r.DocType, 40).ToUpperInvariant(), Cut(r.Title, 200), string.IsNullOrWhiteSpace(r.Reference) ? null : Cut(r.Reference, 255),
                Cut(r.SignerRole, 100), Cut(r.SignerName, 150), string.IsNullOrWhiteSpace(r.SignerNumber) ? null : Cut(r.SignerNumber, 50), r.SignDate);
        }

        private static string Fingerprint(IssueRow r)
        {
            var text = string.Join("|", r.DocType, r.Title, r.Reference ?? "", r.SignerRole, r.SignerName, r.SignerNumber ?? "", r.SignDate.ToString("yyyy-MM-dd"));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        }
    }
}
