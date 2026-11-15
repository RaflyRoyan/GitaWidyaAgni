using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// MoU sponsorship
/// </summary>
[Table("t_sponsorship_mou")]
[Index("SponsorshipId", Name = "fk_t_sponsorship_mou_sponsorship_id")]
[Index("MouNumber", Name = "uq_sponsorship_mou_number", IsUnique = true)]
public partial class TSponsorshipMou
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sponsorship_id")]
    public int SponsorshipId { get; set; }

    /// <summary>
    /// Nomor MoU/PKS
    /// </summary>
    [Column("mou_number")]
    [StringLength(100)]
    public string MouNumber { get; set; } = null!;

    [Column("signed_date")]
    public DateOnly? SignedDate { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// File MoU
    /// </summary>
    [Column("document")]
    public byte[]? Document { get; set; }

    [Column("document_file_name")]
    [StringLength(255)]
    public string? DocumentFileName { get; set; }

    [Column("document_content_type")]
    [StringLength(100)]
    public string? DocumentContentType { get; set; }

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

    [ForeignKey("SponsorshipId")]
    [InverseProperty("TSponsorshipMous")]
    public virtual TSponsorship Sponsorship { get; set; } = null!;
}
