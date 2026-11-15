using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_debt_attachment")]
[Index("DebtId", Name = "fk_debtattach_debt")]
public partial class TDebtAttachment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("debt_id")]
    public int DebtId { get; set; }

    /// <summary>
    /// Bukti hutang (surat perjanjian, nota, faktur, foto)
    /// </summary>
    [Column("file_data", TypeName = "mediumblob")]
    public byte[] FileData { get; set; } = null!;

    [Column("file_name")]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [Column("file_type")]
    [StringLength(100)]
    public string? FileType { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("DebtId")]
    [InverseProperty("TDebtAttachments")]
    public virtual TDebt Debt { get; set; } = null!;
}
