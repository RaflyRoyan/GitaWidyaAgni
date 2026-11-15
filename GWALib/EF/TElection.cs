using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_election")]
[Index("FinalizedBy", Name = "fk_election_finalizer")]
[Index("PeriodId", Name = "fk_election_period")]
public partial class TElection
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Periode kepengurusan yang akan dipilih
    /// </summary>
    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("start_at", TypeName = "datetime")]
    public DateTime StartAt { get; set; }

    [Column("end_at", TypeName = "datetime")]
    public DateTime EndAt { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=REGISTRATION, 3=OPEN, 4=CLOSED, 5=FINALIZED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("finalized_at", TypeName = "datetime")]
    public DateTime? FinalizedAt { get; set; }

    [Column("finalized_by")]
    public int? FinalizedBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("FinalizedBy")]
    [InverseProperty("TElections")]
    public virtual MUser? FinalizedByNavigation { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TElections")]
    public virtual MManagementPeriod? Period { get; set; }

    [InverseProperty("Election")]
    public virtual ICollection<TElectionAuditEvent> TElectionAuditEvents { get; set; } = new List<TElectionAuditEvent>();

    [InverseProperty("Election")]
    public virtual ICollection<TElectionCandidate> TElectionCandidates { get; set; } = new List<TElectionCandidate>();

    [InverseProperty("Election")]
    public virtual ICollection<TElectionEligibleVoter> TElectionEligibleVoters { get; set; } = new List<TElectionEligibleVoter>();

    [InverseProperty("Election")]
    public virtual ICollection<TElectionResult> TElectionResults { get; set; } = new List<TElectionResult>();

    [InverseProperty("Election")]
    public virtual ICollection<TElectionVote> TElectionVotes { get; set; } = new List<TElectionVote>();
}
