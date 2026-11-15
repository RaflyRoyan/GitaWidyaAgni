using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_message_template")]
[Index("TemplateCode", "Channel", Name = "uq_message_template", IsUnique = true)]
public partial class MMessageTemplate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. RESET_PASSWORD, APPROVAL_WAITING, CHECKLIST_DUE, MOU_EXPIRING, FUNDING_UNSETTLED
    /// </summary>
    [Column("template_code")]
    [StringLength(50)]
    public string TemplateCode { get; set; } = null!;

    /// <summary>
    /// 1=IN_APP, 2=EMAIL, 3=WHATSAPP
    /// </summary>
    [Column("channel")]
    public int Channel { get; set; }

    /// <summary>
    /// Untuk email / judul notifikasi
    /// </summary>
    [Column("subject")]
    [StringLength(200)]
    public string? Subject { get; set; }

    /// <summary>
    /// Isi dengan placeholder, mis. Halo {nama}, kode reset Anda {kode}
    /// </summary>
    [Column("body", TypeName = "text")]
    public string Body { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

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
}
