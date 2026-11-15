using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_document_number_format")]
[Index("DocumentType", Name = "uq_number_format", IsUnique = true)]
public partial class MDocumentNumberFormat
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. SURAT_UND, SURAT_SK, TRX, PD, PJM, PGD, SPN, HTG, BA_ST, SERTIFIKAT, NIA, PROPOSAL
    /// </summary>
    [Column("document_type")]
    [StringLength(50)]
    public string DocumentType { get; set; } = null!;

    /// <summary>
    /// Placeholder: {seq} {type} {org} {month_roman} {month} {year} {yy}
    /// </summary>
    [Column("number_format")]
    [StringLength(150)]
    public string NumberFormat { get; set; } = null!;

    /// <summary>
    /// Jumlah digit nomor urut, mis. 3 -&gt; 007
    /// </summary>
    [Column("seq_padding")]
    public int SeqPadding { get; set; }

    /// <summary>
    /// 1=TAHUNAN, 2=BULANAN, 3=TIDAK_PERNAH
    /// </summary>
    [Column("reset_period")]
    public int ResetPeriod { get; set; }

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }
}
