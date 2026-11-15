using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_product")]
[Index("ActivityId", Name = "fk_product_activity")]
[Index("Name", Name = "uq_product_name", IsUnique = true)]
public partial class MProduct
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 1=MERCHANDISE, 2=MAKANAN_MINUMAN, 3=JASA, 4=LAINNYA
    /// </summary>
    [Column("product_type")]
    public int ProductType { get; set; }

    [Column("description")]
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Harga jual dasar
    /// </summary>
    [Column("price")]
    [Precision(15, 2)]
    public decimal Price { get; set; }

    /// <summary>
    /// Harga modal per unit (untuk hitung untung)
    /// </summary>
    [Column("cost_price")]
    [Precision(15, 2)]
    public decimal? CostPrice { get; set; }

    [Column("unit")]
    [StringLength(30)]
    public string Unit { get; set; } = null!;

    /// <summary>
    /// 0=Tidak dilacak (jasa/pre-order), 1=Ya
    /// </summary>
    [Column("track_stock")]
    public int TrackStock { get; set; }

    /// <summary>
    /// Stok produk tanpa varian; produk bervarian memakai stok varian
    /// </summary>
    [Column("stock")]
    public int Stock { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_preorder")]
    public int IsPreorder { get; set; }

    [Column("preorder_close_at", TypeName = "datetime")]
    public DateTime? PreorderCloseAt { get; set; }

    [Column("photo", TypeName = "mediumblob")]
    public byte[]? Photo { get; set; }

    [Column("photo_file_name")]
    [StringLength(255)]
    public string? PhotoFileName { get; set; }

    [Column("photo_content_type")]
    [StringLength(100)]
    public string? PhotoContentType { get; set; }

    /// <summary>
    /// Opsional: danus untuk kegiatan tertentu
    /// </summary>
    [Column("activity_id")]
    public int? ActivityId { get; set; }

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

    [ForeignKey("ActivityId")]
    [InverseProperty("MProducts")]
    public virtual TActivity? Activity { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<MProductVariant> MProductVariants { get; set; } = new List<MProductVariant>();

    [InverseProperty("Product")]
    public virtual ICollection<TProductStockMovement> TProductStockMovements { get; set; } = new List<TProductStockMovement>();

    [InverseProperty("Product")]
    public virtual ICollection<TSalesOrderItem> TSalesOrderItems { get; set; } = new List<TSalesOrderItem>();
}
