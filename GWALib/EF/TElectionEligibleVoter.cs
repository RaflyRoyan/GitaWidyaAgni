using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_election_eligible_voter")]
[Index("MemberId", Name = "fk_voter_member")]
[Index("ElectionId", "MemberId", Name = "uq_eligible_voter", IsUnique = true)]
public partial class TElectionEligibleVoter
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("election_id")]
    public int ElectionId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    /// <summary>
    /// 1=ELIGIBLE, 2=INELIGIBLE
    /// </summary>
    [Column("eligibility_status")]
    public int EligibilityStatus { get; set; }

    /// <summary>
    /// Terisi saat sudah memilih (tanpa menyimpan pilihannya)
    /// </summary>
    [Column("voted_at", TypeName = "datetime")]
    public DateTime? VotedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ElectionId")]
    [InverseProperty("TElectionEligibleVoters")]
    public virtual TElection Election { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TElectionEligibleVoters")]
    public virtual MMember Member { get; set; } = null!;
}
