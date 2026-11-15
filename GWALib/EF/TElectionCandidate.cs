using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_election_candidate")]
[Index("MemberId", Name = "fk_candidate_member")]
[Index("ElectionId", "MemberId", Name = "uq_candidate_member", IsUnique = true)]
[Index("ElectionId", "CandidateNumber", Name = "uq_candidate_number", IsUnique = true)]
public partial class TElectionCandidate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("election_id")]
    public int ElectionId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("candidate_number")]
    public int? CandidateNumber { get; set; }

    [Column("vision", TypeName = "text")]
    public string? Vision { get; set; }

    [Column("mission", TypeName = "text")]
    public string? Mission { get; set; }

    [Column("photo", TypeName = "mediumblob")]
    public byte[]? Photo { get; set; }

    /// <summary>
    /// 1=REGISTERED, 2=VERIFIED, 3=REJECTED, 4=WITHDRAWN
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ElectionId")]
    [InverseProperty("TElectionCandidates")]
    public virtual TElection Election { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TElectionCandidates")]
    public virtual MMember Member { get; set; } = null!;

    [InverseProperty("Candidate")]
    public virtual ICollection<TElectionResult> TElectionResults { get; set; } = new List<TElectionResult>();
}
