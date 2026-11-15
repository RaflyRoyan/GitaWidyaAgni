using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_sequence")]
public partial class MSequence
{
    [Key]
    [Column("table_name")]
    [StringLength(64)]
    public string TableName { get; set; } = null!;

    [Column("last_id")]
    public int LastId { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }
}
