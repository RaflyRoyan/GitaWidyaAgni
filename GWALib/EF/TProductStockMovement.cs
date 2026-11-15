using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_product_stock_movement")]
[Index("OrderId", Name = "fk_stock_order")]
[Index("ProcurementId", Name = "fk_stock_procurement")]
[Index("VariantId", Name = "fk_stock_variant")]
[Index("ProductId", "VariantId", "MovementDate", Name = "ix_stock_product")]
public partial class TProductStockMovement
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("variant_id")]
    public int? VariantId { get; set; }

    /// <summary>
    /// 1=MASUK (produksi/beli), 2=TERJUAL, 3=PENYESUAIAN, 4=RETUR, 5=RUSAK/HILANG
    /// </summary>
    [Column("movement_type")]
    public int MovementType { get; set; }

    /// <summary>
    /// Positif = bertambah, negatif = berkurang
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("movement_date", TypeName = "datetime")]
    public DateTime MovementDate { get; set; }

    /// <summary>
    /// Untuk TERJUAL / RETUR
    /// </summary>
    [Column("order_id")]
    public int? OrderId { get; set; }

    /// <summary>
    /// Untuk MASUK dari pengadaan
    /// </summary>
    [Column("procurement_id")]
    public int? ProcurementId { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("TProductStockMovements")]
    public virtual TSalesOrder? Order { get; set; }

    [ForeignKey("ProcurementId")]
    [InverseProperty("TProductStockMovements")]
    public virtual TProcurementRequest? Procurement { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("TProductStockMovements")]
    public virtual MProduct Product { get; set; } = null!;

    [ForeignKey("VariantId")]
    [InverseProperty("TProductStockMovements")]
    public virtual MProductVariant? Variant { get; set; }
}
