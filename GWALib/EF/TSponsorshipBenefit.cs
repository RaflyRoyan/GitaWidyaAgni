using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Checklist benefit per sponsorship
/// </summary>
[Table("t_sponsorship_benefit")]
[Index("SponsorshipId", Name = "fk_benefit_spn")]
[Index("DoneBy", Name = "fk_t_sponsorship_benefit_done_by")]
public partial class TSponsorshipBenefit
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sponsorship_id")]
    public int SponsorshipId { get; set; }

    /// <summary>
    /// Logo di banner, booth, adlibs MC, posting Instagram, dll.
    /// </summary>
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

    /// <summary>
    /// 1=sudah diberikan ke sponsor
    /// </summary>
    [Column("is_done")]
    public int IsDone { get; set; }

    /// <summary>
    /// Kapan dicentang
    /// </summary>
    [Column("done_at", TypeName = "datetime")]
    public DateTime? DoneAt { get; set; }

    /// <summary>
    /// User yang mencentang
    /// </summary>
    [Column("done_by")]
    public int? DoneBy { get; set; }

    /// <summary>
    /// Bukti, mis. foto banner atau screenshot postingan
    /// </summary>
    [Column("proof_file", TypeName = "mediumblob")]
    public byte[]? ProofFile { get; set; }

    [Column("proof_file_name")]
    [StringLength(255)]
    public string? ProofFileName { get; set; }

    [Column("proof_content_type")]
    [StringLength(100)]
    public string? ProofContentType { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("DoneBy")]
    [InverseProperty("TSponsorshipBenefits")]
    public virtual MUser? DoneByNavigation { get; set; }

    [ForeignKey("SponsorshipId")]
    [InverseProperty("TSponsorshipBenefits")]
    public virtual TSponsorship Sponsorship { get; set; } = null!;
}
