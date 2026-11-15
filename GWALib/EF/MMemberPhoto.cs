using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_member_photo")]
[Index("MemberId", Name = "m_member_photo_m_member_FK")]
public partial class MMemberPhoto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("photo", TypeName = "mediumblob")]
    public byte[] Photo { get; set; } = null!;

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("MMemberPhotos")]
    public virtual MMember Member { get; set; } = null!;
}
