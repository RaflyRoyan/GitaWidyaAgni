using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_aspiration")]
[Index("RespondedBy", Name = "fk_aspiration_responder")]
[Index("UserId", Name = "fk_aspiration_user")]
[Index("Status", Name = "ix_aspiration_target")]
public partial class TAspiration
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// NULL jika dikirim anonim
    /// </summary>
    [Column("user_id")]
    public int? UserId { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_anonymous")]
    public int IsAnonymous { get; set; }

    /// <summary>
    /// 1=ASPIRASI, 2=PENGADUAN, 3=SARAN
    /// </summary>
    [Column("category")]
    public int Category { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("content")]
    [StringLength(2000)]
    public string Content { get; set; } = null!;

    [Column("attachment", TypeName = "mediumblob")]
    public byte[]? Attachment { get; set; }

    [Column("attachment_file_name")]
    [StringLength(255)]
    public string? AttachmentFileName { get; set; }

    /// <summary>
    /// 1=BARU, 2=DIPROSES, 3=SELESAI, 4=DITOLAK
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("response")]
    [StringLength(2000)]
    public string? Response { get; set; }

    [Column("responded_by")]
    public int? RespondedBy { get; set; }

    [Column("responded_at", TypeName = "datetime")]
    public DateTime? RespondedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    /// <summary>
    /// Biarkan NULL jika anonim
    /// </summary>
    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("RespondedBy")]
    [InverseProperty("TAspirationRespondedByNavigations")]
    public virtual MUser? RespondedByNavigation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TAspirationUsers")]
    public virtual MUser? User { get; set; }
}
