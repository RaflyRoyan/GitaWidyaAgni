using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Formulir;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;

namespace GWASeeder
{
    internal static partial class Seed
    {
        public static WorkProgram ProgramOldConcert = null!;
        public static readonly Dictionary<string, int> Assignments = new(); // nama jabatan / anggota → id penugasan aktif
        public static int ApplicantAcceptedMemberId;

        // ---------------------------------------------------------------- Periode lama & serah terima
        private static void SeedPreviousPeriod()
        {
            var start = D(2025, 9, 1);
            Ok(ManagementAssignment.Save(new ManagementAssignment.NewRow { PeriodId = PeriodOld.Id, MemberId = KetuaLama.Id, PositionId = Positions["Ketua Umum"], SkNumber = "SK/01/GWA/IX/2025", StartDate = start }, Admin), "Ketua lama");
            Ok(ManagementAssignment.Save(new ManagementAssignment.NewRow { PeriodId = PeriodOld.Id, MemberId = Ketua.Id, PositionId = Positions["Wakil Ketua"], SkNumber = "SK/01/GWA/IX/2025", StartDate = start }, Admin), "Wakil lama");
            Ok(ManagementAssignment.Save(new ManagementAssignment.NewRow { PeriodId = PeriodOld.Id, MemberId = Sekretaris.Id, PositionId = Positions["Sekretaris"], StartDate = start }, Admin), "Sekretaris lama");
            Ok(ManagementAssignment.Save(new ManagementAssignment.NewRow { PeriodId = PeriodOld.Id, MemberId = Bendahara.Id, PositionId = Positions["Bendahara"], StartDate = start }, Admin), "Bendahara lama");
            Ok(DivisionMember.Save(new DivisionMember.NewRow { DivisionId = Divisions["Battery"], MemberId = KetuaLama.Id, PeriodId = PeriodOld.Id, MemberRole = "Koordinator", JoinedAt = start }, Admin), "Divisi lama");

            ProgramOldConcert = Ok(WorkProgram.Save(new WorkProgram.NewRow
            {
                PeriodId = PeriodOld.Id, PicMemberId = KetuaLama.Id, Title = "Konser Akhir Tahun 2025",
                Description = "Konser tahunan penutup periode 2025/2026", StartDate = D(2025, 10, 1), EndDate = D(2025, 12, 20), BudgetAmount = 12_000_000
            }, Admin), "Program lama");
            ProgramOldConcert = Ok(ProgramOldConcert.SetStatus(ProgramStatus.Ongoing, Admin), "Program lama berjalan");
            ProgramOldConcert = Ok(ProgramOldConcert.SetStatus(ProgramStatus.Done, Admin), "Program lama selesai");

            var handover = Ok(PeriodHandover.Save(new PeriodHandover.NewRow
            {
                FromPeriodId = PeriodOld.Id, ToPeriodId = PeriodActive.Id, HandoverDate = D(2026, 8, 30),
                HandedByMemberId = KetuaLama.Id, ReceivedByMemberId = Ketua.Id, Notes = "Serah terima kepengurusan dalam Musyawarah Besar 2026",
                Items = new()
                {
                    new() { ItemType = HandoverItemType.Saldo, RefId = Accounts["Bank"], Description = "Saldo rekening BNI", Amount = 12_000_000 },
                    new() { ItemType = HandoverItemType.Saldo, RefId = Accounts["Kas"], Description = "Saldo kas tunai", Amount = 2_500_000 },
                    new() { ItemType = HandoverItemType.Aset, RefId = Assets["Trumpet"].Id, Description = "Trumpet Yamaha", Quantity = 6, ItemCondition = ItemCondition.Baik },
                    new() { ItemType = HandoverItemType.Aset, RefId = Assets["Snare"].Id, Description = "Snare drum", Quantity = 5, ItemCondition = ItemCondition.Baik },
                    new() { ItemType = HandoverItemType.Program, RefId = ProgramOldConcert.Id, Description = "Laporan Konser Akhir Tahun 2025" },
                    new() { ItemType = HandoverItemType.Lainnya, Description = "Kunci sekretariat dan lemari arsip", Quantity = 3 }
                }
            }, Admin), "Serah terima");
            Ok(handover.UpdateLpj(Pdf("LPJ Kepengurusan 2025/2026"), "lpj-2025-2026.pdf", Admin), "LPJ");
            Ok(handover.Submit(Admin), "Ajukan serah terima");
            Ok(handover.Approve(Admin), "Setujui serah terima");
            PeriodActive = ManagementPeriod.GetActive() ?? throw new InvalidOperationException("Periode baru tidak aktif setelah serah terima");
        }

        // ---------------------------------------------------------------- Kepengurusan periode aktif
        private static void SeedActivePeriod()
        {
            var start = D(2026, 9, 1);
            void Assign(Member member, string position, string? division = null)
            {
                var row = Ok(ManagementAssignment.Save(new ManagementAssignment.NewRow
                {
                    PeriodId = PeriodActive.Id, MemberId = member.Id, PositionId = Positions[position],
                    DivisionId = division == null ? null : Divisions[division], SkNumber = "SK/02/GWA/IX/2026", StartDate = start
                }, Admin), $"Penugasan {position} {member.Fullname}");
                Assignments[member.Fullname] = row.Id;
            }
            Assign(Ketua, "Ketua Umum");
            Assign(Wakil, "Wakil Ketua");
            Assign(Sekretaris, "Sekretaris");
            Assign(Bendahara, "Bendahara");
            var coordinators = new (int Member, string Division)[] { (5, "Brass"), (6, "Battery"), (7, "Colour Guard"), (8, "Front Ensemble"), (9, "Logistik"), (10, "Humas & Media") };
            foreach (var (index, division) in coordinators)
            {
                Assign(Members[index], "Koordinator Divisi", division);
                Ok(DivisionMember.Save(new DivisionMember.NewRow { DivisionId = Divisions[division], MemberId = Members[index].Id, PeriodId = PeriodActive.Id, MemberRole = "Koordinator", JoinedAt = start }, Admin), "Koordinator divisi");
            }
            // Anggota tersebar ke divisi musik.
            var musicDivisions = new[] { "Brass", "Battery", "Colour Guard", "Front Ensemble" };
            for (var i = 11; i < 22; i++)
            {
                Ok(DivisionMember.Save(new DivisionMember.NewRow { DivisionId = Divisions[musicDivisions[i % 4]], MemberId = Members[i].Id, PeriodId = PeriodActive.Id, JoinedAt = start }, Admin), "Anggota divisi");
            }
            Ok(DivisionMember.Save(new DivisionMember.NewRow { DivisionId = Divisions["Danus"], MemberId = Members[12].Id, PeriodId = PeriodActive.Id, MemberRole = "Anggota Tim", JoinedAt = start }, Admin), "Tim danus");
            Ok(DivisionMember.Save(new DivisionMember.NewRow { DivisionId = Divisions["Danus"], MemberId = Members[14].Id, JoinedAt = start }, Admin), "Tim danus tanpa periode");
        }

        // ---------------------------------------------------------------- Rekrutmen
        private static void SeedRecruitment()
        {
            var recruitment = Ok(Recruitment.Save(new Recruitment.NewRow
            {
                PeriodId = PeriodActive.Id, Title = "Open Recruitment Anggota Baru 2026",
                Description = "Pendaftaran anggota baru Marching Band Gita Widya Agni. Terbuka untuk mahasiswa aktif UPN \"Veteran\" Jawa Timur dan umum (pelatih/alumni SMA).",
                OpenDate = DateTime.Today.AddDays(-10), CloseDate = DateTime.Today.AddDays(20).AddHours(23), Quota = 40, AcceptPublic = RecruitmentAudience.TermasukUmum
            }, Admin), "Rekrutmen");
            recruitment = Ok(recruitment.Open(Admin), "Buka rekrutmen");

            var form = Ok(Form.Save(new Form.NewRow
            {
                FormType = FormType.PendaftaranRekrutmen, RecruitmentId = recruitment.Id, Title = "Pertanyaan Tambahan Rekrutmen 2026",
                Questions = new()
                {
                    new() { QuestionText = "Pilihan divisi yang diminati", QuestionType = QuestionType.PilihanGanda, Options = new() { "Brass", "Battery", "Front Ensemble", "Colour Guard" }, IsRequired = AnswerRequirement.Wajib },
                    new() { QuestionText = "Pengalaman bermusik sebelumnya", QuestionType = QuestionType.Paragraf },
                    new() { QuestionText = "Alat musik yang dikuasai", QuestionType = QuestionType.KotakCentang, Options = new() { "Trumpet", "Snare", "Bass Drum", "Keyboard", "Belum ada" } },
                    new() { QuestionText = "Seberapa siap mengikuti latihan 2x seminggu?", QuestionType = QuestionType.Skala1Sampai5, IsRequired = AnswerRequirement.Wajib }
                }
            }, Admin), "Form rekrutmen");
            form = Ok(form.Open(Admin), "Buka form rekrutmen");
            var questions = FormQuestion.GetDataList(form.Id);

            var applicants = new (MemberCategory Category, string? Nim, string Name, string Email, string Prodi, string? Institution, string Division, string Motivation)[]
            {
                (MemberCategory.Mahasiswa, "26081010101", "Yusuf Alfarizi", "yusuf.alfarizi@student.upnjatim.ac.id", "Informatika", null, "Battery", "Ingin melanjutkan hobi drumband sejak SMA"),
                (MemberCategory.Mahasiswa, "26032010202", "Zahra Khairunnisa", "zahra.k@student.upnjatim.ac.id", "Manajemen", null, "Colour Guard", "Tertarik dengan colour guard setelah menonton konser GWA"),
                (MemberCategory.Mahasiswa, "26041010303", "Arif Budiman", "arif.budiman@student.upnjatim.ac.id", "Teknik Sipil", null, "Brass", "Ingin belajar trumpet"),
                (MemberCategory.Umum, null, "Bella Anggraini", "bella.anggraini@gmail.com", "Seni Musik", "Universitas Negeri Surabaya", "Front Ensemble", "Membantu sebagai pemain keyboard tamu")
            };
            var applicantRows = new List<RecruitmentApplicant>();
            foreach (var a in applicants)
            {
                var applicant = Ok(RecruitmentApplicant.Apply(new RecruitmentApplicant.NewRow
                {
                    RecruitmentId = recruitment.Id, ApplicantCategory = a.Category, Nim = a.Nim, Fullname = a.Name, Email = a.Email,
                    Phone = "0813" + (40000000 + applicantRows.Count * 1111111), ProgramStudi = a.Prodi, Angkatan = 2026, Institution = a.Institution, Motivation = a.Motivation
                }), "Pendaftar " + a.Name);
                applicantRows.Add(applicant);
                Ok(FormResponse.SubmitApplicant(new FormResponse.ApplicantRow
                {
                    FormId = form.Id, ApplicantId = applicant.Id, ApplicantEmail = a.Email,
                    Answers = new()
                    {
                        new() { QuestionId = questions[0].Id, AnswerText = a.Division },
                        new() { QuestionId = questions[1].Id, AnswerText = "Pernah ikut drumband sekolah selama 2 tahun." },
                        new() { QuestionId = questions[2].Id, Choices = new() { a.Division == "Brass" ? "Trumpet" : "Snare" } },
                        new() { QuestionId = questions[3].Id, AnswerText = "4" }
                    }
                }), "Jawaban form pendaftar");
            }
            Ok(applicantRows[0].Review(ApplicantStatus.Screening, "Berkas lengkap", Admin), "Seleksi berkas");
            Ok(applicantRows[0].Review(ApplicantStatus.Interview, "Lanjut wawancara", Admin), "Wawancara");
            var accepted = Ok(applicantRows[0].Review(ApplicantStatus.Accepted, "Diterima di divisi Battery", Admin), "Terima pendaftar");
            ApplicantAcceptedMemberId = accepted.MemberId ?? 0;
            Ok(applicantRows[1].Review(ApplicantStatus.Screening, "Berkas lengkap", Admin), "Seleksi berkas 2");
            Ok(applicantRows[2].Review(ApplicantStatus.Rejected, "Jadwal latihan bentrok dengan praktikum", Admin), "Tolak pendaftar");
        }
    }
}
