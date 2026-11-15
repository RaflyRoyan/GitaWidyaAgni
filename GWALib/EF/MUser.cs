using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_user")]
[Index("Username", Name = "uq_user_username", IsUnique = true)]
public partial class MUser
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Kode reset password; dikosongkan (NULL) setelah dipakai
    /// </summary>
    [Column("reset_key")]
    [StringLength(50)]
    public string? ResetKey { get; set; }

    /// <summary>
    /// 0=Tidak Aktif, 1=Aktif
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("last_login_at", TypeName = "datetime")]
    public DateTime? LastLoginAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<MApprovalFlowStep> MApprovalFlowSteps { get; set; } = new List<MApprovalFlowStep>();

    [InverseProperty("User")]
    public virtual MMember? MMember { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<MUserNotificationPref> MUserNotificationPrefs { get; set; } = new List<MUserNotificationPref>();

    [InverseProperty("User")]
    public virtual ICollection<MUserRole> MUserRoles { get; set; } = new List<MUserRole>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TActivity> TActivities { get; set; } = new List<TActivity>();

    [InverseProperty("DoneByNavigation")]
    public virtual ICollection<TActivityChecklist> TActivityChecklists { get; set; } = new List<TActivityChecklist>();

    [InverseProperty("RespondedByNavigation")]
    public virtual ICollection<TActivityCollaborator> TActivityCollaborators { get; set; } = new List<TActivityCollaborator>();

    [InverseProperty("User")]
    public virtual ICollection<TActivityLog> TActivityLogs { get; set; } = new List<TActivityLog>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<TActivityReport> TActivityReportApprovedByNavigations { get; set; } = new List<TActivityReport>();

    [InverseProperty("SubmittedByNavigation")]
    public virtual ICollection<TActivityReport> TActivityReportSubmittedByNavigations { get; set; } = new List<TActivityReport>();

    [InverseProperty("AuthorUser")]
    public virtual ICollection<TAnnouncement> TAnnouncements { get; set; } = new List<TAnnouncement>();

    [InverseProperty("RespondedByNavigation")]
    public virtual ICollection<TAspiration> TAspirationRespondedByNavigations { get; set; } = new List<TAspiration>();

    [InverseProperty("User")]
    public virtual ICollection<TAspiration> TAspirationUsers { get; set; } = new List<TAspiration>();

    [InverseProperty("InspectedByNavigation")]
    public virtual ICollection<TAssetInspection> TAssetInspections { get; set; } = new List<TAssetInspection>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TAssetMovement> TAssetMovements { get; set; } = new List<TAssetMovement>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<TBorrowRequest> TBorrowRequests { get; set; } = new List<TBorrowRequest>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<TBudget> TBudgets { get; set; } = new List<TBudget>();

    [InverseProperty("ReviewedByNavigation")]
    public virtual ICollection<TCommitteeApplication> TCommitteeApplications { get; set; } = new List<TCommitteeApplication>();

    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<TCompetitionMaterial> TCompetitionMaterials { get; set; } = new List<TCompetitionMaterial>();

    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<TDocument> TDocuments { get; set; } = new List<TDocument>();

    [InverseProperty("ActorUser")]
    public virtual ICollection<TElectionAuditEvent> TElectionAuditEvents { get; set; } = new List<TElectionAuditEvent>();

    [InverseProperty("FinalizedByNavigation")]
    public virtual ICollection<TElection> TElections { get; set; } = new List<TElection>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactionApprovedByNavigations { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactionCreatedByNavigations { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("ApproverUser")]
    public virtual ICollection<TFundingApproval> TFundingApprovals { get; set; } = new List<TFundingApproval>();

    [InverseProperty("RequestedByNavigation")]
    public virtual ICollection<TFundingRequest> TFundingRequestRequestedByNavigations { get; set; } = new List<TFundingRequest>();

    [InverseProperty("SettledByNavigation")]
    public virtual ICollection<TFundingRequest> TFundingRequestSettledByNavigations { get; set; } = new List<TFundingRequest>();

    [InverseProperty("ApproverUser")]
    public virtual ICollection<TLetterApproval> TLetterApprovals { get; set; } = new List<TLetterApproval>();

    [InverseProperty("FromUser")]
    public virtual ICollection<TLetterDisposition> TLetterDispositions { get; set; } = new List<TLetterDisposition>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TLetter> TLetters { get; set; } = new List<TLetter>();

    [InverseProperty("FinalizedByNavigation")]
    public virtual ICollection<TMeetingMinute> TMeetingMinutes { get; set; } = new List<TMeetingMinute>();

    [InverseProperty("User")]
    public virtual ICollection<TMessageOutbox> TMessageOutboxes { get; set; } = new List<TMessageOutbox>();

    [InverseProperty("User")]
    public virtual ICollection<TNotification> TNotifications { get; set; } = new List<TNotification>();

    [InverseProperty("ClosedByNavigation")]
    public virtual ICollection<TPeriodBalance> TPeriodBalances { get; set; } = new List<TPeriodBalance>();

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<TPeriodHandover> TPeriodHandovers { get; set; } = new List<TPeriodHandover>();

    [InverseProperty("ApproverUser")]
    public virtual ICollection<TProcurementApproval> TProcurementApprovals { get; set; } = new List<TProcurementApproval>();

    [InverseProperty("DoneByNavigation")]
    public virtual ICollection<TProgramTask> TProgramTasks { get; set; } = new List<TProgramTask>();

    [InverseProperty("ReviewedByNavigation")]
    public virtual ICollection<TRecruitmentApplicant> TRecruitmentApplicants { get; set; } = new List<TRecruitmentApplicant>();

    [InverseProperty("DoneByNavigation")]
    public virtual ICollection<TSponsorshipBenefit> TSponsorshipBenefits { get; set; } = new List<TSponsorshipBenefit>();

    [InverseProperty("User")]
    public virtual ICollection<TUserToken> TUserTokens { get; set; } = new List<TUserToken>();
}
