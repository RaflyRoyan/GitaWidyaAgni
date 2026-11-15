using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_letter_disposition")]
[Index("FromUserId", Name = "fk_disposition_from")]
[Index("LetterId", Name = "fk_disposition_letter")]
[Index("ToMemberId", Name = "fk_disposition_to")]
public partial class TLetterDisposition
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("letter_id")]
    public int LetterId { get; set; }

    [Column("from_user_id")]
    public int FromUserId { get; set; }

    [Column("to_member_id")]
    public int ToMemberId { get; set; }

    [Column("instruction")]
    [StringLength(500)]
    public string Instruction { get; set; } = null!;

    [Column("due_date")]
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// 1=OPEN, 2=DONE
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("done_at", TypeName = "datetime")]
    public DateTime? DoneAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("FromUserId")]
    [InverseProperty("TLetterDispositions")]
    public virtual MUser FromUser { get; set; } = null!;

    [ForeignKey("LetterId")]
    [InverseProperty("TLetterDispositions")]
    public virtual TLetter Letter { get; set; } = null!;

    [ForeignKey("ToMemberId")]
    [InverseProperty("TLetterDispositions")]
    public virtual MMember ToMember { get; set; } = null!;
}
