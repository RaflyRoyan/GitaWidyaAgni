using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_committee_section")]
[Index("CoordinatorMemberId", Name = "fk_section_coordinator")]
[Index("ActivityId", "Name", Name = "uq_section_name", IsUnique = true)]
public partial class TCommitteeSection
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    /// <summary>
    /// Acara, Konsumsi, Perlengkapan, Humas, Danus, Dokumentasi, Official, Transportasi &amp; Akomodasi, Kostum, dll.
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Job desk sie
    /// </summary>
    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("coordinator_member_id")]
    public int? CoordinatorMemberId { get; set; }

    [Column("seq_no")]
    public int SeqNo { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TCommitteeSections")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("CoordinatorMemberId")]
    [InverseProperty("TCommitteeSections")]
    public virtual MMember? CoordinatorMember { get; set; }

    [InverseProperty("Section")]
    public virtual ICollection<TActivityChecklist> TActivityChecklists { get; set; } = new List<TActivityChecklist>();

    [InverseProperty("Section")]
    public virtual ICollection<TActivityRundown> TActivityRundowns { get; set; } = new List<TActivityRundown>();

    [InverseProperty("SectionChoice1")]
    public virtual ICollection<TCommitteeApplication> TCommitteeApplicationSectionChoice1s { get; set; } = new List<TCommitteeApplication>();

    [InverseProperty("SectionChoice2")]
    public virtual ICollection<TCommitteeApplication> TCommitteeApplicationSectionChoice2s { get; set; } = new List<TCommitteeApplication>();

    [InverseProperty("Section")]
    public virtual ICollection<TCommitteeMember> TCommitteeMembers { get; set; } = new List<TCommitteeMember>();
}
