using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_election_result")]
[Index("CandidateId", Name = "fk_result_candidate")]
[Index("ElectionId", "CandidateId", Name = "uq_election_result", IsUnique = true)]
public partial class TElectionResult
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("election_id")]
    public int ElectionId { get; set; }

    [Column("candidate_id")]
    public int CandidateId { get; set; }

    [Column("vote_count")]
    public int VoteCount { get; set; }

    [Column("vote_percentage")]
    [Precision(5, 2)]
    public decimal VotePercentage { get; set; }

    [Column("rank_no")]
    public int? RankNo { get; set; }

    /// <summary>
    /// 0=Lose, 1=Winner
    /// </summary>
    [Column("is_winner")]
    public int IsWinner { get; set; }

    [Column("calculated_at", TypeName = "datetime")]
    public DateTime CalculatedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CandidateId")]
    [InverseProperty("TElectionResults")]
    public virtual TElectionCandidate Candidate { get; set; } = null!;

    [ForeignKey("ElectionId")]
    [InverseProperty("TElectionResults")]
    public virtual TElection Election { get; set; } = null!;
}
