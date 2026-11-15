using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_competition_participant")]
[Index("MemberId", Name = "fk_compart_member")]
[Index("CompetitionId", "MemberId", Name = "uq_competition_participant", IsUnique = true)]
public partial class TCompetitionParticipant
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("competition_id")]
    public int CompetitionId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("team_name")]
    [StringLength(150)]
    public string? TeamName { get; set; }

    /// <summary>
    /// KETUA_TIM, ANGGOTA, PEMBIMBING
    /// </summary>
    [Column("participant_role")]
    [StringLength(50)]
    public string ParticipantRole { get; set; } = null!;

    /// <summary>
    /// 1=REGISTERED, 2=PARTICIPATING, 3=FINISHED, 4=WITHDRAWN
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

    [ForeignKey("CompetitionId")]
    [InverseProperty("TCompetitionParticipants")]
    public virtual TCompetition Competition { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TCompetitionParticipants")]
    public virtual MMember Member { get; set; } = null!;
}
