using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_procurement_receipt")]
[Index("ReceivedByMemberId", Name = "fk_pgdrcpt_member")]
[Index("RequestId", Name = "fk_pgdrcpt_request")]
public partial class TProcurementReceipt
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("receipt_date")]
    public DateOnly ReceiptDate { get; set; }

    [Column("received_by_member_id")]
    public int ReceivedByMemberId { get; set; }

    [Column("notes")]
    [StringLength(500)]
    public string? Notes { get; set; }

    [Column("photo", TypeName = "mediumblob")]
    public byte[]? Photo { get; set; }

    [Column("photo_file_name")]
    [StringLength(255)]
    public string? PhotoFileName { get; set; }

    [Column("photo_content_type")]
    [StringLength(100)]
    public string? PhotoContentType { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ReceivedByMemberId")]
    [InverseProperty("TProcurementReceipts")]
    public virtual MMember ReceivedByMember { get; set; } = null!;

    [ForeignKey("RequestId")]
    [InverseProperty("TProcurementReceipts")]
    public virtual TProcurementRequest Request { get; set; } = null!;

    [InverseProperty("Receipt")]
    public virtual ICollection<TProcurementReceiptItem> TProcurementReceiptItems { get; set; } = new List<TProcurementReceiptItem>();
}
