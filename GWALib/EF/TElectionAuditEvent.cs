using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_election_audit_event")]
[Index("ActorUserId", Name = "fk_elaudit_actor")]
[Index("ElectionId", Name = "fk_elaudit_election")]
public partial class TElectionAuditEvent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("election_id")]
    public int ElectionId { get; set; }

    [Column("actor_user_id")]
    public int? ActorUserId { get; set; }

    /// <summary>
    /// OPENED, CLOSED, CANDIDATE_VERIFIED, RESULT_FINALIZED, ...
    /// </summary>
    [Column("event_type")]
    [StringLength(50)]
    public string EventType { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("ip_address")]
    [StringLength(45)]
    public string? IpAddress { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActorUserId")]
    [InverseProperty("TElectionAuditEvents")]
    public virtual MUser? ActorUser { get; set; }

    [ForeignKey("ElectionId")]
    [InverseProperty("TElectionAuditEvents")]
    public virtual TElection Election { get; set; } = null!;
}
