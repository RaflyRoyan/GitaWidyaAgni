using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_period_handover_item")]
[Index("HandoverId", "ItemType", Name = "ix_handover_item_type")]
public partial class TPeriodHandoverItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("handover_id")]
    public int HandoverId { get; set; }

    /// <summary>
    /// 1=SALDO, 2=ASET, 3=PROGRAM, 4=PIUTANG, 5=HUTANG, 6=PINJAMAN, 7=DOKUMEN, 8=LAINNYA, 9=MOU_AKTIF, 10=KEWAJIBAN_SPONSOR, 11=SPONSOR_BERJALAN, 12=PENGADAAN_BERJALAN
    /// </summary>
    [Column("item_type")]
    public int ItemType { get; set; }

    /// <summary>
    /// id sesuai item_type: m_financial_account, m_asset, t_program, t_dues_bill, t_debt, t_borrow_request, t_document, -, t_partnership_agreement, t_sponsorship_benefit, t_sponsorship, t_procurement_request
    /// </summary>
    [Column("ref_id")]
    public int? RefId { get; set; }

    [Column("description")]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    [Column("amount")]
    [Precision(15, 2)]
    public decimal? Amount { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG
    /// </summary>
    [Column("item_condition")]
    public int? ItemCondition { get; set; }

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

    [ForeignKey("HandoverId")]
    [InverseProperty("TPeriodHandoverItems")]
    public virtual TPeriodHandover Handover { get; set; } = null!;
}
