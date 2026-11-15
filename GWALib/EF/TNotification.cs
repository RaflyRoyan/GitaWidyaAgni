using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_notification")]
[Index("UserId", "ReadAt", Name = "ix_notification_unread")]
public partial class TNotification
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// LETTER_APPROVAL, FUNDING_APPROVAL, BORROW_DUE, DUES_BILL, ...
    /// </summary>
    [Column("notif_type")]
    [StringLength(50)]
    public string NotifType { get; set; } = null!;

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("message")]
    [StringLength(500)]
    public string? Message { get; set; }

    [Column("link_url")]
    [StringLength(255)]
    public string? LinkUrl { get; set; }

    [Column("ref_type")]
    [StringLength(50)]
    public string? RefType { get; set; }

    [Column("ref_id")]
    public int? RefId { get; set; }

    [Column("read_at", TypeName = "datetime")]
    public DateTime? ReadAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TNotifications")]
    public virtual MUser User { get; set; } = null!;
}
