using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Daftar benefit per paket
/// </summary>
[Table("m_sponsorship_package_benefit")]
[Index("PackageId", Name = "fk_m_sponsorship_package_benefit_package_id")]
public partial class MSponsorshipPackageBenefit
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("package_id")]
    public int PackageId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Urutan benefit
    /// </summary>
    [Column("seq_no")]
    public int SeqNo { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("PackageId")]
    [InverseProperty("MSponsorshipPackageBenefits")]
    public virtual MSponsorshipPackage Package { get; set; } = null!;
}
