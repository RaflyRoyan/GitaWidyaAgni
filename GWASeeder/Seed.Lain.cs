using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Akses;
using GWALib.Lib.Transaksi.Approval;
using GWALib.Lib.Transaksi.EVoting;
using GWALib.Lib.Transaksi.Formulir;
using GWALib.Lib.Transaksi.Information;
using GWALib.Lib.Transaksi.Inventaris;
using GWALib.Lib.Transaksi.Prestasi;
using GWALib.Lib.Transaksi.Surat;
using GWAShared.Enums;

namespace GWASeeder
{
    internal static partial class Seed
    {
        public static int LetterBorrowId;

        // ---------------------------------------------------------------- Inventaris
        private static void SeedInventory()
        {
            List<string> Units(string asset, int count) =>
                AssetUnit.GetDataList(Assets[asset].Id, AssetUnitStatus.Tersedia).Take(count).Select(x => x.UnitCode).ToList();

            // Peminjaman oleh anggota (gratis), sudah dikembalikan.
            var internalBorrow = Ok(BorrowRequest.Save(new BorrowRequest.NewRow
            {
                BorrowerType = BorrowerType.Anggota, MemberId = Members[13].Id, Purpose = "Latihan mandiri di rumah", BorrowDate = DateTime.Now.AddDays(-9), ExpectedReturnDate = DateTime.Now.AddDays(-6),
                RentalScheme = RentalScheme.Gratis, Items = new() { new() { AssetId = Assets["Trumpet"].Id, Quantity = 1 } }
            }, Admin), "Pinjam anggota");
            internalBorrow = Ok(internalBorrow.Approve(UserOf(Members[9])), "Setujui pinjam anggota");
            var item = BorrowItem.GetDataList(internalBorrow.Id).Single();
            var trumpetCode = Units("Trumpet", 1).Single();
            internalBorrow = Ok(internalBorrow.Checkout(new() { new() { BorrowItemId = item.Id, UnitCodes = new() { trumpetCode } } }, UserOf(Members[9])), "Checkout pinjam anggota");
            Ok(BorrowItemUnit.ReturnUnit(internalBorrow.Id, trumpetCode, ItemCondition.Baik, false, "Kembali tepat waktu", UserOf(Members[9])), "Kembalikan trumpet");

            // Penyewaan oleh SMA mitra (tarif tetap), dikembalikan terlambat, dibayar.
            var smaContact = GWALib.Lib.Master.Mitra.ExternalPartyContact.GetDataList(FilterAktif.Aktif, Parties["SMA"].Id).First();
            var rental = Ok(BorrowRequest.Save(new BorrowRequest.NewRow
            {
                BorrowerType = BorrowerType.OrganisasiInstansi, ExternalPartyId = Parties["SMA"].Id, ContactId = smaContact.Id, BorrowerPhone = "081299990001",
                Guarantee = "KTP pembina ekstrakurikuler", Purpose = "Lomba drumband antar SMA", BorrowDate = DateTime.Now.AddDays(-12), ExpectedReturnDate = DateTime.Now.AddDays(-8),
                RentalScheme = RentalScheme.TarifTetap, Items = new() { new() { AssetId = Assets["Snare"].Id, Quantity = 2 }, new() { AssetId = Assets["Bendera"].Id, Quantity = 10 } }
            }, Admin), "Sewa SMA");
            rental = Ok(rental.Approve(Admin), "Setujui sewa");
            var rentalItems = BorrowItem.GetDataList(rental.Id);
            var snareItem = rentalItems.Single(x => x.AssetId == Assets["Snare"].Id);
            var flagItem = rentalItems.Single(x => x.AssetId == Assets["Bendera"].Id);
            var snareCodes = Units("Snare", 2);
            rental = Ok(rental.Checkout(new() { new() { BorrowItemId = snareItem.Id, UnitCodes = snareCodes }, new() { BorrowItemId = flagItem.Id } }, Admin), "Checkout sewa");
            Ok(BorrowItemUnit.ReturnUnit(rental.Id, snareCodes[0], ItemCondition.Baik, false, null, Admin), "Kembali snare 1");
            Ok(BorrowItemUnit.ReturnUnit(rental.Id, snareCodes[1], ItemCondition.RusakRingan, false, "Kulit drum sobek kecil", Admin), "Kembali snare 2");
            Ok(flagItem.ReturnQuantity(9, 1, ItemCondition.Baik, "1 bendera hilang", Admin), "Kembali bendera");
            rental = Ok(new BorrowRequest(rental.EncDecrKey).ReceivePayment(Accounts["Kas"], Categories["Sewa Alat"], rental.TotalDue > 0 ? Math.Min(300_000, rental.TotalDue) : 100_000, Today, Admin), "Bayar sewa");

            // Peminjaman perorangan berjalan (belum kembali) dan satu masih menunggu persetujuan.
            var personal = Ok(BorrowRequest.Save(new BorrowRequest.NewRow
            {
                BorrowerType = BorrowerType.PeroranganLuar, BorrowerName = "Rendra Wijaya", BorrowerInstitution = "Band Indie Rungkut", BorrowerPhone = "085711112222",
                Guarantee = "KTP", Purpose = "Rekaman video musik", BorrowDate = DateTime.Now.AddDays(-1), ExpectedReturnDate = DateTime.Now.AddDays(2),
                RentalScheme = RentalScheme.TarifTetap, Items = new() { new() { AssetId = Assets["Sound"].Id, Quantity = 1 } }
            }, Admin), "Pinjam perorangan");
            personal = Ok(personal.Approve(Admin), "Setujui pinjam perorangan");
            var soundItem = BorrowItem.GetDataList(personal.Id).Single();
            Ok(personal.Checkout(new() { new() { BorrowItemId = soundItem.Id, UnitCodes = Units("Sound", 1) } }, Admin), "Checkout sound");
            Ok(BorrowRequest.Save(new BorrowRequest.NewRow
            {
                BorrowerType = BorrowerType.Anggota, MemberId = Members[20].Id, Purpose = "Dokumentasi latihan", BorrowDate = DateTime.Now.AddDays(3), ExpectedReturnDate = DateTime.Now.AddDays(4),
                RentalScheme = RentalScheme.Seikhlasnya, Items = new() { new() { AssetId = Assets["Tenda"].Id, Quantity = 1 } }
            }, Admin), "Pinjam menunggu");

            Ok(AssetMovement.Save(new AssetMovement.NewRow { AssetId = Assets["Bendera"].Id, MovementType = AssetMovementType.In, Quantity = 6, Notes = "Bendera baru dari konveksi" }, Admin), "Mutasi masuk");
            Ok(AssetMovement.Save(new AssetMovement.NewRow { AssetId = Assets["Tenda"].Id, MovementType = AssetMovementType.Adjustment, Quantity = -1, Notes = "Rangka patah, dikeluarkan dari stok" }, Admin), "Mutasi penyesuaian");

            var unitForInspection = AssetUnit.GetDataList(Assets["Mellophone"].Id, AssetUnitStatus.Tersedia).First();
            Ok(AssetInspection.Save(new AssetInspection.NewRow { AssetId = Assets["Mellophone"].Id, AssetUnitId = unitForInspection.Id, ItemCondition = ItemCondition.RusakRingan, Notes = "Piston macet", Photo = Photo(3) }, UserOf(Members[9])), "Inspeksi mellophone");
            Ok(AssetInspection.Save(new AssetInspection.NewRow { AssetId = Assets["Bendera"].Id, ItemCondition = ItemCondition.Baik, Notes = "Stock opname bulanan" }, UserOf(Members[9])), "Inspeksi bendera");

            var maintenance = Ok(MaintenanceLog.Save(new MaintenanceLog.NewRow
            {
                AssetId = Assets["Mellophone"].Id, AssetUnitId = unitForInspection.Id, MaintenanceDate = Today.AddDays(-3), Description = "Servis piston dan pembersihan", Vendor = "Yamaha Music Surabaya", Cost = 350_000
            }, Admin), "Perawatan mellophone");
            maintenance = Ok(maintenance.Start(Admin), "Mulai perawatan");
            Ok(maintenance.Finish(ItemCondition.Baik, Accounts["Kas"], Categories["Perawatan Alat"], Admin), "Selesai perawatan");
            Ok(MaintenanceLog.Save(new MaintenanceLog.NewRow { AssetId = Assets["Tenda"].Id, MaintenanceDate = Today.AddDays(10), Description = "Ganti terpal tenda", Cost = 200_000 }, Admin), "Perawatan terjadwal");
        }

        // ---------------------------------------------------------------- Surat & dokumen
        private static void SeedLetters()
        {
            var invitation = Ok(Letter.Save(new Letter.NewRow
            {
                Direction = LetterDirection.Out, PeriodId = PeriodActive.Id, TemplateId = LetterTemplates["Undangan"].Id, Subject = "Undangan Konser Gita Swara 2026",
                Recipient = "Rektor UPN \"Veteran\" Jawa Timur", Summary = "Undangan menghadiri konser tahunan"
            }, UserOf(Sekretaris)), "Surat undangan");
            invitation = Ok(invitation.Submit(UserOf(Sekretaris)), "Ajukan surat");
            invitation = Ok(invitation.Approve("Sudah sesuai", null, null, UserOf(Sekretaris)), "Periksa surat");
            invitation = Ok(invitation.Approve("Setuju", Image("assets/images/gwa-logo-sm.png"), null, KetuaUser), "Tanda tangan surat");
            Ok(invitation.UpdateFile(Pdf("Surat Undangan Konser - final"), "undangan-konser.pdf", "application/pdf", UserOf(Sekretaris)), "File surat");
            invitation = Ok(invitation.MarkSent(UserOf(Sekretaris)), "Kirim surat");

            var borrowLetter = Ok(Letter.Save(new Letter.NewRow
            {
                Direction = LetterDirection.Out, TemplateId = LetterTemplates["Peminjaman"].Id, Subject = "Permohonan Peminjaman Gedung Giri Loka", Recipient = "Kepala Biro Umum UPN Jatim",
                Body = "<p>Kami bermaksud meminjam Gedung Giri Loka untuk Konser Gita Swara 2026.</p>"
            }, UserOf(Sekretaris)), "Surat peminjaman");
            borrowLetter = Ok(borrowLetter.Submit(UserOf(Sekretaris)), "Ajukan surat peminjaman");
            Ok(borrowLetter.Reject("Perbaiki tanggal pemakaian", UserOf(Sekretaris)), "Tolak surat");
            LetterBorrowId = borrowLetter.Id;
            Ok(Letter.Save(new Letter.NewRow { Direction = LetterDirection.Out, TemplateId = LetterTemplates["Sponsorship"].Id, Subject = "Permohonan Sponsorship Konser", Recipient = "PT Bank BNI" }, UserOf(Sekretaris)), "Surat draft");

            var incoming = Ok(Letter.Save(new Letter.NewRow
            {
                Direction = LetterDirection.In, LetterType = "SURAT UND", LetterNumber = "B/1234/UN63/KM/2026", Subject = "Undangan Pengisi Acara Wisuda Periode 115", LetterDate = Today.AddDays(-6), ReceivedDate = Today.AddDays(-5),
                Sender = "Biro Akademik dan Kemahasiswaan UPN Jatim", Summary = "Permintaan penampilan marching band pada wisuda", FileData = Pdf("Surat masuk undangan wisuda"), FileName = "surat-masuk-wisuda.pdf", ContentType = "application/pdf"
            }, UserOf(Sekretaris)), "Surat masuk");
            var disposition = Ok(LetterDisposition.Save(new LetterDisposition.NewRow { LetterId = incoming.Id, ToMemberId = Members[5].Id, Instruction = "Siapkan formasi dan daftar pemain untuk wisuda", DueDate = Today.AddDays(7) }, KetuaUser), "Disposisi 1");
            Ok(disposition.MarkDone(UserOf(Members[5])), "Disposisi selesai");
            Ok(LetterDisposition.Save(new LetterDisposition.NewRow { LetterId = incoming.Id, ToMemberId = Members[9].Id, Instruction = "Cek kesiapan alat", DueDate = Today.AddDays(5) }, KetuaUser), "Disposisi 2");
            var archived = Ok(Letter.Save(new Letter.NewRow { Direction = LetterDirection.In, LetterType = "PEMBERITAHUAN", LetterNumber = "045/DISPORA/2026", Subject = "Pemberitahuan Festival Pemuda", LetterDate = Today.AddDays(-30), ReceivedDate = Today.AddDays(-28), Sender = "Dispora Kota Surabaya" }, UserOf(Sekretaris)), "Surat masuk 2");
            Ok(archived.Archive(UserOf(Sekretaris)), "Arsipkan surat");

            var docs = new (string Category, string Title, DocumentVisibility Visibility)[]
            {
                ("AD/ART", "AD/ART UKM Marching Band Gita Widya Agni 2026", DocumentVisibility.Public),
                ("SOP", "SOP Peminjaman Alat", DocumentVisibility.Member),
                ("Notulen", "Notulen Musyawarah Besar 2026", DocumentVisibility.Pengurus),
                ("Laporan", "LPJ Kepengurusan 2025/2026", DocumentVisibility.Pengurus),
                ("Surat Keputusan", "SK Kepengurusan 2026/2027", DocumentVisibility.Member)
            };
            foreach (var d in docs)
            {
                Ok(Document.Save(new Document.NewRow { CategoryId = DocCategories[d.Category], Title = d.Title, Description = d.Title, FileData = Pdf(d.Title), FileName = d.Title.ToLowerInvariant().Replace(' ', '-').Replace('/', '-') + ".pdf", ContentType = "application/pdf", Visibility = d.Visibility }, UserOf(Sekretaris)), "Dokumen " + d.Title);
            }
        }

        // ---------------------------------------------------------------- Prestasi, sertifikat, poin
        private static void SeedAchievements()
        {
            var team = Ok(Achievement.Save(new Achievement.NewRow
            {
                CompetitionId = Competitions["Jatim"].Id, TeamName = "MB Gita Widya Agni", Title = "Kejuaraan Marching Band Jawa Timur 2026", RankTitle = "Juara 2 Kategori Umum",
                AchievedDate = D(2026, 10, 25), Description = "Meraih juara 2 dengan repertoar \"Circle of Life\"", IsFeatured = FeaturedStatus.Disorot
            }, Admin), "Prestasi tim");
            Ok(team.UpdatePhoto(Photo(7), Admin), "Foto prestasi");
            Ok(team.UpdateCertificate(Pdf("Piagam Juara 2 Kejuaraan MB Jatim 2026"), "piagam-juara-2.pdf", Admin), "Piagam prestasi");
            var individual = Ok(Achievement.Save(new Achievement.NewRow
            {
                CompetitionId = Competitions["Jatim"].Id, MemberId = Members[6].Id, Title = "Best Snare Line", RankTitle = "Best Snare Line", AchievedDate = D(2026, 10, 25), IsFeatured = FeaturedStatus.Disorot
            }, Admin), "Prestasi individu");
            Ok(Achievement.Save(new Achievement.NewRow { MemberId = Members[7].Id, Title = "Juara 1 Solo Flag UPN Fest 2025", RankTitle = "Juara 1", Level = EventLevel.Kampus, AchievedDate = D(2025, 11, 20) }, Admin), "Prestasi kampus");
            Ok(Achievement.Save(new Achievement.NewRow { TeamName = "MB Gita Widya Agni", Title = "Bhaskara Cup", RankTitle = "Juara Umum", Level = EventLevel.Regional, AchievedDate = D(1996, 8, 17), Description = "Prestasi bersejarah" }, Admin), "Prestasi sejarah");

            Ok(Certificate.Save(new Certificate.NewRow { MemberId = Members[6].Id, CertificateType = CertificateType.Prestasi, AchievementId = individual.Id, Title = "Sertifikat Best Snare Line Jatim 2026", IssuedDate = D(2026, 10, 30), FileData = Pdf("Sertifikat Best Snare Line"), FileName = "sertifikat-best-snare.pdf" }, Admin), "Sertifikat prestasi");
            Ok(Certificate.Save(new Certificate.NewRow { MemberId = Ketua.Id, CertificateType = CertificateType.Pengurus, AssignmentId = Assignments[Ketua.Fullname], Title = "Sertifikat Ketua Umum 2026/2027", IssuedDate = D(2026, 9, 1) }, Admin), "Sertifikat pengurus");
            foreach (var i in new[] { 7, 12, 14, 16 })
            {
                Ok(Certificate.Save(new Certificate.NewRow { MemberId = Members[i].Id, CertificateType = CertificateType.Peserta, ActivityId = Activities["Workshop"].Id, Title = "Peserta Workshop Colour Guard", IssuedDate = D(2026, 10, 19) }, Admin), "Sertifikat peserta");
            }
            var pemateri = Ok(Certificate.Save(new Certificate.NewRow { MemberId = Members[8].Id, CertificateType = CertificateType.Pemateri, ActivityId = Activities["Workshop"].Id, Title = "Pemateri Workshop Colour Guard", IssuedDate = D(2026, 10, 19) }, Admin), "Sertifikat pemateri");
            var revoked = Ok(Certificate.Save(new Certificate.NewRow { MemberId = Members[20].Id, CertificateType = CertificateType.Lainnya, Title = "Sertifikat Volunteer (salah cetak)", IssuedDate = D(2026, 10, 19) }, Admin), "Sertifikat lain");
            Ok(revoked.Revoke(Admin), "Cabut sertifikat");
            if (Certificate.Verify(pemateri.VerificationCode) == null)
            {
                throw new InvalidOperationException("Verifikasi sertifikat gagal");
            }

            Ok(MemberPoint.GenerateForActivity(Activities["Latihan"].Id, Admin), "Poin latihan");
            Ok(MemberPoint.GenerateForActivity(Activities["Rapat"].Id, Admin), "Poin rapat");
            Ok(MemberPoint.GenerateForActivity(Activities["Konser"].Id, Admin), "Poin panitia konser");
            Ok(MemberPoint.GenerateForPeriod(PeriodActive.Id, Admin), "Poin pengurus");
            Ok(MemberPoint.Save(new MemberPoint.NewRow { MemberId = Members[8].Id, PeriodId = PeriodActive.Id, SourceType = PointSourceType.Pemateri, RefId = Activities["Workshop"].Id, Points = 10, EarnedDate = D(2026, 10, 18), Notes = "Pemateri workshop colour guard" }, Admin), "Poin pemateri");
            Ok(MemberPoint.Save(new MemberPoint.NewRow { MemberId = Members[15].Id, PeriodId = PeriodActive.Id, SourceType = PointSourceType.Lainnya, Points = 3, EarnedDate = Today.AddDays(-4), Notes = "Membantu bersih-bersih gudang" }, Admin), "Poin lainnya");
        }

        // ---------------------------------------------------------------- Formulir & aspirasi
        private static void SeedForms()
        {
            var evaluation = Ok(Form.Save(new Form.NewRow
            {
                FormType = FormType.EvaluasiKegiatan, ActivityId = Activities["DiesNatalis"].Id, Title = "Evaluasi Penampilan Dies Natalis 2026", Description = "Isi evaluasi untuk perbaikan penampilan berikutnya",
                CloseAt = DateTime.Today.AddDays(14).AddHours(23),
                Questions = new()
                {
                    new() { QuestionText = "Kepuasan terhadap penampilan secara keseluruhan", QuestionType = QuestionType.Skala1Sampai5, IsRequired = AnswerRequirement.Wajib },
                    new() { QuestionText = "Bagian yang perlu diperbaiki", QuestionType = QuestionType.KotakCentang, Options = new() { "Transisi formasi", "Tempo", "Dinamika brass", "Koreografi guard", "Kebersihan alat" } },
                    new() { QuestionText = "Apakah persiapan latihan cukup?", QuestionType = QuestionType.PilihanGanda, Options = new() { "Cukup", "Kurang", "Berlebihan" }, IsRequired = AnswerRequirement.Wajib },
                    new() { QuestionText = "Saran untuk penampilan berikutnya", QuestionType = QuestionType.Paragraf }
                }
            }, Admin), "Form evaluasi");
            evaluation = Ok(evaluation.Open(Admin), "Buka form evaluasi");
            var q = FormQuestion.GetDataList(evaluation.Id);
            var options = new[] { "Transisi formasi", "Tempo", "Dinamika brass", "Koreografi guard" };
            for (var i = 5; i < 17; i++)
            {
                Ok(FormResponse.Submit(new FormResponse.NewRow
                {
                    FormId = evaluation.Id,
                    Answers = new()
                    {
                        new() { QuestionId = q[0].Id, AnswerText = (3 + i % 3).ToString() },
                        new() { QuestionId = q[1].Id, Choices = new() { options[i % 4], options[(i + 1) % 4] } },
                        new() { QuestionId = q[2].Id, AnswerText = i % 4 == 0 ? "Kurang" : "Cukup" },
                        new() { QuestionId = q[3].Id, AnswerText = i % 3 == 0 ? "Tambah latihan gabungan menjelang hari H." : null }
                    }
                }, UserOf(Members[i])), "Jawaban evaluasi");
            }

            var survey = Ok(Form.Save(new Form.NewRow
            {
                FormType = FormType.SurveiUmum, Title = "Survei Kepuasan Anggota terhadap Kepengurusan", IsAnonymous = IdentityMode.Anonim,
                Questions = new()
                {
                    new() { QuestionText = "Penilaian kinerja pengurus", QuestionType = QuestionType.Skala1Sampai5, IsRequired = AnswerRequirement.Wajib },
                    new() { QuestionText = "Kritik dan saran", QuestionType = QuestionType.Paragraf }
                }
            }, Admin), "Form survei");
            survey = Ok(survey.Open(Admin), "Buka survei");
            var sq = FormQuestion.GetDataList(survey.Id);
            for (var i = 11; i < 19; i++)
            {
                Ok(FormResponse.Submit(new FormResponse.NewRow { FormId = survey.Id, Answers = new() { new() { QuestionId = sq[0].Id, AnswerText = (2 + i % 4).ToString() }, new() { QuestionId = sq[1].Id, AnswerText = i % 2 == 0 ? "Jadwal latihan diumumkan lebih awal." : null } } }, UserOf(Members[i])), "Jawaban survei");
            }
            Ok(survey.Close(Admin), "Tutup survei");
            Ok(Form.Save(new Form.NewRow { FormType = FormType.SurveiUmum, Title = "Pendataan Ukuran Kostum (draft)", Questions = new() { new() { QuestionText = "Ukuran baju", QuestionType = QuestionType.PilihanGanda, Options = new() { "S", "M", "L", "XL" } } } }, Admin), "Form draft");

            var a1 = Ok(Aspiration.Submit(new Aspiration.NewRow { Category = AspirationCategory.Saran, Title = "Jadwal latihan hari Sabtu", Content = "Mohon dipertimbangkan latihan tambahan hari Sabtu pagi menjelang lomba." }, UserOf(Members[15])), "Aspirasi 1");
            Ok(a1.Respond(AspirationStatus.Diproses, "Akan dibahas di rapat pengurus.", Admin), "Tanggapi aspirasi 1");
            var a2 = Ok(Aspiration.Submit(new Aspiration.NewRow { IsAnonymous = IdentityMode.Anonim, Category = AspirationCategory.Pengaduan, Title = "Alat sering tidak dikembalikan", Content = "Beberapa stik dan mouthpiece tidak dikembalikan ke gudang setelah latihan.", Attachment = Photo(1), AttachmentFileName = "gudang.jpg" }, UserOf(Members[18])), "Aspirasi anonim");
            Ok(a2.Respond(AspirationStatus.Selesai, "Logistik sudah menerapkan daftar pinjam harian.", Admin), "Tanggapi aspirasi 2");
            Ok(Aspiration.Submit(new Aspiration.NewRow { Category = AspirationCategory.Aspirasi, Title = "Kelas teori musik", Content = "Usul ada kelas teori musik dasar untuk anggota baru." }, UserOf(Members[19])), "Aspirasi 3");
            var a4 = Ok(Aspiration.Submit(new Aspiration.NewRow { IsAnonymous = IdentityMode.Anonim, Category = AspirationCategory.Saran, Title = "Tes", Content = "tes" }, UserOf(Members[20])), "Aspirasi 4");
            Ok(a4.Respond(AspirationStatus.Ditolak, "Bukan aspirasi.", Admin), "Tolak aspirasi");
        }

        // ---------------------------------------------------------------- E-Voting
        private static void SeedElection()
        {
            var election = Ok(Election.Save(new Election.NewRow
            {
                Title = "Pemilihan Ketua Umum Periode 2027/2028", Description = "Pemilihan ketua umum secara daring oleh seluruh anggota aktif.",
                StartAt = DateTime.Now.AddHours(-2), EndAt = DateTime.Now.AddDays(2)
            }, Admin), "Pemilihan");
            election = Ok(election.StartRegistration(Admin), "Buka pendaftaran kandidat");
            var c1 = Ok(ElectionCandidate.Register(new ElectionCandidate.NewRow { ElectionId = election.Id, MemberId = Members[5].Id, Vision = "GWA juara nasional dan solid", Mission = "Latihan terstruktur\nRegenerasi pelatih internal", Photo = Avatar(5) }, Admin), "Kandidat 1");
            var c2 = Ok(ElectionCandidate.Register(new ElectionCandidate.NewRow { ElectionId = election.Id, MemberId = Members[7].Id, Vision = "GWA dikenal luas dan berprestasi", Mission = "Perkuat branding\nTambah konser kolaborasi", Photo = Avatar(7) }, Admin), "Kandidat 2");
            var c3 = Ok(ElectionCandidate.Register(new ElectionCandidate.NewRow { ElectionId = election.Id, MemberId = Members[13].Id, Vision = "-" }, Admin), "Kandidat 3");
            Ok(c1.Verify(Admin), "Verifikasi kandidat 1");
            Ok(c2.Verify(Admin), "Verifikasi kandidat 2");
            Ok(c3.Reject(Admin), "Tolak kandidat 3");
            Ok(ElectionEligibleVoter.Generate(election.Id, null, Admin), "Daftar pemilih");
            election = Ok(election.OpenVoting(Admin), "Buka pemungutan suara");
            var candidates = ElectionCandidate.GetBallot(election.Id);
            for (var i = 0; i < 16; i++)
            {
                var choice = candidates[i % 3 == 0 ? 1 : 0];
                Ok(ElectionVote.Cast(new ElectionVote.NewRow { ElectionId = election.Id, CandidateId = choice.Id }, UserOf(Members[i])), "Suara");
            }
            election = Ok(election.Close(Admin), "Tutup pemungutan suara");
            Ok(election.Finalize(Admin), "Finalisasi hasil");

            var open = Ok(Election.Save(new Election.NewRow { Title = "Pemilihan Koordinator Divisi Battery", PeriodId = PeriodActive.Id, StartAt = DateTime.Now.AddHours(-1), EndAt = DateTime.Now.AddDays(5) }, Admin), "Pemilihan 2");
            foreach (var m in new[] { 6, 15 })
            {
                var c = Ok(ElectionCandidate.Register(new ElectionCandidate.NewRow { ElectionId = open.Id, MemberId = Members[m].Id, Vision = "Battery solid" }, Admin), "Kandidat battery");
                Ok(c.Verify(Admin), "Verifikasi kandidat battery");
            }
            Ok(ElectionEligibleVoter.Add(open.Id, new[] { 6, 11, 13, 15, 17, 19, 21 }.Select(i => Members[i].Id).ToList(), Admin), "Pemilih battery");
            open = Ok(open.OpenVoting(Admin), "Buka pemilihan battery");
            var ballot = ElectionCandidate.GetBallot(open.Id);
            foreach (var i in new[] { 11, 13, 17 })
            {
                Ok(ElectionVote.Cast(new ElectionVote.NewRow { ElectionId = open.Id, CandidateId = ballot[i % 2].Id }, UserOf(Members[i])), "Suara battery");
            }
            Ok(Election.Save(new Election.NewRow { Title = "Pemilihan Ketua Pelaksana Konser 2027", StartAt = DateTime.Today.AddDays(60), EndAt = DateTime.Today.AddDays(62) }, Admin), "Pemilihan draft");
        }

        // ---------------------------------------------------------------- Informasi
        private static void SeedInformation()
        {
            var news = new (AnnouncementCategory Category, string Title, string Excerpt, string Content, bool Featured, PublicVisibility Visibility, PublicationStatus Status, int Photo)[]
            {
                (AnnouncementCategory.Berita, "MB Gita Widya Agni Raih Juara 2 Kejuaraan Marching Band Jawa Timur 2026",
                    "Tim GWA membawa pulang trofi juara 2 kategori umum dan penghargaan Best Snare Line.",
                    "Surabaya - Marching Band Gita Widya Agni UPN \"Veteran\" Jawa Timur meraih juara 2 kategori umum pada Kejuaraan Marching Band Jawa Timur 2026 di GOR Kertajaya.\n\nSelain itu, Bagas Setiawan meraih penghargaan Best Snare Line. Prestasi ini menjadi bekal menuju Grand Prix Marching Band 2027.", true, PublicVisibility.Public, PublicationStatus.Published, 7),
                (AnnouncementCategory.Agenda, "Konser Gita Swara 2026: Nyala Nada",
                    "Saksikan konser tahunan MB Gita Widya Agni di Gedung Giri Loka.",
                    "Konser tahunan Gita Swara kembali digelar dengan tema \"Nyala Nada\". Konser terbuka untuk umum dan gratis.\n\nTanggal: satu bulan lagi\nTempat: Gedung Giri Loka UPN \"Veteran\" Jawa Timur", true, PublicVisibility.Public, PublicationStatus.Published, 0),
                (AnnouncementCategory.Pengumuman, "Open Recruitment Anggota Baru 2026 Dibuka",
                    "Pendaftaran anggota baru dibuka hingga akhir bulan ini.",
                    "Marching Band Gita Widya Agni membuka pendaftaran anggota baru untuk divisi brass, battery, front ensemble, dan colour guard. Daftar melalui halaman rekrutmen.", false, PublicVisibility.Public, PublicationStatus.Published, 2),
                (AnnouncementCategory.Pengumuman, "Jadwal Latihan Gabungan Menjelang Konser",
                    "Latihan gabungan ditambah setiap Minggu mulai pekan depan.",
                    "Seluruh anggota wajib mengikuti latihan gabungan tambahan setiap Minggu pukul 08.00 di Lapangan Giri Loka.", false, PublicVisibility.Private, PublicationStatus.Published, 4),
                (AnnouncementCategory.Berita, "Penampilan Display di Dies Natalis UPN Jatim",
                    "GWA tampil membuka upacara Dies Natalis universitas.",
                    "MB Gita Widya Agni tampil membuka upacara Dies Natalis UPN \"Veteran\" Jawa Timur dengan 42 pemain.", false, PublicVisibility.Public, PublicationStatus.Archived, 5),
                (AnnouncementCategory.Berita, "Draft: Liputan Workshop Colour Guard", "Draft liputan", "Isi liputan masih disusun.", false, PublicVisibility.Public, PublicationStatus.Draft, 6)
            };
            foreach (var n in news)
            {
                var item = Ok(Announcement.Save(new Announcement.NewRow
                {
                    Category = n.Category, Title = n.Title, Excerpt = n.Excerpt, Content = n.Content, CoverPhoto = Photo(n.Photo),
                    IsPublic = n.Visibility, IsFeatured = n.Featured ? FeaturedStatus.Disorot : FeaturedStatus.Biasa
                }, UserOf(Members[10])), "Berita " + n.Title);
                if (n.Status != PublicationStatus.Draft)
                {
                    item = Ok(item.Publish(Admin), "Terbitkan berita");
                }
                if (n.Status == PublicationStatus.Archived)
                {
                    Ok(item.Archive(Admin), "Arsipkan berita");
                }
            }

            var banners = new (string Title, string Subtitle, string? Button, string? Link, int Photo, int StartOffset)[]
            {
                ("Konser Gita Swara 2026", "Nyala Nada - gratis dan terbuka untuk umum", "Lihat Agenda", "/#kegiatan", 0, -5),
                ("Juara 2 Kejuaraan MB Jatim 2026", "Terima kasih atas dukungan seluruh civitas UPN \"Veteran\" Jawa Timur", "Baca Berita", "/Landing/Berita", 7, -10),
                ("Open Recruitment 2026", "Gabung bersama keluarga besar Gita Widya Agni", null, null, 2, -10)
            };
            var seq = 1;
            foreach (var b in banners)
            {
                Ok(Banner.Save(new Banner.NewRow { Title = b.Title, Subtitle = b.Subtitle, ButtonText = b.Button, LinkUrl = b.Link, Image = Photo(b.Photo), Seq = seq++, StartAt = DateTime.Today.AddDays(b.StartOffset), EndAt = DateTime.Today.AddDays(40) }, Admin), "Banner " + b.Title);
            }
            Ok(Banner.Save(new Banner.NewRow { Title = "Banner kedaluwarsa", Image = Photo(9), StartAt = DateTime.Today.AddDays(-60), EndAt = DateTime.Today.AddDays(-30), IsActive = ActiveStatus.TidakAktif }, Admin), "Banner nonaktif");

            var memberUsers = Members.Take(22).Select(m => MemberUser[m.Id]).ToList();
            Ok(Notification.Send(new Notification.NewRow { UserIds = memberUsers, NotifType = "PENGUMUMAN", Title = "Konser Gita Swara 2026", Message = "Latihan gabungan tambahan setiap Minggu mulai pekan depan.", LinkUrl = "/Landing/Berita" }, Admin), "Notifikasi pengumuman");
            Ok(Notification.Send(new Notification.NewRow { UserIds = new() { MemberUser[Ketua.Id], MemberUser[Bendahara.Id] }, NotifType = "FUNDING_APPROVAL", Title = "Pengajuan dana menunggu persetujuan", Message = "Pengajuan dana konsumsi gladi bersih menunggu persetujuan.", RefType = "t_funding_request" }, Admin), "Notifikasi persetujuan");
            Ok(Notification.Send(new Notification.NewRow { UserIds = Members.Take(12).Select(m => MemberUser[m.Id]).ToList(), NotifType = "DUES_BILL", Title = "Tagihan iuran November", Message = "Iuran November Rp20.000 jatuh tempo 2 minggu lagi." }, Admin), "Notifikasi iuran");
            foreach (var n in Notification.GetMyList(UserOf(Members[0])).Take(1))
            {
                Ok(n.MarkRead(UserOf(Members[0])), "Baca notifikasi");
            }
            Ok(Notification.MarkAllRead(UserOf(Members[1])), "Baca semua notifikasi");
        }

        // ---------------------------------------------------------------- Akses, outbox, job, log
        private static void SeedSystem()
        {
            Ok(UserToken.Issue(new UserToken.NewRow { UserId = MemberUser[Members[11].Id], TokenType = UserTokenType.VerifyEmail, IpAddress = "127.0.0.1", UserAgent = "GWASeeder" }), "Token verifikasi");
            var reset = Ok(UserToken.Issue(new UserToken.NewRow { UserId = MemberUser[Members[12].Id], TokenType = UserTokenType.ResetPassword, IpAddress = "127.0.0.1" }), "Token reset");
            Ok(UserToken.ResetPassword(reset.Token, DEFAULT_PASSWORD), "Reset password");
            Ok(UserToken.Issue(new UserToken.NewRow { UserId = MemberUser[Ketua.Id], TokenType = UserTokenType.Refresh, UserAgent = "GitaWidyaAgniWeb" }), "Token refresh");

            var member = Members[13];
            var templated = Ok(MessageOutbox.SaveFromTemplate(new MessageOutbox.TemplateRow
            {
                UserId = MemberUser[member.Id], Recipient = member.Email!, Channel = NotificationChannel.Email, TemplateCode = "DUES_BILL", NotifType = "DUES_BILL",
                Values = new() { ["name"] = member.Fullname, ["period"] = "November 2026", ["amount"] = "20.000", ["due"] = Today.AddDays(14).ToString("dd-MM-yyyy") }, RefType = "t_dues_bill"
            }, Admin), "Outbox email");
            if (templated != null)
            {
                Ok(templated.MarkSent(Admin), "Tandai terkirim");
            }
            Ok(MessageOutbox.SaveFromTemplate(new MessageOutbox.TemplateRow
            {
                UserId = MemberUser[Members[14].Id], Recipient = Members[14].Phone!, Channel = NotificationChannel.WhatsApp, TemplateCode = "DUES_BILL", NotifType = "DUES_BILL",
                Values = new() { ["name"] = Members[14].Fullname, ["period"] = "November 2026", ["amount"] = "20.000", ["due"] = Today.AddDays(14).ToString("dd-MM-yyyy") }
            }, Admin), "Outbox WhatsApp");
            var failed = Ok(MessageOutbox.Save(new MessageOutbox.NewRow { Recipient = "081553858791", Channel = NotificationChannel.WhatsApp, Body = "Pengingat: peminjaman speaker jatuh tempo besok.", RefType = "t_borrow_request" }, Admin), "Outbox manual");
            Ok(failed.MarkFailed("Nomor tidak terdaftar di WhatsApp", Admin), "Tandai gagal");

            var job = Ok(ScheduledJobLog.Start("DUES_REMINDER", Admin), "Job mulai");
            Ok(job.Finish(12, "12 pengingat iuran dibuat", Admin), "Job selesai");
            var job2 = Ok(ScheduledJobLog.Start("BORROW_OVERDUE", Admin), "Job 2 mulai");
            var overdue = Ok(BorrowRequest.MarkOverdue(Admin), "Tandai terlambat");
            Ok(job2.Finish(overdue, "Peminjaman terlambat ditandai", Admin), "Job 2 selesai");
            var job3 = Ok(ScheduledJobLog.Start("EMAIL_SENDER", Admin), "Job 3 mulai");
            Ok(job3.Fail("SMTP belum dikonfigurasi", 0, Admin), "Job 3 gagal");

            Ok(ActivityLog.Log(new ActivityLog.NewRow { UserId = int.Parse(Admin), Module = "SISTEM", Action = "SEED", EntityName = "m_organization", NewValue = new { sumber = "GWASeeder", waktu = DateTime.Now } }), "Log seed");
        }
    }
}
