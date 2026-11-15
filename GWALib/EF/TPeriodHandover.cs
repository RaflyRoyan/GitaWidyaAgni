using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_period_handover")]
[Index("ApprovedBy", Name = "fk_handover_approver")]
[Index("HandedByMemberId", Name = "fk_handover_handed")]
[Index("ReceivedByMemberId", Name = "fk_handover_received")]
[Index("ToPeriodId", Name = "fk_handover_to")]
[Index("FromPeriodId", Name = "uq_handover_from", IsUnique = true)]
public partial class TPeriodHandover
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("from_period_id")]
    public int FromPeriodId { get; set; }

    [Column("to_period_id")]
    public int ToPeriodId { get; set; }

    /// <summary>
    /// Nomor berita acara
    /// </summary>
    [Column("handover_number")]
    [StringLength(100)]
    public string? HandoverNumber { get; set; }

    [Column("handover_date")]
    public DateOnly HandoverDate { get; set; }

    /// <summary>
    /// Ketua periode lama
    /// </summary>
    [Column("handed_by_member_id")]
    public int? HandedByMemberId { get; set; }

    /// <summary>
    /// Ketua periode baru
    /// </summary>
    [Column("received_by_member_id")]
    public int? ReceivedByMemberId { get; set; }

    [Column("notes")]
    [StringLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Dokumen LPJ akhir periode
    /// </summary>
    [Column("lpj_document", TypeName = "mediumblob")]
    public byte[]? LpjDocument { get; set; }

    [Column("lpj_file_name")]
    [StringLength(255)]
    public string? LpjFileName { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=DIAJUKAN, 3=DISETUJUI, 4=DITOLAK
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("approved_by")]
    public int? ApprovedBy { get; set; }

    [Column("approved_at", TypeName = "datetime")]
    public DateTime? ApprovedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("TPeriodHandovers")]
    public virtual MUser? ApprovedByNavigation { get; set; }

    [ForeignKey("FromPeriodId")]
    [InverseProperty("TPeriodHandoverFromPeriod")]
    public virtual MManagementPeriod FromPeriod { get; set; } = null!;

    [ForeignKey("HandedByMemberId")]
    [InverseProperty("TPeriodHandoverHandedByMembers")]
    public virtual MMember? HandedByMember { get; set; }

    [ForeignKey("ReceivedByMemberId")]
    [InverseProperty("TPeriodHandoverReceivedByMembers")]
    public virtual MMember? ReceivedByMember { get; set; }

    [InverseProperty("Handover")]
    public virtual ICollection<TPeriodHandoverItem> TPeriodHandoverItems { get; set; } = new List<TPeriodHandoverItem>();

    [ForeignKey("ToPeriodId")]
    [InverseProperty("TPeriodHandoverToPeriods")]
    public virtual MManagementPeriod ToPeriod { get; set; } = null!;
}
