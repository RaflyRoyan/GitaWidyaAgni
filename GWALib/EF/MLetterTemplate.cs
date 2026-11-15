using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_letter_template")]
public partial class MLetterTemplate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// UNDANGAN, PERMOHONAN, PEMINJAMAN, SK, KETERANGAN, ...
    /// </summary>
    [Column("letter_type")]
    [StringLength(50)]
    public string LetterType { get; set; } = null!;

    /// <summary>
    /// Template HTML dengan placeholder
    /// </summary>
    [Column("body")]
    public string? Body { get; set; }

    /// <summary>
    /// File template .docx (opsional, selain body)
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
    /// 0=Tidak Aktif, 1=Aktif
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

    [InverseProperty("Template")]
    public virtual ICollection<TLetter> TLetters { get; set; } = new List<TLetter>();
}
