using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_message_outbox")]
[Index("UserId", Name = "fk_outbox_user")]
[Index("Status", "ScheduledAt", Name = "ix_outbox_queue")]
[Index("RefType", "RefId", Name = "ix_outbox_ref")]
public partial class TMessageOutbox
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Penerima jika punya akun
    /// </summary>
    [Column("user_id")]
    public int? UserId { get; set; }

    /// <summary>
    /// Alamat email / nomor WhatsApp
    /// </summary>
    [Column("recipient")]
    [StringLength(150)]
    public string Recipient { get; set; } = null!;

    /// <summary>
    /// 2=EMAIL, 3=WHATSAPP
    /// </summary>
    [Column("channel")]
    public int Channel { get; set; }

    [Column("template_code")]
    [StringLength(50)]
    public string? TemplateCode { get; set; }

    [Column("subject")]
    [StringLength(200)]
    public string? Subject { get; set; }

    /// <summary>
    /// Isi final. Untuk RESET_PASSWORD, kosongkan/samarkan kode setelah terkirim
    /// </summary>
    [Column("body", TypeName = "text")]
    public string Body { get; set; } = null!;

    /// <summary>
    /// Sumber pesan, mis. t_procurement_request
    /// </summary>
    [Column("ref_type")]
    [StringLength(50)]
    public string? RefType { get; set; }

    [Column("ref_id")]
    public int? RefId { get; set; }

    /// <summary>
    /// 1=MENUNGGU, 2=TERKIRIM, 3=GAGAL, 4=DIBATALKAN
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("last_error")]
    [StringLength(500)]
    public string? LastError { get; set; }

    /// <summary>
    /// Kapan boleh dikirim
    /// </summary>
    [Column("scheduled_at", TypeName = "datetime")]
    public DateTime ScheduledAt { get; set; }

    [Column("sent_at", TypeName = "datetime")]
    public DateTime? SentAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TMessageOutboxes")]
    public virtual MUser? User { get; set; }
}
