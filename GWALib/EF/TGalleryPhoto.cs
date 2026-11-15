using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_gallery_photo")]
[Index("GalleryId", Name = "fk_galphoto_gallery")]
public partial class TGalleryPhoto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("gallery_id")]
    public int GalleryId { get; set; }

    [Column("photo", TypeName = "mediumblob")]
    public byte[] Photo { get; set; } = null!;

    [Column("caption")]
    [StringLength(255)]
    public string? Caption { get; set; }

    [Column("seq")]
    public int Seq { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | foto sampul galeri
    /// </summary>
    [Column("is_cover")]
    public int IsCover { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("GalleryId")]
    [InverseProperty("TGalleryPhotos")]
    public virtual TGallery Gallery { get; set; } = null!;
}
