using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Akses
{
    public partial class UserAccount
    {
        private static readonly PasswordHasher<EF.MUser> _passwordHasher = new();

        #region SYNC
        public List<UserRole> GetUserRoles(bool refresh)
        {
            if (refresh || UserRoleList == null)
            {
                UserRoleList = UserRole.GetDataList(userId: Id);
            }
            return UserRoleList;
        }
        public static List<UserAccount> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<UserAccount, EF.MUser>();
        }
        public static Response<UserAccount> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MUsers.Add(row);
                context.SaveChanges();
                return new UserAccount(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<UserAccount>(msg));
        }
        public Response<UserAccount> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new UserAccount(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserAccount>(msg));
        }
        public Response<UserAccount> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new UserAccount(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<UserAccount>(msg));
        }
        public Response<UserAccount> ChangePassword(string newPassword, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowPassword(row, newPassword, userId);
                context.SaveChanges();
                return new UserAccount(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserAccount>(msg));
        }
        // Login dengan username + password. Username tidak ada, user nonaktif dan password salah memberi pesan yang sama.
        // Berhasil: last_login_at diperbarui, dan hash lama di-rehash bila PasswordHasher memintanya.
        public static Response<UserAccount> Login(string username, string password)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var row = SetLogin(context, username, password);
                context.SaveChanges();
                return new UserAccount(row);
            }, data => ResponseFactory.Success("Login berhasil", data), msg => ResponseFactory.Error<UserAccount>(msg));
        }
        // Admin pertama untuk database kosong: membuat role SUPERADMIN (bila belum ada), user, dan m_user_role tanpa periode.
        // Ditolak begitu m_user sudah berisi; tidak butuh login (belum ada user). Mengembalikan null bila dilewati.
        public static UserAccount? BootstrapAdmin(string username, string password)
        {
            using var context = DBClass.GetContext();
            if (context.MUsers.Any())
            {
                return null;
            }
            using var transaction = context.Database.BeginTransaction();
            var now = DateTime.Now;
            var role = context.MRoles.SingleOrDefault(x => x.Code == UserAccess.SUPERADMIN);
            if (role == null)
            {
                role = new EF.MRole
                {
                    Id = SequenceUtility.GetNextId<EF.MRole>(context),
                    Code = UserAccess.SUPERADMIN,
                    Name = "Super Admin",
                    Description = "Akses penuh ke semua menu dan aksi",
                    IsActive = (int)ActiveStatus.Aktif,
                    InsDate = now
                };
                context.MRoles.Add(role);
            }
            var user = CreateRow(context, new NewRow { Username = username, Password = password }, 0);
            user.InsBy = null;
            context.MUsers.Add(user);
            context.MUserRoles.Add(new EF.MUserRole
            {
                Id = SequenceUtility.GetNextId<EF.MUserRole>(context),
                UserId = user.Id,
                RoleId = role.Id,
                InsDate = now
            });
            context.SaveChanges();
            transaction.Commit();
            return new UserAccount(user);
        }
        public bool VerifyPassword(string password)
        {
            using var context = DBClass.GetContext();
            var row = context.MUsers.SingleOrDefault(x => x.Id == Id);
            return row != null && IsPasswordValid(row, password);
        }
        // Ganti password sendiri (Area Anggota): password lama wajib benar. Reset oleh admin memakai ChangePassword.
        public static Response<UserAccount> ChangeOwnPassword(string currentPassword, string newPassword, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, userId);
                if (!IsPasswordValid(row, currentPassword ?? string.Empty))
                {
                    throw new ArgumentException(ExceptionMessage.PASSWORD_CURRENT_INVALID);
                }
                SetRowPassword(row, newPassword, userId);
                context.SaveChanges();
                return new UserAccount(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserAccount>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<UserRole>> GetUserRolesAsync(bool refresh)
        {
            if (refresh || UserRoleList == null)
            {
                UserRoleList = await UserRole.GetDataListAsync(userId: Id);
            }
            return UserRoleList;
        }
        public static async Task<List<UserAccount>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<UserAccount, EF.MUser>();
        }
        public static async Task<Response<UserAccount>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MUsers.Add(row);
                await context.SaveChangesAsync();
                return new UserAccount(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<UserAccount>(msg));
        }
        public async Task<Response<UserAccount>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new UserAccount(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserAccount>(msg));
        }
        public async Task<Response<UserAccount>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new UserAccount(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<UserAccount>(msg));
        }
        public async Task<Response<UserAccount>> ChangePasswordAsync(string newPassword, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowPassword(row, newPassword, userId);
                await context.SaveChangesAsync();
                return new UserAccount(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserAccount>(msg));
        }
        public static async Task<Response<UserAccount>> LoginAsync(string username, string password)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var row = SetLogin(context, username, password);
                await context.SaveChangesAsync();
                return new UserAccount(row);
            }, data => ResponseFactory.Success("Login berhasil", data), msg => ResponseFactory.Error<UserAccount>(msg));
        }
        public async Task<bool> VerifyPasswordAsync(string password)
        {
            using var context = DBClass.GetContext();
            var row = await context.MUsers.SingleOrDefaultAsync(x => x.Id == Id);
            return row != null && IsPasswordValid(row, password);
        }
        #endregion

        #region HELPER
        // Local: user yang baru di-Add di context yang sama belum ada di DB.
        internal static EF.MUser GetUser(EF.ModelContext context, int userId)
        {
            return context.MUsers.Local.SingleOrDefault(x => x.Id == userId)
                ?? context.MUsers.SingleOrDefault(x => x.Id == userId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "User"));
        }
        private static IQueryable<EF.MUser> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua))
                .OrderBy(x => x.Username);
        }
        private static EF.MUser GetRow(EF.ModelContext context, int id)
        {
            return context.MUsers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "User"));
        }
        private static async Task<EF.MUser> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MUsers.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "User"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Username"));
            }
            ValidateUtility.CheckDuplicateData<EF.MUser>(context, x => x.Username == username && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Username"));
        }
        private static string HashPassword(EF.MUser row, string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Password"));
            }
            return _passwordHasher.HashPassword(row, password);
        }
        private static EF.MUser SetLogin(EF.ModelContext context, string? username, string? password)
        {
            var name = username?.Trim() ?? string.Empty;
            var row = string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password) ? null : context.MUsers.SingleOrDefault(x => x.Username == name);
            if (row == null || row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(ExceptionMessage.LOGIN_INVALID);
            }
            var result = _passwordHasher.VerifyHashedPassword(row, row.Password, password!);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new ArgumentException(ExceptionMessage.LOGIN_INVALID);
            }
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                row.Password = _passwordHasher.HashPassword(row, password!);
            }
            row.LastLoginAt = DateTime.Now;
            return row;
        }
        private static bool IsPasswordValid(EF.MUser row, string password)
        {
            return _passwordHasher.VerifyHashedPassword(row, row.Password, password) != PasswordVerificationResult.Failed;
        }
        private static EF.MUser CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var username = newrow.Username?.Trim() ?? string.Empty;
            ValidateData(context, 0, username);

            var row = new EF.MUser
            {
                Id = SequenceUtility.GetNextId<EF.MUser>(context),
                Username = username,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            row.Password = HashPassword(row, newrow.Password);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MUser row, int userId)
        {
            var username = Username?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, username);

            row.Username = username;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // Dipakai juga UserToken.ResetPassword (reset lewat token, tanpa login).
        internal static void SetRowPassword(EF.MUser row, string newPassword, int userId)
        {
            row.Password = HashPassword(row, newPassword);
            row.ResetKey = null;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetRowDelete(EF.MUser row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
