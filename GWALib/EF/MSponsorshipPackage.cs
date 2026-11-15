using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Paket sponsorship milik organisasi
/// </summary>
[Table("m_sponsorship_package")]
[Index("Name", Name = "uq_package_name", IsUnique = true)]
public partial class MSponsorshipPackage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Nama paket, mis. Paket 1
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Nilai paket
    /// </summary>
    [Column("price")]
    [Precision(15, 2)]
    public decimal Price { get; set; }

    /// <summary>
    /// Urutan tampil
    /// </summary>
    [Column("seq_no")]
    public int SeqNo { get; set; }

    /// <summary>
    /// 1=AKTIF, 0=NONAKTIF
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

    [InverseProperty("Package")]
    public virtual ICollection<MSponsorshipPackageBenefit> MSponsorshipPackageBenefits { get; set; } = new List<MSponsorshipPackageBenefit>();

    [InverseProperty("Package")]
    public virtual ICollection<TSponsorship> TSponsorships { get; set; } = new List<TSponsorship>();
}
