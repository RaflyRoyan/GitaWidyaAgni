using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Akses
{
    /// <summary>
    /// Token sekali pakai / refresh milik user (t_user_token): reset password, verifikasi email, refresh login.
    /// Yang disimpan hanya hash SHA-256; token mentah hanya dikembalikan sekali saat diterbitkan (<see cref="IssuedToken"/>).
    /// Token valid = tipe cocok, belum dipakai (UsedAt NULL), belum kedaluwarsa dan user masih aktif.
    /// Mencabut token juga mengisi UsedAt.
    /// </summary>
    public partial class UserToken : BaseLib
    {
        public class NewRow
        {
            public int UserId { get; set; }
            public UserTokenType TokenType { get; set; }
            // null = masa berlaku bawaan tipe token (lihat DefaultLifetime).
            public TimeSpan? Lifetime { get; set; }
            public string? IpAddress { get; set; }
            public string? UserAgent { get; set; }
        }
        // Token mentah untuk dikirim ke user (link email / cookie); tidak disimpan dan tidak bisa diambil lagi.
        public record IssuedToken(string Token, DateTime ExpiresAt, UserToken Data);

        public int Id { get; private set; }
        public int UserId { get; private set; }
        public UserTokenType TokenType { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TUserToken> _Querys
        {
            get
            {
                return DBClass.GetContext().TUserTokens.AsQueryable();
            }
        }
        public UserToken()
        {
        }
        public UserToken(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<UserToken>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal UserToken(EF.TUserToken row)
        {
            SetData(row);
        }
        private void SetData(EF.TUserToken row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<UserToken>();
            UserId = row.UserId;
            TokenType = (UserTokenType)row.TokenType;
            ExpiresAt = row.ExpiresAt;
            UsedAt = row.UsedAt;
            IpAddress = row.IpAddress;
            UserAgent = row.UserAgent;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
