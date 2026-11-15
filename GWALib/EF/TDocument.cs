using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_document")]
[Index("CategoryId", Name = "fk_document_category")]
[Index("UploadedBy", Name = "fk_document_uploader")]
public partial class TDocument
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("uploaded_by")]
    public int UploadedBy { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("file_data")]
    public byte[] FileData { get; set; } = null!;

    [Column("file_name")]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [Column("content_type")]
    [StringLength(100)]
    public string? ContentType { get; set; }

    [Column("file_size")]
    public int? FileSize { get; set; }

    /// <summary>
    /// 1=PUBLIC, 2=MEMBER, 3=PENGURUS
    /// </summary>
    [Column("visibility")]
    public int Visibility { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("TDocuments")]
    public virtual MDocumentCategory Category { get; set; } = null!;

    [ForeignKey("UploadedBy")]
    [InverseProperty("TDocuments")]
    public virtual MUser UploadedByNavigation { get; set; } = null!;
}
