using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_committee_application")]
[Index("SectionChoice1Id", Name = "fk_comapp_choice1")]
[Index("SectionChoice2Id", Name = "fk_comapp_choice2")]
[Index("MemberId", Name = "fk_comapp_member")]
[Index("FormResponseId", Name = "fk_comapp_response")]
[Index("ReviewedBy", Name = "fk_comapp_reviewer")]
[Index("RecruitmentId", "MemberId", Name = "uq_committee_application", IsUnique = true)]
public partial class TCommitteeApplication
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("recruitment_id")]
    public int RecruitmentId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("section_choice_1_id")]
    public int SectionChoice1Id { get; set; }

    [Column("section_choice_2_id")]
    public int? SectionChoice2Id { get; set; }

    [Column("reason", TypeName = "text")]
    public string? Reason { get; set; }

    [Column("form_response_id")]
    public int? FormResponseId { get; set; }

    /// <summary>
    /// 1=DAFTAR, 2=DITERIMA, 3=DITOLAK, 4=MUNDUR
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("reviewed_by")]
    public int? ReviewedBy { get; set; }

    [Column("reviewed_at", TypeName = "datetime")]
    public DateTime? ReviewedAt { get; set; }

    [Column("review_note")]
    [StringLength(500)]
    public string? ReviewNote { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("FormResponseId")]
    [InverseProperty("TCommitteeApplications")]
    public virtual TFormResponse? FormResponse { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("TCommitteeApplications")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("RecruitmentId")]
    [InverseProperty("TCommitteeApplications")]
    public virtual TCommitteeRecruitment Recruitment { get; set; } = null!;

    [ForeignKey("ReviewedBy")]
    [InverseProperty("TCommitteeApplications")]
    public virtual MUser? ReviewedByNavigation { get; set; }

    [ForeignKey("SectionChoice1Id")]
    [InverseProperty("TCommitteeApplicationSectionChoice1s")]
    public virtual TCommitteeSection SectionChoice1 { get; set; } = null!;

    [ForeignKey("SectionChoice2Id")]
    [InverseProperty("TCommitteeApplicationSectionChoice2s")]
    public virtual TCommitteeSection? SectionChoice2 { get; set; }

    [InverseProperty("Application")]
    public virtual ICollection<TCommitteeMember> TCommitteeMembers { get; set; } = new List<TCommitteeMember>();
}
