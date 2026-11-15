using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_committee_member")]
[Index("ApplicationId", Name = "fk_commem_application")]
[Index("MemberId", Name = "fk_commem_member")]
[Index("SectionId", Name = "fk_commem_section")]
[Index("ActivityId", "CommitteeRole", "Status", Name = "ix_committee_role")]
[Index("ActivityId", "MemberId", Name = "uq_committee_member", IsUnique = true)]
public partial class TCommitteeMember
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    /// <summary>
    /// NULL untuk pengurus inti (ketua pelaksana, sekretaris, bendahara)
    /// </summary>
    [Column("section_id")]
    public int? SectionId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    /// <summary>
    /// 1=KETUA_PELAKSANA, 2=WAKIL_KETUA, 3=SEKRETARIS, 4=BENDAHARA, 5=KOORDINATOR_SIE, 6=STAF, 7=OFFICIAL, 8=PELATIH
    /// </summary>
    [Column("committee_role")]
    public int CommitteeRole { get; set; }

    [Column("job_desc")]
    [StringLength(500)]
    public string? JobDesc { get; set; }

    /// <summary>
    /// 1=AKTIF, 2=MUNDUR, 3=DIKELUARKAN
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// Jika berasal dari pendaftaran tim pelaksana
    /// </summary>
    [Column("application_id")]
    public int? ApplicationId { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TCommitteeMembers")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("ApplicationId")]
    [InverseProperty("TCommitteeMembers")]
    public virtual TCommitteeApplication? Application { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("TCommitteeMembers")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("SectionId")]
    [InverseProperty("TCommitteeMembers")]
    public virtual TCommitteeSection? Section { get; set; }
}
