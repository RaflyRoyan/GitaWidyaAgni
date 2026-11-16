using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Menu;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Akses
{
    /// <summary>
    /// Akses › Menu: controller + aksinya (m_controller / m_controller_action), header menu + sub menu sidebar.
    /// Tiga tab, masing-masing grid _List&lt;X&gt; + modal _Add&lt;X&gt; / _Edit&lt;X&gt; (form bersama _Form&lt;X&gt;).
    /// </summary>
    public class MenuVM
    {
        public static List<GwaVM.OpsiItem> ControllerOpsi()
        {
            return ControllerMenu.GetDataList(FilterAktif.Aktif).OrderBy(x => x.Name).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
        }

        public class Index
        {
        }

        // ================================================================ Controller
        public class ListController
        {
            public List<ControllerRow> Rows { get; set; } = Method.GetControllers().ToList();
        }
        public class ControllerRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string IndexAction { get; set; } = "";
            public string? Description { get; set; }
            public string Aksi { get; set; } = "";
            public int JumlahAksi { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddController
        {
            public NewController Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }
        public class EditController : AddController
        {
            public string Aksi { get; set; } = "";

            public EditController(string encKey)
            {
                var obj = LibExtensions.Load<ControllerMenu>(encKey, "Controller");
                Row = new NewController { Name = obj.Name, IndexAction = obj.IndexAction, Description = obj.Description, IsActive = (int)obj.IsActive };
                Aksi = string.Join(", ", ControllerAction.GetDataList(FilterAktif.Aktif, obj.Id).OrderBy(x => RoleVM.Method.Urutan(x.Action)).Select(x => x.Action));
            }
        }
        /// <summary>Form controller. Status hanya di Edit.</summary>
        public class NewController
        {
            [Required(ErrorMessage = "Nama controller harus diisi")]
            [StringLength(100, ErrorMessage = "Nama controller maksimal 100 karakter")]
            [Display(Name = "Nama controller")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Aksi halaman utama harus diisi")]
            [StringLength(100, ErrorMessage = "Aksi halaman utama maksimal 100 karakter")]
            [Display(Name = "Aksi halaman utama")]
            public string? IndexAction { get; set; } = "Index";

            [StringLength(200, ErrorMessage = "Keterangan maksimal 200 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // Modal _Aksi: daftar aksi controller (diedit di grid lalu disimpan sekaligus).
        public class AksiRow
        {
            public string Action { get; set; } = "";
            public int NeedAuth { get; set; } = (int)AuthRequirement.Auth;
            public string? Description { get; set; }
        }
        public class Aksi
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public List<AksiRow> Rows { get; set; } = new();
            public List<GwaVM.OpsiItem> AuthOpsi { get; set; } = GwaVM.Opsi<AuthRequirement>();

            public Aksi(string encKey)
            {
                var obj = LibExtensions.Load<ControllerMenu>(encKey, "Controller");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Rows = ControllerAction.GetDataList(FilterAktif.Aktif, obj.Id).OrderBy(x => RoleVM.Method.Urutan(x.Action)).ThenBy(x => x.Action)
                    .Select(x => new AksiRow { Action = x.Action, NeedAuth = (int)x.NeedAuth, Description = x.Description }).ToList();
            }
        }
        public class AksiInput
        {
            public string Key { get; set; } = "";
            public List<AksiRow> Rows { get; set; } = new();
        }

        // ================================================================ Header menu
        public class ListHeader
        {
            public List<HeaderRow> Rows { get; set; } = Method.GetHeaders().ToList();
        }
        public class HeaderRow
        {
            public string EncDecrKey { get; set; } = "";
            public int Seq { get; set; }
            public string Name { get; set; } = "";
            public string? Section { get; set; }
            public string? Icon { get; set; }
            public string? Controller { get; set; }
            public string Isi { get; set; } = "";
            public bool Aktif { get; set; }
        }

        public class AddHeader
        {
            public NewHeader Row { get; set; } = new();
            public List<GwaVM.OpsiItem> ControllerList { get; set; } = ControllerOpsi();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;

            public AddHeader()
            {
                Row.Seq = HeaderMenu.GetDataList(FilterAktif.Semua).Select(x => x.Seq).DefaultIfEmpty(0).Max() + 1;
            }
        }
        public class EditHeader : AddHeader
        {
            public EditHeader(string encKey)
            {
                var obj = LibExtensions.Load<HeaderMenu>(encKey, "Header menu");
                Row = new NewHeader
                {
                    Name = obj.Name, Section = obj.Section, ControllerId = obj.ControllerMenu?.Id, Icon = obj.Icon,
                    Description = obj.Description, Seq = obj.Seq, IsActive = (int)obj.IsActive
                };
            }
        }
        /// <summary>Form header menu. Dengan controller = link langsung; tanpa controller = grup sub menu.</summary>
        public class NewHeader
        {
            [Required(ErrorMessage = "Nama harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama")]
            public string? Name { get; set; }

            [StringLength(100, ErrorMessage = "Judul bagian maksimal 100 karakter")]
            [Display(Name = "Judul bagian sidebar")]
            public string? Section { get; set; }

            [Display(Name = "Halaman langsung (controller)")]
            public int? ControllerId { get; set; }

            [StringLength(100, ErrorMessage = "Ikon maksimal 100 karakter")]
            [Display(Name = "Ikon (Remix Icon)")]
            public string? Icon { get; set; }

            [StringLength(200, ErrorMessage = "Keterangan maksimal 200 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Range(0, 9999, ErrorMessage = "Urutan 0 sampai 9999")]
            [Display(Name = "Urutan")]
            public int Seq { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // Modal _Detail: sub menu dalam grup (centang + urutan seret).
        public class DetailItem
        {
            public int Id { get; set; }
            public string Nama { get; set; } = "";
            public string? Ikon { get; set; }
            public string? Controller { get; set; }
            public bool Dipilih { get; set; }
        }
        public class Detail
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public List<DetailItem> Items { get; set; } = new();

            public Detail(string encKey)
            {
                var header = LibExtensions.Load<HeaderMenu>(encKey, "Header menu");
                if (header.ControllerMenu != null)
                {
                    throw new ArgumentException("Header ini menuju halaman langsung; kosongkan controllernya dulu bila ingin dijadikan grup.");
                }
                Key = header.EncDecrKey;
                Nama = header.Name;
                var chosen = header.GetHeaderMenuDetails(true).OrderBy(x => x.Seq).Select(x => x.SubMenu.Id).ToList();
                var subs = SubMenu.GetDataList(FilterAktif.Aktif).ToDictionary(x => x.Id);
                Items = chosen.Where(subs.ContainsKey).Select(id => subs[id])
                    .Concat(subs.Values.Where(x => !chosen.Contains(x.Id)).OrderBy(x => x.Name))
                    .Select(x => new DetailItem { Id = x.Id, Nama = x.Name, Ikon = x.Icon, Controller = x.ControllerMenu?.Name, Dipilih = chosen.Contains(x.Id) })
                    .ToList();
            }
        }
        public class DetailInput
        {
            public string Key { get; set; } = "";
            public List<int> SubMenuIds { get; set; } = new();
        }

        // ================================================================ Sub menu
        public class ListSub
        {
            public List<SubRow> Rows { get; set; } = Method.GetSubs().ToList();
        }
        public class SubRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Controller { get; set; }
            public string? Icon { get; set; }
            public string? Header { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddSub
        {
            public NewSub Row { get; set; } = new();
            public List<GwaVM.OpsiItem> ControllerList { get; set; } = ControllerOpsi();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }
        public class EditSub : AddSub
        {
            public EditSub(string encKey)
            {
                var obj = LibExtensions.Load<SubMenu>(encKey, "Sub menu");
                Row = new NewSub { Name = obj.Name, ControllerId = obj.ControllerMenu?.Id, Icon = obj.Icon, Description = obj.Description, IsActive = (int)obj.IsActive };
            }
        }
        /// <summary>Form sub menu. Status hanya di Edit.</summary>
        public class NewSub
        {
            [Required(ErrorMessage = "Nama harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Pilih controller")]
            [Display(Name = "Controller")]
            public int? ControllerId { get; set; }

            [StringLength(100, ErrorMessage = "Ikon maksimal 100 karakter")]
            [Display(Name = "Ikon (Remix Icon)")]
            public string? Icon { get; set; }

            [StringLength(200, ErrorMessage = "Keterangan maksimal 200 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        public class Method
        {
            // ---------- Controller
            public static IEnumerable<ControllerRow> GetControllers()
            {
                var actions = ControllerAction.GetDataList(FilterAktif.Aktif).GroupBy(x => x.ControllerMenu.Id)
                    .ToDictionary(g => g.Key, g => g.OrderBy(a => RoleVM.Method.Urutan(a.Action)).Select(a => a.Action).ToList());
                return ControllerMenu.GetDataList(FilterAktif.Semua).Select(x => new ControllerRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    IndexAction = x.IndexAction,
                    Description = x.Description,
                    Aksi = actions.TryGetValue(x.Id, out var a) ? string.Join(", ", a) : "",
                    JumlahAksi = actions.TryGetValue(x.Id, out var a2) ? a2.Count : 0,
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.Name);
            }
            public static void SimpanController(NewController input, string user)
            {
                var row = ControllerMenu.Save(new ControllerMenu.NewRow { Name = input.Name ?? "", IndexAction = input.IndexAction ?? "Index", Description = input.Description }, user).Ok();
                // Controller baru langsung diberi aksi halaman utama supaya terdaftar.
                row.SaveControllerActions(new List<ControllerAction.NewRow>
                {
                    new() { Action = row.IndexAction, NeedAuth = AuthRequirement.Auth, Description = "Buka halaman " + row.Name }
                }, user).Ok();
            }
            public static void UpdateController(string encKey, NewController input, string user)
            {
                var obj = LibExtensions.Load<ControllerMenu>(encKey);
                obj.Name = input.Name ?? "";
                obj.IndexAction = input.IndexAction ?? "Index";
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusController(string? encKey, string user) => LibExtensions.Load<ControllerMenu>(encKey).Delete(user).Ok();

            // Daftar aksi lengkap: aksi yang tidak dikirim dinonaktifkan beserta haknya.
            public static void SimpanAksi(AksiInput input, string user)
            {
                var rows = (input.Rows ?? new()).Where(x => !string.IsNullOrWhiteSpace(x.Action))
                    .Select(x => new ControllerAction.NewRow { Action = x.Action.Trim(), NeedAuth = (AuthRequirement)x.NeedAuth, Description = x.Description }).ToList();
                if (rows.Count == 0)
                {
                    throw new ArgumentException("Minimal satu aksi (aksi halaman utama wajib ada).");
                }
                LibExtensions.Load<ControllerMenu>(input.Key).SaveControllerActions(rows, user).Ok();
            }

            // ---------- Header
            public static IEnumerable<HeaderRow> GetHeaders()
            {
                return HeaderMenu.GetDataList(FilterAktif.Semua).OrderBy(x => x.Seq).Select(x => new HeaderRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Seq = x.Seq,
                    Name = x.Name,
                    Section = x.Section,
                    Icon = x.Icon,
                    Controller = x.ControllerMenu?.Name,
                    Isi = x.ControllerMenu != null ? "Halaman " + x.ControllerMenu.Name
                        : string.Join(", ", x.GetHeaderMenuDetails(true).OrderBy(d => d.Seq).Select(d => d.SubMenu.Name)),
                    Aktif = x.IsActive == ActiveStatus.Aktif
                });
            }
            public static void SimpanHeader(NewHeader input, string user)
            {
                HeaderMenu.Save(new HeaderMenu.NewRow
                {
                    Name = input.Name ?? "", Section = input.Section, ControllerId = input.ControllerId > 0 ? input.ControllerId : null,
                    Icon = input.Icon, Description = input.Description, Seq = input.Seq
                }, user).Ok();
            }
            public static void UpdateHeader(string encKey, NewHeader input, string user)
            {
                var obj = LibExtensions.Load<HeaderMenu>(encKey);
                obj.Name = input.Name ?? "";
                obj.Section = input.Section;
                obj.ControllerMenu = input.ControllerId > 0 ? new ControllerMenu { Id = input.ControllerId.Value } : null;
                obj.Icon = input.Icon;
                obj.Description = input.Description;
                obj.Seq = input.Seq;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusHeader(string? encKey, string user) => LibExtensions.Load<HeaderMenu>(encKey).Delete(user).Ok();

            public static void SimpanDetail(DetailInput input, string user)
            {
                var header = LibExtensions.Load<HeaderMenu>(input.Key);
                var seq = 1;
                var rows = (input.SubMenuIds ?? new()).Distinct()
                    .Select(id => new HeaderMenuDetail.NewRow { HeaderMenuId = header.Id, SubMenuId = id, Seq = seq++ }).ToList();
                header.SaveHeaderMenuDetails(rows, user).Ok();
            }

            // ---------- Sub menu
            public static IEnumerable<SubRow> GetSubs()
            {
                var headers = HeaderMenu.GetDataList(FilterAktif.Aktif).Where(x => x.ControllerMenu == null)
                    .SelectMany(h => h.GetHeaderMenuDetails(true).Select(d => (d.SubMenu.Id, h.Name)))
                    .GroupBy(x => x.Id).ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.Name)));
                return SubMenu.GetDataList(FilterAktif.Semua).Select(x => new SubRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Controller = x.ControllerMenu?.Name,
                    Icon = x.Icon,
                    Header = headers.TryGetValue(x.Id, out var h) ? h : null,
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.Name);
            }
            public static void SimpanSub(NewSub input, string user)
            {
                SubMenu.Save(new SubMenu.NewRow { Name = input.Name ?? "", ControllerId = input.ControllerId, Icon = input.Icon, Description = input.Description }, user).Ok();
            }
            public static void UpdateSub(string encKey, NewSub input, string user)
            {
                var obj = LibExtensions.Load<SubMenu>(encKey);
                obj.Name = input.Name ?? "";
                obj.ControllerMenu = input.ControllerId > 0 ? new ControllerMenu { Id = input.ControllerId.Value } : null;
                obj.Icon = input.Icon;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusSub(string? encKey, string user) => LibExtensions.Load<SubMenu>(encKey).Delete(user).Ok();
        }
    }
}
