using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_procurement_receipt_item")]
[Index("ItemId", Name = "fk_rcptitem_item")]
[Index("ReceiptId", "ItemId", Name = "uq_receipt_item", IsUnique = true)]
public partial class TProcurementReceiptItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("receipt_id")]
    public int ReceiptId { get; set; }

    [Column("item_id")]
    public int ItemId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG (sama dengan m_asset.asset_condition)
    /// </summary>
    [Column("item_condition")]
    public int ItemCondition { get; set; }

    [Column("notes")]
    [StringLength(500)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("TProcurementReceiptItems")]
    public virtual TProcurementItem Item { get; set; } = null!;

    [ForeignKey("ReceiptId")]
    [InverseProperty("TProcurementReceiptItems")]
    public virtual TProcurementReceipt Receipt { get; set; } = null!;
}
