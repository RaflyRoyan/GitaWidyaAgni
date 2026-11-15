using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_sales_order")]
[Index("ActivityId", Name = "fk_sales_activity")]
[Index("BuyerMemberId", Name = "fk_sales_buyer")]
[Index("PeriodId", Name = "fk_sales_period")]
[Index("PicMemberId", Name = "fk_sales_pic")]
[Index("PaymentStatus", "FulfillmentStatus", Name = "ix_sales_status")]
[Index("OrderNumber", Name = "uq_sales_order_number", IsUnique = true)]
public partial class TSalesOrder
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Dari penomoran terpusat, jenis DANUS
    /// </summary>
    [Column("order_number")]
    [StringLength(50)]
    public string OrderNumber { get; set; } = null!;

    [Column("period_id")]
    public int? PeriodId { get; set; }

    /// <summary>
    /// Opsional: penjualan untuk kegiatan tertentu
    /// </summary>
    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Pembeli anggota
    /// </summary>
    [Column("buyer_member_id")]
    public int? BuyerMemberId { get; set; }

    /// <summary>
    /// Pembeli non-anggota
    /// </summary>
    [Column("buyer_name")]
    [StringLength(150)]
    public string? BuyerName { get; set; }

    [Column("buyer_phone")]
    [StringLength(30)]
    public string? BuyerPhone { get; set; }

    [Column("order_date")]
    public DateOnly OrderDate { get; set; }

    /// <summary>
    /// Jumlah subtotal item
    /// </summary>
    [Column("total_amount")]
    [Precision(15, 2)]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Jumlah transaksi POSTED dengan sales_order_id ini
    /// </summary>
    [Column("paid_amount")]
    [Precision(15, 2)]
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// 1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS
    /// </summary>
    [Column("payment_status")]
    public int PaymentStatus { get; set; }

    /// <summary>
    /// 1=DIPESAN, 2=SIAP_DIAMBIL, 3=SELESAI, 4=BATAL
    /// </summary>
    [Column("fulfillment_status")]
    public int FulfillmentStatus { get; set; }

    /// <summary>
    /// Penjual / penanggung jawab
    /// </summary>
    [Column("pic_member_id")]
    public int? PicMemberId { get; set; }

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

    [ForeignKey("ActivityId")]
    [InverseProperty("TSalesOrders")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("BuyerMemberId")]
    [InverseProperty("TSalesOrderBuyerMembers")]
    public virtual MMember? BuyerMember { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TSalesOrders")]
    public virtual MManagementPeriod? Period { get; set; }

    [ForeignKey("PicMemberId")]
    [InverseProperty("TSalesOrderPicMembers")]
    public virtual MMember? PicMember { get; set; }

    [InverseProperty("SalesOrder")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Order")]
    public virtual ICollection<TProductStockMovement> TProductStockMovements { get; set; } = new List<TProductStockMovement>();

    [InverseProperty("Order")]
    public virtual ICollection<TSalesOrderItem> TSalesOrderItems { get; set; } = new List<TSalesOrderItem>();
}
