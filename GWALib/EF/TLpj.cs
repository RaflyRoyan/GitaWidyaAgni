using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Laporan pertanggungjawaban kegiatan / kepengurusan
/// </summary>
[Table("t_lpj")]
[Index("ActivityId", Name = "uq_lpj_activity", IsUnique = true)]
[Index("PeriodId", Name = "uq_lpj_period", IsUnique = true)]
public partial class TLpj
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 1=KEGIATAN, 2=KEPENGURUSAN
    /// </summary>
    [Column("lpj_type")]
    public int LpjType { get; set; }

    /// <summary>
    /// Wajib untuk LPJ kegiatan
    /// </summary>
    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Wajib untuk LPJ kepengurusan
    /// </summary>
    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Pendahuluan
    /// </summary>
    [Column("introduction", TypeName = "text")]
    public string? Introduction { get; set; }

    /// <summary>
    /// Latar belakang (kegiatan)
    /// </summary>
    [Column("background", TypeName = "text")]
    public string? Background { get; set; }

    /// <summary>
    /// Macam kegiatan (kegiatan)
    /// </summary>
    [Column("activity_kind", TypeName = "text")]
    public string? ActivityKind { get; set; }

    /// <summary>
    /// Maksud dan tujuan
    /// </summary>
    [Column("purpose", TypeName = "text")]
    public string? Purpose { get; set; }

    /// <summary>
    /// Bentuk kegiatan (kegiatan)
    /// </summary>
    [Column("activity_form", TypeName = "text")]
    public string? ActivityForm { get; set; }

    /// <summary>
    /// Visi dan misi (kepengurusan)
    /// </summary>
    [Column("vision_mission", TypeName = "text")]
    public string? VisionMission { get; set; }

    /// <summary>
    /// Penutup
    /// </summary>
    [Column("closing", TypeName = "text")]
    public string? Closing { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=FINAL
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// PDF final
    /// </summary>
    [Column("document")]
    public byte[]? Document { get; set; }

    [Column("document_name")]
    [StringLength(255)]
    public string? DocumentName { get; set; }

    /// <summary>
    /// User yang memfinalkan
    /// </summary>
    [Column("finalized_by")]
    public int? FinalizedBy { get; set; }

    [Column("finalized_at", TypeName = "datetime")]
    public DateTime? FinalizedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TLpj")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TLpj")]
    public virtual MManagementPeriod? Period { get; set; }
}
