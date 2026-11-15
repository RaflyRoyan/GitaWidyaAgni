using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_document_number_seq")]
[Index("DocumentType", "PeriodKey", Name = "uq_docnum_seq", IsUnique = true)]
public partial class TDocumentNumberSeq
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("document_type")]
    [StringLength(50)]
    public string DocumentType { get; set; } = null!;

    /// <summary>
    /// Sesuai reset_period: 2026 (tahunan), 2026-10 (bulanan), ALL (tidak pernah)
    /// </summary>
    [Column("period_key")]
    [StringLength(10)]
    public string PeriodKey { get; set; } = null!;

    [Column("last_number")]
    public int LastNumber { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }
}
