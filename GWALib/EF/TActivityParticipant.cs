using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_participant")]
[Index("MemberId", Name = "fk_participant_member")]
[Index("ActivityId", "MemberId", Name = "uq_activity_participant", IsUnique = true)]
public partial class TActivityParticipant
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    /// <summary>
    /// PESERTA, PANITIA, PEMATERI, ...
    /// </summary>
    [Column("participant_role")]
    [StringLength(50)]
    public string ParticipantRole { get; set; } = null!;

    /// <summary>
    /// 1=REGISTERED, 2=CONFIRMED, 3=CANCELLED
    /// </summary>
    [Column("registration_status")]
    public int RegistrationStatus { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TActivityParticipants")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TActivityParticipants")]
    public virtual MMember Member { get; set; } = null!;
}
