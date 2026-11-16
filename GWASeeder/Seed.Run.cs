namespace GWASeeder
{
    internal static partial class Seed
    {
        // Hanya katalog hak akses, untuk database yang sudah berisi (mis. setelah halaman web baru ditambahkan).
        public static void RunSyncAccess()
        {
            Section("Sinkron hak akses: controller, menu, role"); SyncAccess();
        }
        public static void RunAll()
        {
            Section("Hak akses: controller, menu, role"); SyncAccess();
            Section("Profil organisasi & pengaturan situs"); SeedOrganization();
            Section("Periode, divisi, jabatan"); SeedPeriods();
            Section("User & anggota"); SeedMembers();
            Section("Persetujuan, penomoran, template pesan"); SeedApprovalMasters();
            Section("Master keuangan, mitra, sponsorship, merchandise"); SeedBusinessMasters();
            Section("Master inventaris, surat, checklist, poin"); SeedOtherMasters();
            Section("Periode lama & serah terima"); SeedPreviousPeriod();
            Section("Kepengurusan periode aktif"); SeedActivePeriod();
            Section("Rekrutmen & formulir pendaftaran"); SeedRecruitment();
            Section("Program kerja, tugas, timeline"); SeedPrograms();
            Section("Lomba & kegiatan"); SeedCompetitionsAndActivities();
            Section("Tim pelaksana"); SeedCommittee();
            Section("Presensi & notulen"); SeedAttendance();
            Section("Keuangan"); SeedFinance();
            Section("Sponsorship"); SeedSponsorship();
            Section("Pengadaan"); SeedProcurement();
            Section("Merchandise"); SeedMerchandise();
            Section("Inventaris"); SeedInventory();
            Section("Surat & dokumen"); SeedLetters();
            Section("Prestasi, sertifikat, poin"); SeedAchievements();
            Section("Formulir & aspirasi"); SeedForms();
            Section("E-Voting"); SeedElection();
            Section("Informasi & notifikasi"); SeedInformation();
            Section("Akses, antrian pesan, job"); SeedSystem();
        }
    }
}
