using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Section musik marching band; semua anggota punya section (migrasi 2026-11-18_section, ditulis tangan).
/// </summary>
[Table("m_section")]
[Index("Name", Name = "uq_section_name", IsUnique = true)]
public partial class MSection
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. Brass, Battery, Front Ensemble, Colour Guard
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

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

    [InverseProperty("Section")]
    public virtual ICollection<TSectionMember> TSectionMembers { get; set; } = new List<TSectionMember>();

    [InverseProperty("Section")]
    public virtual ICollection<TActivity> TActivities { get; set; } = new List<TActivity>();

    [InverseProperty("Section")]
    public virtual ICollection<TManagementAssignment> TManagementAssignments { get; set; } = new List<TManagementAssignment>();
}
