using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_product_variant")]
[Index("ProductId", "Name", Name = "uq_variant_name", IsUnique = true)]
public partial class MProductVariant
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    /// <summary>
    /// mis. S, M, L, XL, Hitam-L
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// NULL = ikut harga produk
    /// </summary>
    [Column("price")]
    [Precision(15, 2)]
    public decimal? Price { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Column("sku")]
    [StringLength(50)]
    public string? Sku { get; set; }

    [Column("seq_no")]
    public int SeqNo { get; set; }

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

    [ForeignKey("ProductId")]
    [InverseProperty("MProductVariants")]
    public virtual MProduct Product { get; set; } = null!;

    [InverseProperty("Variant")]
    public virtual ICollection<TProductStockMovement> TProductStockMovements { get; set; } = new List<TProductStockMovement>();

    [InverseProperty("Variant")]
    public virtual ICollection<TSalesOrderItem> TSalesOrderItems { get; set; } = new List<TSalesOrderItem>();
}
