using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace GWALib.Lib.Master.Anggota
{
    /// <summary>
    /// Kartu Tanda Anggota (KTA) digital. QR kartu berisi token acak m_member.card_token (bukan NIA), dibuat saat KTA pertama kali
    /// dicetak / diunduh (GetCard / GetCards) dan bisa diganti (RenewCard, mis. kartu hilang) sehingga QR lama tidak berlaku.
    /// Verifikasi publik (VerifyCard) menampilkan status keanggotaan terkini: Aktif / Alumni sah, Tidak Aktif = kartu tidak berlaku.
    /// Masa berlaku tercetak = tanggal akhir periode kepengurusan Active.
    /// </summary>
    public partial class Member
    {
        public const int CARD_TOKEN_LENGTH = 32;
        private static readonly Regex CardTokenPattern = new("^[0-9A-F]{32}$", RegexOptions.Compiled);

        // Isi kartu (cetak). Position = jabatan kepengurusan yang sedang berjalan di periode Active (bisa lebih dari satu, dipisah koma).
        public record CardData(int MemberId, string Fullname, string? MemberNumber, MemberType MemberType, MemberStatus Status, string? Section,
            string? Position, short? Angkatan, string? Fakultas, string? ProgramStudi, DateOnly? JoinedAt, byte[]? Photo, string Token,
            DateTime IssuedAt, string? PeriodName, DateOnly? ValidUntil);

        // Hasil verifikasi publik (tanpa kontak, alamat, NIM). Valid = status Aktif atau Alumni.
        public record CardCheck(string Fullname, string? MemberNumber, MemberType MemberType, MemberStatus Status, string? Section, string? Position,
            short? Angkatan, DateOnly? JoinedAt, bool HasPhoto, DateTime IssuedAt, string? PeriodName, DateOnly? ValidUntil)
        {
            public bool Valid => Status == MemberStatus.Active || Status == MemberStatus.Alumni;
        }

        #region SYNC
        /// <summary>Data KTA anggota ini; token dibuat bila belum ada. Anggota Tidak Aktif ditolak.</summary>
        public Response<CardData> GetCard(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                EnsureCardToken(row, false, userId);
                context.SaveChanges();
                return CardRows(context, new List<EF.MMember> { row }).Single();
            }, data => ResponseFactory.DataFound(data), msg => ResponseFactory.Error<CardData>(msg));
        }

        /// <summary>Ganti token KTA (kartu hilang / bocor): QR kartu lama tidak berlaku lagi.</summary>
        public Response<CardData> RenewCard(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                EnsureCardToken(row, true, userId);
                context.SaveChanges();
                return CardRows(context, new List<EF.MMember> { row }).Single();
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CardData>(msg));
        }

        /// <summary>
        /// KTA banyak anggota sekaligus (cetak massal), urut section lalu nama. memberIds kosong = semua anggota Aktif
        /// (opsional satu section saat ini). Anggota Tidak Aktif dilewati; token dibuat bila belum ada.
        /// </summary>
        public static Response<List<CardData>> GetCards(IEnumerable<int>? memberIds, int? sectionId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var ids = memberIds?.Distinct().ToList() ?? new List<int>();
                int inactive = (int)MemberStatus.Inactive, active = (int)MemberStatus.Active;
                var query = context.MMembers.Where(x => x.Status != inactive);
                query = ids.Count > 0 ? query.Where(x => ids.Contains(x.Id)) : query.Where(x => x.Status == active);
                if (sectionId > 0)
                {
                    query = query.Where(x => x.TSectionMembers.Any(s => s.SectionId == sectionId && s.EndDate == null));
                }
                var rows = query.ToList();
                if (rows.Count == 0)
                {
                    throw new ArgumentException("Tidak ada anggota yang bisa dicetak KTA-nya.");
                }
                foreach (var row in rows)
                {
                    EnsureCardToken(row, false, userId);
                }
                context.SaveChanges();
                return CardRows(context, rows).OrderBy(x => x.Section ?? "~").ThenBy(x => x.Fullname).ToList();
            }, data => ResponseFactory.DataFound(data), msg => ResponseFactory.Error<List<CardData>>(msg));
        }

        /// <summary>Verifikasi publik QR KTA. null = token tidak dikenal (atau format salah).</summary>
        public static CardCheck? VerifyCard(string? token)
        {
            var value = NormalizeCardToken(token);
            if (value == null)
            {
                return null;
            }
            using var context = DBClass.GetContext();
            var row = context.MMembers.AsNoTracking().SingleOrDefault(x => x.CardToken == value);
            if (row == null)
            {
                return null;
            }
            var data = CardRows(context, new List<EF.MMember> { row }, false).Single();
            return new CardCheck(data.Fullname, data.MemberNumber, data.MemberType, data.Status, data.Section, data.Position, data.Angkatan,
                data.JoinedAt, context.MMemberPhotos.Any(x => x.MemberId == row.Id), data.IssuedAt, data.PeriodName, data.ValidUntil);
        }

        /// <summary>Foto terbaru pemilik KTA untuk halaman verifikasi; null bila token tidak dikenal, anggota Tidak Aktif, atau tanpa foto.</summary>
        public static byte[]? GetCardPhoto(string? token)
        {
            var value = NormalizeCardToken(token);
            if (value == null)
            {
                return null;
            }
            using var context = DBClass.GetContext();
            int inactive = (int)MemberStatus.Inactive;
            return context.MMemberPhotos.Where(x => x.Member.CardToken == value && x.Member.Status != inactive)
                .OrderByDescending(x => x.Id).Select(x => x.Photo).FirstOrDefault();
        }

        /// <summary>Teks lama keanggotaan, mis. "2 tahun 3 bulan"; null bila tanggal bergabung kosong.</summary>
        public static string? TenureText(DateOnly? joinedAt, DateOnly? until = null)
        {
            if (joinedAt == null)
            {
                return null;
            }
            var end = until ?? DateOnly.FromDateTime(DateTime.Today);
            if (end < joinedAt.Value)
            {
                return "belum bergabung";
            }
            var months = (end.Year - joinedAt.Value.Year) * 12 + end.Month - joinedAt.Value.Month - (end.Day < joinedAt.Value.Day ? 1 : 0);
            if (months < 1)
            {
                return "kurang dari 1 bulan";
            }
            int years = months / 12, rest = months % 12;
            return years == 0 ? $"{rest} bulan" : rest == 0 ? $"{years} tahun" : $"{years} tahun {rest} bulan";
        }
        #endregion

        #region HELPER
        private static string? NormalizeCardToken(string? token)
        {
            var value = token?.Trim().ToUpperInvariant();
            return value != null && CardTokenPattern.IsMatch(value) ? value : null;
        }
        private static void EnsureCardToken(EF.MMember row, bool renew, int userId)
        {
            if (row.Status == (int)MemberStatus.Inactive)
            {
                throw new ArgumentException($"{row.Fullname} tidak aktif; KTA tidak bisa diterbitkan.");
            }
            if (row.CardToken != null && !renew)
            {
                return;
            }
            row.CardToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(CARD_TOKEN_LENGTH / 2));
            row.CardIssuedAt = DateTime.Now;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Lengkapi baris anggota dengan section saat ini, jabatan di periode Active, foto terbaru, dan masa berlaku.
        private static List<CardData> CardRows(EF.ModelContext context, List<EF.MMember> rows, bool withPhoto = true)
        {
            var ids = rows.Select(x => x.Id).ToList();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var period = context.MManagementPeriods.AsNoTracking().FirstOrDefault(x => x.Status == (int)ManagementPeriodStatus.Active);
            var sections = context.TSectionMembers.Where(x => ids.Contains(x.MemberId) && x.EndDate == null)
                .Select(x => new { x.MemberId, x.Section.Name }).ToList()
                .GroupBy(x => x.MemberId).ToDictionary(g => g.Key, g => g.First().Name);
            var positions = period == null ? new Dictionary<int, string>() : context.TManagementAssignments
                .Where(x => ids.Contains(x.MemberId) && x.PeriodId == period.Id && (x.EndDate == null || x.EndDate >= today))
                .Select(x => new
                {
                    x.MemberId,
                    Text = x.Position.Name + (x.Division != null ? " " + x.Division.Name : x.Section != null ? " " + x.Section.Name : "")
                }).ToList()
                .GroupBy(x => x.MemberId).ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.Text).Distinct()));
            var photos = new Dictionary<int, byte[]>();
            if (withPhoto)
            {
                var latest = context.MMemberPhotos.Where(x => ids.Contains(x.MemberId))
                    .GroupBy(x => x.MemberId).Select(g => g.Max(x => x.Id)).ToList();
                photos = context.MMemberPhotos.Where(x => latest.Contains(x.Id)).Select(x => new { x.MemberId, x.Photo }).ToList()
                    .ToDictionary(x => x.MemberId, x => x.Photo);
            }
            return rows.Select(x => new CardData(x.Id, x.Fullname, x.MemberNumber, (MemberType)x.MemberType, (MemberStatus)x.Status,
                sections.GetValueOrDefault(x.Id), positions.GetValueOrDefault(x.Id), x.Angkatan, x.Fakultas, x.ProgramStudi, x.JoinedAt,
                photos.GetValueOrDefault(x.Id), x.CardToken!, x.CardIssuedAt ?? DateTime.Now, period?.Name,
                x.Status == (int)MemberStatus.Active ? period?.EndDate : null)).ToList();
        }
        #endregion
    }
}
