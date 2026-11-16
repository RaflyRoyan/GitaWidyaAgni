using GWALib.General;
using GWAShared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Akses
{
    /// <summary>
    /// Hak akses user untuk aplikasi web (bukan tabel sendiri). Role berlaku bila role aktif dan m_user_role tanpa periode atau
    /// periodenya Active. Role berkode SUPERADMIN melihat semua menu aktif dan boleh semua aksi.
    /// - GetAllowedActions: pasangan "controller/action" (huruf kecil) dari m_role_action, hanya aksi dan controller aktif.
    /// - GetRegisteredActions: aksi aktif di m_controller_action beserta flag need_auth.
    /// - GetMenu: header menu aktif urut Seq. Header dengan controller = link langsung, tampil bila role punya baris m_role_menu
    ///   untuk header itu. Header tanpa controller = grup; sub menu tampil bila ada baris m_role_menu (header, sub menu), dan grup
    ///   tanpa sub menu yang terlihat disembunyikan.
    /// </summary>
    public static class UserAccess
    {
        public const string SUPERADMIN = "SUPERADMIN";

        public record MenuItem(string Name, string? Icon, string? Controller, string? Action, string? Section, List<MenuItem> Children);

        // Nama tampilan untuk header: nama anggota yang terhubung ke user (bila ada), else username; RoleNames = role aktif.
        public record UserProfile(int UserId, string Username, string DisplayName, List<string> RoleNames, int? MemberId);

        public static UserProfile? GetProfile(int userId)
        {
            using var context = DBClass.GetContext();
            var user = context.MUsers.Where(x => x.Id == userId).Select(x => new { x.Id, x.Username }).SingleOrDefault();
            if (user == null)
            {
                return null;
            }
            var member = context.MMembers.Where(x => x.UserId == userId).Select(x => new { x.Id, x.Fullname }).FirstOrDefault();
            var roles = ActiveRoles(context, userId).Select(x => x.Name).Distinct().ToList();
            return new UserProfile(user.Id, user.Username, member?.Fullname ?? user.Username, roles, member?.Id);
        }

        public static List<string> GetRoleCodes(int userId)
        {
            using var context = DBClass.GetContext();
            return ActiveRoles(context, userId).Select(x => x.Code).Distinct().ToList();
        }
        // null = semua aksi (SUPERADMIN).
        public static HashSet<string>? GetAllowedActions(int userId)
        {
            using var context = DBClass.GetContext();
            var roles = ActiveRoles(context, userId).Select(x => new { x.Id, x.Code }).ToList();
            if (roles.Any(x => x.Code == SUPERADMIN))
            {
                return null;
            }
            var roleIds = roles.Select(x => x.Id).ToList();
            return context.MRoleActions
                .Where(x => roleIds.Contains(x.RoleId) && x.ControllerAction.IsActive == (int)ActiveStatus.Aktif
                    && x.ControllerAction.Controller.IsActive == (int)ActiveStatus.Aktif)
                .Select(x => x.ControllerAction.Controller.Name + "/" + x.ControllerAction.Action)
                .AsEnumerable()
                .Select(x => x.ToLowerInvariant())
                .ToHashSet();
        }
        // Aksi aktif yang terdaftar ("controller/action", huruf kecil) → true bila need_auth = 1 (butuh m_role_action).
        public static Dictionary<string, bool> GetRegisteredActions()
        {
            using var context = DBClass.GetContext();
            return context.MControllerActions
                .Where(x => x.IsActive == (int)ActiveStatus.Aktif && x.Controller.IsActive == (int)ActiveStatus.Aktif)
                .Select(x => new { Key = x.Controller.Name + "/" + x.Action, x.NeedAuth })
                .AsEnumerable()
                .GroupBy(x => x.Key.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.Any(x => x.NeedAuth == 1));
        }
        public static List<MenuItem> GetMenu(int userId)
        {
            using var context = DBClass.GetContext();
            var roles = ActiveRoles(context, userId).Select(x => new { x.Id, x.Code }).ToList();
            var all = roles.Any(x => x.Code == SUPERADMIN);
            var roleIds = roles.Select(x => x.Id).ToList();
            var granted = context.MRoleMenus.Where(x => roleIds.Contains(x.RoleId))
                .Select(x => new { x.HeaderMenuId, x.SubMenuId }).ToList();

            var headers = context.MHeaderMenus.AsNoTracking()
                .Where(x => x.IsActive == (int)ActiveStatus.Aktif)
                .Include(x => x.Controller)
                .Include(x => x.MHeaderMenuDetails).ThenInclude(x => x.SubMenu).ThenInclude(x => x.Controller)
                .OrderBy(x => x.Seq).ThenBy(x => x.Id)
                .ToList();

            var result = new List<MenuItem>();
            foreach (var header in headers)
            {
                if (header.Controller != null)
                {
                    if (all || granted.Any(x => x.HeaderMenuId == header.Id))
                    {
                        result.Add(new MenuItem(header.Name, header.Icon, header.Controller.Name, header.Controller.IndexAction, header.Section, new()));
                    }
                    continue;
                }
                var children = header.MHeaderMenuDetails
                    .Where(d => d.SubMenu.IsActive == (int)ActiveStatus.Aktif && d.SubMenu.Controller != null
                        && (all || granted.Any(g => g.HeaderMenuId == header.Id && g.SubMenuId == d.SubMenuId)))
                    .OrderBy(d => d.Seq).ThenBy(d => d.Id)
                    .Select(d => new MenuItem(d.SubMenu.Name, d.SubMenu.Icon, d.SubMenu.Controller!.Name, d.SubMenu.Controller.IndexAction, null, new()))
                    .ToList();
                if (children.Count > 0)
                {
                    result.Add(new MenuItem(header.Name, header.Icon, null, null, header.Section, children));
                }
            }
            return result;
        }
        private static IQueryable<EF.MRole> ActiveRoles(EF.ModelContext context, int userId)
        {
            return context.MUserRoles
                .Where(x => x.UserId == userId && x.Role.IsActive == (int)ActiveStatus.Aktif
                    && (x.PeriodId == null || x.Period!.Status == (int)ManagementPeriodStatus.Active))
                .Select(x => x.Role);
        }
    }
}
