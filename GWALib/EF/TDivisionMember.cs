using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_division_member")]
[Index("MemberId", Name = "fk_divmember_member")]
[Index("PeriodId", Name = "fk_divmember_period")]
[Index("DivisionId", "MemberId", "PeriodId", Name = "uq_division_member", IsUnique = true)]
public partial class TDivisionMember
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("division_id")]
    public int DivisionId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    /// <summary>
    /// NULL = keanggotaan tim tidak terikat periode
    /// </summary>
    [Column("period_id")]
    public int? PeriodId { get; set; }

    /// <summary>
    /// mis. KETUA, KAPTEN, ANGGOTA
    /// </summary>
    [Column("member_role")]
    [StringLength(50)]
    public string MemberRole { get; set; } = null!;

    [Column("joined_at")]
    public DateOnly? JoinedAt { get; set; }

    [Column("left_at")]
    public DateOnly? LeftAt { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("DivisionId")]
    [InverseProperty("TDivisionMembers")]
    public virtual MDivision Division { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TDivisionMembers")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("PeriodId")]
    [InverseProperty("TDivisionMembers")]
    public virtual MManagementPeriod? Period { get; set; }
}
