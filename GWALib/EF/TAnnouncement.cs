using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_announcement")]
[Index("AuthorUserId", Name = "fk_announcement_author")]
[Index("Status", "PublishedAt", Name = "ix_announcement_pub")]
[Index("Slug", Name = "uq_announcement_slug", IsUnique = true)]
public partial class TAnnouncement
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("author_user_id")]
    public int AuthorUserId { get; set; }

    /// <summary>
    /// 1=BERITA, 2=PENGUMUMAN, 3=AGENDA
    /// </summary>
    [Column("category")]
    public int Category { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("slug")]
    [StringLength(220)]
    public string Slug { get; set; } = null!;

    [Column("excerpt")]
    [StringLength(500)]
    public string? Excerpt { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// Foto sampul berita
    /// </summary>
    [Column("cover_photo", TypeName = "mediumblob")]
    public byte[]? CoverPhoto { get; set; }

    /// <summary>
    /// 0=Private, 1=Public | 0 = hanya untuk anggota login
    /// </summary>
    [Column("is_public")]
    public int IsPublic { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | disorot di landing page
    /// </summary>
    [Column("is_featured")]
    public int IsFeatured { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=PUBLISHED, 3=ARCHIVED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("published_at", TypeName = "datetime")]
    public DateTime? PublishedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AuthorUserId")]
    [InverseProperty("TAnnouncements")]
    public virtual MUser AuthorUser { get; set; } = null!;
}
