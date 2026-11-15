using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

public partial class ModelContext : DbContext
{
    public ModelContext()
    {
    }

    public ModelContext(DbContextOptions<ModelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MAlumniProfile> MAlumniProfiles { get; set; }

    public virtual DbSet<MApprovalFlow> MApprovalFlows { get; set; }

    public virtual DbSet<MApprovalFlowStep> MApprovalFlowSteps { get; set; }

    public virtual DbSet<MAsset> MAssets { get; set; }

    public virtual DbSet<MAssetCategory> MAssetCategories { get; set; }

    public virtual DbSet<MAssetPhoto> MAssetPhotos { get; set; }

    public virtual DbSet<MAssetUnit> MAssetUnits { get; set; }

    public virtual DbSet<MChecklistTemplate> MChecklistTemplates { get; set; }

    public virtual DbSet<MChecklistTemplateItem> MChecklistTemplateItems { get; set; }

    public virtual DbSet<MController> MControllers { get; set; }

    public virtual DbSet<MControllerAction> MControllerActions { get; set; }

    public virtual DbSet<MDivision> MDivisions { get; set; }

    public virtual DbSet<MDocumentCategory> MDocumentCategories { get; set; }

    public virtual DbSet<MDocumentNumberFormat> MDocumentNumberFormats { get; set; }

    public virtual DbSet<MDuesSetting> MDuesSettings { get; set; }

    public virtual DbSet<MExternalParty> MExternalParties { get; set; }

    public virtual DbSet<MExternalPartyContact> MExternalPartyContacts { get; set; }

    public virtual DbSet<MFinancialAccount> MFinancialAccounts { get; set; }

    public virtual DbSet<MFinancialCategory> MFinancialCategories { get; set; }

    public virtual DbSet<MHeaderMenu> MHeaderMenus { get; set; }

    public virtual DbSet<MHeaderMenuDetail> MHeaderMenuDetails { get; set; }

    public virtual DbSet<MLetterTemplate> MLetterTemplates { get; set; }

    public virtual DbSet<MManagementPeriod> MManagementPeriods { get; set; }

    public virtual DbSet<MMember> MMembers { get; set; }

    public virtual DbSet<MMemberPhoto> MMemberPhotos { get; set; }

    public virtual DbSet<MMessageTemplate> MMessageTemplates { get; set; }

    public virtual DbSet<MOrganization> MOrganizations { get; set; }

    public virtual DbSet<MOrganizationPhoto> MOrganizationPhotos { get; set; }

    public virtual DbSet<MPointRule> MPointRules { get; set; }

    public virtual DbSet<MPosition> MPositions { get; set; }

    public virtual DbSet<MProduct> MProducts { get; set; }

    public virtual DbSet<MProductVariant> MProductVariants { get; set; }

    public virtual DbSet<MRole> MRoles { get; set; }

    public virtual DbSet<MRoleAction> MRoleActions { get; set; }

    public virtual DbSet<MRoleMenu> MRoleMenus { get; set; }

    public virtual DbSet<MSequence> MSequences { get; set; }

    public virtual DbSet<MSiteSetting> MSiteSettings { get; set; }

    public virtual DbSet<MSponsorshipPackage> MSponsorshipPackages { get; set; }

    public virtual DbSet<MSponsorshipPackageBenefit> MSponsorshipPackageBenefits { get; set; }

    public virtual DbSet<MSubMenu> MSubMenus { get; set; }

    public virtual DbSet<MUser> MUsers { get; set; }

    public virtual DbSet<MUserNotificationPref> MUserNotificationPrefs { get; set; }

    public virtual DbSet<MUserRole> MUserRoles { get; set; }

    public virtual DbSet<TAchievement> TAchievements { get; set; }

    public virtual DbSet<TActivity> TActivities { get; set; }

    public virtual DbSet<TActivityChecklist> TActivityChecklists { get; set; }

    public virtual DbSet<TActivityCollaborator> TActivityCollaborators { get; set; }

    public virtual DbSet<TActivityLog> TActivityLogs { get; set; }

    public virtual DbSet<TActivityParticipant> TActivityParticipants { get; set; }

    public virtual DbSet<TActivityReport> TActivityReports { get; set; }

    public virtual DbSet<TActivityRundown> TActivityRundowns { get; set; }

    public virtual DbSet<TActivitySchedule> TActivitySchedules { get; set; }

    public virtual DbSet<TActivityTripMember> TActivityTripMembers { get; set; }

    public virtual DbSet<TAnnouncement> TAnnouncements { get; set; }

    public virtual DbSet<TAspiration> TAspirations { get; set; }

    public virtual DbSet<TAssetInspection> TAssetInspections { get; set; }

    public virtual DbSet<TAssetMovement> TAssetMovements { get; set; }

    public virtual DbSet<TAttendanceRecord> TAttendanceRecords { get; set; }

    public virtual DbSet<TAttendanceRecordIn> TAttendanceRecordIns { get; set; }

    public virtual DbSet<TAttendanceRecordOut> TAttendanceRecordOuts { get; set; }

    public virtual DbSet<TAttendanceSession> TAttendanceSessions { get; set; }

    public virtual DbSet<TBanner> TBanners { get; set; }

    public virtual DbSet<TBorrowItem> TBorrowItems { get; set; }

    public virtual DbSet<TBorrowItemUnit> TBorrowItemUnits { get; set; }

    public virtual DbSet<TBorrowRequest> TBorrowRequests { get; set; }

    public virtual DbSet<TBudget> TBudgets { get; set; }

    public virtual DbSet<TBudgetDocument> TBudgetDocuments { get; set; }

    public virtual DbSet<TBudgetItem> TBudgetItems { get; set; }

    public virtual DbSet<TCertificate> TCertificates { get; set; }

    public virtual DbSet<TCommitteeApplication> TCommitteeApplications { get; set; }

    public virtual DbSet<TCommitteeMember> TCommitteeMembers { get; set; }

    public virtual DbSet<TCommitteeRecruitment> TCommitteeRecruitments { get; set; }

    public virtual DbSet<TCommitteeSection> TCommitteeSections { get; set; }

    public virtual DbSet<TCompetition> TCompetitions { get; set; }

    public virtual DbSet<TCompetitionMaterial> TCompetitionMaterials { get; set; }

    public virtual DbSet<TCompetitionParticipant> TCompetitionParticipants { get; set; }

    public virtual DbSet<TDebt> TDebts { get; set; }

    public virtual DbSet<TDebtPayment> TDebtPayments { get; set; }

    public virtual DbSet<TDivisionMember> TDivisionMembers { get; set; }

    public virtual DbSet<TDocument> TDocuments { get; set; }

    public virtual DbSet<TDocumentNumberSeq> TDocumentNumberSeqs { get; set; }

    public virtual DbSet<TDuesBill> TDuesBills { get; set; }

    public virtual DbSet<TElection> TElections { get; set; }

    public virtual DbSet<TElectionAuditEvent> TElectionAuditEvents { get; set; }

    public virtual DbSet<TElectionCandidate> TElectionCandidates { get; set; }

    public virtual DbSet<TElectionEligibleVoter> TElectionEligibleVoters { get; set; }

    public virtual DbSet<TElectionResult> TElectionResults { get; set; }

    public virtual DbSet<TElectionVote> TElectionVotes { get; set; }

    public virtual DbSet<TFinancialAttachment> TFinancialAttachments { get; set; }

    public virtual DbSet<TFinancialTransaction> TFinancialTransactions { get; set; }

    public virtual DbSet<TForm> TForms { get; set; }

    public virtual DbSet<TFormAnswer> TFormAnswers { get; set; }

    public virtual DbSet<TFormQuestion> TFormQuestions { get; set; }

    public virtual DbSet<TFormResponse> TFormResponses { get; set; }

    public virtual DbSet<TFundingApproval> TFundingApprovals { get; set; }

    public virtual DbSet<TFundingRealization> TFundingRealizations { get; set; }

    public virtual DbSet<TFundingRequest> TFundingRequests { get; set; }

    public virtual DbSet<TGallery> TGalleries { get; set; }

    public virtual DbSet<TGalleryPhoto> TGalleryPhotos { get; set; }

    public virtual DbSet<TLetter> TLetters { get; set; }

    public virtual DbSet<TLetterApproval> TLetterApprovals { get; set; }

    public virtual DbSet<TLetterDisposition> TLetterDispositions { get; set; }

    public virtual DbSet<TMaintenanceLog> TMaintenanceLogs { get; set; }

    public virtual DbSet<TManagementAssignment> TManagementAssignments { get; set; }

    public virtual DbSet<TMeetingMinute> TMeetingMinutes { get; set; }

    public virtual DbSet<TMemberPoint> TMemberPoints { get; set; }

    public virtual DbSet<TMessageOutbox> TMessageOutboxes { get; set; }

    public virtual DbSet<TNotification> TNotifications { get; set; }

    public virtual DbSet<TPeriodBalance> TPeriodBalances { get; set; }

    public virtual DbSet<TPeriodHandover> TPeriodHandovers { get; set; }

    public virtual DbSet<TPeriodHandoverItem> TPeriodHandoverItems { get; set; }

    public virtual DbSet<TProcurementApproval> TProcurementApprovals { get; set; }

    public virtual DbSet<TProcurementItem> TProcurementItems { get; set; }

    public virtual DbSet<TProcurementQuotation> TProcurementQuotations { get; set; }

    public virtual DbSet<TProcurementReceipt> TProcurementReceipts { get; set; }

    public virtual DbSet<TProcurementReceiptItem> TProcurementReceiptItems { get; set; }

    public virtual DbSet<TProcurementRequest> TProcurementRequests { get; set; }

    public virtual DbSet<TProductStockMovement> TProductStockMovements { get; set; }

    public virtual DbSet<TProgram> TPrograms { get; set; }

    public virtual DbSet<TProgramTask> TProgramTasks { get; set; }

    public virtual DbSet<TRecruitment> TRecruitments { get; set; }

    public virtual DbSet<TRecruitmentApplicant> TRecruitmentApplicants { get; set; }

    public virtual DbSet<TSalesOrder> TSalesOrders { get; set; }

    public virtual DbSet<TSalesOrderItem> TSalesOrderItems { get; set; }

    public virtual DbSet<TScheduledJobLog> TScheduledJobLogs { get; set; }

    public virtual DbSet<TSponsorship> TSponsorships { get; set; }

    public virtual DbSet<TSponsorshipBenefit> TSponsorshipBenefits { get; set; }

    public virtual DbSet<TSponsorshipMou> TSponsorshipMous { get; set; }

    public virtual DbSet<TTimelinePlan> TTimelinePlans { get; set; }

    public virtual DbSet<TUserToken> TUserTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=gitawidyaagni;user=root;password=royanrafly", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<MAlumniProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsContactable).HasComment("0=Tidak, 1=Ya | alumni setuju dihubungi pengurus / adik tingkat");

            entity.HasOne(d => d.Member).WithOne(p => p.MAlumniProfile)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_alumni_member");
        });

        modelBuilder.Entity<MApprovalFlow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActiveKey)
                .HasComputedColumnSql("if((`is_active` = 1),1,NULL)", true)
                .HasComment("Kolom bantu: satu alur aktif per jenis dokumen");
            entity.Property(e => e.DocumentType).HasComment("FUNDING_REQUEST, PROCUREMENT, LETTER, ACTIVITY_REPORT, BUDGET, BORROW_REQUEST, PERIOD_HANDOVER");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
        });

        modelBuilder.Entity<MApprovalFlowStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AllowRevision)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya | boleh mengembalikan untuk revisi");
            entity.Property(e => e.ApproverType).HasComment("1=ROLE (role pengurus), 2=JABATAN (position_id), 3=USER_TERTENTU (user_id)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PositionId).HasComment("Jabatan pengurus yang menyetujui (approver_type=2)");
            entity.Property(e => e.RoleId).HasComment("Untuk approver_type 1 dan 3");
            entity.Property(e => e.StepLevel).HasComment("Urutan, mulai 1");
            entity.Property(e => e.StepName).HasComment("mis. Bendahara, Ketua Umum");
            entity.Property(e => e.UserId).HasComment("Untuk approver_type 6");

            entity.HasOne(d => d.Flow).WithMany(p => p.MApprovalFlowSteps)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_apstep_flow");

            entity.HasOne(d => d.Position).WithMany(p => p.MApprovalFlowSteps).HasConstraintName("fk_m_approval_flow_step_position_id");

            entity.HasOne(d => d.Role).WithMany(p => p.MApprovalFlowSteps).HasConstraintName("fk_apstep_role");

            entity.HasOne(d => d.User).WithMany(p => p.MApprovalFlowSteps).HasConstraintName("fk_apstep_user");
        });

        modelBuilder.Entity<MAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AcquisitionSource)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELI, 2=HIBAH, 3=SPONSOR, 4=LAINNYA");
            entity.Property(e => e.AssetCondition)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LateFeePerDay).HasComment("Denda telat per hari per unit (0 = tanpa denda)");
            entity.Property(e => e.ProcurementItemId).HasComment("Asal dari item pengadaan");
            entity.Property(e => e.Quantity).HasDefaultValueSql("'1'");
            entity.Property(e => e.RentalFeePerDay).HasComment("Tarif sewa per hari per unit (0 = gratis)");
            entity.Property(e => e.SponsorshipId).HasComment("Asal dari sponsor barang");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=AVAILABLE, 2=BORROWED, 3=MAINTENANCE, 4=DISPOSED");
            entity.Property(e => e.TrackUnit).HasComment("1=dicatat per unit (punya kode/QR di m_asset_unit), 0=cukup jumlah");
            entity.Property(e => e.Unit).HasDefaultValueSql("'unit'");
            entity.Property(e => e.UnitPrefix).HasComment("Awalan kode unit, mis. HT, KAM");

            entity.HasOne(d => d.Category).WithMany(p => p.MAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_asset_category");

            entity.HasOne(d => d.ProcurementItem).WithMany(p => p.MAssets).HasConstraintName("fk_asset_pgditem");

            entity.HasOne(d => d.Sponsorship).WithMany(p => p.MAssets).HasConstraintName("fk_asset_sponsorship");
        });

        modelBuilder.Entity<MAssetCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
        });

        modelBuilder.Entity<MAssetPhoto>(entity =>
        {
            entity.HasKey(e => new { e.AssetId, e.SeqPhoto })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Asset).WithMany(p => p.MAssetPhotos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("m_asset_photo_m_asset_FK");
        });

        modelBuilder.Entity<MAssetUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("m_asset_unit", tb => tb.HasComment("Unit fisik aset berkode QR"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.LabelPrintedAt).HasComment("Kapan label QR dicetak (NULL = belum)");
            entity.Property(e => e.SerialNumber).HasComment("Nomor seri pabrik (opsional)");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=TERSEDIA, 2=DIPINJAM, 3=PERAWATAN, 4=HILANG, 5=DIHAPUS");
            entity.Property(e => e.UnitCode).HasComment("Kode inventaris unit: unit_prefix + nomor urut, dicetak sebagai QR (berisi URL halaman unit)");
            entity.Property(e => e.UnitCondition)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT");

            entity.HasOne(d => d.Asset).WithMany(p => p.MAssetUnits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_m_asset_unit_asset_id");
        });

        modelBuilder.Entity<MChecklistTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityType).HasComment("Opsional: cocok untuk t_activity.activity_type tertentu");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.Name).HasComment("mis. Seminar, Lomba luar kota, Konser");
        });

        modelBuilder.Entity<MChecklistTemplateItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsMilestone).HasComment("0=Tidak, 1=Ya | disalin ke t_activity_checklist");
            entity.Property(e => e.OffsetDay).HasComment("Hari relatif terhadap hari H, negatif = sebelum (mis. -21)");
            entity.Property(e => e.Priority)
                .HasDefaultValueSql("'2'")
                .HasComment("1=RENDAH, 2=SEDANG, 3=TINGGI");
            entity.Property(e => e.SectionName).HasComment("Sie tujuan, dicocokkan dengan t_committee_section.name");

            entity.HasOne(d => d.Template).WithMany(p => p.MChecklistTemplateItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cltitem_template");
        });

        modelBuilder.Entity<MController>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IndexAction).HasDefaultValueSql("'Index'");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
            entity.Property(e => e.Name).HasComment("Nama controller, mis. Member");
        });

        modelBuilder.Entity<MControllerAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
            entity.Property(e => e.NeedAuth)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Non Auth, 1=Auth");

            entity.HasOne(d => d.Controller).WithMany(p => p.MControllerActions).HasConstraintName("fk_ctrlaction_ctrl");
        });

        modelBuilder.Entity<MDivision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DivisionType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DIVISI/BIDANG, 2=TIM");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.Name).HasComment("mis. Humas, Acara, Tim Futsal Putra");
        });

        modelBuilder.Entity<MDocumentCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
        });

        modelBuilder.Entity<MDocumentNumberFormat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DocumentType).HasComment("mis. SURAT_UND, SURAT_SK, TRX, PD, PJM, PGD, SPN, HTG, BA_ST, SERTIFIKAT, NIA, PROPOSAL");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.NumberFormat).HasComment("Placeholder: {seq} {type} {org} {month_roman} {month} {year} {yy}");
            entity.Property(e => e.ResetPeriod)
                .HasDefaultValueSql("'1'")
                .HasComment("1=TAHUNAN, 2=BULANAN, 3=TIDAK_PERNAH");
            entity.Property(e => e.SeqPadding)
                .HasDefaultValueSql("'3'")
                .HasComment("Jumlah digit nomor urut, mis. 3 -> 007");
        });

        modelBuilder.Entity<MDuesSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CategoryId).HasComment("Kategori pemasukan saat iuran dibayar");
            entity.Property(e => e.Frequency)
                .HasDefaultValueSql("'1'")
                .HasComment("1=MONTHLY, 2=SEMESTER, 3=YEARLY, 4=ONCE");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
            entity.Property(e => e.Name).HasComment("mis. Iuran Bulanan 2026");

            entity.HasOne(d => d.Category).WithMany(p => p.MDuesSettings).HasConstraintName("fk_dues_category");

            entity.HasOne(d => d.Period).WithMany(p => p.MDuesSettings).HasConstraintName("fk_dues_period");
        });

        modelBuilder.Entity<MExternalParty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("m_external_party", tb => tb.HasComment("Pihak luar: brand, organisasi lain, vendor"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PartyType).HasComment("1=PERUSAHAAN/BRAND, 2=INSTANSI_PEMERINTAH, 3=MEDIA_PARTNER, 4=KOMUNITAS/ORGANISASI, 5=VENDOR/TOKO, 6=LEMBAGA_PENDIDIKAN, 7=LAINNYA");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=AKTIF, 2=TIDAK_AKTIF, 3=BLACKLIST");
        });

        modelBuilder.Entity<MExternalPartyContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("m_external_party_contact", tb => tb.HasComment("Kontak PIC pihak luar"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.IsPrimary).HasComment("0=Tidak, 1=Ya | maksimal satu kontak utama aktif per mitra");

            entity.HasOne(d => d.ExternalParty).WithMany(p => p.MExternalPartyContacts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_contact_party");
        });

        modelBuilder.Entity<MFinancialAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccountType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=CASH, 2=BANK, 3=E_WALLET");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
        });

        modelBuilder.Entity<MFinancialCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CategoryType).HasComment("1=INCOME, 2=EXPENSE");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
        });

        modelBuilder.Entity<MHeaderMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ControllerId).HasComment("Diisi jika header langsung ke halaman (tanpa submenu)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
            entity.Property(e => e.Section).HasComment("Seksi / grup menu di sidebar");

            entity.HasOne(d => d.Controller).WithMany(p => p.MHeaderMenus).HasConstraintName("fk_headermenu_ctrl");
        });

        modelBuilder.Entity<MHeaderMenuDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.HeaderMenu).WithMany(p => p.MHeaderMenuDetails).HasConstraintName("fk_hmd_header");

            entity.HasOne(d => d.SubMenu).WithMany(p => p.MHeaderMenuDetails).HasConstraintName("fk_hmd_sub");
        });

        modelBuilder.Entity<MLetterTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Body).HasComment("Template HTML dengan placeholder");
            entity.Property(e => e.FileData).HasComment("File template .docx (opsional, selain body)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
            entity.Property(e => e.LetterType).HasComment("UNDANGAN, PERMOHONAN, PEMINJAMAN, SK, KETERANGAN, ...");
        });

        modelBuilder.Entity<MManagementPeriod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Name).HasComment("mis. Periode 2026/2027");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=ACTIVE, 3=CLOSED");
        });

        modelBuilder.Entity<MMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Gender).HasComment("1=L, 2=P");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Institution).HasComment("Asal instansi / kampus lain / pekerjaan (untuk UMUM)");
            entity.Property(e => e.JoinedAt).HasComment("Tanggal bergabung");
            entity.Property(e => e.LeftAt).HasComment("Tanggal keluar");
            entity.Property(e => e.MemberCategory)
                .HasDefaultValueSql("'1'")
                .HasComment("1=MAHASISWA, 2=UMUM");
            entity.Property(e => e.MemberNumber).HasComment("Nomor Induk Anggota (NIA)");
            entity.Property(e => e.MemberType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=ANGGOTA, 2=PENGURUS, 4=ALUMNI (3 tidak dipakai)");
            entity.Property(e => e.Nim).HasComment("Wajib untuk MAHASISWA");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=ACTIVE, 2=ALUMNI, 3=INACTIVE");
            entity.Property(e => e.UserId).HasComment("NULL jika belum punya akun login");

            entity.HasOne(d => d.User).WithOne(p => p.MMember).HasConstraintName("fk_member_user");
        });

        modelBuilder.Entity<MMemberPhoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Member).WithMany(p => p.MMemberPhotos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("m_member_photo_m_member_FK");
        });

        modelBuilder.Entity<MMessageTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Body).HasComment("Isi dengan placeholder, mis. Halo {nama}, kode reset Anda {kode}");
            entity.Property(e => e.Channel).HasComment("1=IN_APP, 2=EMAIL, 3=WHATSAPP");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.Subject).HasComment("Untuk email / judul notifikasi");
            entity.Property(e => e.TemplateCode).HasComment("mis. RESET_PASSWORD, APPROVAL_WAITING, CHECKLIST_DUE, MOU_EXPIRING, FUNDING_UNSETTLED");
        });

        modelBuilder.Entity<MOrganization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("m_organization", tb => tb.HasComment("Profil organisasi (satu baris)"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AdvisorEmail).HasComment("Email pembina");
            entity.Property(e => e.AdvisorName).HasComment("Nama pembina (hanya dicatat, bukan akun)");
            entity.Property(e => e.AdvisorPhone).HasComment("Kontak pembina");
            entity.Property(e => e.AdvisorSkFile).HasComment("File SK pembina");
            entity.Property(e => e.AdvisorSkFileName).HasComment("Nama file SK pembina");
            entity.Property(e => e.AdvisorSkNumber).HasComment("Nomor SK pembina");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Logo).HasComment("Logo organisasi (dikompres aplikasi)");
        });

        modelBuilder.Entity<MOrganizationPhoto>(entity =>
        {
            entity.HasKey(e => e.SeqPhoto).HasName("PRIMARY");

            entity.Property(e => e.SeqPhoto).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<MPointRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AchievementLevel).HasComment("Opsional: aturan khusus per tingkat prestasi");
            entity.Property(e => e.ActivityType).HasComment("Opsional: aturan khusus per jenis kegiatan (t_activity.activity_type)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.SourceType).HasComment("1=HADIR_KEGIATAN, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA");
        });

        modelBuilder.Entity<MPosition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
            entity.Property(e => e.IsCore).HasComment("0=Tidak, 1=Ya | 1 = pengurus inti");
            entity.Property(e => e.Name).HasComment("Ketua, Wakil, Sekretaris, Bendahara, Kadiv, ...");
        });

        modelBuilder.Entity<MProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityId).HasComment("Opsional: danus untuk kegiatan tertentu");
            entity.Property(e => e.CostPrice).HasComment("Harga modal per unit (untuk hitung untung)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.IsPreorder).HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.Price).HasComment("Harga jual dasar");
            entity.Property(e => e.ProductType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=MERCHANDISE, 2=MAKANAN_MINUMAN, 3=JASA, 4=LAINNYA");
            entity.Property(e => e.Stock).HasComment("Stok produk tanpa varian; produk bervarian memakai stok varian");
            entity.Property(e => e.TrackStock)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak dilacak (jasa/pre-order), 1=Ya");
            entity.Property(e => e.Unit).HasDefaultValueSql("'pcs'");

            entity.HasOne(d => d.Activity).WithMany(p => p.MProducts).HasConstraintName("fk_product_activity");
        });

        modelBuilder.Entity<MProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.Name).HasComment("mis. S, M, L, XL, Hitam-L");
            entity.Property(e => e.Price).HasComment("NULL = ikut harga produk");

            entity.HasOne(d => d.Product).WithMany(p => p.MProductVariants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_variant_product");
        });

        modelBuilder.Entity<MRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasComment("SUPERADMIN, ADMIN, KETUA, BENDAHARA, SEKRETARIS, ANGGOTA, ...");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
        });

        modelBuilder.Entity<MRoleAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ControllerAction).WithMany(p => p.MRoleActions).HasConstraintName("fk_roleaction_action");

            entity.HasOne(d => d.Role).WithMany(p => p.MRoleActions).HasConstraintName("fk_roleaction_role");
        });

        modelBuilder.Entity<MRoleMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SubMenuId).HasComment("NULL = akses ke header menu saja");

            entity.HasOne(d => d.HeaderMenu).WithMany(p => p.MRoleMenus).HasConstraintName("fk_rolemenu_header");

            entity.HasOne(d => d.Role).WithMany(p => p.MRoleMenus).HasConstraintName("fk_rolemenu_role");

            entity.HasOne(d => d.SubMenu).WithMany(p => p.MRoleMenus)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_rolemenu_sub");
        });

        modelBuilder.Entity<MSequence>(entity =>
        {
            entity.HasKey(e => e.TableName).HasName("PRIMARY");
        });

        modelBuilder.Entity<MSiteSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SettingGroup).HasComment("mis. TENTANG, KONTAK, SOSMED, FOOTER");
            entity.Property(e => e.SettingImage).HasComment("Untuk pengaturan berupa gambar, mis. logo");
            entity.Property(e => e.SettingKey).HasComment("mis. about_text, contact_email, instagram_url");
        });

        modelBuilder.Entity<MSponsorshipPackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("m_sponsorship_package", tb => tb.HasComment("Paket sponsorship milik organisasi"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("1=AKTIF, 0=NONAKTIF");
            entity.Property(e => e.Name).HasComment("Nama paket, mis. Paket 1");
            entity.Property(e => e.Price).HasComment("Nilai paket");
            entity.Property(e => e.SeqNo)
                .HasDefaultValueSql("'1'")
                .HasComment("Urutan tampil");
        });

        modelBuilder.Entity<MSponsorshipPackageBenefit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("m_sponsorship_package_benefit", tb => tb.HasComment("Daftar benefit per paket"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.SeqNo)
                .HasDefaultValueSql("'1'")
                .HasComment("Urutan benefit");

            entity.HasOne(d => d.Package).WithMany(p => p.MSponsorshipPackageBenefits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_m_sponsorship_package_benefit_package_id");
        });

        modelBuilder.Entity<MSubMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");

            entity.HasOne(d => d.Controller).WithMany(p => p.MSubMenus).HasConstraintName("fk_submenu_ctrl");
        });

        modelBuilder.Entity<MUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak Aktif, 1=Aktif");
            entity.Property(e => e.ResetKey).HasComment("Kode reset password; dikosongkan (NULL) setelah dipakai");
        });

        modelBuilder.Entity<MUserNotificationPref>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Channel).HasComment("1=IN_APP, 2=EMAIL, 3=WHATSAPP");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsEnabled)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.NotifType).HasComment("Sama dengan t_notification.notif_type / template_code");

            entity.HasOne(d => d.User).WithMany(p => p.MUserNotificationPrefs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notifpref_user");
        });

        modelBuilder.Entity<MUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PeriodId).HasComment("NULL = tidak terikat periode (mis. admin aplikasi). Jika periode CLOSED, role hanya boleh baca");

            entity.HasOne(d => d.Period).WithMany(p => p.MUserRoles).HasConstraintName("fk_userrole_period");

            entity.HasOne(d => d.Role).WithMany(p => p.MUserRoles).HasConstraintName("fk_userrole_role");

            entity.HasOne(d => d.User).WithMany(p => p.MUserRoles).HasConstraintName("fk_userrole_user");
        });

        modelBuilder.Entity<TAchievement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Certificate).HasComment("File sertifikat");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsFeatured).HasComment("0=Tidak, 1=Ya | disorot di landing page");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Private, 1=Public");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'4'")
                .HasComment("1=INTERNAL, 2=KAMPUS, 3=REGIONAL, 4=NASIONAL, 5=INTERNASIONAL");
            entity.Property(e => e.MemberId).HasComment("NULL jika prestasi tim / organisasi");
            entity.Property(e => e.RankTitle).HasComment("Juara 1, Finalis, Medali Emas, ...");

            entity.HasOne(d => d.Competition).WithMany(p => p.TAchievements).HasConstraintName("fk_achievement_competition");

            entity.HasOne(d => d.Member).WithMany(p => p.TAchievements).HasConstraintName("fk_achievement_member");
        });

        modelBuilder.Entity<TActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=EVENT, 2=RAPAT, 3=LATIHAN, 4=PERTEMUAN, 5=LAINNYA, 6=LOMBA, 7=PIKET (mis. satu kegiatan \"Piket Senin\" + jadwal rutin + petugas di t_activity_participant)");
            entity.Property(e => e.AudienceType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=SEMUA_ANGGOTA, 2=PENGURUS, 3=PESERTA_TERDAFTAR (t_activity_participant), 4=DIVISI (t_division_member), 5=TIM_PELAKSANA (t_committee_member kegiatan induk) | penentu siapa yang dicatat ALPA");
            entity.Property(e => e.CompetitionId).HasComment("Wajib untuk activity_type = 6 (LOMBA)");
            entity.Property(e => e.DivisionId).HasComment("Diisi jika audience_type = 4 (rapat divisi / latihan tim)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsFeatured).HasComment("0=Tidak, 1=Ya | disorot di landing page");
            entity.Property(e => e.IsPublic).HasComment("0=Private, 1=Public | 1 = tampil di landing page");
            entity.Property(e => e.ParentActivityId).HasComment("Kegiatan induk (event/lomba) untuk rapat/latihan persiapan; maksimal 1 tingkat");
            entity.Property(e => e.ProgramId).HasComment("NULL = kegiatan insidental di luar proker");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=PLANNED, 2=ONGOING, 3=DONE, 4=CANCELLED");

            entity.HasOne(d => d.Competition).WithMany(p => p.TActivities).HasConstraintName("fk_activity_competition");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TActivities)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activity_creator");

            entity.HasOne(d => d.Division).WithMany(p => p.TActivities).HasConstraintName("fk_activity_division");

            entity.HasOne(d => d.ParentActivity).WithMany(p => p.InverseParentActivity).HasConstraintName("fk_activity_parent");

            entity.HasOne(d => d.Program).WithMany(p => p.TActivities).HasConstraintName("fk_activity_program");
        });

        modelBuilder.Entity<TActivityChecklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsMilestone).HasComment("0=Tidak, 1=Ya | tampil di timeline kegiatan");
            entity.Property(e => e.Priority)
                .HasDefaultValueSql("'2'")
                .HasComment("1=RENDAH, 2=SEDANG, 3=TINGGI");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM, 2=PROSES, 3=SELESAI, 4=BATAL");
            entity.Property(e => e.TemplateItemId).HasComment("Asal template");

            entity.HasOne(d => d.Activity).WithMany(p => p.TActivityChecklists)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_checklist_activity");

            entity.HasOne(d => d.DoneByNavigation).WithMany(p => p.TActivityChecklists).HasConstraintName("fk_checklist_doneby");

            entity.HasOne(d => d.PicMember).WithMany(p => p.TActivityChecklists).HasConstraintName("fk_checklist_pic");

            entity.HasOne(d => d.Section).WithMany(p => p.TActivityChecklists).HasConstraintName("fk_checklist_section");

            entity.HasOne(d => d.TemplateItem).WithMany(p => p.TActivityChecklists).HasConstraintName("fk_checklist_titem");
        });

        modelBuilder.Entity<TActivityCollaborator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CollabRole)
                .HasDefaultValueSql("'1'")
                .HasComment("1=PENYELENGGARA_BERSAMA, 2=PENDUKUNG");
            entity.Property(e => e.ExternalPartyId).HasComment("Mitra luar yang diajak kolaborasi");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DIUNDANG, 2=DITERIMA, 3=DITOLAK");

            entity.HasOne(d => d.Activity).WithMany(p => p.TActivityCollaborators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_collab_activity");

            entity.HasOne(d => d.ExternalParty).WithMany(p => p.TActivityCollaborators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_activity_collaborator_external_party_id");

            entity.HasOne(d => d.RespondedByNavigation).WithMany(p => p.TActivityCollaborators).HasConstraintName("fk_collab_responder");
        });

        modelBuilder.Entity<TActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Action).HasComment("CREATE, UPDATE, DELETE, LOGIN, APPROVE, ...");
            entity.Property(e => e.EntityName).HasComment("Nama tabel");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithMany(p => p.TActivityLogs).HasConstraintName("fk_log_user");
        });

        modelBuilder.Entity<TActivityParticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ParticipantRole)
                .HasDefaultValueSql("'PESERTA'")
                .HasComment("PESERTA, PANITIA, PEMATERI, ...");
            entity.Property(e => e.RegistrationStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=REGISTERED, 2=CONFIRMED, 3=CANCELLED");

            entity.HasOne(d => d.Activity).WithMany(p => p.TActivityParticipants).HasConstraintName("fk_participant_activity");

            entity.HasOne(d => d.Member).WithMany(p => p.TActivityParticipants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_participant_member");
        });

        modelBuilder.Entity<TActivityReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FileData).HasComment("File laporan kegiatan");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=SUBMITTED, 3=REVISION, 4=APPROVED");

            entity.HasOne(d => d.Activity).WithMany(p => p.TActivityReports).HasConstraintName("fk_report_activity");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.TActivityReportApprovedByNavigations).HasConstraintName("fk_report_approver");

            entity.HasOne(d => d.Program).WithMany(p => p.TActivityReports).HasConstraintName("fk_report_program");

            entity.HasOne(d => d.SubmittedByNavigation).WithMany(p => p.TActivityReportSubmittedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_report_submitter");
        });

        modelBuilder.Entity<TActivityRundown>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SectionId).HasComment("Sie penanggung jawab");
            entity.Property(e => e.Title).HasComment("mis. Berangkat dari kampus, Technical meeting, Tampil");

            entity.HasOne(d => d.Activity).WithMany(p => p.TActivityRundowns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rundown_activity");

            entity.HasOne(d => d.PicMember).WithMany(p => p.TActivityRundowns).HasConstraintName("fk_rundown_pic");

            entity.HasOne(d => d.Section).WithMany(p => p.TActivityRundowns).HasConstraintName("fk_rundown_section");
        });

        modelBuilder.Entity<TActivitySchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DayOfWeek).HasComment("1=Senin, 2=Selasa, 3=Rabu, 4=Kamis, 5=Jumat, 6=Sabtu, 7=Minggu");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.LateToleranceMinute).HasDefaultValueSql("'10'");
            entity.Property(e => e.Location).HasComment("Nama tempat, mis. GOR Kampus");
            entity.Property(e => e.ValidUntil).HasComment("NULL = berlaku sampai kegiatan selesai / dinonaktifkan");

            entity.HasOne(d => d.Activity).WithMany(p => p.TActivitySchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_schedule_activity");
        });

        modelBuilder.Entity<TActivityTripMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.GuestName).HasComment("Wajib jika member_id NULL");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MemberId).HasComment("NULL untuk non-anggota (pelatih eksternal, sopir)");
            entity.Property(e => e.PermissionStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM, 2=ADA, 3=TIDAK_PERLU (izin orang tua/instansi)");
            entity.Property(e => e.TripRole)
                .HasDefaultValueSql("'1'")
                .HasComment("1=PESERTA, 2=OFFICIAL, 3=PELATIH, 4=KRU, 5=LAINNYA");

            entity.HasOne(d => d.Activity).WithMany(p => p.TActivityTripMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trip_activity");

            entity.HasOne(d => d.Member).WithMany(p => p.TActivityTripMembers).HasConstraintName("fk_trip_member");
        });

        modelBuilder.Entity<TAnnouncement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Category)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BERITA, 2=PENGUMUMAN, 3=AGENDA");
            entity.Property(e => e.CoverPhoto).HasComment("Foto sampul berita");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsFeatured).HasComment("0=Tidak, 1=Ya | disorot di landing page");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Private, 1=Public | 0 = hanya untuk anggota login");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=PUBLISHED, 3=ARCHIVED");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.TAnnouncements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_announcement_author");
        });

        modelBuilder.Entity<TAspiration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Category)
                .HasDefaultValueSql("'1'")
                .HasComment("1=ASPIRASI, 2=PENGADUAN, 3=SARAN");
            entity.Property(e => e.InsBy).HasComment("Biarkan NULL jika anonim");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsAnonymous).HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BARU, 2=DIPROSES, 3=SELESAI, 4=DITOLAK");
            entity.Property(e => e.UserId).HasComment("NULL jika dikirim anonim");

            entity.HasOne(d => d.RespondedByNavigation).WithMany(p => p.TAspirationRespondedByNavigations).HasConstraintName("fk_aspiration_responder");

            entity.HasOne(d => d.User).WithMany(p => p.TAspirationUsers).HasConstraintName("fk_aspiration_user");
        });

        modelBuilder.Entity<TAssetInspection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssetUnitId).HasComment("Unit yang diinspeksi (aset track_unit=1)");
            entity.Property(e => e.BorrowItemId).HasComment("Terisi jika inspeksi saat pengembalian");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ItemCondition).HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG");

            entity.HasOne(d => d.Asset).WithMany(p => p.TAssetInspections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_inspection_asset");

            entity.HasOne(d => d.AssetUnit).WithMany(p => p.TAssetInspections).HasConstraintName("fk_t_asset_inspection_asset_unit_id");

            entity.HasOne(d => d.BorrowItem).WithMany(p => p.TAssetInspections).HasConstraintName("fk_inspection_borrow");

            entity.HasOne(d => d.InspectedByNavigation).WithMany(p => p.TAssetInspections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_inspection_inspector");
        });

        modelBuilder.Entity<TAssetMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssetUnitId).HasComment("Unit yang bergerak (aset track_unit=1)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MovementType).HasComment("1=IN, 2=OUT, 3=BORROW, 4=RETURN, 5=ADJUSTMENT, 6=DISPOSAL");

            entity.HasOne(d => d.Asset).WithMany(p => p.TAssetMovements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_movement_asset");

            entity.HasOne(d => d.AssetUnit).WithMany(p => p.TAssetMovements).HasConstraintName("fk_t_asset_movement_asset_unit_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TAssetMovements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_movement_creator");
        });

        modelBuilder.Entity<TAttendanceRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=HADIR, 2=TERLAMBAT, 3=IZIN, 4=SAKIT, 5=ALPA");

            entity.HasOne(d => d.Member).WithMany(p => p.TAttendanceRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_attendance_member");

            entity.HasOne(d => d.Session).WithMany(p => p.TAttendanceRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_attendance_session");
        });

        modelBuilder.Entity<TAttendanceRecordIn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Photo).HasComment("Foto check-in (wajib)");

            entity.HasOne(d => d.Attendance).WithOne(p => p.TAttendanceRecordIn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_record_in_attendance");
        });

        modelBuilder.Entity<TAttendanceRecordOut>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Photo).HasComment("Foto check-out (wajib)");

            entity.HasOne(d => d.Attendance).WithOne(p => p.TAttendanceRecordOut)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_record_out_attendance");
        });

        modelBuilder.Entity<TAttendanceSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CancelReason).HasComment("Diisi jika status 4, mis. libur UTS");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LateToleranceMinute)
                .HasDefaultValueSql("'10'")
                .HasComment("Check-in lewat open_at + menit ini = TERLAMBAT");
            entity.Property(e => e.Location).HasComment("Nama tempat");
            entity.Property(e => e.Name).HasComment("mis. Hari 1 - Sesi Pagi");
            entity.Property(e => e.RadiusMeter).HasComment("Radius geofence yang diizinkan");
            entity.Property(e => e.ScheduleId).HasComment("Terisi jika sesi dibuat otomatis dari jadwal rutin");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=SCHEDULED, 2=OPEN, 3=CLOSED, 4=CANCELLED");

            entity.HasOne(d => d.Activity).WithMany(p => p.TAttendanceSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_session_activity");

            entity.HasOne(d => d.Schedule).WithMany(p => p.TAttendanceSessions).HasConstraintName("fk_session_schedule");
        });

        modelBuilder.Entity<TBanner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EndAt).HasComment("NULL = tanpa batas");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.StartAt).HasComment("NULL = langsung tayang");
        });

        modelBuilder.Entity<TBorrowItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ConditionOnBorrow).HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG");
            entity.Property(e => e.ConditionOnReturn).HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LateDays).HasComment("Hari terlambat (track_unit=0); untuk track_unit=1 dari unit terakhir");
            entity.Property(e => e.LateFee).HasComment("track_unit=0: late_days x late_fee_per_day x quantity; track_unit=1: SUM(t_borrow_item_unit.late_fee)");
            entity.Property(e => e.LateFeePerDay).HasComment("Salinan tarif denda saat pinjam");
            entity.Property(e => e.Quantity).HasDefaultValueSql("'1'");
            entity.Property(e => e.RentalDays).HasComment("Lama sewa (hari)");
            entity.Property(e => e.RentalFee).HasComment("rental_days x rental_fee_per_day x quantity");
            entity.Property(e => e.RentalFeePerDay).HasComment("Salinan tarif sewa saat pinjam");

            entity.HasOne(d => d.Asset).WithMany(p => p.TBorrowItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_borrowitem_asset");

            entity.HasOne(d => d.BorrowRequest).WithMany(p => p.TBorrowItems).HasConstraintName("fk_borrowitem_request");
        });

        modelBuilder.Entity<TBorrowItemUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("t_borrow_item_unit", tb => tb.HasComment("Unit yang diambil dan dikembalikan per peminjaman"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ConditionIn).HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT");
            entity.Property(e => e.ConditionOut).HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT");
            entity.Property(e => e.LateDays).HasComment("MAX(0, CEIL(hari returned_at - expected_return_date))");
            entity.Property(e => e.LateFee).HasComment("late_days x t_borrow_item.late_fee_per_day");
            entity.Property(e => e.ReturnStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM_KEMBALI, 2=KEMBALI, 3=KEMBALI_RUSAK, 4=HILANG");
            entity.Property(e => e.ReturnedAt).HasComment("Waktu unit di-scan saat dikembalikan");
            entity.Property(e => e.TakenAt).HasComment("Waktu unit di-scan saat diambil");

            entity.HasOne(d => d.AssetUnit).WithMany(p => p.TBorrowItemUnits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_borrow_item_unit_asset_unit_id");

            entity.HasOne(d => d.BorrowItem).WithMany(p => p.TBorrowItemUnits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_borrow_item_unit_borrow_item_id");
        });

        modelBuilder.Entity<TBorrowRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BorrowerEmail).HasComment("Email peminjam");
            entity.Property(e => e.BorrowerInstitution).HasComment("Asal instansi peminjam");
            entity.Property(e => e.BorrowerName).HasComment("Nama peminjam saat transaksi (wajib untuk borrower_type=3)");
            entity.Property(e => e.BorrowerPhone).HasComment("Kontak peminjam");
            entity.Property(e => e.BorrowerType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=ANGGOTA (member_id), 2=ORGANISASI/INSTANSI (external_party_id), 3=PERORANGAN_LUAR (borrower_name)");
            entity.Property(e => e.ContactId).HasComment("PIC dari organisasi peminjam");
            entity.Property(e => e.ExternalPartyId).HasComment("Organisasi peminjam, diisi jika borrower_type=2");
            entity.Property(e => e.Guarantee).HasComment("Jaminan, mis. KTM/KTP");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LateFeeWaived).HasComment("Potongan/penghapusan denda oleh pengurus");
            entity.Property(e => e.LetterId).HasComment("Surat permohonan peminjaman (opsional)");
            entity.Property(e => e.MemberId).HasComment("Diisi jika borrower_type=1");
            entity.Property(e => e.PaidAmount).HasComment("SUM(t_financial_transaction.amount) dengan borrow_request_id ini");
            entity.Property(e => e.PaymentStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS, 4=TIDAK_ADA_TAGIHAN. Lunas jika paid_amount >= total_rental_fee + total_late_fee - late_fee_waived");
            entity.Property(e => e.RentalScheme)
                .HasDefaultValueSql("'1'")
                .HasComment("1=TARIF_TETAP, 2=SEIKHLASNYA (anggota, sewa tidak ditagih), 3=GRATIS");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=PENDING, 2=APPROVED, 3=REJECTED, 4=BORROWED, 5=RETURNED, 6=OVERDUE, 7=CANCELLED");
            entity.Property(e => e.TotalLateFee).HasComment("SUM(t_borrow_item.late_fee), dihitung otomatis saat pengembalian");
            entity.Property(e => e.TotalRentalFee).HasComment("SUM(t_borrow_item.rental_fee); 0 untuk skema 2 dan 3");
            entity.Property(e => e.WaiveNote).HasComment("Alasan potongan denda");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.TBorrowRequests).HasConstraintName("fk_borrow_approver");

            entity.HasOne(d => d.Contact).WithMany(p => p.TBorrowRequests).HasConstraintName("fk_t_borrow_request_contact_id");

            entity.HasOne(d => d.ExternalParty).WithMany(p => p.TBorrowRequests).HasConstraintName("fk_t_borrow_request_external_party_id");

            entity.HasOne(d => d.Letter).WithMany(p => p.TBorrowRequests).HasConstraintName("fk_t_borrow_request_letter_id");

            entity.HasOne(d => d.Member).WithMany(p => p.TBorrowRequests).HasConstraintName("fk_borrow_member");
        });

        modelBuilder.Entity<TBudget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=SUBMITTED, 3=APPROVED, 4=REJECTED");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.TBudgets).HasConstraintName("fk_budget_approver");

            entity.HasOne(d => d.Period).WithMany(p => p.TBudgets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_budget_period");
        });

        modelBuilder.Entity<TBudgetDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Budget).WithMany(p => p.TBudgetDocuments).HasConstraintName("fk_budgetdoc_budget");
        });

        modelBuilder.Entity<TBudgetItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasComment("quantity x unit_price");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Quantity).HasDefaultValueSql("'1.00'");

            entity.HasOne(d => d.Budget).WithMany(p => p.TBudgetItems).HasConstraintName("fk_budgetitem_budget");

            entity.HasOne(d => d.Category).WithMany(p => p.TBudgetItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_budgetitem_category");

            entity.HasOne(d => d.Program).WithMany(p => p.TBudgetItems).HasConstraintName("fk_budgetitem_program");
        });

        modelBuilder.Entity<TCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AchievementId).HasComment("Untuk sertifikat prestasi");
            entity.Property(e => e.AssignmentId).HasComment("Untuk sertifikat kepengurusan");
            entity.Property(e => e.CertificateType).HasComment("1=PESERTA, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=TERBIT, 2=DICABUT");
            entity.Property(e => e.VerificationCode)
                .IsFixedLength()
                .HasComment("Kode acak untuk cek keaslian sertifikat secara publik");

            entity.HasOne(d => d.Achievement).WithMany(p => p.TCertificates).HasConstraintName("fk_cert_achievement");

            entity.HasOne(d => d.Activity).WithMany(p => p.TCertificates).HasConstraintName("fk_cert_activity");

            entity.HasOne(d => d.Assignment).WithMany(p => p.TCertificates).HasConstraintName("fk_cert_assignment");

            entity.HasOne(d => d.Member).WithMany(p => p.TCertificates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cert_member");
        });

        modelBuilder.Entity<TCommitteeApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DAFTAR, 2=DITERIMA, 3=DITOLAK, 4=MUNDUR");

            entity.HasOne(d => d.FormResponse).WithMany(p => p.TCommitteeApplications).HasConstraintName("fk_comapp_response");

            entity.HasOne(d => d.Member).WithMany(p => p.TCommitteeApplications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comapp_member");

            entity.HasOne(d => d.Recruitment).WithMany(p => p.TCommitteeApplications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comapp_recruitment");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.TCommitteeApplications).HasConstraintName("fk_comapp_reviewer");

            entity.HasOne(d => d.SectionChoice1).WithMany(p => p.TCommitteeApplicationSectionChoice1s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comapp_choice1");

            entity.HasOne(d => d.SectionChoice2).WithMany(p => p.TCommitteeApplicationSectionChoice2s).HasConstraintName("fk_comapp_choice2");
        });

        modelBuilder.Entity<TCommitteeMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApplicationId).HasComment("Jika berasal dari pendaftaran tim pelaksana");
            entity.Property(e => e.CommitteeRole).HasComment("1=KETUA_PELAKSANA, 2=WAKIL_KETUA, 3=SEKRETARIS, 4=BENDAHARA, 5=KOORDINATOR_SIE, 6=STAF, 7=OFFICIAL, 8=PELATIH");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SectionId).HasComment("NULL untuk pengurus inti (ketua pelaksana, sekretaris, bendahara)");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=AKTIF, 2=MUNDUR, 3=DIKELUARKAN");

            entity.HasOne(d => d.Activity).WithMany(p => p.TCommitteeMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_commem_activity");

            entity.HasOne(d => d.Application).WithMany(p => p.TCommitteeMembers).HasConstraintName("fk_commem_application");

            entity.HasOne(d => d.Member).WithMany(p => p.TCommitteeMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_commem_member");

            entity.HasOne(d => d.Section).WithMany(p => p.TCommitteeMembers).HasConstraintName("fk_commem_section");
        });

        modelBuilder.Entity<TCommitteeRecruitment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FormId).HasComment("Pertanyaan tambahan (t_form)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=BUKA, 3=TUTUP");

            entity.HasOne(d => d.Activity).WithOne(p => p.TCommitteeRecruitment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comrec_activity");

            entity.HasOne(d => d.Form).WithMany(p => p.TCommitteeRecruitments).HasConstraintName("fk_comrec_form");
        });

        modelBuilder.Entity<TCommitteeSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasComment("Job desk sie");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Name).HasComment("Acara, Konsumsi, Perlengkapan, Humas, Danus, Dokumentasi, Official, Transportasi & Akomodasi, Kostum, dll.");

            entity.HasOne(d => d.Activity).WithMany(p => p.TCommitteeSections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_section_activity");

            entity.HasOne(d => d.CoordinatorMember).WithMany(p => p.TCommitteeSections).HasConstraintName("fk_section_coordinator");
        });

        modelBuilder.Entity<TCompetition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsFeatured).HasComment("0=Tidak, 1=Ya | disorot di landing page");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Private, 1=Public");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'4'")
                .HasComment("1=INTERNAL, 2=KAMPUS, 3=REGIONAL, 4=NASIONAL, 5=INTERNASIONAL");
            entity.Property(e => e.Organizer).HasComment("Penyelenggara");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=UPCOMING, 2=REGISTRATION, 3=ONGOING, 4=FINISHED, 5=CANCELLED");
        });

        modelBuilder.Entity<TCompetitionMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CompetitionId).HasComment("NULL = materi umum, tidak terikat lomba tertentu");
            entity.Property(e => e.FileData).HasComment("NULL jika materi berupa link (url)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MaterialType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=FILE, 2=LINK, 3=VIDEO");
            entity.Property(e => e.Visibility)
                .HasDefaultValueSql("'2'")
                .HasComment("1=PUBLIC, 2=MEMBER, 3=PARTICIPANT");

            entity.HasOne(d => d.Competition).WithMany(p => p.TCompetitionMaterials).HasConstraintName("fk_material_competition");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.TCompetitionMaterials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_material_uploader");
        });

        modelBuilder.Entity<TCompetitionParticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ParticipantRole)
                .HasDefaultValueSql("'ANGGOTA'")
                .HasComment("KETUA_TIM, ANGGOTA, PEMBIMBING");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=REGISTERED, 2=PARTICIPATING, 3=FINISHED, 4=WITHDRAWN");

            entity.HasOne(d => d.Competition).WithMany(p => p.TCompetitionParticipants).HasConstraintName("fk_compart_competition");

            entity.HasOne(d => d.Member).WithMany(p => p.TCompetitionParticipants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_compart_member");
        });

        modelBuilder.Entity<TDebt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreditorName).HasComment("Pihak pemberi hutang");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PeriodId).HasComment("Periode saat hutang dibuat");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS, 4=DIHAPUSKAN");
            entity.Property(e => e.TransactionId).HasComment("Transaksi uang masuk jika hutang berupa pinjaman uang");

            entity.HasOne(d => d.Period).WithMany(p => p.TDebts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_debt_period");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TDebts).HasConstraintName("fk_debt_trx");
        });

        modelBuilder.Entity<TDebtPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.TransactionId).HasComment("Transaksi pengeluaran untuk pembayaran");

            entity.HasOne(d => d.Debt).WithMany(p => p.TDebtPayments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_debtpay_debt");

            entity.HasOne(d => d.Transaction).WithOne(p => p.TDebtPayment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_debtpay_trx");
        });

        modelBuilder.Entity<TDivisionMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.MemberRole)
                .HasDefaultValueSql("'ANGGOTA'")
                .HasComment("mis. KETUA, KAPTEN, ANGGOTA");
            entity.Property(e => e.PeriodId).HasComment("NULL = keanggotaan tim tidak terikat periode");

            entity.HasOne(d => d.Division).WithMany(p => p.TDivisionMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_divmember_division");

            entity.HasOne(d => d.Member).WithMany(p => p.TDivisionMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_divmember_member");

            entity.HasOne(d => d.Period).WithMany(p => p.TDivisionMembers).HasConstraintName("fk_divmember_period");
        });

        modelBuilder.Entity<TDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Visibility)
                .HasDefaultValueSql("'3'")
                .HasComment("1=PUBLIC, 2=MEMBER, 3=PENGURUS");

            entity.HasOne(d => d.Category).WithMany(p => p.TDocuments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_document_category");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.TDocuments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_document_uploader");
        });

        modelBuilder.Entity<TDocumentNumberSeq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PeriodKey).HasComment("Sesuai reset_period: 2026 (tahunan), 2026-10 (bulanan), ALL (tidak pernah)");
        });

        modelBuilder.Entity<TDuesBill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BillPeriod).HasComment("mis. 2026-10, 2026-GANJIL, 2026");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=UNPAID, 2=PAID, 3=WAIVED");
            entity.Property(e => e.TransactionId).HasComment("Transaksi kas masuk saat dibayar");

            entity.HasOne(d => d.DuesSetting).WithMany(p => p.TDuesBills)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_duesbill_setting");

            entity.HasOne(d => d.Member).WithMany(p => p.TDuesBills)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_duesbill_member");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TDuesBills).HasConstraintName("fk_duesbill_trx");
        });

        modelBuilder.Entity<TElection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PeriodId).HasComment("Periode kepengurusan yang akan dipilih");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=REGISTRATION, 3=OPEN, 4=CLOSED, 5=FINALIZED");

            entity.HasOne(d => d.FinalizedByNavigation).WithMany(p => p.TElections).HasConstraintName("fk_election_finalizer");

            entity.HasOne(d => d.Period).WithMany(p => p.TElections).HasConstraintName("fk_election_period");
        });

        modelBuilder.Entity<TElectionAuditEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EventType).HasComment("OPENED, CLOSED, CANDIDATE_VERIFIED, RESULT_FINALIZED, ...");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ActorUser).WithMany(p => p.TElectionAuditEvents).HasConstraintName("fk_elaudit_actor");

            entity.HasOne(d => d.Election).WithMany(p => p.TElectionAuditEvents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_elaudit_election");
        });

        modelBuilder.Entity<TElectionCandidate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=REGISTERED, 2=VERIFIED, 3=REJECTED, 4=WITHDRAWN");

            entity.HasOne(d => d.Election).WithMany(p => p.TElectionCandidates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_candidate_election");

            entity.HasOne(d => d.Member).WithMany(p => p.TElectionCandidates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_candidate_member");
        });

        modelBuilder.Entity<TElectionEligibleVoter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EligibilityStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=ELIGIBLE, 2=INELIGIBLE");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.VotedAt).HasComment("Terisi saat sudah memilih (tanpa menyimpan pilihannya)");

            entity.HasOne(d => d.Election).WithMany(p => p.TElectionEligibleVoters)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_voter_election");

            entity.HasOne(d => d.Member).WithMany(p => p.TElectionEligibleVoters)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_voter_member");
        });

        modelBuilder.Entity<TElectionResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsWinner).HasComment("0=Lose, 1=Winner");

            entity.HasOne(d => d.Candidate).WithMany(p => p.TElectionResults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_result_candidate");

            entity.HasOne(d => d.Election).WithMany(p => p.TElectionResults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_result_election");
        });

        modelBuilder.Entity<TElectionVote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("ACAK (bukan dari m_sequence) agar urutan memilih tidak terbaca");
            entity.Property(e => e.BallotReference)
                .IsFixedLength()
                .HasComment("Hash acak untuk verifikasi pemilih tanpa membuka identitas");
            entity.Property(e => e.EncryptedChoice).HasComment("AES-256-GCM: id kandidat + nilai acak. Didekripsi hanya saat finalisasi");
            entity.Property(e => e.InsBy).HasComment("Selalu NULL, jangan isi id pemilih");
            entity.Property(e => e.InsDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Isi tanggal saja (jam 00:00), jangan waktu persis");

            entity.HasOne(d => d.Election).WithMany(p => p.TElectionVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_vote_election");
        });

        modelBuilder.Entity<TFinancialAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FileData).HasComment("Bukti transaksi (nota, kuitansi, foto)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TFinancialAttachments).HasConstraintName("fk_finattach_trx");
        });

        modelBuilder.Entity<TFinancialTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityId).HasComment("Kegiatan (event, rapat, latihan, ...)");
            entity.Property(e => e.BorrowRequestId).HasComment("Pembayaran sewa/sumbangan/denda peminjaman; jenisnya lewat category_id");
            entity.Property(e => e.BudgetItemId).HasComment("Realisasi terhadap item anggaran");
            entity.Property(e => e.CompetitionId).HasComment("Lomba");
            entity.Property(e => e.FundingRequestId).HasComment("Pencairan dari pengajuan dana");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ProcurementId).HasComment("Pengeluaran untuk pengadaan barang");
            entity.Property(e => e.ProgramId).HasComment("Program kerja");
            entity.Property(e => e.SalesOrderId).HasComment("Pembayaran penjualan danus/merchandise");
            entity.Property(e => e.SponsorshipId).HasComment("Pemasukan dari sponsor");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=POSTED, 3=VOID");
            entity.Property(e => e.TransactionType).HasComment("1=INCOME, 2=EXPENSE");

            entity.HasOne(d => d.Account).WithMany(p => p.TFinancialTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trx_account");

            entity.HasOne(d => d.Activity).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_activity");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.TFinancialTransactionApprovedByNavigations).HasConstraintName("fk_trx_approver");

            entity.HasOne(d => d.BorrowRequest).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_t_financial_transaction_borrow_request_id");

            entity.HasOne(d => d.BudgetItem).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_budgetitem");

            entity.HasOne(d => d.Category).WithMany(p => p.TFinancialTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trx_category");

            entity.HasOne(d => d.Competition).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_competition");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TFinancialTransactionCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trx_creator");

            entity.HasOne(d => d.FundingRequest).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_funding");

            entity.HasOne(d => d.Period).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_period");

            entity.HasOne(d => d.Procurement).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_procurement");

            entity.HasOne(d => d.Program).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_program");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_sales");

            entity.HasOne(d => d.Sponsorship).WithMany(p => p.TFinancialTransactions).HasConstraintName("fk_trx_sponsorship");
        });

        modelBuilder.Entity<TForm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityId).HasComment("Untuk evaluasi kegiatan");
            entity.Property(e => e.FormType).HasComment("1=EVALUASI_KEGIATAN, 2=PENDAFTARAN_REKRUTMEN, 3=SURVEI_UMUM");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsAnonymous).HasComment("0=Tidak, 1=Ya | jawaban tidak menyimpan identitas");
            entity.Property(e => e.RecruitmentId).HasComment("Untuk pertanyaan tambahan rekrutmen");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=OPEN, 3=CLOSED");

            entity.HasOne(d => d.Activity).WithMany(p => p.TForms).HasConstraintName("fk_form_activity");

            entity.HasOne(d => d.Recruitment).WithMany(p => p.TForms).HasConstraintName("fk_form_recruitment");
        });

        modelBuilder.Entity<TFormAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AnswerFile).HasComment("Untuk pertanyaan tipe FILE");
            entity.Property(e => e.AnswerText).HasComment("Teks / pilihan / angka skala / tanggal");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Question).WithMany(p => p.TFormAnswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_answer_question");

            entity.HasOne(d => d.Response).WithMany(p => p.TFormAnswers).HasConstraintName("fk_answer_response");
        });

        modelBuilder.Entity<TFormQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsRequired).HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.Options).HasComment("Daftar pilihan untuk tipe 3 dan 4, mis. [\"Ya\",\"Tidak\"]");
            entity.Property(e => e.QuestionType).HasComment("1=TEKS_SINGKAT, 2=PARAGRAF, 3=PILIHAN_GANDA, 4=KOTAK_CENTANG, 5=SKALA_1_5, 6=TANGGAL, 7=FILE");

            entity.HasOne(d => d.Form).WithMany(p => p.TFormQuestions).HasConstraintName("fk_question_form");
        });

        modelBuilder.Entity<TFormResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApplicantId).HasComment("Diisi untuk jawaban formulir rekrutmen");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MemberId).HasComment("NULL jika form anonim atau responden bukan anggota");

            entity.HasOne(d => d.Applicant).WithMany(p => p.TFormResponses).HasConstraintName("fk_response_applicant");

            entity.HasOne(d => d.Form).WithMany(p => p.TFormResponses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_response_form");

            entity.HasOne(d => d.Member).WithMany(p => p.TFormResponses).HasConstraintName("fk_response_member");
        });

        modelBuilder.Entity<TFundingApproval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApprovalLevel).HasDefaultValueSql("'1'");
            entity.Property(e => e.Decision)
                .HasDefaultValueSql("'1'")
                .HasComment("1=PENDING, 2=APPROVED, 3=REJECTED, 4=REVISION");
            entity.Property(e => e.FlowStepId).HasComment("Langkah m_approval_flow_step yang diputuskan");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ApproverUser).WithMany(p => p.TFundingApprovals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_fundappr_user");

            entity.HasOne(d => d.FlowStep).WithMany(p => p.TFundingApprovals).HasConstraintName("fk_fundappr_step");

            entity.HasOne(d => d.Request).WithMany(p => p.TFundingApprovals).HasConstraintName("fk_fundappr_request");
        });

        modelBuilder.Entity<TFundingRealization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasComment("mis. Konsumsi rapat 20 orang");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Receipt).HasComment("Foto / scan nota");

            entity.HasOne(d => d.FundingRequest).WithMany(p => p.TFundingRealizations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_realization_request");
        });

        modelBuilder.Entity<TFundingRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CompetitionId).HasComment("Lomba");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RealizedAmount).HasComment("Total terpakai (jumlah t_funding_realization)");
            entity.Property(e => e.ReturnedAmount).HasComment("Sisa dana yang dikembalikan ke kas");
            entity.Property(e => e.SettledBy).HasComment("Bendahara yang memeriksa");
            entity.Property(e => e.SettlementStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM, 2=DIAJUKAN, 3=SELESAI");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=SUBMITTED, 3=APPROVED, 4=REJECTED, 5=DISBURSED");

            entity.HasOne(d => d.Activity).WithMany(p => p.TFundingRequests).HasConstraintName("fk_funding_activity");

            entity.HasOne(d => d.BudgetItem).WithMany(p => p.TFundingRequests).HasConstraintName("fk_funding_budgetitem");

            entity.HasOne(d => d.Competition).WithMany(p => p.TFundingRequests).HasConstraintName("fk_funding_competition");

            entity.HasOne(d => d.Period).WithMany(p => p.TFundingRequests).HasConstraintName("fk_funding_period");

            entity.HasOne(d => d.Program).WithMany(p => p.TFundingRequests).HasConstraintName("fk_funding_program");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.TFundingRequestRequestedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_funding_requester");

            entity.HasOne(d => d.SettledByNavigation).WithMany(p => p.TFundingRequestSettledByNavigations).HasConstraintName("fk_funding_settler");
        });

        modelBuilder.Entity<TGallery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsFeatured).HasComment("0=Tidak, 1=Ya | disorot di landing page");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Private, 1=Public");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=PUBLISHED, 3=ARCHIVED");

            entity.HasOne(d => d.Activity).WithMany(p => p.TGalleries).HasConstraintName("fk_gallery_activity");
        });

        modelBuilder.Entity<TGalleryPhoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsCover).HasComment("0=Tidak, 1=Ya | foto sampul galeri");

            entity.HasOne(d => d.Gallery).WithMany(p => p.TGalleryPhotos).HasConstraintName("fk_galphoto_gallery");
        });

        modelBuilder.Entity<TLetter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Direction).HasComment("1=IN, 2=OUT | IN = surat masuk, OUT = surat keluar");
            entity.Property(e => e.FileData).HasComment("File PDF final / scan surat masuk");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LetterNumber).HasComment("OUT: diisi saat approved; IN: nomor dari pengirim");
            entity.Property(e => e.ReceivedDate).HasComment("Khusus surat masuk");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=WAITING_APPROVAL, 3=APPROVED, 4=REJECTED, 5=SENT, 6=RECEIVED, 7=ARCHIVED");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TLetters)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_letter_creator");

            entity.HasOne(d => d.Period).WithMany(p => p.TLetters).HasConstraintName("fk_letter_period");

            entity.HasOne(d => d.Template).WithMany(p => p.TLetters).HasConstraintName("fk_letter_template");
        });

        modelBuilder.Entity<TLetterApproval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApprovalLevel).HasDefaultValueSql("'1'");
            entity.Property(e => e.Decision)
                .HasDefaultValueSql("'1'")
                .HasComment("1=PENDING, 2=APPROVED, 3=REJECTED");
            entity.Property(e => e.FlowStepId).HasComment("Langkah m_approval_flow_step yang diputuskan");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PositionId).HasComment("Jabatan penandatangan (Ketua, Sekretaris, ...)");
            entity.Property(e => e.Signature).HasComment("Gambar tanda tangan");

            entity.HasOne(d => d.ApproverUser).WithMany(p => p.TLetterApprovals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_letterappr_user");

            entity.HasOne(d => d.FlowStep).WithMany(p => p.TLetterApprovals).HasConstraintName("fk_letterappr_step");

            entity.HasOne(d => d.Letter).WithMany(p => p.TLetterApprovals).HasConstraintName("fk_letterappr_letter");

            entity.HasOne(d => d.Position).WithMany(p => p.TLetterApprovals).HasConstraintName("fk_letterappr_position");
        });

        modelBuilder.Entity<TLetterDisposition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=OPEN, 2=DONE");

            entity.HasOne(d => d.FromUser).WithMany(p => p.TLetterDispositions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disposition_from");

            entity.HasOne(d => d.Letter).WithMany(p => p.TLetterDispositions).HasConstraintName("fk_disposition_letter");

            entity.HasOne(d => d.ToMember).WithMany(p => p.TLetterDispositions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disposition_to");
        });

        modelBuilder.Entity<TMaintenanceLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssetUnitId).HasComment("Unit yang dirawat (aset track_unit=1)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=SCHEDULED, 2=IN_PROGRESS, 3=DONE");
            entity.Property(e => e.TransactionId).HasComment("Link ke pengeluaran kas jika ada biaya");

            entity.HasOne(d => d.Asset).WithMany(p => p.TMaintenanceLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_maintenance_asset");

            entity.HasOne(d => d.AssetUnit).WithMany(p => p.TMaintenanceLogs).HasConstraintName("fk_t_maintenance_log_asset_unit_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TMaintenanceLogs).HasConstraintName("fk_maintenance_trx");
        });

        modelBuilder.Entity<TManagementAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DivisionId).HasComment("Divisi yang dipimpin, mis. untuk jabatan Kadiv");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsCurrent)
                .HasDefaultValueSql("'1'")
                .HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.SkNumber).HasComment("Nomor SK pengangkatan");

            entity.HasOne(d => d.Division).WithMany(p => p.TManagementAssignments).HasConstraintName("fk_assignment_division");

            entity.HasOne(d => d.Member).WithMany(p => p.TManagementAssignments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_assignment_member");

            entity.HasOne(d => d.Period).WithMany(p => p.TManagementAssignments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_assignment_period");

            entity.HasOne(d => d.Position).WithMany(p => p.TManagementAssignments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_assignment_position");
        });

        modelBuilder.Entity<TMeetingMinute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ChairedByMemberId).HasComment("Pimpinan rapat");
            entity.Property(e => e.Content).HasComment("Isi / jalannya rapat");
            entity.Property(e => e.Decisions).HasComment("Keputusan rapat");
            entity.Property(e => e.FileData).HasComment("Lampiran (foto papan tulis, materi, dll.)");
            entity.Property(e => e.FollowUp).HasComment("Tindak lanjut + penanggung jawab");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.NotulisMemberId).HasComment("Penulis notulen");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=FINAL");

            entity.HasOne(d => d.ChairedByMember).WithMany(p => p.TMeetingMinuteChairedByMembers).HasConstraintName("fk_minutes_chair");

            entity.HasOne(d => d.FinalizedByNavigation).WithMany(p => p.TMeetingMinutes).HasConstraintName("fk_minutes_finalizer");

            entity.HasOne(d => d.NotulisMember).WithMany(p => p.TMeetingMinuteNotulisMembers).HasConstraintName("fk_minutes_notulis");

            entity.HasOne(d => d.Session).WithOne(p => p.TMeetingMinute)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_minutes_session");
        });

        modelBuilder.Entity<TMemberPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RefId).HasComment("id sumber: t_attendance_record, t_activity_participant, t_management_assignment, t_achievement");
            entity.Property(e => e.SourceType).HasComment("1=HADIR_KEGIATAN, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA");

            entity.HasOne(d => d.Member).WithMany(p => p.TMemberPoints)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_point_member");

            entity.HasOne(d => d.Period).WithMany(p => p.TMemberPoints).HasConstraintName("fk_point_period");
        });

        modelBuilder.Entity<TMessageOutbox>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Body).HasComment("Isi final. Untuk RESET_PASSWORD, kosongkan/samarkan kode setelah terkirim");
            entity.Property(e => e.Channel).HasComment("2=EMAIL, 3=WHATSAPP");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Recipient).HasComment("Alamat email / nomor WhatsApp");
            entity.Property(e => e.RefType).HasComment("Sumber pesan, mis. t_procurement_request");
            entity.Property(e => e.ScheduledAt).HasComment("Kapan boleh dikirim");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=MENUNGGU, 2=TERKIRIM, 3=GAGAL, 4=DIBATALKAN");
            entity.Property(e => e.UserId).HasComment("Penerima jika punya akun");

            entity.HasOne(d => d.User).WithMany(p => p.TMessageOutboxes).HasConstraintName("fk_outbox_user");
        });

        modelBuilder.Entity<TNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.NotifType).HasComment("LETTER_APPROVAL, FUNDING_APPROVAL, BORROW_DUE, DUES_BILL, ...");

            entity.HasOne(d => d.User).WithMany(p => p.TNotifications).HasConstraintName("fk_notification_user");
        });

        modelBuilder.Entity<TPeriodBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ClosingBalance).HasComment("Diisi saat periode ditutup");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.OpeningBalance).HasComment("Saldo awal = closing_balance periode sebelumnya");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=OPEN, 2=CLOSED");

            entity.HasOne(d => d.Account).WithMany(p => p.TPeriodBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_balance_account");

            entity.HasOne(d => d.ClosedByNavigation).WithMany(p => p.TPeriodBalances).HasConstraintName("fk_balance_closer");

            entity.HasOne(d => d.Period).WithMany(p => p.TPeriodBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_balance_period");
        });

        modelBuilder.Entity<TPeriodHandover>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.HandedByMemberId).HasComment("Ketua periode lama");
            entity.Property(e => e.HandoverNumber).HasComment("Nomor berita acara");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LpjDocument).HasComment("Dokumen LPJ akhir periode");
            entity.Property(e => e.ReceivedByMemberId).HasComment("Ketua periode baru");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=DIAJUKAN, 3=DISETUJUI, 4=DITOLAK");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.TPeriodHandovers).HasConstraintName("fk_handover_approver");

            entity.HasOne(d => d.FromPeriod).WithOne(p => p.TPeriodHandoverFromPeriod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_handover_from");

            entity.HasOne(d => d.HandedByMember).WithMany(p => p.TPeriodHandoverHandedByMembers).HasConstraintName("fk_handover_handed");

            entity.HasOne(d => d.ReceivedByMember).WithMany(p => p.TPeriodHandoverReceivedByMembers).HasConstraintName("fk_handover_received");

            entity.HasOne(d => d.ToPeriod).WithMany(p => p.TPeriodHandoverToPeriods)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_handover_to");
        });

        modelBuilder.Entity<TPeriodHandoverItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ItemCondition).HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG");
            entity.Property(e => e.ItemType).HasComment("1=SALDO, 2=ASET, 3=PROGRAM, 4=PIUTANG, 5=HUTANG, 6=PINJAMAN, 7=DOKUMEN, 8=LAINNYA, 9=MOU_AKTIF, 10=KEWAJIBAN_SPONSOR, 11=SPONSOR_BERJALAN, 12=PENGADAAN_BERJALAN");
            entity.Property(e => e.RefId).HasComment("id sesuai item_type: m_financial_account, m_asset, t_program, t_dues_bill, t_debt, t_borrow_request, t_document, -, t_partnership_agreement, t_sponsorship_benefit, t_sponsorship, t_procurement_request");

            entity.HasOne(d => d.Handover).WithMany(p => p.TPeriodHandoverItems).HasConstraintName("fk_handover_item");
        });

        modelBuilder.Entity<TProcurementApproval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApprovalLevel).HasComment("Urutan langkah m_approval_flow (PROCUREMENT), mis. 1=Bendahara, 2=Ketua Umum");
            entity.Property(e => e.Decision).HasComment("1=SETUJU, 2=REVISI, 3=TOLAK");
            entity.Property(e => e.FlowStepId).HasComment("Langkah m_approval_flow_step yang diputuskan");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ApproverUser).WithMany(p => p.TProcurementApprovals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pgdappr_user");

            entity.HasOne(d => d.FlowStep).WithMany(p => p.TProcurementApprovals).HasConstraintName("fk_pgdappr_step");

            entity.HasOne(d => d.Request).WithMany(p => p.TProcurementApprovals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pgdappr_request");
        });

        modelBuilder.Entity<TProcurementItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssetCategoryId).HasComment("Wajib jika is_asset = 1");
            entity.Property(e => e.ExternalPartyId).HasComment("Vendor (m_partner)");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsAsset).HasComment("0=Habis pakai, 1=Masuk inventaris sebagai aset");
            entity.Property(e => e.ReceivedQuantity).HasComment("Dihitung dari penerimaan, tidak boleh melebihi quantity");
            entity.Property(e => e.Specification).HasComment("Merek, ukuran, spesifikasi");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'pcs'")
                .HasComment("pcs, set, lusin, dll.");

            entity.HasOne(d => d.AssetCategory).WithMany(p => p.TProcurementItems).HasConstraintName("fk_pgditem_category");

            entity.HasOne(d => d.ExternalParty).WithMany(p => p.TProcurementItems).HasConstraintName("fk_pgditem_party");

            entity.HasOne(d => d.Request).WithMany(p => p.TProcurementItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pgditem_request");
        });

        modelBuilder.Entity<TProcurementQuotation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsSelected).HasComment("0=Tidak, 1=Ya");
            entity.Property(e => e.ItemId).HasComment("NULL = penawaran untuk seluruh usulan");
            entity.Property(e => e.VendorName).HasComment("Jika vendor belum terdaftar di m_partner");

            entity.HasOne(d => d.ExternalParty).WithMany(p => p.TProcurementQuotations).HasConstraintName("fk_quote_party");

            entity.HasOne(d => d.Item).WithMany(p => p.TProcurementQuotations).HasConstraintName("fk_quote_item");

            entity.HasOne(d => d.Request).WithMany(p => p.TProcurementQuotations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quote_request");
        });

        modelBuilder.Entity<TProcurementReceipt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ReceivedByMember).WithMany(p => p.TProcurementReceipts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pgdrcpt_member");

            entity.HasOne(d => d.Request).WithMany(p => p.TProcurementReceipts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pgdrcpt_request");
        });

        modelBuilder.Entity<TProcurementReceiptItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ItemCondition)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG (sama dengan m_asset.asset_condition)");

            entity.HasOne(d => d.Item).WithMany(p => p.TProcurementReceiptItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rcptitem_item");

            entity.HasOne(d => d.Receipt).WithMany(p => p.TProcurementReceiptItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rcptitem_receipt");
        });

        modelBuilder.Entity<TProcurementRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BudgetItemId).HasComment("Item anggaran sumber dana");
            entity.Property(e => e.BudgetRemaining).HasComment("Sisa item anggaran saat diajukan (untuk ditampilkan ke approver)");
            entity.Property(e => e.FundingRequestId).HasComment("Wajib jika payment_method = 2 mulai status DIBELI");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsOverBudget).HasComment("0=Tidak, 1=Ya | total melebihi sisa item anggaran saat diajukan; hanya peringatan, tidak menolak");
            entity.Property(e => e.PaymentMethod)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DIBAYAR_LANGSUNG (transaksi pengeluaran oleh bendahara), 2=PENGAJUAN_DANA (uang muka lalu dipertanggungjawabkan)");
            entity.Property(e => e.ProgramId).HasComment("Tujuan: maksimal satu dari activity/program");
            entity.Property(e => e.Reason).HasComment("Alasan kebutuhan");
            entity.Property(e => e.RequestNumber).HasComment("PGD-{org}-{yyyy}-{seq}");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=DIAJUKAN, 3=REVISI, 4=DISETUJUI, 5=DITOLAK, 6=DIBELI, 7=DITERIMA_SEBAGIAN, 8=SELESAI, 9=BATAL");
            entity.Property(e => e.TotalActual).HasComment("Jumlah transaksi POSTED dengan procurement_id ini");
            entity.Property(e => e.TotalEstimate).HasComment("Dihitung dari item");

            entity.HasOne(d => d.Activity).WithMany(p => p.TProcurementRequests).HasConstraintName("fk_pgd_activity");

            entity.HasOne(d => d.BudgetItem).WithMany(p => p.TProcurementRequests).HasConstraintName("fk_pgd_budgetitem");

            entity.HasOne(d => d.FundingRequest).WithMany(p => p.TProcurementRequests).HasConstraintName("fk_pgd_funding");

            entity.HasOne(d => d.Period).WithMany(p => p.TProcurementRequests).HasConstraintName("fk_pgd_period");

            entity.HasOne(d => d.Program).WithMany(p => p.TProcurementRequests).HasConstraintName("fk_pgd_program");

            entity.HasOne(d => d.RequestedByMember).WithMany(p => p.TProcurementRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pgd_requester");
        });

        modelBuilder.Entity<TProductStockMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MovementType).HasComment("1=MASUK (produksi/beli), 2=TERJUAL, 3=PENYESUAIAN, 4=RETUR, 5=RUSAK/HILANG");
            entity.Property(e => e.OrderId).HasComment("Untuk TERJUAL / RETUR");
            entity.Property(e => e.ProcurementId).HasComment("Untuk MASUK dari pengadaan");
            entity.Property(e => e.Quantity).HasComment("Positif = bertambah, negatif = berkurang");

            entity.HasOne(d => d.Order).WithMany(p => p.TProductStockMovements).HasConstraintName("fk_stock_order");

            entity.HasOne(d => d.Procurement).WithMany(p => p.TProductStockMovements).HasConstraintName("fk_stock_procurement");

            entity.HasOne(d => d.Product).WithMany(p => p.TProductStockMovements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_stock_product");

            entity.HasOne(d => d.Variant).WithMany(p => p.TProductStockMovements).HasConstraintName("fk_stock_variant");
        });

        modelBuilder.Entity<TProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PicMemberId).HasComment("Penanggung jawab");
            entity.Property(e => e.PreviousProgramId).HasComment("Program periode sebelumnya yang dilanjutkan");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=PLANNED, 2=ONGOING, 3=DONE, 4=CANCELLED");

            entity.HasOne(d => d.Period).WithMany(p => p.TPrograms)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_program_period");

            entity.HasOne(d => d.PicMember).WithMany(p => p.TPrograms).HasConstraintName("fk_program_pic");

            entity.HasOne(d => d.PreviousProgram).WithMany(p => p.InversePreviousProgram).HasConstraintName("fk_program_previous");
        });

        modelBuilder.Entity<TProgramTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DivisionId).HasComment("Opsional: tugas untuk divisi tertentu");
            entity.Property(e => e.DoneBy).HasComment("User yang menandai selesai");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsMilestone).HasComment("0=Tidak, 1=Ya | tampil di timeline periode");
            entity.Property(e => e.Priority)
                .HasDefaultValueSql("'2'")
                .HasComment("1=RENDAH, 2=SEDANG, 3=TINGGI");
            entity.Property(e => e.ProgramId).HasComment("NULL = tugas umum pengurus (bukan bagian program kerja)");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM, 2=PROSES, 3=SELESAI, 4=BATAL (sama dengan t_activity_checklist)");

            entity.HasOne(d => d.Division).WithMany(p => p.TProgramTasks).HasConstraintName("fk_task_division");

            entity.HasOne(d => d.DoneByNavigation).WithMany(p => p.TProgramTasks).HasConstraintName("fk_task_doneby");

            entity.HasOne(d => d.Period).WithMany(p => p.TProgramTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_task_period");

            entity.HasOne(d => d.Program).WithMany(p => p.TProgramTasks)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_task_program");

            entity.HasOne(d => d.ResponsibleMember).WithMany(p => p.TProgramTasks).HasConstraintName("fk_task_member");
        });

        modelBuilder.Entity<TRecruitment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AcceptPublic).HasComment("0=Hanya mahasiswa, 1=Juga menerima pendaftar umum");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DRAFT, 2=OPEN, 3=CLOSED");

            entity.HasOne(d => d.Period).WithMany(p => p.TRecruitments).HasConstraintName("fk_recruitment_period");
        });

        modelBuilder.Entity<TRecruitmentApplicant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApplicantCategory)
                .HasDefaultValueSql("'1'")
                .HasComment("1=MAHASISWA, 2=UMUM");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Institution).HasComment("Asal instansi / kampus lain / pekerjaan (untuk UMUM)");
            entity.Property(e => e.MemberId).HasComment("Terisi jika sudah terdaftar sebagai m_member");
            entity.Property(e => e.Nim).HasComment("Wajib untuk MAHASISWA");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=SUBMITTED, 2=SCREENING, 3=INTERVIEW, 4=ACCEPTED, 5=REJECTED");

            entity.HasOne(d => d.Member).WithMany(p => p.TRecruitmentApplicants).HasConstraintName("fk_applicant_member");

            entity.HasOne(d => d.Recruitment).WithMany(p => p.TRecruitmentApplicants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_applicant_recruitment");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.TRecruitmentApplicants).HasConstraintName("fk_applicant_reviewer");
        });

        modelBuilder.Entity<TSalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityId).HasComment("Opsional: penjualan untuk kegiatan tertentu");
            entity.Property(e => e.BuyerMemberId).HasComment("Pembeli anggota");
            entity.Property(e => e.BuyerName).HasComment("Pembeli non-anggota");
            entity.Property(e => e.FulfillmentStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DIPESAN, 2=SIAP_DIAMBIL, 3=SELESAI, 4=BATAL");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.OrderNumber).HasComment("Dari penomoran terpusat, jenis DANUS");
            entity.Property(e => e.PaidAmount).HasComment("Jumlah transaksi POSTED dengan sales_order_id ini");
            entity.Property(e => e.PaymentStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS");
            entity.Property(e => e.PicMemberId).HasComment("Penjual / penanggung jawab");
            entity.Property(e => e.TotalAmount).HasComment("Jumlah subtotal item");

            entity.HasOne(d => d.Activity).WithMany(p => p.TSalesOrders).HasConstraintName("fk_sales_activity");

            entity.HasOne(d => d.BuyerMember).WithMany(p => p.TSalesOrderBuyerMembers).HasConstraintName("fk_sales_buyer");

            entity.HasOne(d => d.Period).WithMany(p => p.TSalesOrders).HasConstraintName("fk_sales_period");

            entity.HasOne(d => d.PicMember).WithMany(p => p.TSalesOrderPicMembers).HasConstraintName("fk_sales_pic");
        });

        modelBuilder.Entity<TSalesOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Notes).HasComment("mis. nama sablon");
            entity.Property(e => e.UnitPrice).HasComment("Harga saat dipesan");

            entity.HasOne(d => d.Order).WithMany(p => p.TSalesOrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_salesitem_order");

            entity.HasOne(d => d.Product).WithMany(p => p.TSalesOrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_salesitem_product");

            entity.HasOne(d => d.Variant).WithMany(p => p.TSalesOrderItems).HasConstraintName("fk_salesitem_variant");
        });

        modelBuilder.Entity<TScheduledJobLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.JobName).HasComment("mis. SEND_OUTBOX, REMIND_CHECKLIST, EXPIRE_MOU, CLOSE_ATTENDANCE");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=BERJALAN, 2=SUKSES, 3=GAGAL");
        });

        modelBuilder.Entity<TSponsorship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("t_sponsorship", tb => tb.HasComment("Sponsorship: brand memilih paket"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityId).HasComment("Kegiatan yang disponsori (opsional)");
            entity.Property(e => e.Amount).HasComment("Nilai sponsorship yang disepakati");
            entity.Property(e => e.InKindDescription).HasComment("Rincian jika berupa barang/jasa");
            entity.Property(e => e.PackageId).HasComment("Paket yang dipilih (NULL = kesepakatan khusus)");
            entity.Property(e => e.PackageName).HasComment("Salinan nama paket saat dicatat");
            entity.Property(e => e.SponsorshipForm)
                .HasDefaultValueSql("'1'")
                .HasComment("1=DANA, 2=BARANG, 3=JASA, 4=MEDIA");
            entity.Property(e => e.SponsorshipNumber).HasComment("SPN-{org}-{yyyy}-{seq}");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=AKTIF, 2=SELESAI, 3=BATAL");

            entity.HasOne(d => d.Activity).WithMany(p => p.TSponsorships).HasConstraintName("fk_spn_activity");

            entity.HasOne(d => d.Contact).WithMany(p => p.TSponsorships).HasConstraintName("fk_spn_contact");

            entity.HasOne(d => d.ExternalParty).WithMany(p => p.TSponsorships)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_spn_party");

            entity.HasOne(d => d.Package).WithMany(p => p.TSponsorships).HasConstraintName("fk_t_sponsorship_package_id");

            entity.HasOne(d => d.Period).WithMany(p => p.TSponsorships).HasConstraintName("fk_spn_period");

            entity.HasOne(d => d.PicMember).WithMany(p => p.TSponsorships).HasConstraintName("fk_spn_pic");
        });

        modelBuilder.Entity<TSponsorshipBenefit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("t_sponsorship_benefit", tb => tb.HasComment("Checklist benefit per sponsorship"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DoneAt).HasComment("Kapan dicentang");
            entity.Property(e => e.DoneBy).HasComment("User yang mencentang");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsDone).HasComment("1=sudah diberikan ke sponsor");
            entity.Property(e => e.ProofFile).HasComment("Bukti, mis. foto banner atau screenshot postingan");
            entity.Property(e => e.SeqNo)
                .HasDefaultValueSql("'1'")
                .HasComment("Urutan benefit");
            entity.Property(e => e.Title).HasComment("Logo di banner, booth, adlibs MC, posting Instagram, dll.");

            entity.HasOne(d => d.DoneByNavigation).WithMany(p => p.TSponsorshipBenefits).HasConstraintName("fk_t_sponsorship_benefit_done_by");

            entity.HasOne(d => d.Sponsorship).WithMany(p => p.TSponsorshipBenefits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_benefit_spn");
        });

        modelBuilder.Entity<TSponsorshipMou>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("t_sponsorship_mou", tb => tb.HasComment("MoU sponsorship"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Document).HasComment("File MoU");
            entity.Property(e => e.MouNumber).HasComment("Nomor MoU/PKS");

            entity.HasOne(d => d.Sponsorship).WithMany(p => p.TSponsorshipMous)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_sponsorship_mou_sponsorship_id");
        });

        modelBuilder.Entity<TTimelinePlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivityId).HasComment("Terisi saat rencana dijadikan kegiatan");
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PicMemberId).HasComment("Penanggung jawab");
            entity.Property(e => e.PlanMonth).HasComment("1-12");
            entity.Property(e => e.PlanType)
                .HasDefaultValueSql("'1'")
                .HasComment("1=EVENT, 2=RAPAT, 3=LATIHAN, 4=PERTEMUAN, 5=DEADLINE, 6=LAINNYA");
            entity.Property(e => e.ProgramId).HasComment("Program kerja terkait");
            entity.Property(e => e.StartDate).HasComment("NULL = tanggal belum pasti");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("1=RENCANA, 2=TERJADWAL, 3=SELESAI, 4=BATAL");

            entity.HasOne(d => d.Activity).WithMany(p => p.TTimelinePlans).HasConstraintName("fk_plan_activity");

            entity.HasOne(d => d.Period).WithMany(p => p.TTimelinePlans).HasConstraintName("fk_plan_period");

            entity.HasOne(d => d.PicMember).WithMany(p => p.TTimelinePlans).HasConstraintName("fk_plan_pic");

            entity.HasOne(d => d.Program).WithMany(p => p.TTimelinePlans).HasConstraintName("fk_plan_program");
        });

        modelBuilder.Entity<TUserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InsDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.TokenHash)
                .IsFixedLength()
                .HasComment("Simpan hash (SHA-256), bukan token mentah");
            entity.Property(e => e.TokenType).HasComment("1=RESET_PASSWORD, 2=VERIFY_EMAIL, 3=REFRESH");

            entity.HasOne(d => d.User).WithMany(p => p.TUserTokens).HasConstraintName("fk_token_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
