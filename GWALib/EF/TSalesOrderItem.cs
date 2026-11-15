using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_sales_order_item")]
[Index("OrderId", Name = "fk_salesitem_order")]
[Index("ProductId", Name = "fk_salesitem_product")]
[Index("VariantId", Name = "fk_salesitem_variant")]
public partial class TSalesOrderItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("variant_id")]
    public int? VariantId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Harga saat dipesan
    /// </summary>
    [Column("unit_price")]
    [Precision(15, 2)]
    public decimal UnitPrice { get; set; }

    [Column("subtotal")]
    [Precision(15, 2)]
    public decimal Subtotal { get; set; }

    /// <summary>
    /// mis. nama sablon
    /// </summary>
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
    [InverseProperty("TSalesOrderItems")]
    public virtual TSalesOrder Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("TSalesOrderItems")]
    public virtual MProduct Product { get; set; } = null!;

    [ForeignKey("VariantId")]
    [InverseProperty("TSalesOrderItems")]
    public virtual MProductVariant? Variant { get; set; }
}
