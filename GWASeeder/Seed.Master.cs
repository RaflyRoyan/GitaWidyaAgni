using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Approval;
using GWALib.Lib.Master.Information;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Master.Keuangan;
using GWALib.Lib.Master.Menu;
using GWALib.Lib.Master.Merchandise;
using GWALib.Lib.Master.Mitra;
using GWALib.Lib.Master.Organization;
using GWALib.Lib.Master.Prestasi;
using GWALib.Lib.Master.Sponsorship;
using GWALib.Lib.Master.Surat;
using GWALib.Lib.Master.TimPelaksana;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;
using SectionMaster = GWALib.Lib.Master.Anggota.Section;

namespace GWASeeder
{
    internal static partial class Seed
    {
        // ---- State yang dipakai bagian berikutnya ----
        public static ManagementPeriod PeriodOld = null!;
        public static ManagementPeriod PeriodActive = null!;
        public static readonly Dictionary<string, int> Divisions = new();
        public static readonly Dictionary<string, int> MusicSections = new(); // section musik: semua anggota punya section
        public static readonly Dictionary<string, int> Positions = new();
        public static readonly List<Member> Members = new();
        public static readonly Dictionary<int, int> MemberUser = new(); // memberId → userId
        public static Member Ketua = null!, Wakil = null!, Sekretaris = null!, Bendahara = null!, KetuaLama = null!;
        public static readonly Dictionary<string, int> Accounts = new();
        public static readonly Dictionary<string, int> Categories = new();
        public static DuesSetting Dues = null!;
        public static readonly Dictionary<string, ExternalParty> Parties = new();
        public static readonly Dictionary<string, SponsorshipPackage> Packages = new();
        public static readonly Dictionary<string, Product> Products = new();
        public static readonly Dictionary<string, int> AssetCategories = new();
        public static readonly Dictionary<string, Asset> Assets = new();
        public static readonly Dictionary<string, int> DocCategories = new();
        public static readonly Dictionary<string, LetterTemplate> LetterTemplates = new();
        public static ChecklistTemplate EventChecklist = null!;

        public static string UserOf(Member member) => MemberUser[member.Id].ToString();

        // ---------------------------------------------------------------- Organisasi
        private static void SeedOrganization()
        {
            var profile = Ok(OrganizationProfile.Save(new OrganizationProfile.NewRow
            {
                Code = "GWA",
                Name = "Marching Band Gita Widya Agni",
                Description = "Unit Kegiatan Mahasiswa marching band Universitas Pembangunan Nasional \"Veteran\" Jawa Timur. Berdiri sejak 20 Juli 1990 dan memakai nama Gita Widya Agni sejak 1995, yang berarti semangat yang berapi-api dalam belajar musik.",
                Vision = "Menjadi unit marching band mahasiswa yang berprestasi di tingkat nasional dan membawa nama baik UPN \"Veteran\" Jawa Timur sebagai Kampus Bela Negara.",
                Mission = "Menyelenggarakan latihan rutin brass, perkusi, dan colour guard yang terarah\nMengikuti kejuaraan marching band tingkat regional dan nasional\nMembangun kekeluargaan, disiplin, dan kepemimpinan anggota\nMendukung kegiatan resmi universitas dan masyarakat",
                Email = "mbgwa.upnjatim@gmail.com",
                Phone = "081553858791",
                Instagram = "mbgwaupn",
                FoundedDate = D(1990, 7, 20),
                AdvisorName = "Dr. Ir. Bambang Sutejo, M.T.",
                AdvisorPhone = "081234560001",
                AdvisorEmail = "pembina.mbgwa@upnjatim.ac.id",
                AdvisorSkNumber = "SK/123/UN63/KM/2026"
            }, Admin), "Profil organisasi");
            Ok(profile.UpdateLogo(Image("assets/images/gwa-logo.png"), Admin), "Logo organisasi");
            Ok(profile.UpdateAdvisorSkFile(Pdf("SK Pembina UKM Marching Band Gita Widya Agni 2026"), "sk-pembina-2026.pdf", Admin), "SK pembina");
            for (var i = 0; i < 4; i++)
            {
                Ok(OrganizationPhoto.Save(new OrganizationPhoto.NewRow { Data = Photo(i) }, Admin), "Foto organisasi");
            }

            var settings = new (string Group, string Key, string Value, string Description)[]
            {
                ("LANDING", "hero_title", "Marching Band Gita Widya Agni", "Judul hero landing page"),
                ("LANDING", "hero_subtitle", "UKM Marching Band UPN \"Veteran\" Jawa Timur", "Subjudul hero"),
                ("KONTAK", "alamat", "Gedung Serba Guna Giri Loka, Pintu Timur Lt. 1, UPN \"Veteran\" Jawa Timur, Jl. Raya Rungkut Madya, Gunung Anyar, Surabaya 60294", "Alamat sekretariat"),
                ("KONTAK", "whatsapp", "6281553858791", "WhatsApp humas"),
                ("SOSMED", "instagram", "https://instagram.com/mbgwaupn", "Instagram"),
                ("SOSMED", "youtube", "https://youtube.com/@mbgwaupn", "YouTube"),
                ("SOSMED", "tiktok", "https://www.tiktok.com/@mbgwaupnvjt", "TikTok")
            };
            foreach (var s in settings)
            {
                Ok(SiteSetting.Save(new SiteSetting.NewRow { SettingGroup = s.Group, SettingKey = s.Key, SettingValue = s.Value, Description = s.Description }, Admin), "Setting " + s.Key);
            }
            Ok(SiteSetting.Save(new SiteSetting.NewRow { SettingGroup = "LANDING", SettingKey = "hero_image", Description = "Gambar hero", SettingImage = Photo(5) }, Admin), "Setting hero_image");
        }

        // ---------------------------------------------------------------- Periode, section, divisi, jabatan
        private static void SeedPeriods()
        {
            PeriodOld = Ok(ManagementPeriod.Save(new ManagementPeriod.NewRow { Name = "Periode 2025/2026", StartDate = D(2025, 9, 1), EndDate = D(2026, 8, 31) }, Admin), "Periode lama");
            PeriodOld.Status = ManagementPeriodStatus.Active;
            PeriodOld = Ok(PeriodOld.Update(Admin), "Aktifkan periode lama");
            PeriodActive = Ok(ManagementPeriod.Save(new ManagementPeriod.NewRow { Name = "Periode 2026/2027", StartDate = D(2026, 9, 1), EndDate = D(2027, 8, 31) }, Admin), "Periode baru");

            // Section musik: semua anggota punya satu section (riwayat di t_section_member).
            var sections = new (string Name, string Description)[]
            {
                ("Brass", "Trumpet, mellophone, baritone, tuba"),
                ("Battery", "Snare, tenor, bass drum, cymbal"),
                ("Front Ensemble", "Pit percussion: marimba, vibraphone, synth"),
                ("Colour Guard", "Bendera, rifle, sabre, tari")
            };
            var order = 1;
            foreach (var s in sections)
            {
                MusicSections[s.Name] = Ok(SectionMaster.Save(new SectionMaster.NewRow { Name = s.Name, Description = s.Description, SortOrder = order++ }, Admin), "Section " + s.Name).Id;
            }
            // Divisi: tugas organisasi, hanya anggota pilihan Ketua Umum (dummy; bisa ditambah lewat halaman Divisi).
            var divisions = new (string Name, DivisionType Type, string Description)[]
            {
                ("Humas & Media", DivisionType.Divisi, "Publikasi, dokumentasi, media sosial"),
                ("Logistik", DivisionType.Divisi, "Perawatan dan peminjaman alat"),
                ("Danus", DivisionType.Divisi, "Dana usaha dan merchandise")
            };
            order = 1;
            foreach (var d in divisions)
            {
                Divisions[d.Name] = Ok(Division.Save(new Division.NewRow { Name = d.Name, DivisionType = d.Type, Description = d.Description, SortOrder = order++ }, Admin), "Divisi " + d.Name).Id;
            }
            var positions = new (string Name, PositionLevel Level)[]
            {
                ("Ketua Umum", PositionLevel.Inti), ("Wakil Ketua Umum", PositionLevel.Inti), ("Sekretaris", PositionLevel.Inti),
                ("Bendahara", PositionLevel.Inti), ("Kepala Divisi", PositionLevel.Biasa), ("Staf Divisi", PositionLevel.Biasa),
                ("Section Leader", PositionLevel.Biasa)
            };
            order = 1;
            foreach (var p in positions)
            {
                Positions[p.Name] = Ok(Position.Save(new Position.NewRow { Name = p.Name, IsCore = p.Level, SortOrder = order++ }, Admin), "Jabatan " + p.Name).Id;
            }
        }

        // ---------------------------------------------------------------- User & anggota
        // Akun contoh: username mudah diingat (lihat juga README), password semua DEFAULT_PASSWORD.
        // Indeks dipakai bagian lain: 0 ketua, 1 wakil, 2 sekretaris, 3 bendahara, 4 ketua lama, 5-8 section leader
        // (Brass, Battery, Colour Guard, Front Ensemble), 9-10 kepala divisi (Logistik, Humas & Media),
        // 11-21 anggota biasa, 22-23 alumni.
        private static readonly (string Username, string Name, Gender Gender, string Prodi, string Fakultas, short Angkatan)[] People =
        {
            ("ketua", "Rizky Pratama Putra", Gender.L, "Informatika", "Fakultas Ilmu Komputer", 2023),
            ("wakil", "Nabila Ayu Lestari", Gender.P, "Manajemen", "Fakultas Ekonomi dan Bisnis", 2023),
            ("sekretaris", "Dimas Arya Saputra", Gender.L, "Sistem Informasi", "Fakultas Ilmu Komputer", 2023),
            ("bendahara", "Salsabila Putri Ramadhani", Gender.P, "Akuntansi", "Fakultas Ekonomi dan Bisnis", 2023),
            ("ketua.lama", "Fajar Nugroho", Gender.L, "Teknik Sipil", "Fakultas Teknik dan Sains", 2022),
            ("leader.brass", "Aditya Wicaksono", Gender.L, "Teknik Mesin", "Fakultas Teknik dan Sains", 2024),
            ("leader.battery", "Bagas Setiawan", Gender.L, "Teknik Kimia", "Fakultas Teknik dan Sains", 2024),
            ("leader.guard", "Citra Maharani", Gender.P, "Ilmu Komunikasi", "Fakultas Ilmu Sosial dan Ilmu Politik", 2024),
            ("leader.front", "Dewi Kartika Sari", Gender.P, "Arsitektur", "Fakultas Arsitektur dan Desain", 2024),
            ("kadiv.logistik", "Eko Prasetyo", Gender.L, "Agroteknologi", "Fakultas Pertanian", 2024),
            ("kadiv.humas", "Farah Aulia", Gender.P, "Desain Komunikasi Visual", "Fakultas Arsitektur dan Desain", 2024),
            ("anggota01", "Galih Ramadhan", Gender.L, "Ilmu Hukum", "Fakultas Hukum", 2025),
            ("anggota02", "Hana Safitri", Gender.P, "Administrasi Bisnis", "Fakultas Ilmu Sosial dan Ilmu Politik", 2025),
            ("anggota03", "Ilham Maulana", Gender.L, "Teknik Industri", "Fakultas Teknik dan Sains", 2025),
            ("anggota04", "Jasmine Amelia", Gender.P, "Ekonomi Pembangunan", "Fakultas Ekonomi dan Bisnis", 2025),
            ("anggota05", "Kevin Ardiansyah", Gender.L, "Informatika", "Fakultas Ilmu Komputer", 2025),
            ("anggota06", "Laila Nur Azizah", Gender.P, "Agribisnis", "Fakultas Pertanian", 2025),
            ("anggota07", "Muhammad Rafi", Gender.L, "Teknik Lingkungan", "Fakultas Teknik dan Sains", 2025),
            ("anggota08", "Nadia Permata", Gender.P, "Hubungan Internasional", "Fakultas Ilmu Sosial dan Ilmu Politik", 2025),
            ("anggota09", "Oktavian Hidayat", Gender.L, "Sains Data", "Fakultas Ilmu Komputer", 2025),
            ("anggota10", "Putri Wulandari", Gender.P, "Kedokteran", "Fakultas Kedokteran", 2025),
            ("anggota11", "Raka Firmansyah", Gender.L, "Teknik Elektro", "Fakultas Teknik dan Sains", 2025),
            ("alumni01", "Siti Rahmawati", Gender.P, "Akuntansi", "Fakultas Ekonomi dan Bisnis", 2020),
            ("alumni02", "Taufik Hidayatullah", Gender.L, "Teknik Kimia", "Fakultas Teknik dan Sains", 2019)
        };
        // Ringkasan akun untuk dicetak di akhir seeding: (username, nama, jabatan, role).
        public static readonly List<(string Username, string Name, string Jabatan, string Roles)> AccountSummary = new();

        // Section awal anggota: 5-8 sesuai section yang dipimpin, anggota biasa bergantian, sisanya tetap.
        private static readonly string[] SectionOrder = { "Brass", "Battery", "Colour Guard", "Front Ensemble" };
        private static string SectionOf(int i) => i switch
        {
            0 => "Brass", 1 => "Colour Guard", 2 => "Battery", 3 => "Front Ensemble", 4 => "Battery",
            >= 5 and <= 8 => SectionOrder[i - 5], 9 => "Battery", 10 => "Colour Guard", 22 => "Brass", 23 => "Battery",
            _ => SectionOrder[i % 4]
        };

        private static void SeedMembers()
        {
            for (var i = 0; i < People.Length; i++)
            {
                var p = People[i];
                var nim = $"{p.Angkatan % 100:D2}0{(i % 9) + 1}10{10000 + i * 37:D5}";
                var user = Ok(UserAccount.Save(new UserAccount.NewRow { Username = p.Username, Password = DEFAULT_PASSWORD }, Admin), "User " + p.Name);
                var member = Ok(Member.Save(new Member.NewRow
                {
                    UserId = user.Id,
                    MemberNumber = $"GWA-{p.Angkatan}-{i + 1:D3}",
                    MemberType = i <= 10 ? MemberType.Pengurus : MemberType.Anggota,
                    JoinedAt = D(p.Angkatan, 10, 1),
                    MemberCategory = MemberCategory.Mahasiswa,
                    Nim = nim,
                    Fullname = p.Name,
                    Gender = p.Gender,
                    BirthDate = D(p.Angkatan - 18, (i % 12) + 1, (i % 27) + 1),
                    Fakultas = p.Fakultas,
                    ProgramStudi = p.Prodi,
                    Angkatan = p.Angkatan,
                    Institution = "UPN \"Veteran\" Jawa Timur",
                    Phone = $"08123{4500000 + i * 1311:D7}",
                    Email = $"{nim}@student.upnjatim.ac.id",
                    Address = $"Jl. Rungkut Madya No. {i + 10}, Surabaya",
                    SectionId = MusicSections[SectionOf(i)]
                }, Admin), "Anggota " + p.Name);
                Members.Add(member);
                MemberUser[member.Id] = user.Id;
                if (i < 12)
                {
                    Ok(MemberPhoto.Save(new MemberPhoto.NewRow { MemberId = member.Id, Data = Avatar(i) }, Admin), "Foto anggota");
                }
            }
            Ketua = Members[0]; Wakil = Members[1]; Sekretaris = Members[2]; Bendahara = Members[3]; KetuaLama = Members[4];

            // Contoh riwayat pindah section (anggota bisa berganti section antar-event).
            Ok(SectionMember.Move(new SectionMember.NewRow { MemberId = Members[13].Id, SectionId = MusicSections["Front Ensemble"], StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-75)), Note = "Mengisi pit untuk Konser Akhir Tahun" }, Admin), "Pindah section 1");
            Ok(SectionMember.Move(new SectionMember.NewRow { MemberId = Members[13].Id, SectionId = MusicSections["Colour Guard"], StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-20)), Note = "Kembali ke colour guard untuk parade" }, Admin), "Pindah section 2");
            Ok(SectionMember.Move(new SectionMember.NewRow { MemberId = Members[17].Id, SectionId = MusicSections["Brass"], StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)), Note = "Kebutuhan pemain baritone" }, Admin), "Pindah section 3");

            // Alumni
            for (var i = 22; i < 24; i++)
            {
                var alumni = Members[i];
                alumni.Status = MemberStatus.Alumni;
                alumni.MemberType = MemberType.Alumni;
                Members[i] = Ok(alumni.Update(Admin), "Status alumni " + alumni.Fullname);
                Ok(AlumniProfile.Save(new AlumniProfile.NewRow
                {
                    MemberId = alumni.Id,
                    GraduationYear = (short)(People[i].Angkatan + 4),
                    CurrentJob = i == 22 ? "Auditor" : "Process Engineer",
                    Company = i == 22 ? "KAP Surabaya" : "PT Petrokimia Gresik",
                    Industry = i == 22 ? "Akuntansi" : "Manufaktur",
                    City = i == 22 ? "Surabaya" : "Gresik",
                    LinkedinUrl = $"https://www.linkedin.com/in/alumni-gwa-{i}",
                    IsContactable = ContactPermission.Bersedia,
                    Notes = "Bersedia menjadi pelatih tamu"
                }, Admin), "Profil alumni");
            }

            // Role: pengurus inti, section leader dan kepala divisi terikat periode aktif; role ANGGOTA tanpa periode (Area Anggota untuk semua).
            void Grant(Member member, string roleCode, bool perPeriod)
            {
                Ok(UserRole.Save(new UserRole.NewRow { UserId = MemberUser[member.Id], RoleId = Roles[roleCode].Id, PeriodId = perPeriod ? PeriodActive.Id : null }, Admin), $"Role {roleCode} {member.Fullname}");
            }
            Grant(Ketua, "KETUA", true);
            Grant(Wakil, "WAKIL", true);
            Grant(Sekretaris, "SEKRETARIS", true);
            Grant(Bendahara, "BENDAHARA", true);
            foreach (var m in Members.Skip(5).Take(4))
            {
                Grant(m, "SECTION_LEADER", true);
            }
            foreach (var m in Members.Skip(9).Take(2))
            {
                Grant(m, "KADIV", true);
            }
            foreach (var m in Members.Take(22))
            {
                Grant(m, "ANGGOTA", false);
            }

            var jabatan = new Dictionary<int, (string Jabatan, string Roles)>
            {
                [0] = ("Ketua Umum", "KETUA, ANGGOTA"), [1] = ("Wakil Ketua Umum", "WAKIL, ANGGOTA"), [2] = ("Sekretaris", "SEKRETARIS, ANGGOTA"),
                [3] = ("Bendahara", "BENDAHARA, ANGGOTA"), [4] = ("Anggota (ketua periode lalu)", "ANGGOTA"),
                [5] = ("Section Leader Brass", "SECTION_LEADER, ANGGOTA"), [6] = ("Section Leader Battery", "SECTION_LEADER, ANGGOTA"), [7] = ("Section Leader Colour Guard", "SECTION_LEADER, ANGGOTA"),
                [8] = ("Section Leader Front Ensemble", "SECTION_LEADER, ANGGOTA"), [9] = ("Kepala Divisi Logistik", "KADIV, ANGGOTA"), [10] = ("Kepala Divisi Humas & Media", "KADIV, ANGGOTA")
            };
            for (var i = 0; i < People.Length; i++)
            {
                var (title, roles) = jabatan.TryGetValue(i, out var j) ? j : i >= 22 ? ("Alumni", "-") : ("Anggota", "ANGGOTA");
                AccountSummary.Add((People[i].Username, People[i].Name, title, roles));
            }
        }

        // ---------------------------------------------------------------- Persetujuan & penomoran
        private static void SeedApprovalMasters()
        {
            var formats = new (string Type, string Format, int Pad, NumberResetPeriod Reset, string Description)[]
            {
                ("LETTER", "{seq}/{org}/SK/{month_roman}/{year}", 3, NumberResetPeriod.Tahunan, "Nomor surat keluar"),
                ("FUNDING_REQUEST", "PD/{seq}/{org}/{month_roman}/{year}", 3, NumberResetPeriod.Tahunan, "Pengajuan dana"),
                ("PROCUREMENT", "PGD/{seq}/{org}/{year}", 3, NumberResetPeriod.Tahunan, "Pengadaan barang"),
                ("FIN_TRX", "TRX/{yy}{month}/{seq}", 4, NumberResetPeriod.Bulanan, "Transaksi keuangan"),
                ("SPONSORSHIP", "SPN/{seq}/{org}/{year}", 3, NumberResetPeriod.Tahunan, "Sponsorship"),
                ("DANUS", "DNS/{yy}{month}/{seq}", 4, NumberResetPeriod.Bulanan, "Pesanan danus"),
                ("BORROW", "PJM/{seq}/{org}/{month_roman}/{year}", 3, NumberResetPeriod.Tahunan, "Peminjaman alat"),
                ("CERTIFICATE", "{seq}/SERT/{org}/{year}", 4, NumberResetPeriod.Tahunan, "Sertifikat"),
                ("MOU", "MOU/{seq}/{org}/{year}", 3, NumberResetPeriod.Tahunan, "Nota kesepahaman"),
                ("BA_ST", "BAST/{seq}/{org}/{year}", 2, NumberResetPeriod.Tahunan, "Berita acara serah terima"),
                ("DEBT", "HT/{seq}/{org}/{year}", 3, NumberResetPeriod.Tahunan, "Hutang"),
                ("SURAT UND", "{seq}/{org}/UND/{month_roman}/{year}", 3, NumberResetPeriod.Tahunan, "Surat undangan")
            };
            foreach (var f in formats)
            {
                Ok(DocumentNumberFormat.Save(new DocumentNumberFormat.NewRow
                {
                    DocumentType = f.Type, NumberFormat = f.Format, SeqPadding = f.Pad, ResetPeriod = f.Reset, Description = f.Description
                }, Admin), "Format nomor " + f.Type);
            }

            Ok(ApprovalFlow.Save(new ApprovalFlow.NewRow
            {
                DocumentType = "FUNDING_REQUEST",
                Name = "Alur Pengajuan Dana",
                Steps = new()
                {
                    new() { StepLevel = 1, StepName = "Verifikasi Bendahara", ApproverType = ApproverType.Role, RoleId = Roles["BENDAHARA"].Id, AllowRevision = RevisionPermission.Boleh },
                    new() { StepLevel = 2, StepName = "Persetujuan Ketua Umum", ApproverType = ApproverType.Role, RoleId = Roles["KETUA"].Id, AllowRevision = RevisionPermission.Boleh }
                }
            }, Admin), "Alur pengajuan dana");
            Ok(ApprovalFlow.Save(new ApprovalFlow.NewRow
            {
                DocumentType = "PROCUREMENT",
                Name = "Alur Pengadaan Barang",
                Steps = new()
                {
                    new() { StepLevel = 1, StepName = "Verifikasi Bendahara", ApproverType = ApproverType.Role, RoleId = Roles["BENDAHARA"].Id },
                    new() { StepLevel = 2, StepName = "Persetujuan Ketua Umum", ApproverType = ApproverType.Jabatan, PositionId = Positions["Ketua Umum"] }
                }
            }, Admin), "Alur pengadaan");
            Ok(ApprovalFlow.Save(new ApprovalFlow.NewRow
            {
                DocumentType = "LETTER",
                Name = "Alur Surat Keluar",
                Steps = new()
                {
                    new() { StepLevel = 1, StepName = "Pemeriksaan Sekretaris", ApproverType = ApproverType.Role, RoleId = Roles["SEKRETARIS"].Id },
                    new() { StepLevel = 2, StepName = "Tanda Tangan Ketua Umum", ApproverType = ApproverType.Jabatan, PositionId = Positions["Ketua Umum"], AllowRevision = RevisionPermission.TidakBoleh }
                }
            }, Admin), "Alur surat");

            var templates = new (string Code, NotificationChannel Channel, string? Subject, string Body)[]
            {
                ("RESET_PASSWORD", NotificationChannel.Email, "Reset password {app}", "Halo {name},\n\nGunakan tautan berikut untuk mengatur ulang password Anda: {link}\nTautan berlaku 60 menit.\n\nSalam,\n{app}"),
                ("VERIFY_EMAIL", NotificationChannel.Email, "Verifikasi email {app}", "Halo {name}, klik {link} untuk memverifikasi email Anda."),
                ("DUES_BILL", NotificationChannel.WhatsApp, null, "Halo {name}, tagihan iuran {period} sebesar Rp{amount} jatuh tempo {due}. Terima kasih - Bendahara MB GWA"),
                ("DUES_BILL", NotificationChannel.Email, "Tagihan iuran {period}", "Halo {name}, tagihan iuran {period} sebesar Rp{amount} jatuh tempo {due}."),
                ("FUNDING_APPROVAL", NotificationChannel.InApp, "Pengajuan dana menunggu persetujuan", "Pengajuan dana {number} ({purpose}) menunggu persetujuan Anda."),
                ("LETTER_APPROVAL", NotificationChannel.InApp, "Surat menunggu persetujuan", "Surat {subject} menunggu persetujuan Anda."),
                ("BORROW_DUE", NotificationChannel.WhatsApp, null, "Halo {name}, peminjaman {number} jatuh tempo {due}. Mohon segera dikembalikan.")
            };
            foreach (var t in templates)
            {
                Ok(MessageTemplate.Save(new MessageTemplate.NewRow { TemplateCode = t.Code, Channel = t.Channel, Subject = t.Subject, Body = t.Body, Description = "Template " + t.Code }, Admin), "Template " + t.Code);
            }
            Ok(UserNotificationPref.Save(new UserNotificationPref.NewRow { UserId = MemberUser[Members[15].Id], NotifType = "DUES_BILL", Channel = NotificationChannel.WhatsApp, IsEnabled = NotificationState.Nonaktif }, Admin), "Preferensi notifikasi");
            Ok(UserNotificationPref.Save(new UserNotificationPref.NewRow { UserId = MemberUser[Members[16].Id], NotifType = "PENGUMUMAN", Channel = NotificationChannel.InApp, IsEnabled = NotificationState.Nonaktif }, Admin), "Preferensi notifikasi");
            Ok(UserNotificationPref.Save(new UserNotificationPref.NewRow { UserId = MemberUser[Ketua.Id], NotifType = "FUNDING_APPROVAL", Channel = NotificationChannel.InApp, IsEnabled = NotificationState.Aktif }, Admin), "Preferensi notifikasi");
        }

        // ---------------------------------------------------------------- Keuangan, mitra, sponsorship, merchandise
        private static void SeedBusinessMasters()
        {
            Accounts["Kas"] = Ok(FinancialAccount.Save(new FinancialAccount.NewRow { Name = "Kas Tunai", AccountType = FinancialAccountType.Cash, OpeningBalance = 2_500_000 }, Admin), "Akun kas").Id;
            Accounts["Bank"] = Ok(FinancialAccount.Save(new FinancialAccount.NewRow { Name = "Rekening BNI", AccountType = FinancialAccountType.Bank, BankName = "BNI", AccountNumber = "1234567890", AccountHolder = "UKM Marching Band GWA", OpeningBalance = 12_000_000 }, Admin), "Akun bank").Id;
            Accounts["EWallet"] = Ok(FinancialAccount.Save(new FinancialAccount.NewRow { Name = "DANA Danus", AccountType = FinancialAccountType.EWallet, AccountNumber = "081553858791", AccountHolder = "Bendahara GWA", OpeningBalance = 500_000 }, Admin), "Akun e-wallet").Id;

            var income = new[] { "Iuran Anggota", "Sponsorship", "Penjualan Danus", "Sewa Alat", "Donasi", "Hibah Kampus", "Pinjaman Diterima", "Denda Keterlambatan", "Pengembalian Dana" };
            var expense = new[] { "Konsumsi", "Transportasi", "Perlengkapan & Alat", "Perawatan Alat", "Kostum", "Pendaftaran Lomba", "Administrasi", "Pembayaran Hutang", "Honor Pelatih", "Kekurangan Dana" };
            foreach (var name in income)
            {
                Categories[name] = Ok(FinancialCategory.Save(new FinancialCategory.NewRow { Name = name, CategoryType = CashFlowType.Income }, Admin), "Kategori " + name).Id;
            }
            foreach (var name in expense)
            {
                Categories[name] = Ok(FinancialCategory.Save(new FinancialCategory.NewRow { Name = name, CategoryType = CashFlowType.Expense }, Admin), "Kategori " + name).Id;
            }
            Dues = Ok(DuesSetting.Save(new DuesSetting.NewRow { PeriodId = PeriodActive.Id, Name = "Iuran Bulanan 2026/2027", Amount = 20_000, Frequency = DuesFrequency.Monthly, CategoryId = Categories["Iuran Anggota"] }, Admin), "Pengaturan iuran");
            Ok(DuesSetting.Save(new DuesSetting.NewRow { PeriodId = PeriodActive.Id, Name = "Iuran Kostum Lomba", Amount = 150_000, Frequency = DuesFrequency.Once, CategoryId = Categories["Iuran Anggota"] }, Admin), "Iuran kostum");

            var parties = new (string Key, string Name, ExternalPartyType Type, string Industry, string City, string Contact, string Position)[]
            {
                ("Yamaha", "Yamaha Music Surabaya", ExternalPartyType.Vendor, "Alat musik", "Surabaya", "Andi Kurniawan", "Sales Marching"),
                ("Drumline", "CV Drumline Percussion", ExternalPartyType.Vendor, "Alat perkusi", "Sidoarjo", "Yoga Pratama", "Owner"),
                ("BNI", "PT Bank Negara Indonesia KCU Rungkut", ExternalPartyType.Perusahaan, "Perbankan", "Surabaya", "Rina Marlina", "Relationship Manager"),
                ("Suara", "Radio Suara Surabaya", ExternalPartyType.MediaPartner, "Media", "Surabaya", "Bima Santoso", "Event Coordinator"),
                ("Komunitas", "Komunitas Marching Surabaya", ExternalPartyType.Komunitas, "Komunitas musik", "Surabaya", "Wahyu Hidayat", "Ketua"),
                ("Dispora", "Dinas Pemuda dan Olahraga Kota Surabaya", ExternalPartyType.InstansiPemerintah, "Pemerintahan", "Surabaya", "Drs. Hendra Gunawan", "Kepala Bidang Kepemudaan"),
                ("SMA", "SMA Negeri 17 Surabaya", ExternalPartyType.LembagaPendidikan, "Pendidikan", "Surabaya", "Ibu Ratna", "Pembina Ekstrakurikuler"),
                ("Konveksi", "Konveksi Sinar Jaya", ExternalPartyType.Vendor, "Konveksi", "Surabaya", "Pak Slamet", "Pemilik")
            };
            var n = 0;
            foreach (var p in parties)
            {
                n++;
                Parties[p.Key] = Ok(ExternalParty.Save(new ExternalParty.NewRow
                {
                    Name = p.Name, PartyType = p.Type, Industry = p.Industry, City = p.City, Address = $"Jl. Contoh No. {n * 7}, {p.City}",
                    Phone = $"031-87{n:D2}{n * 113:D4}", Email = $"kontak{n}@contoh.co.id", Website = $"https://contoh{n}.co.id",
                    Contacts = new()
                    {
                        new() { Name = p.Contact, Position = p.Position, Phone = $"0812{70000000 + n * 1234567:D8}", Email = $"pic{n}@contoh.co.id", IsPrimary = ContactRank.Utama },
                        new() { Name = "Admin " + p.Name.Split(' ')[0], Position = "Admin", Phone = $"0857{30000000 + n * 7654321:D8}", IsPrimary = ContactRank.Tambahan }
                    }
                }, Admin), "Mitra " + p.Name);
            }
            Ok(Parties["Yamaha"].UpdateLogo(Photo(8), "yamaha.jpg", Admin), "Logo mitra");

            var packages = new (string Name, decimal Price, string[] Benefits)[]
            {
                ("Platinum", 15_000_000, new[] { "Logo besar di banner panggung", "Logo di kostum seluruh pemain", "Penyebutan di setiap penampilan", "Booth promosi saat konser", "Konten Instagram khusus (3 post)" }),
                ("Gold", 7_500_000, new[] { "Logo sedang di banner panggung", "Penyebutan di pembukaan acara", "Konten Instagram (1 post)" }),
                ("Silver", 3_000_000, new[] { "Logo kecil di poster", "Ucapan terima kasih di media sosial" })
            };
            var seq = 1;
            foreach (var p in packages)
            {
                Packages[p.Name] = Ok(SponsorshipPackage.Save(new SponsorshipPackage.NewRow
                {
                    Name = p.Name, Price = p.Price, SeqNo = seq++,
                    Benefits = p.Benefits.Select((b, i) => new PackageBenefit.NewRow { Title = b, SeqNo = i + 1 }).ToList()
                }, Admin), "Paket " + p.Name);
            }

            Products["Kaos"] = Ok(Product.Save(new Product.NewRow
            {
                Name = "Kaos GWA 2026", ProductType = ProductType.Merchandise, Description = "Kaos katun combed 30s logo Gita Widya Agni", Price = 85_000, CostPrice = 55_000,
                Unit = "pcs", TrackStock = StockTracking.Dilacak, Photo = Photo(2), PhotoFileName = "kaos.jpg",
                Variants = new()
                {
                    new() { Name = "S", Stock = 10, Sku = "KAOS26-S", SeqNo = 1 }, new() { Name = "M", Stock = 20, Sku = "KAOS26-M", SeqNo = 2 },
                    new() { Name = "L", Stock = 20, Sku = "KAOS26-L", SeqNo = 3 }, new() { Name = "XL", Price = 90_000, Stock = 10, Sku = "KAOS26-XL", SeqNo = 4 }
                }
            }, Admin), "Produk kaos");
            Products["Gantungan"] = Ok(Product.Save(new Product.NewRow { Name = "Gantungan Kunci Stik Drum", Price = 15_000, CostPrice = 7_000, Stock = 50, Description = "Miniatur stik drum" }, Admin), "Produk gantungan");
            Products["Totebag"] = Ok(Product.Save(new Product.NewRow { Name = "Totebag Kanvas GWA", Price = 45_000, CostPrice = 25_000, Stock = 30 }, Admin), "Produk totebag");
            Products["Snack"] = Ok(Product.Save(new Product.NewRow { Name = "Snack Box Danus", ProductType = ProductType.MakananMinuman, Price = 12_000, CostPrice = 8_000, TrackStock = StockTracking.TidakDilacak, Unit = "box" }, Admin), "Produk snack");
            Products["Jaket"] = Ok(Product.Save(new Product.NewRow
            {
                Name = "Jaket Varsity GWA (Pre-order)", Price = 250_000, CostPrice = 180_000, IsPreorder = SaleMode.PreOrder,
                PreorderCloseAt = DateTime.Today.AddDays(30).AddHours(23), TrackStock = StockTracking.TidakDilacak, Description = "Pre-order jaket varsity angkatan"
            }, Admin), "Produk jaket");
        }

        // ---------------------------------------------------------------- Inventaris, surat, checklist, poin
        private static void SeedOtherMasters()
        {
            foreach (var c in new[] { ("Alat Musik Brass", "Trumpet, mellophone, baritone, tuba"), ("Alat Perkusi", "Snare, tenor, bass drum, cymbal"),
                ("Perlengkapan Colour Guard", "Bendera, rifle, sabre"), ("Elektronik & Sound System", "Speaker, mixer, mikrofon"), ("Perlengkapan Umum", "Tenda, meja, kostum") })
            {
                AssetCategories[c.Item1] = Ok(AssetCategory.Save(new AssetCategory.NewRow { Name = c.Item1, Description = c.Item2 }, Admin), "Kategori aset " + c.Item1).Id;
            }
            var assets = new (string Key, string Category, string Code, string Name, string Brand, int Qty, UnitTracking Track, string? Prefix, decimal Cost, decimal Rent, decimal Late, string Unit)[]
            {
                ("Trumpet", "Alat Musik Brass", "GWA-BRS-001", "Trumpet Yamaha YTR-2330", "Yamaha", 6, UnitTracking.PerUnit, "TRP", 6_500_000, 25_000, 10_000, "unit"),
                ("Mellophone", "Alat Musik Brass", "GWA-BRS-002", "Mellophone Jupiter JMP-1000", "Jupiter", 4, UnitTracking.PerUnit, "MLP", 9_000_000, 30_000, 10_000, "unit"),
                ("Snare", "Alat Perkusi", "GWA-PRK-001", "Marching Snare Drum 14\"", "Pearl", 5, UnitTracking.PerUnit, "SNR", 7_500_000, 30_000, 15_000, "unit"),
                ("BassDrum", "Alat Perkusi", "GWA-PRK-002", "Marching Bass Drum Set", "Pearl", 5, UnitTracking.PerUnit, "BSD", 5_000_000, 25_000, 10_000, "unit"),
                ("Bendera", "Perlengkapan Colour Guard", "GWA-CG-001", "Bendera Colour Guard 6 kaki", "Lokal", 30, UnitTracking.CukupJumlah, null, 150_000, 2_000, 1_000, "pcs"),
                ("Sound", "Elektronik & Sound System", "GWA-ELK-001", "Speaker Portable Aktif 15\"", "Huper", 2, UnitTracking.PerUnit, "SND", 4_500_000, 75_000, 25_000, "unit"),
                ("Tenda", "Perlengkapan Umum", "GWA-UMM-001", "Tenda Lipat 3x3", "Lokal", 4, UnitTracking.CukupJumlah, null, 1_200_000, 50_000, 20_000, "unit")
            };
            var i = 0;
            foreach (var a in assets)
            {
                Assets[a.Key] = Ok(Asset.Save(new Asset.NewRow
                {
                    CategoryId = AssetCategories[a.Category], AssetCode = a.Code, Name = a.Name, Brand = a.Brand, Quantity = a.Qty, Unit = a.Unit,
                    TrackUnit = a.Track, UnitPrefix = a.Prefix, AcquisitionDate = D(2022 + (i % 3), 3 + i, 10), AcquisitionCost = a.Cost * a.Qty,
                    AcquisitionSource = i == 4 ? AcquisitionSource.Hibah : AcquisitionSource.Beli, Location = "Gudang Giri Loka",
                    AssetCondition = ItemCondition.Baik, RentalFeePerDay = a.Rent, LateFeePerDay = a.Late, Specification = a.Name
                }, Admin), "Aset " + a.Name);
                if (i < 4)
                {
                    Ok(AssetPhoto.Save(new AssetPhoto.NewRow { AssetId = Assets[a.Key].Id, Data = Photo(i + 3) }, Admin), "Foto aset");
                }
                i++;
            }
            Ok(AssetUnit.Save(new AssetUnit.NewRow { AssetId = Assets["Trumpet"].Id, SerialNumber = "YTR2330-77821", UnitCondition = ItemCondition.Baik, Notes = "Hibah alumni 2026" }, Admin), "Unit tambahan");

            foreach (var c in new[] { ("AD/ART", "Anggaran dasar & rumah tangga"), ("SOP", "Prosedur operasional"), ("Notulen", "Notulen rapat"), ("Laporan", "Laporan kegiatan & keuangan"), ("Surat Keputusan", "SK organisasi") })
            {
                DocCategories[c.Item1] = Ok(DocumentCategory.Save(new DocumentCategory.NewRow { Name = c.Item1, Description = c.Item2 }, Admin), "Kategori dokumen " + c.Item1).Id;
            }
            LetterTemplates["Undangan"] = Ok(LetterTemplate.Save(new LetterTemplate.NewRow
            {
                Name = "Surat Undangan", LetterType = "SURAT UND",
                Body = "<p>Dengan hormat,</p><p>Bersama surat ini kami mengundang Bapak/Ibu/Saudara untuk menghadiri {acara} yang akan dilaksanakan pada {tanggal} bertempat di {tempat}.</p><p>Atas perhatian dan kehadirannya kami ucapkan terima kasih.</p>"
            }, Admin), "Template undangan");
            LetterTemplates["Peminjaman"] = Ok(LetterTemplate.Save(new LetterTemplate.NewRow
            {
                Name = "Surat Permohonan Peminjaman", LetterType = "SURAT PINJAM",
                Body = "<p>Dengan hormat,</p><p>Kami bermaksud meminjam {barang} untuk keperluan {keperluan} pada {tanggal}.</p>"
            }, Admin), "Template peminjaman");
            LetterTemplates["Sponsorship"] = Ok(LetterTemplate.Save(new LetterTemplate.NewRow
            {
                Name = "Surat Permohonan Sponsorship", LetterType = "SURAT SPONSOR",
                Body = "<p>Dengan hormat,</p><p>Sehubungan dengan {acara}, kami mengajukan permohonan kerja sama sponsorship sesuai proposal terlampir.</p>",
                FileData = Docx("Template Surat Permohonan Sponsorship - Marching Band Gita Widya Agni"), FileName = "template-sponsorship.docx"
            }, Admin), "Template sponsorship");

            EventChecklist = Ok(ChecklistTemplate.Save(new ChecklistTemplate.NewRow
            {
                Name = "Persiapan Konser / Event", ActivityType = ActivityType.Event, Description = "Checklist standar event besar",
                Items = new()
                {
                    new() { SectionName = "Acara", Title = "Susun rundown final", OffsetDay = -14, Priority = Priority.Tinggi, IsMilestone = MilestoneFlag.Milestone, SeqNo = 1 },
                    new() { SectionName = "Humas", Title = "Publikasi poster di media sosial", OffsetDay = -21, Priority = Priority.Sedang, SeqNo = 2 },
                    new() { SectionName = "Logistik", Title = "Cek kondisi seluruh alat", OffsetDay = -7, Priority = Priority.Tinggi, SeqNo = 3 },
                    new() { SectionName = "Konsumsi", Title = "Pesan konsumsi pemain & panitia", OffsetDay = -5, Priority = Priority.Sedang, SeqNo = 4 },
                    new() { SectionName = "Acara", Title = "Gladi bersih", OffsetDay = -1, Priority = Priority.Tinggi, IsMilestone = MilestoneFlag.Milestone, SeqNo = 5 }
                }
            }, Admin), "Template checklist event");
            Ok(ChecklistTemplate.Save(new ChecklistTemplate.NewRow
            {
                Name = "Persiapan Lomba", ActivityType = ActivityType.Lomba,
                Items = new()
                {
                    new() { Title = "Daftarkan tim ke panitia lomba", OffsetDay = -30, Priority = Priority.Tinggi, SeqNo = 1 },
                    new() { Title = "Pengukuran & pemesanan kostum", OffsetDay = -25, SeqNo = 2 },
                    new() { Title = "Sewa transportasi rombongan", OffsetDay = -10, SeqNo = 3 }
                }
            }, Admin), "Template checklist lomba");

            var rules = new (PointSourceType Source, ActivityType? Type, EventLevel? Level, int Points, string Description)[]
            {
                (PointSourceType.HadirKegiatan, null, null, 2, "Hadir kegiatan (umum)"),
                (PointSourceType.HadirKegiatan, ActivityType.Latihan, null, 1, "Hadir latihan rutin"),
                (PointSourceType.Panitia, null, null, 5, "Menjadi panitia"),
                (PointSourceType.Pemateri, null, null, 10, "Menjadi pemateri / pelatih"),
                (PointSourceType.Pengurus, null, null, 15, "Menjabat pengurus satu periode"),
                (PointSourceType.Prestasi, null, null, 5, "Prestasi (umum)"),
                (PointSourceType.Prestasi, null, EventLevel.Kampus, 10, "Prestasi tingkat kampus"),
                (PointSourceType.Prestasi, null, EventLevel.Regional, 20, "Prestasi tingkat regional"),
                (PointSourceType.Prestasi, null, EventLevel.Nasional, 40, "Prestasi tingkat nasional"),
                (PointSourceType.Prestasi, null, EventLevel.Internasional, 75, "Prestasi tingkat internasional")
            };
            foreach (var r in rules)
            {
                Ok(PointRule.Save(new PointRule.NewRow { SourceType = r.Source, ActivityType = r.Type, AchievementLevel = r.Level, Points = r.Points, Description = r.Description }, Admin), "Aturan poin " + r.Description);
            }
        }
    }
}
