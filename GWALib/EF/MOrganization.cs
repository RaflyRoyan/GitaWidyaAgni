using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Profil organisasi (satu baris)
/// </summary>
[Table("m_organization")]
public partial class MOrganization
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(2000)]
    public string? Description { get; set; }

    [Column("vision")]
    [StringLength(2000)]
    public string? Vision { get; set; }

    [Column("mission")]
    [StringLength(2000)]
    public string? Mission { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Column("phone")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Column("instagram")]
    [StringLength(100)]
    public string? Instagram { get; set; }

    [Column("founded_date")]
    public DateOnly? FoundedDate { get; set; }

    /// <summary>
    /// Logo organisasi (dikompres aplikasi)
    /// </summary>
    [Column("logo", TypeName = "mediumblob")]
    public byte[]? Logo { get; set; }

    /// <summary>
    /// Nama pembina (hanya dicatat, bukan akun)
    /// </summary>
    [Column("advisor_name")]
    [StringLength(150)]
    public string? AdvisorName { get; set; }

    /// <summary>
    /// Kontak pembina
    /// </summary>
    [Column("advisor_phone")]
    [StringLength(30)]
    public string? AdvisorPhone { get; set; }

    /// <summary>
    /// Email pembina
    /// </summary>
    [Column("advisor_email")]
    [StringLength(150)]
    public string? AdvisorEmail { get; set; }

    /// <summary>
    /// Nomor SK pembina
    /// </summary>
    [Column("advisor_sk_number")]
    [StringLength(100)]
    public string? AdvisorSkNumber { get; set; }

    /// <summary>
    /// File SK pembina
    /// </summary>
    [Column("advisor_sk_file", TypeName = "mediumblob")]
    public byte[]? AdvisorSkFile { get; set; }

    /// <summary>
    /// Nama file SK pembina
    /// </summary>
    [Column("advisor_sk_file_name")]
    [StringLength(255)]
    public string? AdvisorSkFileName { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }
}
