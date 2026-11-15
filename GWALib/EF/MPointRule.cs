using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_point_rule")]
[Index("SourceType", Name = "ix_point_rule")]
public partial class MPointRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 1=HADIR_KEGIATAN, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA
    /// </summary>
    [Column("source_type")]
    public int SourceType { get; set; }

    /// <summary>
    /// Opsional: aturan khusus per jenis kegiatan (t_activity.activity_type)
    /// </summary>
    [Column("activity_type")]
    public int? ActivityType { get; set; }

    /// <summary>
    /// Opsional: aturan khusus per tingkat prestasi
    /// </summary>
    [Column("achievement_level")]
    public int? AchievementLevel { get; set; }

    [Column("points")]
    public int Points { get; set; }

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
