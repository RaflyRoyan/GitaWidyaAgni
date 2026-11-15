using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_banner")]
[Index("IsActive", "Seq", Name = "ix_banner_active")]
public partial class TBanner
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("subtitle")]
    [StringLength(500)]
    public string? Subtitle { get; set; }

    [Column("image", TypeName = "mediumblob")]
    public byte[] Image { get; set; } = null!;

    [Column("button_text")]
    [StringLength(50)]
    public string? ButtonText { get; set; }

    [Column("link_url")]
    [StringLength(255)]
    public string? LinkUrl { get; set; }

    [Column("seq")]
    public int Seq { get; set; }

    /// <summary>
    /// NULL = langsung tayang
    /// </summary>
    [Column("start_at", TypeName = "datetime")]
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// NULL = tanpa batas
    /// </summary>
    [Column("end_at", TypeName = "datetime")]
    public DateTime? EndAt { get; set; }

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
}
