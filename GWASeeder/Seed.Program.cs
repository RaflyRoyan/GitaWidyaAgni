using GWALib.Lib.Transaksi.Formulir;
using GWALib.Lib.Transaksi.Presensi;
using GWALib.Lib.Transaksi.Prestasi;
using GWALib.Lib.Transaksi.Program;
using GWALib.Lib.Transaksi.TimPelaksana;
using GWAShared.Enums;

namespace GWASeeder
{
    internal static partial class Seed
    {
        public static readonly Dictionary<string, WorkProgram> Programs = new();
        public static readonly Dictionary<string, Activity> Activities = new();
        public static readonly Dictionary<string, Competition> Competitions = new();
        public static readonly Dictionary<string, int> Sections = new();
        private static readonly List<string> ActivitiesToFinish = new();

        // ---------------------------------------------------------------- Program kerja, tugas, timeline
        private static void SeedPrograms()
        {
            var defs = new (string Key, string Title, string Description, int Pic, DateOnly Start, DateOnly End, decimal Budget, int? Previous)[]
            {
                ("Latihan", "Latihan Rutin & Pembinaan", "Latihan rutin brass, battery, front ensemble, dan colour guard", 5, D(2026, 9, 1), D(2027, 7, 31), 6_000_000, null),
                ("Konser", "Konser Gita Swara 2026", "Konser tahunan Marching Band Gita Widya Agni", 1, D(2026, 10, 1), D(2026, 12, 31), 25_000_000, ProgramOldConcert.Id),
                ("Kejurnas", "Persiapan Kejuaraan Nasional", "Persiapan tim untuk GPMB dan kejuaraan nasional", 6, D(2026, 9, 15), D(2027, 3, 31), 40_000_000, null),
                ("Rekrutmen", "Rekrutmen & Kaderisasi", "Open recruitment dan diklat anggota baru", 2, D(2026, 9, 1), D(2026, 11, 30), 3_000_000, null),
                ("Humas", "Publikasi & Branding", "Konten media sosial dan dokumentasi", 10, D(2026, 9, 1), D(2027, 8, 31), 2_000_000, null)
            };
            foreach (var p in defs)
            {
                var program = Ok(WorkProgram.Save(new WorkProgram.NewRow
                {
                    PeriodId = PeriodActive.Id, PreviousProgramId = p.Previous, PicMemberId = Members[p.Pic].Id, Title = p.Title, Description = p.Description,
                    StartDate = p.Start, EndDate = p.End, BudgetAmount = p.Budget
                }, Admin), "Program " + p.Title);
                if (p.Key != "Humas")
                {
                    program = Ok(program.SetStatus(ProgramStatus.Ongoing, Admin), "Program berjalan");
                }
                Programs[p.Key] = program;
            }

            var tasks = new (string Program, string Title, string? Division, int Pic, Priority Priority, int StartOffset, int DueOffset, TaskProgressStatus Status)[]
            {
                ("Konser", "Booking gedung konser", null, 1, Priority.Tinggi, -30, -10, TaskProgressStatus.Selesai),
                ("Konser", "Desain poster dan tiket", "Humas & Media", 10, Priority.Sedang, -20, 5, TaskProgressStatus.Proses),
                ("Konser", "Aransemen lagu pembuka", null, 5, Priority.Tinggi, -40, 10, TaskProgressStatus.Proses),
                ("Kejurnas", "Pendaftaran tim GPMB", null, 6, Priority.Tinggi, -5, 15, TaskProgressStatus.Belum),
                ("Kejurnas", "Pengukuran kostum pemain", null, 7, Priority.Sedang, 0, 20, TaskProgressStatus.Belum),
                ("Latihan", "Servis rutin alat brass", "Logistik", 9, Priority.Sedang, -14, -2, TaskProgressStatus.Selesai)
            };
            foreach (var t in tasks)
            {
                var task = Ok(ProgramTask.Save(new ProgramTask.NewRow
                {
                    ProgramId = Programs[t.Program].Id, DivisionId = t.Division == null ? null : Divisions[t.Division], ResponsibleMemberId = Members[t.Pic].Id,
                    Title = t.Title, Priority = t.Priority, StartDate = Today.AddDays(t.StartOffset), DueDate = Today.AddDays(t.DueOffset),
                    IsMilestone = t.Priority == Priority.Tinggi ? MilestoneFlag.Milestone : MilestoneFlag.Biasa
                }, Admin), "Tugas " + t.Title);
                if (t.Status != TaskProgressStatus.Belum)
                {
                    task = Ok(task.SetStatus(TaskProgressStatus.Proses, Admin), "Tugas proses");
                }
                if (t.Status == TaskProgressStatus.Selesai)
                {
                    Ok(task.SetStatus(TaskProgressStatus.Selesai, Admin), "Tugas selesai");
                }
            }
            Ok(ProgramTask.Save(new ProgramTask.NewRow { PeriodId = PeriodActive.Id, Title = "Perbarui inventaris sekretariat", ResponsibleMemberId = Sekretaris.Id, DueDate = Today.AddDays(14) }, Admin), "Tugas umum");

            var plans = new (string Title, TimelinePlanType Type, DateOnly Start, DateOnly End, string? Program, int Pic)[]
            {
                ("Konser Gita Swara 2026", TimelinePlanType.Event, Today.AddDays(30), Today.AddDays(30), "Konser", 1),
                ("Diklat Anggota Baru", TimelinePlanType.Latihan, Today.AddDays(25), Today.AddDays(26), "Rekrutmen", 2),
                ("Grand Prix Marching Band 2027", TimelinePlanType.Event, D(2027, 2, 10), D(2027, 2, 14), "Kejurnas", 6),
                ("Deadline LPJ Semester Ganjil", TimelinePlanType.Deadline, D(2027, 1, 31), D(2027, 1, 31), null, 2),
                ("Rapat Kerja Tengah Periode", TimelinePlanType.Rapat, D(2027, 3, 5), D(2027, 3, 5), null, 0)
            };
            foreach (var p in plans)
            {
                var plan = Ok(TimelinePlan.Save(new TimelinePlan.NewRow
                {
                    PeriodId = PeriodActive.Id, Title = p.Title, PlanType = p.Type, PlanYear = (short)p.Start.Year, PlanMonth = p.Start.Month,
                    StartDate = p.Start, EndDate = p.End, ProgramId = p.Program == null ? null : Programs[p.Program].Id, PicMemberId = Members[p.Pic].Id
                }, Admin), "Timeline " + p.Title);
                TimelinePlans[p.Title] = plan;
            }
        }
        public static readonly Dictionary<string, TimelinePlan> TimelinePlans = new();

        // ---------------------------------------------------------------- Lomba & kegiatan
        private static void SeedCompetitionsAndActivities()
        {
            // Lomba yang sudah selesai (untuk prestasi) dan yang sedang buka pendaftaran.
            var jatim = Ok(Competition.Save(new Competition.NewRow
            {
                Name = "Kejuaraan Marching Band Jawa Timur 2026", Organizer = "PDBI Jawa Timur", Level = EventLevel.Regional, Category = "Marching Band Umum",
                Description = "Kejuaraan tingkat provinsi", RegistrationDeadline = DateTime.Today.AddDays(5).AddHours(23), StartDate = D(2026, 10, 24), EndDate = D(2026, 10, 25),
                Location = "GOR Kertajaya, Surabaya", IsPublic = PublicVisibility.Public
            }, Admin), "Lomba Jatim");
            jatim = Ok(jatim.SetStatus(CompetitionStatus.Registration, Admin), "Lomba Jatim pendaftaran");
            foreach (var i in new[] { 0, 5, 6, 7, 8, 11, 12, 13 })
            {
                Ok(CompetitionParticipant.Register(new CompetitionParticipant.NewRow
                {
                    CompetitionId = jatim.Id, MemberId = Members[i].Id, TeamName = "MB Gita Widya Agni",
                    ParticipantRole = i == 0 ? CompetitionParticipant.ROLE_KETUA_TIM : CompetitionParticipant.ROLE_ANGGOTA
                }, Admin), "Peserta lomba Jatim");
            }
            Ok(CompetitionParticipant.Register(new CompetitionParticipant.NewRow { CompetitionId = jatim.Id, MemberId = Members[1].Id, ParticipantRole = CompetitionParticipant.ROLE_PEMBIMBING }, Admin), "Pembimbing lomba");
            jatim = Ok(jatim.SetStatus(CompetitionStatus.Ongoing, Admin), "Lomba Jatim berlangsung");
            jatim = Ok(jatim.SetStatus(CompetitionStatus.Finished, Admin), "Lomba Jatim selesai");
            Ok(jatim.UpdatePoster(Photo(6), Admin), "Poster lomba");
            Competitions["Jatim"] = jatim;

            var gpmb = Ok(Competition.Save(new Competition.NewRow
            {
                Name = "Grand Prix Marching Band 2027", Organizer = "PB PDBI", Level = EventLevel.Nasional, Category = "Marching Band Perguruan Tinggi",
                Description = "Kejuaraan marching band tingkat nasional", RegistrationDeadline = DateTime.Today.AddDays(45).AddHours(23), StartDate = D(2027, 2, 10), EndDate = D(2027, 2, 14),
                Location = "Istora Senayan, Jakarta", LinkUrl = "https://gpmb.or.id", IsPublic = PublicVisibility.Public, IsFeatured = FeaturedStatus.Disorot
            }, Admin), "Lomba GPMB");
            gpmb = Ok(gpmb.SetStatus(CompetitionStatus.Registration, Admin), "GPMB pendaftaran");
            foreach (var i in new[] { 0, 5, 6, 7, 9, 13, 15 })
            {
                Ok(CompetitionParticipant.Register(new CompetitionParticipant.NewRow { CompetitionId = gpmb.Id, MemberId = Members[i].Id, TeamName = "MB Gita Widya Agni" }, Admin), "Peserta GPMB");
            }
            Competitions["GPMB"] = gpmb;
            Competitions["Solo"] = Ok(Competition.Save(new Competition.NewRow
            {
                Name = "Lomba Solo Percussion UPN Fest 2027", Organizer = "BEM UPN \"Veteran\" Jawa Timur", Level = EventLevel.Kampus, StartDate = D(2027, 4, 20), Location = "Kampus UPN Jatim"
            }, Admin), "Lomba solo");

            Ok(CompetitionMaterial.Save(new CompetitionMaterial.NewRow { CompetitionId = gpmb.Id, Title = "Juknis GPMB 2027", MaterialType = MaterialType.File, FileData = Pdf("Petunjuk Teknis GPMB 2027"), FileName = "juknis-gpmb-2027.pdf", ContentType = "application/pdf", Visibility = MaterialVisibility.Participant }, Admin), "Materi juknis");
            Ok(CompetitionMaterial.Save(new CompetitionMaterial.NewRow { CompetitionId = gpmb.Id, Title = "Video referensi show 2025", MaterialType = MaterialType.Video, Url = "https://www.youtube.com/watch?v=ttDaKyRaP74", Visibility = MaterialVisibility.Member }, Admin), "Materi video");
            Ok(CompetitionMaterial.Save(new CompetitionMaterial.NewRow { Title = "Dasar Rudiment Snare (PAS 40)", Description = "Materi umum latihan battery", MaterialType = MaterialType.Link, Url = "https://www.pas.org/resources/rudiments", Visibility = MaterialVisibility.Public }, Admin), "Materi umum");

            Activity Add(string key, Activity.NewRow row, ProgramStatus? status = null)
            {
                var activity = Ok(Activity.Save(row, Admin), "Kegiatan " + row.Title);
                if (status is ProgramStatus.Ongoing or ProgramStatus.Done)
                {
                    activity = Ok(activity.SetStatus(ProgramStatus.Ongoing, Admin), "Kegiatan berjalan");
                }
                if (status == ProgramStatus.Done)
                {
                    // Ditandai Selesai di akhir SeedCommittee: kegiatan Selesai mengunci data turunannya.
                    ActivitiesToFinish.Add(key);
                }
                Activities[key] = activity;
                return activity;
            }
            var campus = (Lat: -7.3333m, Lng: 112.7884m);
            Add("Latihan", new Activity.NewRow
            {
                ProgramId = Programs["Latihan"].Id, ActivityType = ActivityType.Latihan, AudienceType = AudienceType.SemuaAnggota, Title = "Latihan Rutin Gabungan",
                Description = "Latihan rutin setiap Selasa dan Jumat", Location = "Lapangan Giri Loka", Latitude = campus.Lat, Longitude = campus.Lng,
                StartAt = new DateTime(2026, 9, 1, 18, 30, 0), EndAt = new DateTime(2027, 7, 31, 21, 0, 0)
            }, ProgramStatus.Ongoing);
            Add("Rapat", new Activity.NewRow
            {
                ActivityType = ActivityType.Rapat, AudienceType = AudienceType.Pengurus, Title = "Rapat Pengurus Bulanan November",
                Location = "Sekretariat GWA", StartAt = DateTime.Today.AddHours(-1), EndAt = DateTime.Today.AddHours(22)
            }, ProgramStatus.Ongoing);
            Add("Brass", new Activity.NewRow
            {
                ProgramId = Programs["Latihan"].Id, ActivityType = ActivityType.Latihan, AudienceType = AudienceType.Section, SectionId = MusicSections["Brass"],
                Title = "Sectional Brass", Location = "Ruang Musik Giri Loka", StartAt = DateTime.Today.AddDays(2).AddHours(16), EndAt = DateTime.Today.AddDays(2).AddHours(18)
            });
            Add("Konser", new Activity.NewRow
            {
                ProgramId = Programs["Konser"].Id, ActivityType = ActivityType.Event, AudienceType = AudienceType.SemuaAnggota, Title = "Konser Gita Swara 2026",
                Description = "Konser tahunan Marching Band Gita Widya Agni bertema \"Nyala Nada\". Terbuka untuk umum.", Location = "Gedung Giri Loka UPN \"Veteran\" Jawa Timur",
                StartAt = DateTime.Today.AddDays(30).AddHours(18), EndAt = DateTime.Today.AddDays(30).AddHours(22), IsPublic = PublicVisibility.Public, IsFeatured = FeaturedStatus.Disorot
            });
            Add("Gladi", new Activity.NewRow
            {
                ParentActivityId = Activities["Konser"].Id, ActivityType = ActivityType.Latihan, AudienceType = AudienceType.TimPelaksana, Title = "Gladi Bersih Konser",
                Location = "Gedung Giri Loka", StartAt = DateTime.Today.AddDays(29).AddHours(15), EndAt = DateTime.Today.AddDays(29).AddHours(21)
            });
            Add("LombaJatim", new Activity.NewRow
            {
                ProgramId = Programs["Kejurnas"].Id, CompetitionId = jatim.Id, ActivityType = ActivityType.Lomba, AudienceType = AudienceType.PesertaTerdaftar,
                Title = "Keberangkatan Kejuaraan MB Jatim 2026", Location = "GOR Kertajaya, Surabaya", StartAt = new DateTime(2026, 10, 24, 6, 0, 0), EndAt = new DateTime(2026, 10, 25, 20, 0, 0),
                IsPublic = PublicVisibility.Public
            }, ProgramStatus.Done);
            Add("DiesNatalis", new Activity.NewRow
            {
                ActivityType = ActivityType.Event, AudienceType = AudienceType.SemuaAnggota, Title = "Penampilan Dies Natalis UPN \"Veteran\" Jawa Timur",
                Description = "Display marching band pada upacara Dies Natalis universitas", Location = "Lapangan Upacara UPN Jatim",
                StartAt = new DateTime(2026, 10, 5, 7, 0, 0), EndAt = new DateTime(2026, 10, 5, 11, 0, 0), IsPublic = PublicVisibility.Public, IsFeatured = FeaturedStatus.Disorot
            }, ProgramStatus.Done);
            Add("Workshop", new Activity.NewRow
            {
                ProgramId = Programs["Rekrutmen"].Id, ActivityType = ActivityType.Pertemuan, AudienceType = AudienceType.PesertaTerdaftar, Title = "Workshop Colour Guard bersama Alumni",
                Description = "Kelas teknik flag dan rifle dibawakan alumni", Location = "Aula Giri Loka",
                StartAt = new DateTime(2026, 10, 18, 9, 0, 0), EndAt = new DateTime(2026, 10, 18, 15, 0, 0), IsPublic = PublicVisibility.Public
            }, ProgramStatus.Done);
            Ok(TimelinePlans["Konser Gita Swara 2026"].LinkActivity(Activities["Konser"].Id, Admin), "Tautkan timeline");

            Ok(ActivityCollaborator.Save(new ActivityCollaborator.NewRow { ActivityId = Activities["Konser"].Id, ExternalPartyId = Parties["Suara"].Id, CollabRole = CollaboratorRole.Pendukung, Notes = "Media partner radio" }, Admin), "Kolaborator")
                .Respond(CollaboratorStatus.Diterima, "Siap meliput", Admin);
            Ok(ActivityCollaborator.Save(new ActivityCollaborator.NewRow { ActivityId = Activities["Konser"].Id, ExternalPartyId = Parties["Komunitas"].Id, CollabRole = CollaboratorRole.PenyelenggaraBersama }, Admin), "Kolaborator 2");
            Ok(ActivityCollaborator.Save(new ActivityCollaborator.NewRow { ActivityId = Activities["DiesNatalis"].Id, ExternalPartyId = Parties["Dispora"].Id, CollabRole = CollaboratorRole.Pendukung }, Admin), "Kolaborator 3")
                .Respond(CollaboratorStatus.Ditolak, "Bentrok jadwal", Admin);

            foreach (var i in new[] { 7, 12, 14, 16, 18, 20 })
            {
                var participant = Ok(ActivityParticipant.Save(new ActivityParticipant.NewRow { ActivityId = Activities["Workshop"].Id, MemberId = Members[i].Id }, Admin), "Peserta workshop");
                Ok(participant.Confirm(Admin), "Konfirmasi peserta");
            }
            Ok(ActivityParticipant.Save(new ActivityParticipant.NewRow { ActivityId = Activities["Workshop"].Id, MemberId = Members[8].Id, ParticipantRole = "PEMATERI" }, Admin), "Pemateri workshop").Confirm(Admin);
            foreach (var i in new[] { 0, 5, 6, 7, 8, 11, 12, 13 })
            {
                Ok(ActivityParticipant.Save(new ActivityParticipant.NewRow { ActivityId = Activities["LombaJatim"].Id, MemberId = Members[i].Id }, Admin), "Peserta lomba").Confirm(Admin);
            }

            var report = Ok(ActivityReport.Save(new ActivityReport.NewRow { ActivityId = Activities["DiesNatalis"].Id, Title = "Laporan Penampilan Dies Natalis 2026", Summary = "Penampilan berjalan lancar dengan 42 pemain. Evaluasi: perlu tambahan latihan transisi formasi." }, Admin), "Laporan kegiatan");
            Ok(report.UpdateFile(Pdf("Laporan Dies Natalis 2026"), "laporan-dies-natalis.pdf", "application/pdf", Admin), "File laporan");
            Ok(report.Submit(Admin), "Ajukan laporan");
            Ok(report.Approve(Admin), "Setujui laporan");
            var report2 = Ok(ActivityReport.Save(new ActivityReport.NewRow { ActivityId = Activities["LombaJatim"].Id, Title = "Laporan Kejuaraan MB Jatim 2026", Summary = "Juara 2 kategori umum." }, Admin), "Laporan lomba");
            Ok(report2.Submit(Admin), "Ajukan laporan lomba");
            Ok(report2.RequestRevision(Admin), "Revisi laporan lomba");

            var gallery = Ok(Gallery.Save(new Gallery.NewRow { ActivityId = Activities["DiesNatalis"].Id, Title = "Dies Natalis UPN Jatim 2026", Description = "Dokumentasi penampilan", EventDate = D(2026, 10, 5), IsFeatured = FeaturedStatus.Disorot }, Admin), "Galeri");
            for (var i = 0; i < 5; i++)
            {
                Ok(GalleryPhoto.Save(new GalleryPhoto.NewRow { GalleryId = gallery.Id, Data = Photo(i), Caption = $"Formasi {i + 1}" }, Admin), "Foto galeri");
            }
            Ok(gallery.Publish(Admin), "Terbitkan galeri");
            var gallery2 = Ok(Gallery.Save(new Gallery.NewRow { ActivityId = Activities["LombaJatim"].Id, Title = "Kejuaraan MB Jatim 2026", EventDate = D(2026, 10, 24) }, Admin), "Galeri lomba");
            for (var i = 5; i < 8; i++)
            {
                Ok(GalleryPhoto.Save(new GalleryPhoto.NewRow { GalleryId = gallery2.Id, Data = Photo(i), Caption = "Kejuaraan Jatim" }, Admin), "Foto galeri lomba");
            }
            Ok(gallery2.Publish(Admin), "Terbitkan galeri lomba");
            Ok(Gallery.Save(new Gallery.NewRow { Title = "Latihan Persiapan Konser (draft)", IsPublic = PublicVisibility.Private }, Admin), "Galeri draft");
        }

        // ---------------------------------------------------------------- Tim pelaksana
        private static void SeedCommittee()
        {
            var concert = Activities["Konser"];
            var seq = 1;
            foreach (var name in new[] { "Acara", "Humas", "Logistik", "Konsumsi", "Keamanan" })
            {
                Sections[name] = Ok(CommitteeSection.Save(new CommitteeSection.NewRow { ActivityId = concert.Id, Name = name, Description = "Sie " + name, SeqNo = seq++ }, Admin), "Sie " + name).Id;
            }
            void AddMember(int member, CommitteeRole role, string? section, string job)
            {
                Ok(CommitteeMember.Save(new CommitteeMember.NewRow { ActivityId = concert.Id, MemberId = Members[member].Id, CommitteeRole = role, SectionId = section == null ? null : Sections[section], JobDesc = job }, Admin), "Panitia " + Members[member].Fullname);
            }
            AddMember(11, CommitteeRole.KetuaPelaksana, null, "Penanggung jawab konser");
            AddMember(12, CommitteeRole.Sekretaris, null, "Administrasi & surat");
            AddMember(14, CommitteeRole.Bendahara, null, "Keuangan konser");
            AddMember(13, CommitteeRole.KoordinatorSie, "Acara", "Susunan acara");
            AddMember(10, CommitteeRole.KoordinatorSie, "Humas", "Publikasi");
            AddMember(9, CommitteeRole.KoordinatorSie, "Logistik", "Alat & panggung");
            AddMember(16, CommitteeRole.KoordinatorSie, "Konsumsi", "Konsumsi");
            AddMember(17, CommitteeRole.Staf, "Logistik", "Angkut alat");
            AddMember(18, CommitteeRole.Staf, "Humas", "Dokumentasi");
            AddMember(5, CommitteeRole.Pelatih, null, "Pelatih brass");

            // Open recruitment panitia dengan formulir tambahan.
            var form = Ok(Form.Save(new Form.NewRow
            {
                FormType = FormType.PendaftaranRekrutmen, Title = "Formulir Volunteer Konser Gita Swara 2026",
                Questions = new()
                {
                    new() { QuestionText = "Alasan ingin menjadi panitia", QuestionType = QuestionType.Paragraf, IsRequired = AnswerRequirement.Wajib },
                    new() { QuestionText = "Tanggal mulai bisa membantu", QuestionType = QuestionType.Tanggal }
                }
            }, Admin), "Form volunteer");
            form = Ok(form.Open(Admin), "Buka form volunteer");
            var questions = FormQuestion.GetDataList(form.Id);
            var recruitment = Ok(CommitteeRecruitment.Save(new CommitteeRecruitment.NewRow { ActivityId = concert.Id, OpenAt = DateTime.Today.AddDays(-3), CloseAt = DateTime.Today.AddDays(10).AddHours(23), Quota = 10, FormId = form.Id }, Admin), "Rekrutmen panitia");
            recruitment = Ok(recruitment.Open(Admin), "Buka rekrutmen panitia");
            var applications = new List<CommitteeApplication>();
            foreach (var (member, first, second) in new[] { (19, "Keamanan", "Logistik"), (20, "Konsumsi", (string?)null), (21, "Humas", "Acara") })
            {
                var response = Ok(FormResponse.Submit(new FormResponse.NewRow
                {
                    FormId = form.Id,
                    Answers = new()
                    {
                        new() { QuestionId = questions[0].Id, AnswerText = "Ingin belajar mengelola event besar." },
                        new() { QuestionId = questions[1].Id, AnswerText = Today.AddDays(3).ToString("yyyy-MM-dd") }
                    }
                }, UserOf(Members[member])), "Jawaban form volunteer");
                applications.Add(Ok(CommitteeApplication.Apply(new CommitteeApplication.NewRow
                {
                    RecruitmentId = recruitment.Id, MemberId = Members[member].Id, SectionChoice1Id = Sections[first], SectionChoice2Id = second == null ? null : Sections[second],
                    Reason = "Siap membantu", FormResponseId = response.Id
                }, UserOf(Members[member])), "Lamaran panitia"));
            }
            Ok(applications[0].Accept(null, "Diterima di sie keamanan", Admin), "Terima lamaran");
            Ok(applications[1].Reject("Kuota sie konsumsi penuh", Admin), "Tolak lamaran");

            var rundown = new (int Day, int StartH, int StartM, int EndH, int EndM, string Title, string? Section, int Pic)[]
            {
                (0, 15, 0, 16, 30, "Persiapan & check sound", "Logistik", 9),
                (0, 17, 0, 18, 0, "Open gate penonton", "Keamanan", 19),
                (0, 18, 0, 18, 15, "Pembukaan oleh MC", "Acara", 13),
                (0, 18, 15, 19, 30, "Penampilan sesi 1: Nyala", "Acara", 13),
                (0, 19, 45, 21, 0, "Penampilan sesi 2: Nada", "Acara", 13),
                (0, 21, 0, 21, 30, "Penutupan & foto bersama", "Humas", 10)
            };
            seq = 1;
            foreach (var r in rundown)
            {
                Ok(ActivityRundown.Save(new ActivityRundown.NewRow
                {
                    ActivityId = concert.Id, RundownDate = DateOnly.FromDateTime(concert.StartAt).AddDays(r.Day), StartTime = new TimeOnly(r.StartH, r.StartM), EndTime = new TimeOnly(r.EndH, r.EndM),
                    Title = r.Title, LocationName = "Gedung Giri Loka", SectionId = r.Section == null ? null : Sections[r.Section], PicMemberId = Members[r.Pic].Id, SeqNo = seq++
                }, Admin), "Rundown " + r.Title);
            }

            Ok(ActivityChecklist.GenerateFromTemplate(concert.Id, EventChecklist.Id, Admin), "Checklist dari template");
            var checklist = Ok(ActivityChecklist.Save(new ActivityChecklist.NewRow { ActivityId = concert.Id, SectionId = Sections["Humas"], Title = "Kirim undangan rektorat", DueDate = Today.AddDays(7), PicMemberId = Members[12].Id, Priority = Priority.Tinggi }, Admin), "Checklist manual");
            checklist = Ok(checklist.SetStatus(TaskProgressStatus.Proses, Admin), "Checklist proses");
            Ok(checklist.SetStatus(TaskProgressStatus.Selesai, Admin), "Checklist selesai");

            var trip = Activities["LombaJatim"];
            foreach (var (i, role, vehicle) in new[] { (0, TripRole.Peserta, "Bus 1"), (5, TripRole.Peserta, "Bus 1"), (6, TripRole.Peserta, "Bus 1"), (9, TripRole.Kru, "Truk alat") })
            {
                var member = Ok(ActivityTripMember.Save(new ActivityTripMember.NewRow
                {
                    ActivityId = trip.Id, MemberId = Members[i].Id, TripRole = role, Vehicle = vehicle, Room = "Kamar " + (i + 1),
                    EmergencyContactName = "Orang tua " + Members[i].Fullname.Split(' ')[0], EmergencyContactPhone = "0811" + (30000000 + i * 54321)
                }, Admin), "Rombongan");
                if (i != 9)
                {
                    Ok(member.UpdatePermissionFile(Pdf("Surat izin orang tua " + Members[i].Fullname), "izin.pdf", "application/pdf", Admin), "Izin orang tua");
                }
            }
            Ok(ActivityTripMember.Save(new ActivityTripMember.NewRow { ActivityId = trip.Id, GuestName = "Coach Hendra (pelatih tamu)", TripRole = TripRole.Pelatih, Vehicle = "Mobil official" }, Admin), "Rombongan tamu");

            foreach (var key in ActivitiesToFinish)
            {
                Activities[key] = Ok(Activities[key].SetStatus(ProgramStatus.Done, Admin), "Kegiatan selesai " + key);
            }
        }

        // ---------------------------------------------------------------- Presensi & notulen
        private static void SeedAttendance()
        {
            var latihan = Activities["Latihan"];
            foreach (var (day, place) in new[] { (ScheduleDay.Selasa, "Lapangan Giri Loka"), (ScheduleDay.Jumat, "Lapangan Giri Loka") })
            {
                Ok(ActivitySchedule.Save(new ActivitySchedule.NewRow
                {
                    ActivityId = latihan.Id, DayOfWeek = day, StartTime = new TimeOnly(18, 30), EndTime = new TimeOnly(21, 0), Location = place,
                    Latitude = latihan.Latitude, Longitude = latihan.Longitude, RadiusMeter = 300, LateToleranceMinute = 15, ValidFrom = D(2026, 9, 1), ValidUntil = D(2027, 7, 31)
                }, Admin), "Jadwal latihan");
            }
            Ok(ActivitySchedule.GenerateSessions(Today.AddDays(-14), Today.AddDays(7), latihan.Id, Admin), "Generate sesi latihan");

            // Sesi latihan yang sudah lewat: dibuka, dicatat manual, lalu ditutup (sisanya Alpa otomatis).
            var past = AttendanceSession.GetDataList(latihan.Id).Where(s => s.OpenAt < DateTime.Now.AddDays(-1)).OrderBy(s => s.OpenAt).Take(2).ToList();
            foreach (var session in past)
            {
                var opened = Ok(session.Open(Admin), "Buka sesi latihan");
                for (var i = 0; i < 18; i++)
                {
                    var status = i % 9 == 4 ? AttendanceStatus.Izin : i % 9 == 7 ? AttendanceStatus.Sakit : i % 6 == 5 ? AttendanceStatus.Terlambat : AttendanceStatus.Hadir;
                    Ok(AttendanceRecord.SetManual(opened.Id, Members[i].Id, status, status == AttendanceStatus.Izin ? "Ujian praktikum" : null, Admin), "Presensi manual");
                }
                Ok(opened.Close(Admin), "Tutup sesi latihan");
            }

            // Latihan rutin diikuti otomatis semua anggota; anggota mengajukan tidak ikut satu pertemuan / seluruh kegiatan.
            var next = AttendanceSession.GetDataList(latihan.Id).Where(s => s.Status == AttendanceSessionStatus.Scheduled && s.OpenAt > DateTime.Now)
                .OrderBy(s => s.OpenAt).FirstOrDefault();
            if (next != null)
            {
                var izin1 = Ok(ActivityAbsenceRequest.Submit(new ActivityAbsenceRequest.NewRow { ActivityId = latihan.Id, SessionId = next.Id, Reason = "Ujian tengah semester praktikum" }, UserOf(Members[19])), "Izin sesi");
                Ok(izin1.Approve("Semoga lancar ujiannya", Admin), "Setujui izin sesi");
                var izin2 = Ok(ActivityAbsenceRequest.Submit(new ActivityAbsenceRequest.NewRow { ActivityId = latihan.Id, SessionId = next.Id, Reason = "Ada acara keluarga" }, UserOf(Members[21])), "Izin sesi 2");
                Ok(izin2.Reject("Sesi ini persiapan lomba, wajib hadir", Admin), "Tolak izin sesi");
            }
            Ok(ActivityAbsenceRequest.Submit(new ActivityAbsenceRequest.NewRow { ActivityId = latihan.Id, Reason = "Magang di luar kota sampai Januari" }, UserOf(Members[20])), "Izin seluruh kegiatan");

            // Rapat: QR + foto check-in oleh pengurus, notulen.
            var rapat = Ok(AttendanceSession.Save(new AttendanceSession.NewRow
            {
                ActivityId = Activities["Rapat"].Id, Name = "Rapat Pengurus November", OpenAt = DateTime.Now.AddMinutes(-30), CloseAt = DateTime.Now.AddHours(3), LateToleranceMinute = 15, Location = "Sekretariat GWA",
                AttendanceMode = AttendanceMode.Qr, Latitude = latihan.Latitude, Longitude = latihan.Longitude, RadiusMeter = 300
            }, Admin), "Sesi rapat");
            rapat = Ok(rapat.Open(Admin), "Buka sesi rapat");
            for (var i = 0; i < 8; i++)
            {
                Ok(AttendanceRecord.CheckIn(new AttendanceRecord.CheckRow { QrToken = rapat.QrToken!, Latitude = latihan.Latitude, Longitude = latihan.Longitude, Photo = Avatar(i) }, UserOf(Members[i])), "Check-in rapat");
            }
            Ok(AttendanceRecord.CheckOut(new AttendanceRecord.CheckRow { QrToken = rapat.QrToken!, Latitude = latihan.Latitude, Longitude = latihan.Longitude, Photo = Avatar(0) }, UserOf(Members[0])), "Check-out rapat");
            Ok(AttendanceRecord.SetManual(rapat.Id, Members[9].Id, AttendanceStatus.Izin, "Sakit flu", Admin), "Izin rapat");
            Ok(rapat.Close(Admin), "Tutup sesi rapat");
            var minutes = Ok(MeetingMinutes.Save(new MeetingMinutes.NewRow
            {
                SessionId = rapat.Id, ChairedByMemberId = Ketua.Id, NotulisMemberId = Sekretaris.Id,
                Agenda = "1. Evaluasi Dies Natalis\n2. Persiapan Konser Gita Swara\n3. Laporan keuangan",
                Content = "Rapat dibuka pukul 19.00 oleh Ketua Umum. Evaluasi penampilan Dies Natalis berjalan baik. Persiapan konser sudah 40%.",
                Decisions = "Konser dilaksanakan sesuai jadwal; anggaran konser diajukan minggu ini.",
                FollowUp = "Sie Humas menyiapkan poster paling lambat 7 hari lagi."
            }, Admin), "Notulen");
            Ok(minutes.UpdateFile(Pdf("Notulen Rapat Pengurus November 2026"), "notulen-november.pdf", "application/pdf", Admin), "File notulen");
            Ok(minutes.Finalize(Admin), "Finalisasi notulen");

            // Sesi lain: dibatalkan.
            var cancelled = Ok(AttendanceSession.Save(new AttendanceSession.NewRow { ActivityId = Activities["Brass"].Id, Name = "Sectional Brass (dibatalkan)", OpenAt = DateTime.Today.AddDays(2).AddHours(16), CloseAt = DateTime.Today.AddDays(2).AddHours(18), Latitude = latihan.Latitude, Longitude = latihan.Longitude, RadiusMeter = 300 }, Admin), "Sesi brass");
            Ok(cancelled.Cancel("Ruang musik dipakai ujian", Admin), "Batalkan sesi");
        }
    }
}
