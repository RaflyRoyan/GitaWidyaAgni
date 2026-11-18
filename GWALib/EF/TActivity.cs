using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity")]
[Index("CompetitionId", Name = "fk_activity_competition")]
[Index("CreatedBy", Name = "fk_activity_creator")]
[Index("DivisionId", Name = "fk_activity_division")]
[Index("SectionId", Name = "fk_activity_section")]
[Index("ParentActivityId", Name = "fk_activity_parent")]
[Index("ProgramId", Name = "fk_activity_program")]
[Index("StartAt", Name = "ix_activity_start")]
[Index("ActivityType", "StartAt", Name = "ix_activity_type")]
public partial class TActivity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// NULL = kegiatan insidental di luar proker
    /// </summary>
    [Column("program_id")]
    public int? ProgramId { get; set; }

    /// <summary>
    /// Kegiatan induk (event/lomba) untuk rapat/latihan persiapan; maksimal 1 tingkat
    /// </summary>
    [Column("parent_activity_id")]
    public int? ParentActivityId { get; set; }

    /// <summary>
    /// Wajib untuk activity_type = 6 (LOMBA)
    /// </summary>
    [Column("competition_id")]
    public int? CompetitionId { get; set; }

    /// <summary>
    /// 1=EVENT, 2=RAPAT, 3=LATIHAN, 4=PERTEMUAN, 5=LAINNYA, 6=LOMBA, 7=PIKET (mis. satu kegiatan &quot;Piket Senin&quot; + jadwal rutin + petugas di t_activity_participant)
    /// </summary>
    [Column("activity_type")]
    public int ActivityType { get; set; }

    /// <summary>
    /// 1=SEMUA_ANGGOTA, 2=PENGURUS, 3=PESERTA_TERDAFTAR (t_activity_participant), 4=DIVISI (t_division_member), 5=TIM_PELAKSANA (t_committee_member kegiatan induk) | penentu siapa yang dicatat ALPA
    /// </summary>
    [Column("audience_type")]
    public int AudienceType { get; set; }

    /// <summary>
    /// Diisi jika audience_type = 4 (rapat divisi / latihan tim)
    /// </summary>
    [Column("division_id")]
    public int? DivisionId { get; set; }

    /// <summary>
    /// Wajib bila audience_type = SECTION
    /// </summary>
    [Column("section_id")]
    public int? SectionId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("location")]
    [StringLength(255)]
    public string? Location { get; set; }

    [Column("latitude")]
    [Precision(10, 7)]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    [Precision(10, 7)]
    public decimal? Longitude { get; set; }

    [Column("start_at", TypeName = "datetime")]
    public DateTime StartAt { get; set; }

    [Column("end_at", TypeName = "datetime")]
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 0=Private, 1=Public | 1 = tampil di landing page
    /// </summary>
    [Column("is_public")]
    public int IsPublic { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | disorot di landing page
    /// </summary>
    [Column("is_featured")]
    public int IsFeatured { get; set; }

    /// <summary>
    /// 1=PLANNED, 2=ONGOING, 3=DONE, 4=CANCELLED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("TActivities")]
    public virtual TCompetition? Competition { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TActivities")]
    public virtual MUser CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DivisionId")]
    [InverseProperty("TActivities")]
    public virtual MDivision? Division { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("TActivities")]
    public virtual MSection? Section { get; set; }

    [InverseProperty("ParentActivity")]
    public virtual ICollection<TActivity> InverseParentActivity { get; set; } = new List<TActivity>();

    [InverseProperty("Activity")]
    public virtual TLpj? TLpj { get; set; }

    [InverseProperty("Activity")]
    public virtual ICollection<MProduct> MProducts { get; set; } = new List<MProduct>();

    [ForeignKey("ParentActivityId")]
    [InverseProperty("InverseParentActivity")]
    public virtual TActivity? ParentActivity { get; set; }

    [ForeignKey("ProgramId")]
    [InverseProperty("TActivities")]
    public virtual TProgram? Program { get; set; }

    [InverseProperty("Activity")]
    public virtual ICollection<TActivityChecklist> TActivityChecklists { get; set; } = new List<TActivityChecklist>();

    [InverseProperty("Activity")]
    public virtual ICollection<TActivityCollaborator> TActivityCollaborators { get; set; } = new List<TActivityCollaborator>();

    [InverseProperty("Activity")]
    public virtual ICollection<TActivityParticipant> TActivityParticipants { get; set; } = new List<TActivityParticipant>();

    [InverseProperty("Activity")]
    public virtual ICollection<TActivityReport> TActivityReports { get; set; } = new List<TActivityReport>();

    [InverseProperty("Activity")]
    public virtual ICollection<TActivityRundown> TActivityRundowns { get; set; } = new List<TActivityRundown>();

    [InverseProperty("Activity")]
    public virtual ICollection<TActivitySchedule> TActivitySchedules { get; set; } = new List<TActivitySchedule>();

    [InverseProperty("Activity")]
    public virtual ICollection<TActivityTripMember> TActivityTripMembers { get; set; } = new List<TActivityTripMember>();

    [InverseProperty("Activity")]
    public virtual ICollection<TAttendanceSession> TAttendanceSessions { get; set; } = new List<TAttendanceSession>();

    [InverseProperty("Activity")]
    public virtual ICollection<TCertificate> TCertificates { get; set; } = new List<TCertificate>();

    [InverseProperty("Activity")]
    public virtual ICollection<TCommitteeMember> TCommitteeMembers { get; set; } = new List<TCommitteeMember>();

    [InverseProperty("Activity")]
    public virtual TCommitteeRecruitment? TCommitteeRecruitment { get; set; }

    [InverseProperty("Activity")]
    public virtual ICollection<TCommitteeSection> TCommitteeSections { get; set; } = new List<TCommitteeSection>();

    [InverseProperty("Activity")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Activity")]
    public virtual ICollection<TForm> TForms { get; set; } = new List<TForm>();

    [InverseProperty("Activity")]
    public virtual ICollection<TFundingRequest> TFundingRequests { get; set; } = new List<TFundingRequest>();

    [InverseProperty("Activity")]
    public virtual ICollection<TGallery> TGalleries { get; set; } = new List<TGallery>();

    [InverseProperty("Activity")]
    public virtual ICollection<TProcurementRequest> TProcurementRequests { get; set; } = new List<TProcurementRequest>();

    [InverseProperty("Activity")]
    public virtual ICollection<TSalesOrder> TSalesOrders { get; set; } = new List<TSalesOrder>();

    [InverseProperty("Activity")]
    public virtual ICollection<TSponsorship> TSponsorships { get; set; } = new List<TSponsorship>();

    [InverseProperty("Activity")]
    public virtual ICollection<TTimelinePlan> TTimelinePlans { get; set; } = new List<TTimelinePlan>();
}
