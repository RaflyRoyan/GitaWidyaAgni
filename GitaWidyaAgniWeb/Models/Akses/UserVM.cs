using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Akses
{
    /// <summary>Akses › User: akun login, role per periode, reset password oleh admin.</summary>
    public class UserVM
    {
        public class Form
        {
            public bool IsEdit { get; set; }
            public string Username { get; set; } = "";
            public string? Password { get; set; }
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
        public class Add : Form
        {
        }
        public class Edit : Form
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<UserAccount>(encKey, "User");
                IsEdit = true;
                Username = obj.Username;
                IsActive = (int)obj.IsActive;
            }
        }

        public class ResetPassword
        {
            public string Key { get; set; } = "";
            public string Username { get; set; } = "";
            public string PasswordBaru { get; set; } = "";
            public string UlangiPassword { get; set; } = "";
        }

        // Partial _Role: daftar role user + form tambah.
        public class RoleIndex
        {
            public string Key { get; set; } = "";
            public string Username { get; set; } = "";
            public List<GwaVM.OpsiItem> RoleOpsi { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = new();

            public RoleIndex(string encKey)
            {
                var user = LibExtensions.Load<UserAccount>(encKey, "User");
                Key = user.EncDecrKey;
                Username = user.Username;
                RoleOpsi = Role.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, $"{x.Name} ({x.Code})")).ToList();
                PeriodeOpsi = ManagementPeriod.GetDataList().Where(x => x.Status != ManagementPeriodStatus.Closed)
                    .Select(x => new GwaVM.OpsiItem(x.Id, $"{x.Name} — {EnumHelper.Text(x.Status)}")).ToList();
            }
        }
        public class TambahRoleInput
        {
            public string Key { get; set; } = "";
            public int RoleId { get; set; }
            public int? PeriodId { get; set; }
        }

        public class ViewModel
        {
            public class Row
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
                public string Username { get; set; } = "";
                public string? NamaAnggota { get; set; }
                public string? NomorAnggota { get; set; }
                public string Role { get; set; } = "";
                public DateTime? LastLoginAt { get; set; }
                public int IsActive { get; set; }
            }
            public class RoleRow
            {
                public string EncDecrKey { get; set; } = "";
                public string Kode { get; set; } = "";
                public string Nama { get; set; } = "";
                public string? Periode { get; set; }
                public string? StatusPeriode { get; set; }
            }
        }

        public class Method
        {
            public static IEnumerable<ViewModel.Row> GetDataList()
            {
                var periods = ManagementPeriod.GetDataList().ToDictionary(x => x.Id, x => x.Name);
                var roles = UserRole.GetDataList().GroupBy(x => x.User.Id).ToDictionary(g => g.Key,
                    g => string.Join(", ", g.Select(x => x.Role.Name + (x.PeriodId != null && periods.TryGetValue(x.PeriodId.Value, out var p) ? $" ({p})" : ""))));
                var members = Member.GetDataList().Where(x => x.User != null).GroupBy(x => x.User!.Id).ToDictionary(g => g.Key, g => g.First());
                return UserAccount.GetDataList(FilterAktif.Semua).Select(u => new ViewModel.Row
                {
                    EncDecrKey = u.EncDecrKey,
                    Id = u.Id,
                    Username = u.Username,
                    NamaAnggota = members.TryGetValue(u.Id, out var m) ? m.Fullname : null,
                    NomorAnggota = members.TryGetValue(u.Id, out var m2) ? m2.MemberNumber : null,
                    Role = roles.TryGetValue(u.Id, out var r) ? r : "",
                    LastLoginAt = u.LastLoginAt,
                    IsActive = (int)u.IsActive
                });
            }

            public static void Simpan(Form input, string user)
            {
                if (string.IsNullOrWhiteSpace(input.Password) || input.Password.Length < 8)
                {
                    throw new ArgumentException("Password minimal 8 karakter.");
                }
                UserAccount.Save(new UserAccount.NewRow { Username = input.Username, Password = input.Password }, user).Ok();
            }

            // Mengembalikan id user yang diubah (untuk membersihkan cache aksesnya).
            public static int Update(string? encKey, Form input, string user, int currentUserId)
            {
                var obj = LibExtensions.Load<UserAccount>(encKey);
                if (obj.Id == currentUserId && input.IsActive == (int)ActiveStatus.TidakAktif)
                {
                    throw new ArgumentException("Anda tidak bisa menonaktifkan akun sendiri.");
                }
                obj.Username = input.Username;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
                return obj.Id;
            }

            public static int Nonaktifkan(string? encKey, string user, int currentUserId)
            {
                var obj = LibExtensions.Load<UserAccount>(encKey);
                if (obj.Id == currentUserId)
                {
                    throw new ArgumentException("Anda tidak bisa menonaktifkan akun sendiri.");
                }
                obj.Delete(user).Ok();
                return obj.Id;
            }

            public static void GantiPassword(ResetPassword input, string user)
            {
                if (string.IsNullOrWhiteSpace(input.PasswordBaru) || input.PasswordBaru.Length < 8)
                {
                    throw new ArgumentException("Password baru minimal 8 karakter.");
                }
                if (input.PasswordBaru != input.UlangiPassword)
                {
                    throw new ArgumentException("Ulangi password tidak sama dengan password baru.");
                }
                LibExtensions.Load<UserAccount>(input.Key).ChangePassword(input.PasswordBaru, user).Ok();
            }

            public static IEnumerable<ViewModel.RoleRow> GetRoles(string? encKey)
            {
                var userId = LibExtensions.Load<UserAccount>(encKey).Id;
                var periods = ManagementPeriod.GetDataList().ToDictionary(x => x.Id);
                return UserRole.GetDataList(userId: userId).Select(x => new ViewModel.RoleRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Kode = x.Role.Code,
                    Nama = x.Role.Name,
                    Periode = x.PeriodId != null && periods.TryGetValue(x.PeriodId.Value, out var p) ? p.Name : null,
                    StatusPeriode = x.PeriodId != null && periods.TryGetValue(x.PeriodId.Value, out var p2) ? EnumHelper.Text(p2.Status) : null
                });
            }

            public static int TambahRole(TambahRoleInput input, string user)
            {
                var userId = LibExtensions.Load<UserAccount>(input.Key).Id;
                if (input.RoleId <= 0)
                {
                    throw new ArgumentException("Pilih role.");
                }
                UserRole.Save(new UserRole.NewRow { UserId = userId, RoleId = input.RoleId, PeriodId = input.PeriodId > 0 ? input.PeriodId : null }, user).Ok();
                return userId;
            }

            public static int HapusRole(string? encKey, string user, int currentUserId)
            {
                var row = LibExtensions.Load<UserRole>(encKey);
                if (row.User.Id == currentUserId && row.Role.Code == UserAccess.SUPERADMIN)
                {
                    throw new ArgumentException("Anda tidak bisa mencabut role SUPERADMIN dari akun sendiri.");
                }
                row.Delete(user).Ok();
                return row.User.Id;
            }
        }
    }
}
