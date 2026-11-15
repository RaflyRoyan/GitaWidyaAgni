using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Sponsorship: brand memilih paket
/// </summary>
[Table("t_sponsorship")]
[Index("ActivityId", Name = "fk_spn_activity")]
[Index("ContactId", Name = "fk_spn_contact")]
[Index("ExternalPartyId", Name = "fk_spn_party")]
[Index("PeriodId", Name = "fk_spn_period")]
[Index("PicMemberId", Name = "fk_spn_pic")]
[Index("PackageId", Name = "fk_t_sponsorship_package_id")]
[Index("SponsorshipNumber", Name = "uq_sponsorship_number", IsUnique = true)]
public partial class TSponsorship
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// SPN-{org}-{yyyy}-{seq}
    /// </summary>
    [Column("sponsorship_number")]
    [StringLength(50)]
    public string SponsorshipNumber { get; set; } = null!;

    [Column("external_party_id")]
    public int ExternalPartyId { get; set; }

    [Column("contact_id")]
    public int? ContactId { get; set; }

    /// <summary>
    /// Paket yang dipilih (NULL = kesepakatan khusus)
    /// </summary>
    [Column("package_id")]
    public int? PackageId { get; set; }

    /// <summary>
    /// Salinan nama paket saat dicatat
    /// </summary>
    [Column("package_name")]
    [StringLength(100)]
    public string? PackageName { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    /// <summary>
    /// Kegiatan yang disponsori (opsional)
    /// </summary>
    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// 1=DANA, 2=BARANG, 3=JASA, 4=MEDIA
    /// </summary>
    [Column("sponsorship_form")]
    public int SponsorshipForm { get; set; }

    /// <summary>
    /// Nilai sponsorship yang disepakati
    /// </summary>
    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Rincian jika berupa barang/jasa
    /// </summary>
    [Column("in_kind_description", TypeName = "text")]
    public string? InKindDescription { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("pic_member_id")]
    public int? PicMemberId { get; set; }

    /// <summary>
    /// 1=AKTIF, 2=SELESAI, 3=BATAL
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("notes")]
    [StringLength(500)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TSponsorships")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("ContactId")]
    [InverseProperty("TSponsorships")]
    public virtual MExternalPartyContact? Contact { get; set; }

    [ForeignKey("ExternalPartyId")]
    [InverseProperty("TSponsorships")]
    public virtual MExternalParty ExternalParty { get; set; } = null!;

    [InverseProperty("Sponsorship")]
    public virtual ICollection<MAsset> MAssets { get; set; } = new List<MAsset>();

    [ForeignKey("PackageId")]
    [InverseProperty("TSponsorships")]
    public virtual MSponsorshipPackage? Package { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TSponsorships")]
    public virtual MManagementPeriod? Period { get; set; }

    [ForeignKey("PicMemberId")]
    [InverseProperty("TSponsorships")]
    public virtual MMember? PicMember { get; set; }

    [InverseProperty("Sponsorship")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Sponsorship")]
    public virtual ICollection<TSponsorshipBenefit> TSponsorshipBenefits { get; set; } = new List<TSponsorshipBenefit>();

    [InverseProperty("Sponsorship")]
    public virtual ICollection<TSponsorshipMou> TSponsorshipMous { get; set; } = new List<TSponsorshipMou>();
}
