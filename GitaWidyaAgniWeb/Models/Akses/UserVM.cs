using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Akses
{
    /// <summary>Akses › User: akun login, role per periode, reset password oleh admin.</summary>
    public class UserVM
    {
        public class Index
        {
        }

        /// <summary>Grid User (partial _List, dimuat ulang setelah simpan).</summary>
        public class List
        {
            public List<Row> Rows { get; set; }
            public List()
            {
                Rows = Method.GetDataList().ToList();
            }
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Username { get; set; } = "";
            public string? NamaAnggota { get; set; }
            public string? NomorAnggota { get; set; }
            public string Role { get; set; } = "";
            public DateTime? LastLoginAt { get; set; }
            public bool Aktif { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        /// <summary>Modal Edit: form yang sama dengan Add (_Form) + status + info akun.</summary>
        public class Edit : Add
        {
            public string? NamaAnggota { get; set; }
            public DateTime? LastLoginAt { get; set; }

            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<UserAccount>(encKey, "User");
                Row = new NewRow { Username = obj.Username, IsActive = (int)obj.IsActive };
                NamaAnggota = Member.GetDataList().FirstOrDefault(x => x.User?.Id == obj.Id)?.Fullname;
                LastLoginAt = obj.LastLoginAt;
            }
        }

        /// <summary>Form modal Add &amp; Edit. Password hanya di Add; status hanya di Edit (user baru selalu Aktif).</summary>
        public class NewRow
        {
            [Required(ErrorMessage = "Username harus diisi")]
            [StringLength(100, ErrorMessage = "Username maksimal 100 karakter")]
            [Display(Name = "Username")]
            public string? Username { get; set; }

            [Display(Name = "Password")]
            public string? Password { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        public class ResetPassword
        {
            public string Key { get; set; } = "";
            public string Username { get; set; } = "";

            [Required(ErrorMessage = "Password baru harus diisi")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password baru minimal 8 karakter")]
            [Display(Name = "Password baru")]
            public string? PasswordBaru { get; set; }

            [Required(ErrorMessage = "Ulangi password harus diisi")]
            [Display(Name = "Ulangi password")]
            public string? UlangiPassword { get; set; }
        }

        /// <summary>Modal _Role: ringkasan user, panel tambah role, daftar role (_RoleList).</summary>
        public class RoleIndex
        {
            public string Key { get; set; } = "";
            public string Username { get; set; } = "";
            public TambahRoleInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> RoleOpsi { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = new();
            public RoleList List { get; set; }

            public RoleIndex(string encKey)
            {
                var user = LibExtensions.Load<UserAccount>(encKey, "User");
                Key = user.EncDecrKey;
                Username = user.Username;
                RoleOpsi = Role.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, $"{x.Name} ({x.Code})")).ToList();
                PeriodeOpsi = ManagementPeriod.GetDataList().Where(x => x.Status != ManagementPeriodStatus.Closed)
                    .Select(x => new GwaVM.OpsiItem(x.Id, $"{x.Name} — {EnumHelper.Text(x.Status)}")).ToList();
                List = new RoleList(Key);
            }
        }

        /// <summary>Grid role user (partial _RoleList, dimuat ulang setelah tambah / cabut).</summary>
        public class RoleList
        {
            public string Key { get; set; }
            public List<RoleRow> Rows { get; set; }
            public RoleList(string encKey)
            {
                Key = encKey;
                Rows = Method.GetRoles(encKey).ToList();
            }
        }

        public class RoleRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Kode { get; set; } = "";
            public string Nama { get; set; } = "";
            public string? Periode { get; set; }
            public string? StatusPeriode { get; set; }
        }

        public class TambahRoleInput
        {
            public string Key { get; set; } = "";

            [Range(1, int.MaxValue, ErrorMessage = "Pilih role")]
            [Display(Name = "Role")]
            public int RoleId { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                var periods = ManagementPeriod.GetDataList().ToDictionary(x => x.Id, x => x.Name);
                var roles = UserRole.GetDataList().GroupBy(x => x.User.Id).ToDictionary(g => g.Key,
                    g => string.Join(", ", g.Select(x => x.Role.Name + (x.PeriodId != null && periods.TryGetValue(x.PeriodId.Value, out var p) ? $" ({p})" : ""))));
                var members = Member.GetDataList().Where(x => x.User != null).GroupBy(x => x.User!.Id).ToDictionary(g => g.Key, g => g.First());
                return UserAccount.GetDataList(FilterAktif.Semua).Select(u => new Row
                {
                    EncDecrKey = u.EncDecrKey,
                    Username = u.Username,
                    NamaAnggota = members.TryGetValue(u.Id, out var m) ? m.Fullname : null,
                    NomorAnggota = members.TryGetValue(u.Id, out var m2) ? m2.MemberNumber : null,
                    Role = roles.TryGetValue(u.Id, out var r) ? r : "",
                    LastLoginAt = u.LastLoginAt,
                    Aktif = u.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.Username);
            }

            public static void Simpan(NewRow input, string user)
            {
                if (string.IsNullOrWhiteSpace(input.Password) || input.Password.Length < 8)
                {
                    throw new ArgumentException("Password minimal 8 karakter.");
                }
                UserAccount.Save(new UserAccount.NewRow { Username = input.Username ?? "", Password = input.Password }, user).Ok();
            }

            // Mengembalikan id user yang diubah (untuk membersihkan cache aksesnya).
            public static int Update(string encKey, NewRow input, string user, int currentUserId)
            {
                var obj = LibExtensions.Load<UserAccount>(encKey);
                if (obj.Id == currentUserId && input.IsActive == (int)ActiveStatus.TidakAktif)
                {
                    throw new ArgumentException("Anda tidak bisa menonaktifkan akun sendiri.");
                }
                obj.Username = input.Username ?? "";
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

            public static IEnumerable<RoleRow> GetRoles(string? encKey)
            {
                var userId = LibExtensions.Load<UserAccount>(encKey).Id;
                var periods = ManagementPeriod.GetDataList().ToDictionary(x => x.Id);
                return UserRole.GetDataList(userId: userId).Select(x => new RoleRow
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
