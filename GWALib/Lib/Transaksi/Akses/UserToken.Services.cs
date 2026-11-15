using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Akses;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Akses
{
    public partial class UserToken
    {
        private const int TOKEN_BYTES = 32;
        private const int IP_LENGTH = 45;
        private const int USER_AGENT_LENGTH = 255;

        // Masa berlaku bawaan per tipe token.
        public static TimeSpan DefaultLifetime(UserTokenType tokenType)
        {
            return tokenType switch
            {
                UserTokenType.ResetPassword => TimeSpan.FromMinutes(60),
                UserTokenType.VerifyEmail => TimeSpan.FromHours(24),
                _ => TimeSpan.FromDays(30)
            };
        }

        #region SYNC
        public static List<UserToken> GetDataList(int? userId = null, UserTokenType? tokenType = null, bool activeOnly = false, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(userId, tokenType, activeOnly);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<UserToken, EF.TUserToken>();
        }
        // Menerbitkan token baru. Tidak butuh login (mis. lupa password): ins_by = pemilik token.
        // Token reset password / verifikasi email lama yang belum dipakai otomatis dicabut.
        public static Response<IssuedToken> Issue(NewRow newrow)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var issued = CreateRow(context, newrow, out var row);
                context.SaveChanges();
                return issued with { Data = new UserToken(row) };
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<IssuedToken>(msg));
        }
        // Mengecek token tanpa memakainya (mis. sebelum menampilkan form password baru).
        public static Response<UserToken> Validate(string token, UserTokenType tokenType)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                return new UserToken(GetValidRow(context, token, tokenType));
            }, data => ResponseFactory.DataFound(data), msg => ResponseFactory.Error<UserToken>(msg));
        }
        // Memakai token reset password dan mengganti password dalam satu SaveChanges; token lain milik user ikut dicabut.
        public static Response<UserToken> ResetPassword(string token, string newPassword)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var row = GetValidRow(context, token, UserTokenType.ResetPassword);
                var userRow = UserAccount.GetUser(context, row.UserId);
                UserAccount.SetRowPassword(userRow, newPassword, row.UserId);
                SetUsed(row, row.UserId);
                RevokeOthers(context, row.UserId, null, row.Id, row.UserId);
                context.SaveChanges();
                return new UserToken(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserToken>(msg));
        }
        // Memakai token verifikasi email. Penandaan email terverifikasi dilakukan pemanggil (m_user tidak menyimpan email).
        public static Response<UserToken> VerifyEmail(string token)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var row = GetValidRow(context, token, UserTokenType.VerifyEmail);
                SetUsed(row, row.UserId);
                context.SaveChanges();
                return new UserToken(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserToken>(msg));
        }
        // Refresh token rotation: token lama dipakai, token baru diterbitkan (masa berlaku dihitung ulang).
        public static Response<IssuedToken> Rotate(string refreshToken, string? ipAddress = null, string? userAgent = null)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var old = GetValidRow(context, refreshToken, UserTokenType.Refresh);
                SetUsed(old, old.UserId);
                var issued = CreateRow(context, new NewRow { UserId = old.UserId, TokenType = UserTokenType.Refresh, IpAddress = ipAddress, UserAgent = userAgent }, out var row);
                context.SaveChanges();
                return issued with { Data = new UserToken(row) };
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<IssuedToken>(msg));
        }
        // Mencabut semua token aktif user (mis. logout dari semua perangkat); tokenType null = semua tipe.
        public static Response<int> RevokeAll(int userId, UserTokenType? tokenType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var actorId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = RevokeOthers(context, userId, tokenType, 0, actorId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        // Menghapus token yang sudah kedaluwarsa / dipakai sebelum tanggal tertentu (untuk job pembersihan).
        public static Response<int> Purge(DateTime before, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                return context.TUserTokens.Where(x => x.ExpiresAt < before || x.UsedAt < before).ExecuteDelete();
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<int>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<UserToken>> GetDataListAsync(int? userId = null, UserTokenType? tokenType = null, bool activeOnly = false, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(userId, tokenType, activeOnly);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<UserToken, EF.TUserToken>();
        }
        public static async Task<Response<IssuedToken>> IssueAsync(NewRow newrow)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var issued = CreateRow(context, newrow, out var row);
                await context.SaveChangesAsync();
                return issued with { Data = new UserToken(row) };
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<IssuedToken>(msg));
        }
        public static async Task<Response<UserToken>> ValidateAsync(string token, UserTokenType tokenType)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                return new UserToken(await GetValidRowAsync(context, token, tokenType));
            }, data => ResponseFactory.DataFound(data), msg => ResponseFactory.Error<UserToken>(msg));
        }
        public static async Task<Response<UserToken>> ResetPasswordAsync(string token, string newPassword)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var row = await GetValidRowAsync(context, token, UserTokenType.ResetPassword);
                var userRow = UserAccount.GetUser(context, row.UserId);
                UserAccount.SetRowPassword(userRow, newPassword, row.UserId);
                SetUsed(row, row.UserId);
                RevokeOthers(context, row.UserId, null, row.Id, row.UserId);
                await context.SaveChangesAsync();
                return new UserToken(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserToken>(msg));
        }
        public static async Task<Response<UserToken>> VerifyEmailAsync(string token)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var row = await GetValidRowAsync(context, token, UserTokenType.VerifyEmail);
                SetUsed(row, row.UserId);
                await context.SaveChangesAsync();
                return new UserToken(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserToken>(msg));
        }
        public static async Task<Response<IssuedToken>> RotateAsync(string refreshToken, string? ipAddress = null, string? userAgent = null)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var old = await GetValidRowAsync(context, refreshToken, UserTokenType.Refresh);
                SetUsed(old, old.UserId);
                var issued = CreateRow(context, new NewRow { UserId = old.UserId, TokenType = UserTokenType.Refresh, IpAddress = ipAddress, UserAgent = userAgent }, out var row);
                await context.SaveChangesAsync();
                return issued with { Data = new UserToken(row) };
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<IssuedToken>(msg));
        }
        public static async Task<Response<int>> RevokeAllAsync(int userId, UserTokenType? tokenType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var actorId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = RevokeOthers(context, userId, tokenType, 0, actorId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        public static async Task<Response<int>> PurgeAsync(DateTime before, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                return await context.TUserTokens.Where(x => x.ExpiresAt < before || x.UsedAt < before).ExecuteDeleteAsync();
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<int>(msg));
        }
        #endregion

        #region HELPER
        // SHA-256 heksadesimal huruf kecil (64 karakter) sesuai kolom token_hash.
        private static string Hash(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
        }
        // Token acak 256 bit, base64url supaya aman ditaruh di URL.
        private static string NewToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(TOKEN_BYTES)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
        private static IQueryable<EF.TUserToken> FilterQuery(int? userId, UserTokenType? tokenType, bool activeOnly)
        {
            var now = DateTime.Now;
            return _Querys.Where(x => (userId == null || x.UserId == userId)
                    && (tokenType == null || x.TokenType == (int)tokenType)
                    && (!activeOnly || (x.UsedAt == null && x.ExpiresAt > now)))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        // Pesan sama untuk token salah / dipakai / kedaluwarsa / user nonaktif, supaya tidak membocorkan alasannya.
        private static EF.TUserToken CheckValid(EF.TUserToken? row, EF.ModelContext context)
        {
            if (row == null || row.UsedAt != null || row.ExpiresAt <= DateTime.Now
                || !context.MUsers.Any(x => x.Id == row.UserId && x.IsActive == (int)ActiveStatus.Aktif))
            {
                throw new ArgumentException(ExceptionMessage.TOKEN_INVALID);
            }
            return row;
        }
        private static EF.TUserToken GetValidRow(EF.ModelContext context, string token, UserTokenType tokenType)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException(ExceptionMessage.TOKEN_INVALID);
            }
            var hash = Hash(token.Trim());
            return CheckValid(context.TUserTokens.SingleOrDefault(x => x.TokenHash == hash && x.TokenType == (int)tokenType), context);
        }
        private static async Task<EF.TUserToken> GetValidRowAsync(EF.ModelContext context, string token, UserTokenType tokenType)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException(ExceptionMessage.TOKEN_INVALID);
            }
            var hash = Hash(token.Trim());
            return CheckValid(await context.TUserTokens.SingleOrDefaultAsync(x => x.TokenHash == hash && x.TokenType == (int)tokenType), context);
        }
        private static void SetUsed(EF.TUserToken row, int userId)
        {
            var now = DateTime.Now;
            row.UsedAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        // Cabut token aktif milik user (kecuali exceptId); tokenType null = semua tipe.
        private static int RevokeOthers(EF.ModelContext context, int userId, UserTokenType? tokenType, int exceptId, int actorId)
        {
            var now = DateTime.Now;
            var rows = context.TUserTokens.Where(x => x.UserId == userId && x.Id != exceptId && x.UsedAt == null && x.ExpiresAt > now
                && (tokenType == null || x.TokenType == (int)tokenType)).ToList();
            rows.ForEach(x => SetUsed(x, actorId));
            return rows.Count;
        }
        private static IssuedToken CreateRow(EF.ModelContext context, NewRow newrow, out EF.TUserToken row)
        {
            if (!Enum.IsDefined(newrow.TokenType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Token"));
            }
            var lifetime = newrow.Lifetime ?? DefaultLifetime(newrow.TokenType);
            if (lifetime <= TimeSpan.Zero)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Masa Berlaku Token"));
            }
            if (UserAccount.GetUser(context, newrow.UserId).IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "User"));
            }
            if (newrow.TokenType != UserTokenType.Refresh)
            {
                RevokeOthers(context, newrow.UserId, newrow.TokenType, 0, newrow.UserId);
            }

            var token = NewToken();
            var now = DateTime.Now;
            row = new EF.TUserToken
            {
                Id = SequenceUtility.GetNextId<EF.TUserToken>(context),
                UserId = newrow.UserId,
                TokenType = (int)newrow.TokenType,
                TokenHash = Hash(token),
                ExpiresAt = now.Add(lifetime),
                IpAddress = TransaksiHelper.Truncate(newrow.IpAddress?.Trim(), IP_LENGTH),
                UserAgent = TransaksiHelper.Truncate(newrow.UserAgent?.Trim(), USER_AGENT_LENGTH),
                InsDate = now,
                InsBy = newrow.UserId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TUserTokens.Add(row);
            return new IssuedToken(token, row.ExpiresAt, null!);
        }
        #endregion
    }
}
