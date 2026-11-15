using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_gallery")]
[Index("ActivityId", Name = "fk_gallery_activity")]
public partial class TGallery
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int? ActivityId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("event_date")]
    public DateOnly? EventDate { get; set; }

    /// <summary>
    /// 0=Private, 1=Public
    /// </summary>
    [Column("is_public")]
    public int IsPublic { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=PUBLISHED, 3=ARCHIVED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("published_at", TypeName = "datetime")]
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | disorot di landing page
    /// </summary>
    [Column("is_featured")]
    public int IsFeatured { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TGalleries")]
    public virtual TActivity? Activity { get; set; }

    [InverseProperty("Gallery")]
    public virtual ICollection<TGalleryPhoto> TGalleryPhotos { get; set; } = new List<TGalleryPhoto>();
}
