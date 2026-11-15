using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_financial_attachment")]
[Index("TransactionId", Name = "fk_finattach_trx")]
public partial class TFinancialAttachment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("transaction_id")]
    public int TransactionId { get; set; }

    /// <summary>
    /// Bukti transaksi (nota, kuitansi, foto)
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

    [ForeignKey("TransactionId")]
    [InverseProperty("TFinancialAttachments")]
    public virtual TFinancialTransaction Transaction { get; set; } = null!;
}
