using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_organization_photo")]
public partial class MOrganizationPhoto
{
    [Key]
    [Column("seq_photo")]
    public int SeqPhoto { get; set; }

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
}
