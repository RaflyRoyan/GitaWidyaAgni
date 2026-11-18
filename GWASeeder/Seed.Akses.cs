using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Menu;
using GWAShared.Enums;

namespace GWASeeder
{
    /// <summary>
    /// Katalog hak akses: controller + aksi, menu sidebar, dan role. Satu sumber untuk seeding penuh dan --sync-akses.
    /// Sync bersifat idempotent: controller / menu / role yang belum ada dibuat, yang ada diperbarui, dan daftar aksi,
    /// detail menu, aksi role serta menu role milik role katalog disamakan persis dengan katalog ini.
    /// </summary>
    internal static partial class Seed
    {
        // ---- Kosakata aksi web (nama ini dipakai controller di GitaWidyaAgniWeb) ----
        // Index  : buka halaman + semua data baca (daftar, detail, unduh file).
        // Create / Edit / Delete : tambah / ubah / hapus data utama maupun data anak di halaman itu.
        // Submit : ajukan dokumen ke alur persetujuan.     Approve : setujui / tolak / minta revisi.
        // Process: perubahan status operasional (buka/tutup, publikasi, checkout, pengembalian, dsb.).
        // Payment: terima / catat / batalkan pembayaran yang membuat transaksi keuangan.
        private const string SUBMIT = "Submit", APPROVE = "Approve", PROCESS = "Process", PAYMENT = "Payment";

        private record ControllerDef(string Name, string Description, string[] Extra, bool SelfService = false);
        private record MenuDef(string Controller, string Name, string Icon);
        private record HeaderDef(string Section, string Name, string Icon, string? Controller, MenuDef[] Subs);
        private record RoleDef(string Code, string Name, string Description, string[] Controllers);

        public static readonly Dictionary<string, int> Controllers = new();
        public static readonly Dictionary<string, Role> Roles = new();

        private static readonly ControllerDef[] ControllerCatalog =
        {
            // Area anggota (cukup login; controller memakai [LoginOnly]).
            new("ProfilSaya", "Profil saya", Array.Empty<string>(), true),
            new("PresensiSaya", "Presensi scan QR", Array.Empty<string>(), true),
            new("KegiatanSaya", "Kegiatan & tim pelaksana saya", Array.Empty<string>(), true),
            new("IuranSaya", "Iuran saya", Array.Empty<string>(), true),
            new("PeminjamanSaya", "Peminjaman alat saya", Array.Empty<string>(), true),
            new("Toko", "Toko merchandise & pesanan saya", Array.Empty<string>(), true),
            new("FormulirSaya", "Isi formulir & aspirasi", Array.Empty<string>(), true),
            new("Pemilihan", "E-voting (memilih)", Array.Empty<string>(), true),
            new("PrestasiSaya", "Prestasi, poin & sertifikat saya", Array.Empty<string>(), true),
            new("DisposisiSaya", "Disposisi untuk saya", Array.Empty<string>(), true),
            new("Notifikasi", "Notifikasi saya", Array.Empty<string>(), true),

            // Pengelolaan.
            new("Organisasi", "Profil organisasi", Array.Empty<string>()),
            new("Periode", "Periode & serah terima", new[] { SUBMIT, APPROVE }),
            new("Section", "Section musik & riwayat section anggota", Array.Empty<string>()),
            new("Divisi", "Divisi & anggota divisi", Array.Empty<string>()),
            new("Jabatan", "Jabatan pengurus", Array.Empty<string>()),
            new("Kepengurusan", "Penugasan pengurus", new[] { PROCESS }),
            new("Anggota", "Data anggota", Array.Empty<string>()),
            new("Alumni", "Profil alumni", Array.Empty<string>()),
            new("Rekrutmen", "Rekrutmen anggota", new[] { PROCESS }),
            new("ProgramKerja", "Program kerja, tugas, timeline", new[] { PROCESS }),
            new("Kegiatan", "Kegiatan, peserta, laporan", new[] { PROCESS, SUBMIT, APPROVE }),
            new("Lpj", "Laporan pertanggungjawaban kegiatan & kepengurusan", new[] { PROCESS }),
            new("TimPelaksana", "Tim pelaksana kegiatan", new[] { PROCESS }),
            new("Presensi", "Presensi & notulen", new[] { PROCESS }),
            new("Galeri", "Galeri kegiatan", new[] { PROCESS }),
            new("Transaksi", "Transaksi keuangan & saldo periode", new[] { PROCESS }),
            new("Anggaran", "Anggaran", new[] { SUBMIT, APPROVE }),
            new("PengajuanDana", "Pengajuan dana", new[] { SUBMIT, APPROVE, PROCESS }),
            new("Iuran", "Iuran anggota", new[] { PROCESS, PAYMENT }),
            new("Hutang", "Hutang organisasi", new[] { PROCESS, PAYMENT }),
            new("Aset", "Aset inventaris", new[] { PROCESS }),
            new("Peminjaman", "Peminjaman alat", new[] { APPROVE, PROCESS, PAYMENT }),
            new("Pengadaan", "Pengadaan barang", new[] { SUBMIT, APPROVE, PROCESS, PAYMENT }),
            new("Merchandise", "Danus & merchandise", new[] { PROCESS, PAYMENT }),
            new("Mitra", "Mitra & pihak luar", Array.Empty<string>()),
            new("Sponsorship", "Sponsorship", new[] { PROCESS, PAYMENT }),
            new("Surat", "Persuratan", new[] { SUBMIT, APPROVE, PROCESS }),
            new("Dokumen", "Arsip dokumen", Array.Empty<string>()),
            new("Prestasi", "Lomba, prestasi & poin", new[] { PROCESS }),
            new("Sertifikat", "Sertifikat", new[] { PROCESS }),
            new("Formulir", "Formulir & survei", new[] { PROCESS }),
            new("Aspirasi", "Aspirasi anggota", new[] { PROCESS }),
            new("EVoting", "Pemilihan", new[] { PROCESS }),
            new("Berita", "Berita & pengumuman", new[] { PROCESS }),
            new("Banner", "Banner landing page", Array.Empty<string>()),
            new("User", "User", new[] { PROCESS }),
            new("Role", "Role & hak akses", Array.Empty<string>()),
            new("Menu", "Menu & controller", Array.Empty<string>()),
            new("Pengaturan", "Pengaturan sistem", new[] { PROCESS })
        };

        private static readonly HeaderDef[] MenuCatalog =
        {
            new("Anggota", "Area Anggota", "ri-user-heart-line", null, new[]
            {
                new MenuDef("ProfilSaya", "Profil Saya", "ri-account-circle-line"), new MenuDef("PresensiSaya", "Presensi", "ri-qr-scan-line"),
                new MenuDef("KegiatanSaya", "Kegiatan Saya", "ri-calendar-check-line"), new MenuDef("IuranSaya", "Iuran Saya", "ri-wallet-line"),
                new MenuDef("PeminjamanSaya", "Peminjaman Alat", "ri-swap-box-line"), new MenuDef("Toko", "Toko Merchandise", "ri-store-2-line"),
                new MenuDef("FormulirSaya", "Formulir & Aspirasi", "ri-survey-line"), new MenuDef("Pemilihan", "E-Voting", "ri-checkbox-circle-line"),
                new MenuDef("PrestasiSaya", "Prestasi & Poin", "ri-medal-2-line"), new MenuDef("DisposisiSaya", "Disposisi", "ri-mail-check-line"),
                new MenuDef("Notifikasi", "Notifikasi", "ri-notification-3-line")
            }),
            new("Organisasi", "Organisasi", "ri-building-4-line", null, new[]
            {
                new MenuDef("Organisasi", "Profil Organisasi", "ri-information-line"), new MenuDef("Periode", "Periode", "ri-calendar-2-line"),
                new MenuDef("Section", "Section", "ri-music-2-line"), new MenuDef("Divisi", "Divisi", "ri-team-line"),
                new MenuDef("Jabatan", "Jabatan", "ri-medal-line"),
                new MenuDef("Kepengurusan", "Kepengurusan", "ri-user-star-line")
            }),
            new("Organisasi", "Anggota", "ri-group-line", null, new[]
            {
                new MenuDef("Anggota", "Data Anggota", "ri-user-line"), new MenuDef("Alumni", "Alumni", "ri-graduation-cap-line"),
                new MenuDef("Rekrutmen", "Rekrutmen", "ri-user-add-line")
            }),
            new("Program", "Program Kerja", "ri-briefcase-4-line", "ProgramKerja", Array.Empty<MenuDef>()),
            new("Program", "Kegiatan", "ri-calendar-event-line", null, new[]
            {
                new MenuDef("Kegiatan", "Daftar Kegiatan", "ri-calendar-line"), new MenuDef("TimPelaksana", "Tim Pelaksana", "ri-team-line"),
                new MenuDef("Presensi", "Presensi", "ri-qr-scan-2-line"), new MenuDef("Galeri", "Galeri", "ri-gallery-line")
            }),
            new("Program", "LPJ", "ri-file-chart-line", "Lpj", Array.Empty<MenuDef>()),
            new("Keuangan", "Keuangan", "ri-wallet-3-line", null, new[]
            {
                new MenuDef("Transaksi", "Transaksi", "ri-exchange-dollar-line"), new MenuDef("Anggaran", "Anggaran", "ri-file-list-3-line"),
                new MenuDef("PengajuanDana", "Pengajuan Dana", "ri-hand-coin-line"), new MenuDef("Iuran", "Iuran", "ri-money-dollar-circle-line"),
                new MenuDef("Hutang", "Hutang", "ri-bank-card-line")
            }),
            new("Operasional", "Inventaris", "ri-archive-line", null, new[]
            {
                new MenuDef("Aset", "Aset", "ri-music-2-line"), new MenuDef("Peminjaman", "Peminjaman", "ri-swap-box-line")
            }),
            new("Operasional", "Pengadaan", "ri-shopping-cart-2-line", "Pengadaan", Array.Empty<MenuDef>()),
            new("Operasional", "Merchandise", "ri-t-shirt-line", "Merchandise", Array.Empty<MenuDef>()),
            new("Operasional", "Kemitraan", "ri-handshake-line", null, new[]
            {
                new MenuDef("Mitra", "Mitra", "ri-building-line"), new MenuDef("Sponsorship", "Sponsorship", "ri-award-line")
            }),
            new("Operasional", "Persuratan", "ri-mail-line", null, new[]
            {
                new MenuDef("Surat", "Surat", "ri-mail-send-line"), new MenuDef("Dokumen", "Arsip Dokumen", "ri-folder-2-line")
            }),
            new("Lainnya", "Prestasi", "ri-trophy-line", null, new[]
            {
                new MenuDef("Prestasi", "Lomba & Prestasi", "ri-trophy-line"), new MenuDef("Sertifikat", "Sertifikat", "ri-file-paper-2-line")
            }),
            new("Lainnya", "Formulir", "ri-survey-line", null, new[]
            {
                new MenuDef("Formulir", "Formulir", "ri-survey-line"), new MenuDef("Aspirasi", "Aspirasi", "ri-chat-3-line")
            }),
            new("Lainnya", "E-Voting", "ri-checkbox-multiple-line", "EVoting", Array.Empty<MenuDef>()),
            new("Lainnya", "Informasi", "ri-megaphone-line", null, new[]
            {
                new MenuDef("Berita", "Berita", "ri-newspaper-line"), new MenuDef("Banner", "Banner", "ri-image-line")
            }),
            new("Pengaturan", "Akses", "ri-shield-user-line", null, new[]
            {
                new MenuDef("User", "User", "ri-user-settings-line"), new MenuDef("Role", "Role", "ri-key-2-line"),
                new MenuDef("Menu", "Menu", "ri-menu-line")
            }),
            new("Pengaturan", "Pengaturan Sistem", "ri-settings-3-line", "Pengaturan", Array.Empty<MenuDef>())
        };

        // Role mendapat semua aksi controller yang tercantum. Siapa yang boleh menyetujui dokumen tetap ditentukan alur persetujuan.
        // Semua role juga mendapat menu Area Anggota.
        private static readonly string[] KetuaControllers =
        {
            "Organisasi", "Periode", "Section", "Divisi", "Jabatan", "Kepengurusan", "Anggota", "Alumni", "Rekrutmen", "ProgramKerja", "Kegiatan", "Lpj",
            "TimPelaksana", "Presensi", "Galeri", "Transaksi", "Anggaran", "PengajuanDana", "Iuran", "Hutang", "Aset", "Peminjaman",
            "Pengadaan", "Merchandise", "Mitra", "Sponsorship", "Surat", "Dokumen", "Prestasi", "Sertifikat", "Formulir", "Aspirasi",
            "EVoting", "Berita", "Banner"
        };
        private static readonly RoleDef[] RoleCatalog =
        {
            new("KETUA", "Ketua Umum", "Pimpinan organisasi, akses semua modul operasional", KetuaControllers),
            new("WAKIL", "Wakil Ketua Umum", "Mendampingi ketua umum, akses semua modul operasional", KetuaControllers),
            new("SEKRETARIS", "Sekretaris", "Administrasi anggota, kegiatan, surat, dokumen", new[]
            {
                "Anggota", "Section", "Alumni", "Rekrutmen", "Kegiatan", "Lpj", "Presensi", "Surat", "Dokumen", "Sertifikat", "Formulir", "Aspirasi", "Berita"
            }),
            new("BENDAHARA", "Bendahara", "Pengelolaan keuangan, danus, sponsorship", new[]
            {
                "Transaksi", "Anggaran", "PengajuanDana", "Iuran", "Hutang", "Pengadaan", "Merchandise", "Sponsorship"
            }),
            new("KADIV", "Kepala Divisi", "Kepala divisi: program, kegiatan, presensi, alat", new[]
            {
                "ProgramKerja", "Kegiatan", "TimPelaksana", "Presensi", "Galeri", "Aset", "Peminjaman", "Prestasi", "Berita"
            }),
            new("SECTION_LEADER", "Section Leader", "Pemimpin section musik: anggota section, latihan sectional, presensi, alat", new[]
            {
                "Section", "Kegiatan", "Presensi", "Galeri", "Aset", "Peminjaman", "Prestasi"
            }),
            new("ANGGOTA", "Anggota", "Anggota biasa (hanya Area Anggota)", Array.Empty<string>())
        };

        // ---------------------------------------------------------------- Sync
        private static void SyncAccess()
        {
            // Controller + aksi
            var existingControllers = ControllerMenu.GetDataList(FilterAktif.Semua).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var def in ControllerCatalog)
            {
                if (!existingControllers.TryGetValue(def.Name, out var controller))
                {
                    controller = Ok(ControllerMenu.Save(new ControllerMenu.NewRow { Name = def.Name, IndexAction = "Index", Description = def.Description }, Admin), "Controller " + def.Name);
                }
                else if (controller.Description != def.Description || controller.IsActive != ActiveStatus.Aktif)
                {
                    controller.Description = def.Description;
                    controller.IsActive = ActiveStatus.Aktif;
                    controller = Ok(controller.Update(Admin), "Controller " + def.Name);
                }
                var actions = def.SelfService
                    ? new[] { new ControllerAction.NewRow { Action = "Index", NeedAuth = AuthRequirement.NonAuth, Description = def.Description } }
                    : new[] { "Index", "Create", "Edit", "Delete" }.Concat(def.Extra)
                        .Select(a => new ControllerAction.NewRow { Action = a, NeedAuth = AuthRequirement.Auth, Description = $"{a} {def.Description}" }).ToArray();
                Ok(controller.SaveControllerActions(actions.ToList(), Admin), "Aksi " + def.Name);
                Controllers[def.Name] = controller.Id;
            }

            // Sub menu (dicari per controller), header menu (dicari per nama), detail header.
            var subMenus = SubMenu.GetDataList(FilterAktif.Semua);
            var headers = HeaderMenu.GetDataList(FilterAktif.Semua).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var headerIds = new Dictionary<string, int>();
            var subMenuIds = new Dictionary<string, int>();
            var seq = 1;
            foreach (var def in MenuCatalog)
            {
                var controllerId = def.Controller == null ? (int?)null : Controllers[def.Controller];
                if (!headers.TryGetValue(def.Name, out var header))
                {
                    header = Ok(HeaderMenu.Save(new HeaderMenu.NewRow { Name = def.Name, Section = def.Section, ControllerId = controllerId, Icon = def.Icon, Seq = seq }, Admin), "Header menu " + def.Name);
                }
                else if (header.Section != def.Section || header.ControllerMenu?.Id != controllerId || header.Icon != def.Icon || header.Seq != seq || header.IsActive != ActiveStatus.Aktif)
                {
                    header.Section = def.Section;
                    header.ControllerMenu = controllerId == null ? null : existingOrNew(controllerId.Value);
                    header.Icon = def.Icon;
                    header.Seq = seq;
                    header.IsActive = ActiveStatus.Aktif;
                    header = Ok(header.Update(Admin), "Header menu " + def.Name);
                }
                seq++;
                headerIds[def.Name] = header.Id;
                if (def.Subs.Length == 0)
                {
                    if (header.GetHeaderMenuDetails(true).Count > 0)
                    {
                        Console.WriteLine($"     ! Header '{def.Name}' masih punya sub menu; biarkan atau hapus lewat halaman Menu.");
                    }
                    continue;
                }
                var details = new List<HeaderMenuDetail.NewRow>();
                var detailSeq = 1;
                foreach (var sub in def.Subs)
                {
                    var subMenu = subMenus.FirstOrDefault(x => x.ControllerMenu?.Id == Controllers[sub.Controller]);
                    if (subMenu == null)
                    {
                        subMenu = Ok(SubMenu.Save(new SubMenu.NewRow { Name = sub.Name, ControllerId = Controllers[sub.Controller], Icon = sub.Icon }, Admin), "Sub menu " + sub.Name);
                        subMenus.Add(subMenu);
                    }
                    else if (subMenu.Name != sub.Name || subMenu.Icon != sub.Icon || subMenu.IsActive != ActiveStatus.Aktif)
                    {
                        subMenu.Name = sub.Name;
                        subMenu.Icon = sub.Icon;
                        subMenu.IsActive = ActiveStatus.Aktif;
                        subMenu = Ok(subMenu.Update(Admin), "Sub menu " + sub.Name);
                    }
                    subMenuIds[sub.Controller] = subMenu.Id;
                    details.Add(new HeaderMenuDetail.NewRow { HeaderMenuId = header.Id, SubMenuId = subMenu.Id, Seq = detailSeq++ });
                }
                Ok(header.SaveHeaderMenuDetails(details, Admin), "Detail menu " + def.Name);
            }

            // Role: aksi = semua aksi controller yang diizinkan; menu = header langsung / sub menu yang controllernya diizinkan + Area Anggota.
            var roles = Role.GetDataList(FilterAktif.Semua).ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
            var selfService = ControllerCatalog.Where(x => x.SelfService).Select(x => x.Name).ToArray();
            foreach (var def in RoleCatalog)
            {
                if (!roles.TryGetValue(def.Code, out var role))
                {
                    role = Ok(Role.Save(new Role.NewRow { Code = def.Code, Name = def.Name, Description = def.Description }, Admin), "Role " + def.Code);
                }
                else if (role.Name != def.Name || role.Description != def.Description || role.IsActive != ActiveStatus.Aktif)
                {
                    role.Name = def.Name;
                    role.Description = def.Description;
                    role.IsActive = ActiveStatus.Aktif;
                    role = Ok(role.Update(Admin), "Role " + def.Code);
                }
                Roles[def.Code] = role;

                var allowed = def.Controllers.Concat(selfService).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var actionIds = allowed.SelectMany(c => ControllerAction.GetDataList(FilterAktif.Aktif, Controllers[c]).Select(a => a.Id)).ToList();
                Ok(role.SaveRoleActions(actionIds, Admin), "Aksi role " + def.Code);
                var rows = new List<RoleMenu.NewRow>();
                foreach (var item in MenuCatalog)
                {
                    if (item.Controller != null)
                    {
                        if (allowed.Contains(item.Controller))
                        {
                            rows.Add(new RoleMenu.NewRow { RoleId = role.Id, HeaderMenuId = headerIds[item.Name] });
                        }
                        continue;
                    }
                    foreach (var sub in item.Subs.Where(s => allowed.Contains(s.Controller)))
                    {
                        rows.Add(new RoleMenu.NewRow { RoleId = role.Id, HeaderMenuId = headerIds[item.Name], SubMenuId = subMenuIds[sub.Controller] });
                    }
                }
                Ok(role.SaveRoleMenus(rows, Admin), "Menu role " + def.Code);
            }
            var others = roles.Keys.Where(code => code != UserAccess.SUPERADMIN && RoleCatalog.All(r => !string.Equals(r.Code, code, StringComparison.OrdinalIgnoreCase))).ToList();
            if (others.Count > 0)
            {
                Console.WriteLine("     ! Role di luar katalog dibiarkan apa adanya: " + string.Join(", ", others));
            }

            static ControllerMenu existingOrNew(int id) => new() { Id = id };
        }
    }
}
