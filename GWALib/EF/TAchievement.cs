using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_achievement")]
[Index("CompetitionId", Name = "fk_achievement_competition")]
[Index("MemberId", Name = "fk_achievement_member")]
public partial class TAchievement
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("competition_id")]
    public int? CompetitionId { get; set; }

    /// <summary>
    /// NULL jika prestasi tim / organisasi
    /// </summary>
    [Column("member_id")]
    public int? MemberId { get; set; }

    [Column("team_name")]
    [StringLength(150)]
    public string? TeamName { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Juara 1, Finalis, Medali Emas, ...
    /// </summary>
    [Column("rank_title")]
    [StringLength(100)]
    public string? RankTitle { get; set; }

    /// <summary>
    /// 1=INTERNAL, 2=KAMPUS, 3=REGIONAL, 4=NASIONAL, 5=INTERNASIONAL
    /// </summary>
    [Column("level")]
    public int Level { get; set; }

    [Column("achieved_date")]
    public DateOnly AchievedDate { get; set; }

    /// <summary>
    /// File sertifikat
    /// </summary>
    [Column("certificate", TypeName = "mediumblob")]
    public byte[]? Certificate { get; set; }

    [Column("certificate_file_name")]
    [StringLength(255)]
    public string? CertificateFileName { get; set; }

    [Column("photo", TypeName = "mediumblob")]
    public byte[]? Photo { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// 0=Private, 1=Public
    /// </summary>
    [Column("is_public")]
    public int IsPublic { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | disorot di landing page
    /// </summary>
    [Column("is_featured")]
    public int IsFeatured { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("TAchievements")]
    public virtual TCompetition? Competition { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("TAchievements")]
    public virtual MMember? Member { get; set; }

    [InverseProperty("Achievement")]
    public virtual ICollection<TCertificate> TCertificates { get; set; } = new List<TCertificate>();
}
