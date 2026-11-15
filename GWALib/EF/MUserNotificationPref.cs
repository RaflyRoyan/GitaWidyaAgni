using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_user_notification_pref")]
[Index("UserId", "NotifType", "Channel", Name = "uq_notif_pref", IsUnique = true)]
public partial class MUserNotificationPref
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// Sama dengan t_notification.notif_type / template_code
    /// </summary>
    [Column("notif_type")]
    [StringLength(50)]
    public string NotifType { get; set; } = null!;

    /// <summary>
    /// 1=IN_APP, 2=EMAIL, 3=WHATSAPP
    /// </summary>
    [Column("channel")]
    public int Channel { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_enabled")]
    public int IsEnabled { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("MUserNotificationPrefs")]
    public virtual MUser User { get; set; } = null!;
}
