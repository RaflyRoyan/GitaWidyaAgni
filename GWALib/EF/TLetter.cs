using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_letter")]
[Index("CreatedBy", Name = "fk_letter_creator")]
[Index("PeriodId", Name = "fk_letter_period")]
[Index("TemplateId", Name = "fk_letter_template")]
[Index("Direction", "Status", Name = "ix_letter_status")]
[Index("Direction", "LetterNumber", Name = "uq_letter_number", IsUnique = true)]
public partial class TLetter
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("template_id")]
    public int? TemplateId { get; set; }

    /// <summary>
    /// 1=IN, 2=OUT | IN = surat masuk, OUT = surat keluar
    /// </summary>
    [Column("direction")]
    public int Direction { get; set; }

    [Column("letter_type")]
    [StringLength(50)]
    public string LetterType { get; set; } = null!;

    /// <summary>
    /// OUT: diisi saat approved; IN: nomor dari pengirim
    /// </summary>
    [Column("letter_number")]
    [StringLength(100)]
    public string? LetterNumber { get; set; }

    [Column("subject")]
    [StringLength(255)]
    public string Subject { get; set; } = null!;

    [Column("letter_date")]
    public DateOnly? LetterDate { get; set; }

    /// <summary>
    /// Khusus surat masuk
    /// </summary>
    [Column("received_date")]
    public DateOnly? ReceivedDate { get; set; }

    [Column("sender")]
    [StringLength(200)]
    public string? Sender { get; set; }

    [Column("recipient")]
    [StringLength(200)]
    public string? Recipient { get; set; }

    [Column("summary", TypeName = "text")]
    public string? Summary { get; set; }

    [Column("body")]
    public string? Body { get; set; }

    /// <summary>
    /// File PDF final / scan surat masuk
    /// </summary>
    [Column("file_data", TypeName = "mediumblob")]
    public byte[]? FileData { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string? FileName { get; set; }

    [Column("content_type")]
    [StringLength(100)]
    public string? ContentType { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=WAITING_APPROVAL, 3=APPROVED, 4=REJECTED, 5=SENT, 6=RECEIVED, 7=ARCHIVED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TLetters")]
    public virtual MUser CreatedByNavigation { get; set; } = null!;

    [ForeignKey("PeriodId")]
    [InverseProperty("TLetters")]
    public virtual MManagementPeriod? Period { get; set; }

    [InverseProperty("Letter")]
    public virtual ICollection<TBorrowRequest> TBorrowRequests { get; set; } = new List<TBorrowRequest>();

    [InverseProperty("Letter")]
    public virtual ICollection<TLetterApproval> TLetterApprovals { get; set; } = new List<TLetterApproval>();

    [InverseProperty("Letter")]
    public virtual ICollection<TLetterDisposition> TLetterDispositions { get; set; } = new List<TLetterDisposition>();

    [ForeignKey("TemplateId")]
    [InverseProperty("TLetters")]
    public virtual MLetterTemplate? Template { get; set; }
}
