using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_site_setting")]
[Index("SettingKey", Name = "uq_site_setting", IsUnique = true)]
public partial class MSiteSetting
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. TENTANG, KONTAK, SOSMED, FOOTER
    /// </summary>
    [Column("setting_group")]
    [StringLength(50)]
    public string SettingGroup { get; set; } = null!;

    /// <summary>
    /// mis. about_text, contact_email, instagram_url
    /// </summary>
    [Column("setting_key")]
    [StringLength(100)]
    public string SettingKey { get; set; } = null!;

    [Column("setting_value")]
    [StringLength(2000)]
    public string? SettingValue { get; set; }

    /// <summary>
    /// Untuk pengaturan berupa gambar, mis. logo
    /// </summary>
    [Column("setting_image", TypeName = "mediumblob")]
    public byte[]? SettingImage { get; set; }

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }
}
