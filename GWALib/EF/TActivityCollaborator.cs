using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_collaborator")]
[Index("ExternalPartyId", Name = "fk_collab_org")]
[Index("RespondedBy", Name = "fk_collab_responder")]
[Index("ActivityId", "ExternalPartyId", Name = "uq_collaborator", IsUnique = true)]
public partial class TActivityCollaborator
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    /// <summary>
    /// Mitra luar yang diajak kolaborasi
    /// </summary>
    [Column("external_party_id")]
    public int ExternalPartyId { get; set; }

    /// <summary>
    /// 1=PENYELENGGARA_BERSAMA, 2=PENDUKUNG
    /// </summary>
    [Column("collab_role")]
    public int CollabRole { get; set; }

    /// <summary>
    /// 1=DIUNDANG, 2=DITERIMA, 3=DITOLAK
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("responded_by")]
    public int? RespondedBy { get; set; }

    [Column("responded_at", TypeName = "datetime")]
    public DateTime? RespondedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TActivityCollaborators")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("ExternalPartyId")]
    [InverseProperty("TActivityCollaborators")]
    public virtual MExternalParty ExternalParty { get; set; } = null!;

    [ForeignKey("RespondedBy")]
    [InverseProperty("TActivityCollaborators")]
    public virtual MUser? RespondedByNavigation { get; set; }
}
