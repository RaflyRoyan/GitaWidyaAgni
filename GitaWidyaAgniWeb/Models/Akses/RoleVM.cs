using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Menu;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Akses
{
    /// <summary>Akses › Role: data role dan hak aksesnya (aksi controller + menu sidebar).</summary>
    public class RoleVM
    {
        public class Form
        {
            public bool IsEdit { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
        public class Add : Form
        {
        }
        public class Edit : Form
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Role>(encKey, "Role");
                IsEdit = true;
                Code = obj.Code;
                Name = obj.Name;
                Description = obj.Description;
                IsActive = (int)obj.IsActive;
            }
        }

        // Node TreeList hak akses. Id string unik (c12 / a34, h3 / h3s7); daun membawa ActionId atau MenuValue.
        public class Node
        {
            public string Id { get; set; } = "";
            public string? ParentId { get; set; }
            public string Nama { get; set; } = "";
            public string? Keterangan { get; set; }
            public string? Ikon { get; set; }
            public int? ActionId { get; set; }
            // "headerId" untuk header langsung, "headerId:subMenuId" untuk sub menu.
            public string? MenuValue { get; set; }
            public bool Checked { get; set; }
        }
        public class Akses
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public List<Node> ActionNodes { get; set; } = new();
            public List<Node> MenuNodes { get; set; } = new();

            public Akses(string encKey)
            {
                var role = LibExtensions.Load<Role>(encKey, "Role");
                if (role.Code == UserAccess.SUPERADMIN)
                {
                    throw new ArgumentException("SUPERADMIN selalu boleh semua aksi; hak aksesnya tidak perlu diatur.");
                }
                Key = role.EncDecrKey;
                Nama = role.Name;
                var actions = role.GetRoleActions(true).Select(x => x.ControllerAction.Id).ToHashSet();
                var menus = role.GetRoleMenus(true).Select(x => (x.HeaderMenu.Id, SubId: x.SubMenu?.Id)).ToHashSet();

                foreach (var g in ControllerAction.GetDataList(FilterAktif.Aktif).GroupBy(x => x.ControllerMenu.Id).OrderBy(g => g.First().ControllerMenu.Name))
                {
                    var ctl = g.First().ControllerMenu;
                    ActionNodes.Add(new Node { Id = "c" + ctl.Id, Nama = ctl.Name, Keterangan = ctl.Description });
                    ActionNodes.AddRange(g.OrderBy(a => Method.Urutan(a.Action)).ThenBy(a => a.Action).Select(a => new Node
                    {
                        Id = "a" + a.Id,
                        ParentId = "c" + ctl.Id,
                        Nama = a.Action,
                        Keterangan = (a.Description ?? "") + (a.NeedAuth == AuthRequirement.NonAuth ? " (cukup login)" : ""),
                        ActionId = a.Id,
                        Checked = actions.Contains(a.Id)
                    }));
                }
                foreach (var h in HeaderMenu.GetDataList(FilterAktif.Aktif).OrderBy(x => x.Seq))
                {
                    var direct = h.ControllerMenu != null;
                    MenuNodes.Add(new Node
                    {
                        Id = "h" + h.Id,
                        Nama = h.Name,
                        Keterangan = direct ? "Halaman langsung" : h.Section,
                        Ikon = h.Icon,
                        MenuValue = direct ? h.Id.ToString() : null,
                        Checked = direct && menus.Contains((h.Id, null))
                    });
                    if (direct)
                    {
                        continue;
                    }
                    MenuNodes.AddRange(h.GetHeaderMenuDetails(true).OrderBy(d => d.Seq).Select(d => new Node
                    {
                        Id = $"h{h.Id}s{d.SubMenu.Id}",
                        ParentId = "h" + h.Id,
                        Nama = d.SubMenu.Name,
                        Keterangan = d.SubMenu.ControllerMenu?.Name,
                        Ikon = d.SubMenu.Icon,
                        MenuValue = $"{h.Id}:{d.SubMenu.Id}",
                        Checked = menus.Contains((h.Id, d.SubMenu.Id))
                    }));
                }
            }
        }
        public class AksesInput
        {
            public string Key { get; set; } = "";
            public List<int> ActionIds { get; set; } = new();
            public List<string> Menus { get; set; } = new();
        }

        public class ViewModel
        {
            public class Row
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
                public string Code { get; set; } = "";
                public string Name { get; set; } = "";
                public string? Description { get; set; }
                public int JumlahUser { get; set; }
                public int IsActive { get; set; }
            }
        }

        public class Method
        {
            public static IEnumerable<ViewModel.Row> GetDataList()
            {
                var users = UserRole.GetDataList().GroupBy(x => x.Role.Id).ToDictionary(g => g.Key, g => g.Select(x => x.User.Id).Distinct().Count());
                return Role.GetDataList(FilterAktif.Semua).Select(x => new ViewModel.Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    JumlahUser = users.TryGetValue(x.Id, out var n) ? n : 0,
                    IsActive = (int)x.IsActive
                });
            }

            public static void Simpan(Form input, string user)
            {
                Role.Save(new Role.NewRow { Code = input.Code, Name = input.Name, Description = input.Description }, user).Ok();
            }

            public static void Update(string? encKey, Form input, string user)
            {
                var obj = LibExtensions.Load<Role>(encKey);
                if (obj.Code == UserAccess.SUPERADMIN && (input.IsActive != (int)ActiveStatus.Aktif || !string.Equals(input.Code, UserAccess.SUPERADMIN, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Kode dan status role SUPERADMIN tidak boleh diubah.");
                }
                obj.Code = input.Code;
                obj.Name = input.Name;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void Nonaktifkan(string? encKey, string user)
            {
                var obj = LibExtensions.Load<Role>(encKey);
                if (obj.Code == UserAccess.SUPERADMIN)
                {
                    throw new ArgumentException("Role SUPERADMIN tidak boleh dinonaktifkan.");
                }
                obj.Delete(user).Ok();
            }

            public static void SimpanAkses(AksesInput input, string user)
            {
                var role = LibExtensions.Load<Role>(input.Key);
                role.SaveRoleActions(input.ActionIds ?? new(), user).Ok();
                var rows = new List<RoleMenu.NewRow>();
                foreach (var item in input.Menus ?? new())
                {
                    var parts = item.Split(':');
                    if (int.TryParse(parts[0], out var headerId))
                    {
                        rows.Add(new RoleMenu.NewRow
                        {
                            RoleId = role.Id,
                            HeaderMenuId = headerId,
                            SubMenuId = parts.Length > 1 && int.TryParse(parts[1], out var subId) ? subId : null
                        });
                    }
                }
                role.SaveRoleMenus(rows, user).Ok();
            }

            internal static int Urutan(string action) => action switch
            {
                "Index" => 0, "Create" => 1, "Edit" => 2, "Delete" => 3, "Submit" => 4, "Approve" => 5, "Process" => 6, "Payment" => 7, _ => 9
            };
        }
    }
}
