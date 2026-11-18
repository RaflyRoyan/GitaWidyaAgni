using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_management_assignment")]
[Index("DivisionId", Name = "fk_assignment_division")]
[Index("SectionId", Name = "fk_assignment_section")]
[Index("MemberId", Name = "fk_assignment_member")]
[Index("PositionId", Name = "fk_assignment_position")]
[Index("PeriodId", "IsCurrent", Name = "ix_assignment_current")]
public partial class TManagementAssignment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int PeriodId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("position_id")]
    public int PositionId { get; set; }

    /// <summary>
    /// Divisi yang dipimpin, mis. untuk jabatan Kadiv
    /// </summary>
    [Column("division_id")]
    public int? DivisionId { get; set; }

    /// <summary>
    /// Section yang dipimpin (Section Leader)
    /// </summary>
    [Column("section_id")]
    public int? SectionId { get; set; }

    /// <summary>
    /// Nomor SK pengangkatan
    /// </summary>
    [Column("sk_number")]
    [StringLength(100)]
    public string? SkNumber { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_current")]
    public int IsCurrent { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("DivisionId")]
    [InverseProperty("TManagementAssignments")]
    public virtual MDivision? Division { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("TManagementAssignments")]
    public virtual MSection? Section { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("TManagementAssignments")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("PeriodId")]
    [InverseProperty("TManagementAssignments")]
    public virtual MManagementPeriod Period { get; set; } = null!;

    [ForeignKey("PositionId")]
    [InverseProperty("TManagementAssignments")]
    public virtual MPosition Position { get; set; } = null!;

    [InverseProperty("Assignment")]
    public virtual ICollection<TCertificate> TCertificates { get; set; } = new List<TCertificate>();
}
