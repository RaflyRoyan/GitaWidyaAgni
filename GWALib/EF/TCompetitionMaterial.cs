using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_competition_material")]
[Index("CompetitionId", Name = "fk_material_competition")]
[Index("UploadedBy", Name = "fk_material_uploader")]
public partial class TCompetitionMaterial
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// NULL = materi umum, tidak terikat lomba tertentu
    /// </summary>
    [Column("competition_id")]
    public int? CompetitionId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// 1=FILE, 2=LINK, 3=VIDEO
    /// </summary>
    [Column("material_type")]
    public int MaterialType { get; set; }

    /// <summary>
    /// NULL jika materi berupa link (url)
    /// </summary>
    [Column("file_data")]
    public byte[]? FileData { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string? FileName { get; set; }

    [Column("content_type")]
    [StringLength(100)]
    public string? ContentType { get; set; }

    [Column("url")]
    [StringLength(255)]
    public string? Url { get; set; }

    /// <summary>
    /// 1=PUBLIC, 2=MEMBER, 3=PARTICIPANT
    /// </summary>
    [Column("visibility")]
    public int Visibility { get; set; }

    [Column("uploaded_by")]
    public int UploadedBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("TCompetitionMaterials")]
    public virtual TCompetition? Competition { get; set; }

    [ForeignKey("UploadedBy")]
    [InverseProperty("TCompetitionMaterials")]
    public virtual MUser UploadedByNavigation { get; set; } = null!;
}
